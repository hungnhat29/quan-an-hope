# F-045 (docs/sprint-5/impl-plan.md): mot lenh chay mo phong Python + bot C# tren luat that, 4.000 van moi kieu.
# Dong Unity Editor truoc khi chay (L-004).
# Chay:  powershell -NoProfile -ExecutionPolicy Bypass -File balance/run_bots.ps1
# Tuy chon: -Games 1000 (it van de thu nhanh), -Unity "D:\Unity\6000.6.9f1\Editor\Unity.exe"
param(
    [int]$Games = 4000,
    [string]$Unity = "D:\Unity\6000.6.9f1\Editor\Unity.exe"
)
$ErrorActionPreference = "Stop"
$here = $PSScriptRoot
$root = Split-Path -Parent $here
$tmp = Join-Path $env:TEMP "quananhope-bots"
New-Item -ItemType Directory -Force $tmp | Out-Null

Write-Host "1/2 sim_balance.py ($Games van moi kieu)..."
$env:PYTHONUTF8 = "1"
$env:N = "$Games"
$sim = & python (Join-Path $here "sim_balance.py")
$sim | Set-Content -Encoding utf8 (Join-Path $here "sim_report.txt")
$sim | ForEach-Object { Write-Host $_ }

Write-Host "2/2 bot C# tren luat that ($Games van moi kieu)..."
$env:BOT_GAMES = "$Games"
$results = Join-Path $tmp "bot_results.xml"
$log = Join-Path $tmp "bot.log"
if (Test-Path $results) { Remove-Item $results }
$args = @("-batchmode", "-projectPath", "`"$root\My project`"", "-runTests", "-testPlatform", "EditMode",
    "-testFilter", "QuanAnHope.Tests.Sprint5BotTests.F045_BaoCao_4000VanMoiKieu",
    "-testResults", "`"$results`"", "-logFile", "`"$log`"")
$p = Start-Process -FilePath $Unity -ArgumentList $args -Wait -PassThru -NoNewWindow
if (-not (Test-Path $results)) { Write-Host "Khong co ket qua test, xem $log"; exit 1 }
$xml = Get-Content $results -Raw
if ($xml -match '<test-run [^>]*result="([A-Za-z()]*)"') { Write-Host "Ket qua test: $($Matches[1])" }
Get-Content -Encoding utf8 (Join-Path $here "bot_report.md") | ForEach-Object { Write-Host $_ }
exit $p.ExitCode
