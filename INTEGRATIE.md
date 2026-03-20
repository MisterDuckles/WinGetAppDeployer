# Integratie met Windows11-Unattended-Debloat

Dit document legt uit hoe je WinAppInstaller integreert met je bestaande Windows 11 unattended installatie.

## Optie 1: Integreren in bestaande debloat.ps1

Voeg het volgende toe aan je `debloat.ps1` script (aan het einde):

```powershell
# ============================================
# Install WinAppInstaller
# ============================================
Write-Host "Installing WinAppInstaller..." -ForegroundColor Cyan

try {
    # Configuration
    $gitHubUser = "MisterDuckles"
    $repoName = "WinAppInstaller"
    $installDir = "$env:ProgramFiles\WinAppInstaller"
    $launcherUrl = "https://github.com/$gitHubUser/$repoName/releases/latest/download/Launcher.exe"
    $launcherPath = Join-Path $installDir "WinAppInstaller-Launcher.exe"

    # Create directory
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null

    # Download launcher
    Invoke-WebRequest -Uri $launcherUrl -OutFile $launcherPath -UseBasicParsing
    Write-Host "✓ Downloaded WinAppInstaller Launcher" -ForegroundColor Green

    # Create desktop shortcut for all users
    $publicDesktop = [Environment]::GetFolderPath([Environment+SpecialFolder]::CommonDesktopDirectory)
    $shortcutPath = Join-Path $publicDesktop "WinApp Installer.lnk"

    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = $launcherPath
    $Shortcut.WorkingDirectory = $installDir
    $Shortcut.Description = "Install Windows applications using Winget"
    $Shortcut.Save()

    Write-Host "✓ Created desktop shortcut" -ForegroundColor Green

    # Auto-launch on first logon (optional)
    # Uncomment if you want to launch automatically:
    # Start-Process $launcherPath

} catch {
    Write-Host "✗ Failed to install WinAppInstaller: $($_.Exception.Message)" -ForegroundColor Red
}
```

## Optie 2: Aparte scheduled task (First Logon)

Als je WinAppInstaller wil runnen bij eerste login (zoals je nu met je debloat script doet):

### Methode A: Via Registry RunOnce

Voeg toe aan je autounattend.xml of setup script:

```powershell
# Download launcher
$launcherUrl = "https://github.com/MisterDuckles/WinGetAppDeployer/releases/latest/download/Launcher.exe"
$launcherPath = "$env:ProgramFiles\WinAppInstaller\Launcher.exe"

New-Item -ItemType Directory -Path (Split-Path $launcherPath) -Force | Out-Null
Invoke-WebRequest -Uri $launcherUrl -OutFile $launcherPath -UseBasicParsing

# Add to RunOnce registry
$runOncePath = "HKLM:\Software\Microsoft\Windows\CurrentVersion\RunOnce"
Set-ItemProperty -Path $runOncePath -Name "WinAppInstaller" -Value $launcherPath
```

### Methode B: Via Scheduled Task (zoals je huidige debloat setup)

Voeg toe aan je `autounattend.xml` in de `<FirstLogonCommands>` sectie:

```xml
<SynchronousCommand wcm:action="add">
    <Order>20</Order>
    <CommandLine>powershell.exe -ExecutionPolicy Bypass -File C:\Setup\Scripts\install-winappinstaller.ps1</CommandLine>
    <Description>Install WinAppInstaller</Description>
</SynchronousCommand>
```

En maak `C:\Setup\Scripts\install-winappinstaller.ps1`:

```powershell
# Install and setup WinAppInstaller
$taskName = "WinAppInstaller-FirstRun"
$gitHubUser = "MisterDuckles"
$repoName = "WinAppInstaller"

# Download launcher
$installDir = "$env:ProgramFiles\WinAppInstaller"
$launcherPath = Join-Path $installDir "Launcher.exe"
$launcherUrl = "https://github.com/$gitHubUser/$repoName/releases/latest/download/Launcher.exe"

New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Invoke-WebRequest -Uri $launcherUrl -OutFile $launcherPath -UseBasicParsing

# Create scheduled task to run on first logon
$action = New-ScheduledTaskAction -Execute $launcherPath
$trigger = New-ScheduledTaskTrigger -AtLogOn
$principal = New-ScheduledTaskPrincipal -UserId "$env:COMPUTERNAME\$env:USERNAME" -LogonType Interactive -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

# Register task
Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force

# Create desktop shortcut
$desktop = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
$shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut("$desktop\WinApp Installer.lnk")
$shortcut.TargetPath = $launcherPath
$shortcut.Save()
```

## Optie 3: Volledig Standalone (Geen Integratie)

Als je WinAppInstaller gewoon beschikbaar wil maken zonder automatische installatie:

1. Build het project
2. Upload releases naar GitHub
3. Gebruikers kunnen het handmatig downloaden en installeren
4. Of: Plaats de launcher in je Windows installatie media en kopieer het naar C:\ tijdens setup

## Voorbeeld: Volledige Integratie Flow

Hier is een compleet voorbeeld van hoe je huidige setup eruit kan zien:

```
autounattend.xml
  ↓
FirstLogonCommands
  ↓
C:\Setup\Scripts\launcher.ps1 (jouw huidige launcher)
  ↓ (downloads debloat.ps1 from GitHub)
  ↓
debloat.ps1
  ├─ Debloat Windows
  ├─ Remove OneDrive
  ├─ Install Firefox/Chrome
  └─ ✨ NEW: Install WinAppInstaller
       ├─ Download Launcher.exe
       ├─ Create Desktop Shortcut
       └─ (Optionally) Auto-launch
```

### Aangepaste launcher.ps1

Update je bestaande `launcher.ps1` om ook WinAppInstaller te installeren:

```powershell
# ... jouw bestaande code ...

# Run debloat script
Invoke-Expression $debloatScript

# NEW: Install WinAppInstaller
Write-Host "Installing WinAppInstaller..." -ForegroundColor Cyan
$installerScript = Invoke-WebRequest -Uri "https://raw.githubusercontent.com/MisterDuckles/WinAppInstaller/main/scripts/deploy.ps1" -UseBasicParsing
Invoke-Expression $installerScript.Content

Write-Host "Setup complete!" -ForegroundColor Green
```

## Best Practices

1. **Test eerst lokaal** - Test de integratie in een VM voordat je het in production neemt
2. **Error handling** - Voeg try/catch blocks toe voor betere error reporting
3. **Logging** - Log alle stappen naar een file voor troubleshooting
4. **User choice** - Overweeg om te vragen of user WinAppInstaller wil installeren
5. **Timing** - Launch WinAppInstaller NA de debloat is voltooid

## Troubleshooting

### WinAppInstaller start niet
- Check of Winget beschikbaar is (`winget --version`)
- Check internet connectie
- Check Windows Defender/Firewall

### Launcher download failed
- Zorg dat GitHub release is gepubliceerd
- Check download URL
- Check of GitHub bereikbaar is tijdens setup

### Desktop shortcut niet aangemaakt
- Check permissions (moet als admin runnen)
- Check of $publicDesktop path bestaat

## Vragen?

Open een issue op GitHub of contact me! 🚀
