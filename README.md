# WinApp Installer 🪟

Een moderne, grafische applicatie voor het snel installeren van Windows-applicaties via Winget. Perfect om te gebruiken na een fresh Windows installatie!

## ✨ Features

- 🎨 **Moderne UI** met 3 thema's (Minimal, Fluent, Material Design)
- 📦 **200+ apps** verdeeld over categorieën
- 🔍 **Zoekfunctie** om apps snel te vinden
- ⏰ **Auto-update** scheduled tasks voor geïnstalleerde apps
- 🔄 **Zelf-updatende** app (download automatisch nieuwste versie van GitHub)
- 🚀 **Bootstrap launcher** (~5KB) die main app download
- 💾 **JSON-based app database** - eenvoudig apps toevoegen zonder recompile
- 🎯 **Subcategorieën** voor betere organisatie
- ⭐ **Populaire apps** markering

## 🏗️ Project Structuur

```
WinAppInstaller/
├── src/
│   ├── WinAppInstaller/          # Main WPF app
│   │   ├── Models/                # Data models (App, Category, Settings)
│   │   ├── Services/              # Business logic (Winget, GitHub, TaskScheduler)
│   │   ├── Views/                 # XAML windows (Install, Settings, Schedule)
│   │   ├── Themes/                # UI themes
│   │   └── App.xaml               # Main application
│   └── Launcher/                  # Bootstrap launcher (klein exe)
├── data/
│   └── apps.json                  # App database (200+ apps)
├── scripts/
│   └── deploy.ps1                 # Deploy script voor autounattend.xml
└── README.md
```

## 🚀 Quick Start

### Optie 1: Build from Source

**Requirements:**
- .NET 8 SDK
- Visual Studio 2022 of JetBrains Rider

```powershell
# Clone repo
git clone https://github.com/MisterDuckles/WinGetAppDeployer.git
cd WinAppInstaller

# Build main app
cd src/WinAppInstaller
dotnet build -c Release

# Build launcher
cd ../Launcher
dotnet build -c Release

# Executables zijn nu in:
# - src/WinAppInstaller/bin/Release/net8.0-windows/WinAppInstaller.exe
# - src/Launcher/bin/Release/net8.0-windows/Launcher.exe
```

### Optie 2: Download van GitHub Releases

Download de nieuwste release van de [Releases pagina](https://github.com/MisterDuckles/WinGetAppDeployer/releases).

## 🔧 Setup voor Autounattend.xml

Integreer WinAppInstaller in je Windows unattended installatie:

### 1. Voeg PowerShell script toe aan autounattend.xml

```xml
<RunSynchronousCommand wcm:action="add">
    <Order>10</Order>
    <Path>powershell.exe -ExecutionPolicy Bypass -File C:\Setup\Scripts\install-winappinstaller.ps1</Path>
</RunSynchronousCommand>
```

### 2. Maak install script

Maak `install-winappinstaller.ps1`:

```powershell
# Download launcher naar Program Files
$launcherUrl = "https://github.com/MisterDuckles/WinGetAppDeployer/releases/latest/download/Launcher.exe"
$installDir = "C:\Program Files\WinAppInstaller"
$launcherPath = Join-Path $installDir "WinAppInstaller-Launcher.exe"

# Create directory
New-Item -ItemType Directory -Path $installDir -Force | Out-Null

# Download launcher
Invoke-WebRequest -Uri $launcherUrl -OutFile $launcherPath

# Create desktop shortcut
$desktopPath = [Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path $desktopPath "WinApp Installer.lnk"
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $launcherPath
$shortcut.IconLocation = $launcherPath
$shortcut.Save()

# Run launcher (will download and start main app)
Start-Process $launcherPath
```

### 3. Of: Direct integreren met je huidige debloat script

Voeg toe aan je `debloat.ps1`:

```powershell
# Install WinAppInstaller
Write-Host "Installing WinAppInstaller..."
$launcherUrl = "https://github.com/MisterDuckles/WinGetAppDeployer/releases/latest/download/Launcher.exe"
$installDir = "$env:ProgramFiles\WinAppInstaller"
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Invoke-WebRequest -Uri $launcherUrl -OutFile "$installDir\Launcher.exe"

# Create shortcut
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\WinApp Installer.lnk")
$Shortcut.TargetPath = "$installDir\Launcher.exe"
$Shortcut.Save()
```

## 📝 Apps Toevoegen/Aanpassen

Apps worden beheerd in `data/apps.json`. Om een app toe te voegen:

```json
{
  "name": "Nieuwe App",
  "wingetId": "Publisher.AppName",
  "description": "Korte beschrijving van de app",
  "popular": false
}
```

**Winget ID vinden:**
```powershell
winget search "App Name"
```

**Apps.json updaten:**
1. Edit `data/apps.json`
2. Commit en push naar GitHub
3. Alle gebruikers zien automatisch de nieuwe apps!

## 🎨 Thema's

De app ondersteunt 3 thema's:

1. **Minimal** (Default) - Clean, modern, veel witruimte
2. **Fluent** - Windows 11 native look
3. **Material** - Google Material Design 3

Wissel van thema via: **Settings → Appearance → Theme**

## ⏰ Auto-Update Setup

Na het installeren van apps wordt gevraagd of je auto-updates wil instellen:

- **Daily** - Check elke dag om [tijd]
- **Weekly** - Check wekelijks op maandag
- **On Startup** - Check bij elke Windows login

Dit maakt een scheduled task aan in Task Scheduler die:
```powershell
WinAppInstaller.exe /autoupdate
```
uitvoert, welke `winget upgrade --all` runt.

## 🔄 App Updates

De app checkt automatisch bij opstarten naar nieuwe versies op GitHub. Als er een update is:

1. Popup met nieuwe versie nummer
2. "Yes" = Download en installeer update
3. App herstart automatisch

**Handmatig checken:**
```powershell
# Windows Task Scheduler task
schtasks /run /tn "WinAppInstaller_AutoUpdate"
```

## 📦 GitHub Release Proces

### 1. Build Release versie

```powershell
# Build main app
cd src/WinAppInstaller
dotnet publish -c Release -r win-x64 --self-contained false

# Build launcher
cd ../Launcher
dotnet publish -c Release -r win-x64 --self-contained false
```

### 2. Create GitHub Release

1. Ga naar GitHub → Releases → New Release
2. Tag version: `v1.0.0`
3. Release title: `WinApp Installer v1.0.0`
4. Upload:
   - `WinAppInstaller.exe` (main app)
   - `Launcher.exe` (bootstrap launcher)
5. Publish release

### 3. Gebruikers krijgen automatisch update bij volgende start!

## 🛠️ Development

### Project opzetten

```bash
# Clone repository
git clone https://github.com/MisterDuckles/WinGetAppDeployer.git
cd WinAppInstaller

# Restore packages
dotnet restore src/WinAppInstaller/WinAppInstaller.csproj
dotnet restore src/Launcher/Launcher.csproj

# Open in IDE
# Visual Studio: Open WinAppInstaller.sln
# Rider: Open WinAppInstaller.sln
# VS Code: Open folder
```

### Belangrijke Files

| File | Beschrijving |
|------|--------------|
| `data/apps.json` | App database - voeg hier apps toe |
| `Services/WingetService.cs` | Winget installatie logica |
| `Services/GitHubService.cs` | Download apps.json & check updates |
| `Services/TaskSchedulerService.cs` | Scheduled tasks aanmaken |
| `MainWindow.xaml.cs` | Main UI logica |
| `Launcher/Program.cs` | Bootstrap launcher |

### Dependencies

- **MaterialDesignThemes** - Modern UI components
- **Newtonsoft.Json** - JSON parsing
- **CommunityToolkit.Mvvm** - MVVM helpers

## 📋 TODO / Roadmap

- [x] Basic app installatie via Winget
- [x] JSON-based app database
- [x] GitHub auto-update
- [x] Bootstrap launcher
- [x] Scheduled tasks voor auto-update
- [x] Settings window
- [ ] Dark mode theme
- [ ] Installatie profiles (Gaming Setup, Dev Setup, etc.)
- [ ] Export/Import geselecteerde apps
- [ ] Check of apps al geïnstalleerd zijn (status indicator)
- [ ] Parallel app installatie (meerdere tegelijk)
- [ ] App icons tonen (download van GitHub/CDN)
- [ ] Nederlands/Engels taalondersteuning

## 🤝 Contributing

Contributions zijn welkom! Om een app toe te voegen:

1. Fork het project
2. Edit `data/apps.json`
3. Voeg je app toe met correcte wingetId
4. Create Pull Request

## 📜 License

MIT License - zie LICENSE file

## ⚡ Troubleshooting

### "Winget not found"
- Installeer **App Installer** via Microsoft Store
- Of download van: https://github.com/microsoft/winget-cli/releases

### "Failed to download app database"
- Check internet verbinding
- Check of GitHub bereikbaar is
- Firewall kan GitHub blokkeren

### "Scheduled task failed"
- Run app als Administrator
- Check Task Scheduler voor error logs
- Task naam: `WinAppInstaller_AutoUpdate`

### App installeert niet
- Check winget: `winget search <appname>`
- Check winget ID in apps.json
- Sommige apps vereisen admin rechten

## 📞 Support

Issues of vragen? Open een [GitHub Issue](https://github.com/MisterDuckles/WinGetAppDeployer/issues)!

---

**Gemaakt met ❤️ voor eenvoudigere Windows installaties**
