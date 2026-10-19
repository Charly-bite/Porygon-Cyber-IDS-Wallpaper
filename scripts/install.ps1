# Install & Launch Script for Porygon Cyber IDS Wallpaper Suite

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$installDir = Join-Path $env:LOCALAPPDATA "Programs\HackerWallpaper"
New-Item -ItemType Directory -Path "$installDir\Models" -Force | Out-Null
New-Item -ItemType Directory -Path "$installDir\web" -Force | Out-Null

# Compile first
& "$PSScriptRoot\build.ps1"

Stop-Process -Name "HackerWallpaper" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "TrayAutoHide" -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400

# Clean up legacy Startup shortcuts that pointed to old TrayAutoHide
$startupDir = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
Remove-Item "$startupDir\TrayAutoHide.lnk" -Force -ErrorAction SilentlyContinue
Remove-Item "$startupDir\HackerWallpaper.lnk" -Force -ErrorAction SilentlyContinue

Copy-Item "$repoRoot\bin\HackerWallpaper.exe" "$installDir\HackerWallpaper.exe" -Force
Copy-Item "$repoRoot\bin\HackerLockScreen.exe" "$installDir\HackerLockScreen.exe" -Force
Copy-Item "$repoRoot\bin\TrayAutoHide.exe" "$installDir\TrayAutoHide.exe" -Force
Copy-Item "$repoRoot\Models\*.glb" "$installDir\Models\" -Force
if (Test-Path "$repoRoot\Models\flywire_brain.bin") {
    Copy-Item "$repoRoot\Models\flywire_brain.bin" "$installDir\Models\" -Force
    Copy-Item "$repoRoot\Models\flywire_brain_manifest.json" "$installDir\Models\" -Force -ErrorAction SilentlyContinue
}
if (Test-Path "$repoRoot\web") {
    Copy-Item "$repoRoot\web\*" "$installDir\web\" -Recurse -Force
}

# Register auto-start in HKCU Run, Startup Folder, Start Menu Hotkeys, and Interactive Scheduled Task
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Set-ItemProperty -Path $runKey -Name "HackerWallpaper" -Value "`"$installDir\HackerWallpaper.exe`""
Set-ItemProperty -Path $runKey -Name "TrayAutoHide" -Value "`"$installDir\TrayAutoHide.exe`""
Remove-ItemProperty -Path $runKey -Name "TranslucentTB" -ErrorAction SilentlyContinue

try {
    $wsh = New-Object -ComObject WScript.Shell
    $startPrograms = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs"

    # Startup folder shortcut
    $lnkBoot = $wsh.CreateShortcut("$startupDir\PorygonWallpaper.lnk")
    $lnkBoot.TargetPath = "$installDir\HackerWallpaper.exe"
    $lnkBoot.WorkingDirectory = $installDir
    $lnkBoot.Save()

    # Start Menu shortcut for Control Panel (Ctrl+Alt+G)
    $lnkCtrlG = $wsh.CreateShortcut("$startPrograms\Porygon Control Panel.lnk")
    $lnkCtrlG.TargetPath = "$installDir\HackerWallpaper.exe"
    $lnkCtrlG.Arguments = "--config"
    $lnkCtrlG.WorkingDirectory = $installDir
    $lnkCtrlG.Hotkey = "Ctrl+Alt+G"
    $lnkCtrlG.Save()

    # Start Menu shortcut for Control Panel (Ctrl+Alt+W)
    $lnkCtrlW = $wsh.CreateShortcut("$startPrograms\Porygon Wallpaper Config.lnk")
    $lnkCtrlW.TargetPath = "$installDir\HackerWallpaper.exe"
    $lnkCtrlW.Arguments = "--config"
    $lnkCtrlW.WorkingDirectory = $installDir
    $lnkCtrlW.Hotkey = "Ctrl+Alt+W"
    $lnkCtrlW.Save()

    # Start Menu shortcut for Porygon Web SOC Dashboard (Ctrl+Alt+S)
    $lnkWebSoc = $wsh.CreateShortcut("$startPrograms\Porygon Web SOC.lnk")
    $lnkWebSoc.TargetPath = "$installDir\HackerWallpaper.exe"
    $lnkWebSoc.Arguments = "--web"
    $lnkWebSoc.WorkingDirectory = $installDir
    $lnkWebSoc.Hotkey = "Ctrl+Alt+S"
    $lnkWebSoc.Save()

    # Start Menu shortcut for Quick Terminal Opacity Toggle 100% <-> 50% (Ctrl+Alt+O)
    $lnkOpToggle = $wsh.CreateShortcut("$startPrograms\Porygon Terminal Opacity Toggle.lnk")
    $lnkOpToggle.TargetPath = "$installDir\HackerWallpaper.exe"
    $lnkOpToggle.Arguments = "--opacity-toggle"
    $lnkOpToggle.WorkingDirectory = $installDir
    $lnkOpToggle.Hotkey = "Ctrl+Alt+O"
    $lnkOpToggle.Save()

    # Start Menu shortcut for Increasing Terminal Opacity +15%
    $lnkOpUp = $wsh.CreateShortcut("$startPrograms\Porygon Aumentar Opacidad Terminal (+15%).lnk")
    $lnkOpUp.TargetPath = "$installDir\HackerWallpaper.exe"
    $lnkOpUp.Arguments = "--opacity-up"
    $lnkOpUp.WorkingDirectory = $installDir
    $lnkOpUp.Save()
} catch { }

$trigger = New-ScheduledTaskTrigger -AtLogOn -User $env:USERNAME
$trigger.Delay = "PT4S"
$principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero)

$actionWp = New-ScheduledTaskAction -Execute "$installDir\HackerWallpaper.exe" -WorkingDirectory $installDir
Register-ScheduledTask -TaskName "Porygon_Cyber_IDS_Wallpaper" -Action $actionWp -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
Start-ScheduledTask -TaskName "Porygon_Cyber_IDS_Wallpaper"

$actionTray = New-ScheduledTaskAction -Execute "$installDir\TrayAutoHide.exe" -WorkingDirectory $installDir
Register-ScheduledTask -TaskName "Porygon_Tray_AutoHide" -Action $actionTray -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
Start-ScheduledTask -TaskName "Porygon_Tray_AutoHide"

Write-Host "HackerWallpaper + TrayAutoHide instalados e iniciados desde: $installDir" -ForegroundColor Green
