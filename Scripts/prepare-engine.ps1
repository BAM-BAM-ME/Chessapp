[CmdletBinding(SupportsShouldProcess = $true)]
Param(
    [Parameter(Mandatory = $true)]
    [string]$Path
)

$destDir = Join-Path $PSScriptRoot "..\Engines"
$destFile = Join-Path $destDir "stockfish.exe"

if (-not (Test-Path $Path)) {
    throw "Stockfish executable not found at: $Path"
}

if ($PSCmdlet.ShouldProcess($destDir, "Create engine directory")) {
    New-Item -ItemType Directory -Force -Path $destDir | Out-Null
}

$sourceHash = Get-FileHash -Path $Path -Algorithm SHA256
$copyNeeded = $true

if (Test-Path $destFile) {
    $destHash = Get-FileHash -Path $destFile -Algorithm SHA256
    if ($sourceHash.Hash -eq $destHash.Hash) {
        $copyNeeded = $false
        Write-Host "Stockfish is already installed at $destFile (SHA256: $destHash.Hash)."
    }
}

if ($copyNeeded -and $PSCmdlet.ShouldProcess($destFile, "Copy stockfish.exe")) {
    Copy-Item -LiteralPath $Path -Destination $destFile -Force
    $destHash = Get-FileHash -Path $destFile -Algorithm SHA256
    Write-Host "Engine copied to $destFile (SHA256: $destHash.Hash)."
}

Write-Host "Next: run 'dotnet run --project Gui' or update Data/appsettings.json with the engine path."

