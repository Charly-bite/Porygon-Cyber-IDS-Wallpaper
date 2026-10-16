# Install & Launch Script for Porygon Cyber IDS Wallpaper Suite

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$installDir = Join-Path $env:LOCALAPPDATA "Programs\HackerWallpaper"
New-Item -ItemType Directory -Path "$installDir\Models" -Force | Out-Null

# Compile first
& "$PSScriptRoot\build.ps1"

Stop-Process -Name "HackerWallpaper" -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400

Copy-Item "$repoRoot\bin\HackerWallpaper.exe" "$installDir\HackerWallpaper.exe" -Force
Copy-Item "$repoRoot\bin\HackerLockScreen.exe" "$installDir\HackerLockScreen.exe" -Force
Copy-Item "$repoRoot\Models\*.glb" "$installDir\Models\" -Force

# Register auto-start in HKCU Run
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Set-ItemProperty -Path $runKey -Name "HackerWallpaper" -Value "`"$installDir\HackerWallpaper.exe`""

# Launch Wallpaper
([wmiclass]"Win32_Process").Create("$installDir\HackerWallpaper.exe") | Out-Null
Write-Host "HackerWallpaper instalado e iniciado desde: $installDir" -ForegroundColor Green
