# Build Script for Porygon Cyber IDS Wallpaper Suite
# Uses the built-in Windows .NET Framework 4.x C# Compiler (csc.exe) — Zero external SDK required!

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$binDir   = Join-Path $repoRoot "bin"
New-Item -ItemType Directory -Path $binDir -Force | Out-Null

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    Write-Error "No se encontro csc.exe en $csc"
}

Write-Host "[1/3] Compilando HackerWallpaper.exe (Enjambre Porygon 3D + IDS + Radar 360)..." -ForegroundColor Cyan
& $csc /target:winexe `
       /out:"$binDir\HackerWallpaper.exe" `
       /win32manifest:"$repoRoot\src\app.manifest" `
       /optimize+ `
       /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Net.NetworkInformation.dll,System.Web.Extensions.dll `
       "$repoRoot\src\HackerWallpaper.cs"

Write-Host "[2/3] Compilando HackerLockScreen.exe (Pantalla de Bloqueo Cyberpunk)..." -ForegroundColor Cyan
& $csc /target:winexe `
       /out:"$binDir\HackerLockScreen.exe" `
       /win32manifest:"$repoRoot\src\app.manifest" `
       /optimize+ `
       /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Net.NetworkInformation.dll `
       "$repoRoot\src\HackerLockScreen.cs"

Write-Host "[3/3] Compilando TrayAutoHide.exe (Auto-Ocultar Iconos de Bandeja)..." -ForegroundColor Cyan
& $csc /target:winexe `
       /out:"$binDir\TrayAutoHide.exe" `
       /optimize+ `
       /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll `
       "$repoRoot\src\TrayAutoHide.cs"

Copy-Item "$repoRoot\Models" "$binDir\Models" -Recurse -Force

# 4. Firma Digital Authenticode Corporativa (CN=QBOSS Code Signing, O=Quimica Boss, OU=IT)
try {
    $qbossCert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Subject -match 'QBOSS Code Signing' -and $_.HasPrivateKey } |
        Select-Object -First 1
    if (-not $qbossCert) {
        $qbossCert = New-SelfSignedCertificate -Subject "CN=QBOSS Code Signing, O=Quimica Boss, OU=IT" `
            -Type CodeSigningCert -KeyUsage DigitalSignature -CertStoreLocation "Cert:\CurrentUser\My" `
            -NotAfter (Get-Date).AddYears(5) -ErrorAction Stop
    }
    if ($qbossCert) {
        foreach ($exeName in @("HackerWallpaper.exe", "HackerLockScreen.exe", "TrayAutoHide.exe")) {
            $exePath = Join-Path $binDir $exeName
            if (Test-Path $exePath) {
                $sig = Set-AuthenticodeSignature -FilePath $exePath -Certificate $qbossCert -HashAlgorithm SHA256
                Write-Host "[SIGN] $exeName firmado con $($qbossCert.Subject) [$($qbossCert.Thumbprint.Substring(0,10))]" -ForegroundColor Yellow
            }
        }
    }
} catch {
    Write-Warning "Aviso de firma Authenticode: $($_.Exception.Message)"
}

Write-Host "Compilacion y firma completadas con exito en: $binDir" -ForegroundColor Green
