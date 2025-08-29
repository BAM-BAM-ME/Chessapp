Param(
    [string]$EngineZip = ""
)
$dest = Join-Path $PSScriptRoot "..\Engines"
New-Item -ItemType Directory -Force -Path $dest | Out-Null

if ($EngineZip -and (Test-Path $EngineZip)) {
    Expand-Archive -Path $EngineZip -DestinationPath $dest -Force
    Write-Host "Arhiva expandata in $dest"
} else {
    Write-Host "Copiaza executabilul oficial Stockfish in folderul Engines si redenumeste-l exact: stockfish.exe"
}
