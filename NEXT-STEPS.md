# WingetAppDeployer - Volgende Stappen

Het project is volledig opgezet! Hier is wat je nu moet doen:

## 1. GitHub Repository Setup

### A. Update GitHub gebruikersnaam

✅ **Voltooid!** GitHub username is al ingesteld op `MisterDuckles` in:
- ✅ `src/WingetAppDeployer/Services/GitHubService.cs`
- ✅ `src/Launcher/Program.cs`
- ✅ `README.md`
- ✅ `INTEGRATIE.md`

### B. Maak GitHub Repository

```bash
cd d:\WingetAppDeployer
git add .
git commit -m "Initial commit - WingetAppDeployer v1.0.0"
git remote add origin https://github.com/MisterDuckles/WinGetAppDeployer.git
git push -u origin master
```

## 2. Build het Project

### Optie A: Visual Studio / Rider

1. Open `WingetAppDeployer.sln`
2. Build > Build Solution (Ctrl+Shift+B)
3. Output files:
   - `src/WingetAppDeployer/bin/Release/net8.0-windows/WingetAppDeployer.exe`
   - `src/Launcher/bin/Release/net8.0-windows/Launcher.exe`

### Optie B: Command Line

```bash
cd d:\WingetAppDeployer

# Build main app
dotnet build src/WingetAppDeployer/WingetAppDeployer.csproj -c Release

# Build launcher
dotnet build src/Launcher/Launcher.csproj -c Release
```

## 3. Create GitHub Release

1. Ga naar GitHub → Repositories → WingetAppDeployer
2. Klik "Releases" → "Create a new release"
3. Tag version: `v1.0.0`
4. Release title: `WinApp Installer v1.0.0`
5. Description:
   ```
   First release of WingetAppDeployer!

   Features:
   - 200+ apps across 8 categories
   - Modern UI with theme support
   - Auto-update functionality
   - Scheduled task integration
   - Bootstrap launcher

   Download Launcher.exe to get started!
   ```
6. Upload files:
   - `Launcher.exe` (from `src/Launcher/bin/Release/...`)
   - `WingetAppDeployer.exe` (from `src/WingetAppDeployer/bin/Release/...`)
7. Click "Publish release"

## 4. Test de Installatie

### Test Launcher

```powershell
# Download launcher
Invoke-WebRequest -Uri "https://github.com/MisterDuckles/WinGetAppDeployer/releases/latest/download/Launcher.exe" -OutFile "C:\Temp\Launcher.exe"

# Run it
Start-Process "C:\Temp\Launcher.exe"
```

De launcher zou moeten:
1. De main app downloaden van GitHub
2. De main app opstarten
3. Apps.json laden en tonen

### Test App Installatie

1. Zoek een app (bijv. "7-Zip")
2. Selecteer checkbox
3. Klik "Install Selected Apps"
4. Controleer of installatie werkt

## 5. Integreer met Windows11-Unattended-Debloat

### Methode 1: Direct in debloat.ps1

Voeg toe aan je bestaande `debloat.ps1`:

```powershell
# Install WingetAppDeployer
Write-Host "Installing WingetAppDeployer..." -ForegroundColor Cyan
$installDir = "$env:ProgramFiles\WingetAppDeployer"
$launcherUrl = "https://github.com/MisterDuckles/WinGetAppDeployer/releases/latest/download/Launcher.exe"
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Invoke-WebRequest -Uri $launcherUrl -OutFile "$installDir\Launcher.exe" -UseBasicParsing

# Create desktop shortcut
$desktop = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
$shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut("$desktop\WinApp Installer.lnk")
$shortcut.TargetPath = "$installDir\Launcher.exe"
$shortcut.Save()
```

### Methode 2: Gebruik deploy.ps1

```powershell
# Download and run deploy script
$deployScript = Invoke-WebRequest "https://raw.githubusercontent.com/MisterDuckles/WingetAppDeployer/main/scripts/deploy.ps1" -UseBasicParsing
Invoke-Expression $deployScript.Content
```

## 6. Test Volledige Flow

In een VM of test machine:

1. Run je Windows 11 unattended installatie
2. Debloat script zou moeten runnen
3. WingetAppDeployer launcher wordt geïnstalleerd
4. Desktop shortcut wordt aangemaakt
5. Open WingetAppDeployer
6. Test app installatie

## 7. Optioneel: Extra Features Toevoegen

Zie `CONTRIBUTING.md` voor features om toe te voegen:

- [ ] Dark mode
- [ ] App status indicator (check if already installed)
- [ ] Parallel installations
- [ ] App icons
- [ ] Installation profiles
- [ ] Multi-language support

## Troubleshooting

### Build Errors

**Error: MaterialDesignThemes not found**
```bash
dotnet restore src/WingetAppDeployer/WingetAppDeployer.csproj
```

**Error: .NET 8 not found**
- Download: https://dotnet.microsoft.com/download/dotnet/8.0

### Runtime Errors

**"Winget not found"**
- Installeer App Installer via Microsoft Store
- Of: https://github.com/microsoft/winget-cli/releases

**"Failed to download app database"**
- Check of GitHub repository public is
- Check of `apps.json` in `main` branch zit
- Check internet connectie

### GitHub Release Issues

**"Asset not found"**
- Zorg dat je `Launcher.exe` en `WingetAppDeployer.exe` upload
- Exact die namen gebruiken (case-sensitive on Linux)

## Project Structure Overview

```
d:\WingetAppDeployer\
├── src/
│   ├── WingetAppDeployer/          # Main WPF app
│   │   ├── Models/               # App, Category, Settings models
│   │   ├── Services/             # Winget, GitHub, TaskScheduler services
│   │   ├── Views/                # Install, Settings, Schedule windows
│   │   ├── Themes/               # UI themes
│   │   ├── App.xaml              # Application entry
│   │   └── MainWindow.xaml       # Main UI
│   └── Launcher/                 # Bootstrap launcher (5KB)
│       └── Program.cs            # Downloads & runs main app
├── data/
│   └── apps.json                 # 200+ apps database
├── scripts/
│   └── deploy.ps1                # Deployment script
├── README.md                     # Main documentation
├── INTEGRATIE.md                 # Integration guide
├── CONTRIBUTING.md               # Contribution guide
└── WingetAppDeployer.sln           # Visual Studio solution

Total: ~25 files | ~3000 lines of code
```

## Apps in Database

De `apps.json` bevat nu:

- **8 hoofdcategorieën**
- **15 subcategorieën**
- **200+ apps**

Categorieën:
1. 🌐 Browsers (9 apps)
2. 💼 Development (6 subcategorieën, 40+ apps)
3. 🔐 Security & Privacy (6 apps)
4. 📝 Productivity (3 subcategorieën, 15+ apps)
5. 💬 Communication (7 apps)
6. 🎵 Media & Entertainment (3 subcategorieën, 15+ apps)
7. 🛠️ Utilities (4 subcategorieën, 20+ apps)
8. 🎨 Creative & Design (3 subcategorieën, 10+ apps)

## Volgende Releases

### v1.1.0 (Next)
- [ ] Dark mode
- [ ] App installed status check
- [ ] Better error messages

### v1.2.0
- [ ] Installation profiles
- [ ] Parallel installations
- [ ] App icons

### v2.0.0
- [ ] Plugin system
- [ ] Custom app sources
- [ ] Cloud sync settings

## Support

Vragen? Open een issue op GitHub of stuur me een bericht!

**Succes met je project! 🚀**
