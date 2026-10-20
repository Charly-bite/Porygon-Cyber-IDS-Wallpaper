# Enable Wake-on-LAN (WoL) on Realtek Gaming 2.5GbE & Intel I225-V and disable Fast Startup (Hiberboot)
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Start-Process powershell.exe -Verb RunAs -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`""
    exit
}

Write-Host "[1/3] Habilitando Wake-on-Magic-Packet y S5 Wake-on-LAN en Ethernet (Realtek 2.5GbE)..." -ForegroundColor Cyan
Set-NetAdapterAdvancedProperty -Name "Ethernet" -RegistryKeyword "*WakeOnMagicPacket" -RegistryValue 1 -ErrorAction SilentlyContinue
Set-NetAdapterAdvancedProperty -Name "Ethernet" -RegistryKeyword "*WakeOnPattern" -RegistryValue 1 -ErrorAction SilentlyContinue
Set-NetAdapterAdvancedProperty -Name "Ethernet" -RegistryKeyword "S5WakeOnLan" -RegistryValue 1 -ErrorAction SilentlyContinue
Set-NetAdapterAdvancedProperty -Name "Ethernet" -RegistryKeyword "*EEE" -RegistryValue 0 -ErrorAction SilentlyContinue
Set-NetAdapterAdvancedProperty -Name "Ethernet" -RegistryKeyword "EnableGreenEthernet" -RegistryValue 0 -ErrorAction SilentlyContinue
Set-NetAdapterAdvancedProperty -Name "Ethernet" -RegistryKeyword "PowerSavingMode" -RegistryValue 0 -ErrorAction SilentlyContinue

Write-Host "[2/3] Armando dispositivo Realtek Gaming 2.5GbE Family Controller en powercfg..." -ForegroundColor Cyan
powercfg -deviceenablewake "Realtek Gaming 2.5GbE Family Controller"
powercfg -deviceenablewake "Intel(R) Ethernet Controller I225-V" 2>$null

Write-Host "[3/3] Desactivando Inicio Rapido (HiberbootEnabled=0) para permitir WoL desde apagado S5..." -ForegroundColor Cyan
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Power" -Name "HiberbootEnabled" -Value 0 -Type DWord

Write-Host "Dispositivos armados para Wake-on-LAN:" -ForegroundColor Green
powercfg -devicequery wake_armed
