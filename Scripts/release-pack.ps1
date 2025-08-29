Param()

$root = Join-Path $PSScriptRoot ".."
$buildDir = Join-Path $root "Gui\bin\Release\net8.0-windows"
$dest = Join-Path $root "Artifacts\release"

if (Test-Path $dest) {
    Remove-Item $dest -Recurse -Force
}
New-Item -ItemType Directory -Path $dest | Out-Null

Copy-Item -Path (Join-Path $buildDir "*") -Destination $dest -Recurse

Write-Host "Release files staged at $dest"
