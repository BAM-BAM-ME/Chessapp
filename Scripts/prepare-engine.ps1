Param(
    [string]$EngineZip = ""
)
$dest = Join-Path $PSScriptRoot "..\Engines"
New-Item -ItemType Directory -Force -Path $dest | Out-Null

if ($EngineZip -and (Test-Path $EngineZip)) {
    Expand-Archive -Path $EngineZip -DestinationPath $dest -Force
    Write-Host "Archive expanded to $dest"
} else {
    Write-Host "Copy the official Stockfish executable to Engines and name it: stockfish.exe"
}
