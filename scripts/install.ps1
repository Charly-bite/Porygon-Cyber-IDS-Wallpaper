# Install & Launch Script for Porygon Cyber IDS Wallpaper Suite

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$installDir = Join-Path $env:LOCALAPPDATA "Programs\HackerWallpaper"
New-Item -ItemType Directory -Path "$installDir\Models" -Force | Out-Null

# Compile first
& "$PSScriptRoot\build.ps1"

Stop-Process -Name "HackerWallpaper" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "TrayAutoHide" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "TranslucentTB" -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400

# Clean up legacy Startup shortcuts that pointed to old TrayAutoHide or TranslucentTB
$startupDir = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
Remove-Item "$startupDir\TranslucentTB.lnk" -Force -ErrorAction SilentlyContinue
Remove-Item "$startupDir\TrayAutoHide.lnk" -Force -ErrorAction SilentlyContinue
Remove-Item "$startupDir\HackerWallpaper.lnk" -Force -ErrorAction SilentlyContinue

Copy-Item "$repoRoot\bin\HackerWallpaper.exe" "$installDir\HackerWallpaper.exe" -Force
Copy-Item "$repoRoot\bin\HackerLockScreen.exe" "$installDir\HackerLockScreen.exe" -Force
Copy-Item "$repoRoot\bin\TrayAutoHide.exe" "$installDir\TrayAutoHide.exe" -Force
Copy-Item "$repoRoot\Models\*.glb" "$installDir\Models\" -Force

# Register auto-start in HKCU Run and Interactive Scheduled Task
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Set-ItemProperty -Path $runKey -Name "HackerWallpaper" -Value "`"$installDir\HackerWallpaper.exe`""
Set-ItemProperty -Path $runKey -Name "TrayAutoHide" -Value "`"$installDir\TrayAutoHide.exe`""
Remove-ItemProperty -Path $runKey -Name "TranslucentTB" -ErrorAction SilentlyContinue

$trigger = New-ScheduledTaskTrigger -AtLogOn -User $env:USERNAME
$principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero)

$actionWp = New-ScheduledTaskAction -Execute "$installDir\HackerWallpaper.exe" -WorkingDirectory $installDir
Register-ScheduledTask -TaskName "Porygon_Cyber_IDS_Wallpaper" -Action $actionWp -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
Start-ScheduledTask -TaskName "Porygon_Cyber_IDS_Wallpaper"

$actionTray = New-ScheduledTaskAction -Execute "$installDir\TrayAutoHide.exe" -WorkingDirectory $installDir
Register-ScheduledTask -TaskName "Porygon_Tray_AutoHide" -Action $actionTray -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
Start-ScheduledTask -TaskName "Porygon_Tray_AutoHide"

Write-Host "HackerWallpaper + TrayAutoHide instalados e iniciados desde: $installDir" -ForegroundColor Green
