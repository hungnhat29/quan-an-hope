# F-060 (sprint 7): chay bot C# cho nhieu bo so trong mot lan mo Unity -> balance/bot_sweep.md
# Dong Unity Editor truoc khi chay (L-004).
# Chay:  powershell -NoProfile -ExecutionPolicy Bypass -File balance/run_sweep.ps1 -Sweep "hien tai;no22 36k|d22=36000" [-Games 1000]
# Khoa: d4 d9 d15 d22 (no), price (nhan gia goc), tickets (ve / ngay), first (ve ngay 1), boss (thuong chu no).
param(
    [string]$Sweep = "hien tai",
    [int]$Games = 1000,
    [string]$Unity = "D:\Unity\6000.6.9f1\Editor\Unity.exe"
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$tmp = Join-Path $env:TEMP "quananhope-bots"
New-Item -ItemType Directory -Force $tmp | Out-Null
$env:BOT_GAMES = "$Games"
$env:BOT_SWEEP = $Sweep
$results = Join-Path $tmp "sweep_results.xml"
$log = Join-Path $tmp "sweep.log"
if (Test-Path $results) { Remove-Item $results }
& $Unity -batchmode -projectPath "$root\My project" -runTests -testPlatform EditMode `
    -testFilter "QuanAnHope.Tests.Sprint7BotTests.F060_QuetPhuongAn" -testResults $results -logFile $log | Out-Null
if (-not (Test-Path $results)) { Write-Host "Khong co ket qua test, xem $log"; exit 1 }
Get-Content -Encoding utf8 (Join-Path $PSScriptRoot "bot_sweep.md") | ForEach-Object { Write-Host $_ }
