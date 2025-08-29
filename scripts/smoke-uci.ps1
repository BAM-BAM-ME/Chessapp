$ErrorActionPreference = 'Stop'
$engine = Join-Path $PSScriptRoot '..' | Join-Path 'Engines/stockfish.exe'
if (Test-Path $engine) {
    Write-Host 'Engine found, performing handshake...'
    $p = Start-Process -FilePath $engine -RedirectStandardInput StandardInput -RedirectStandardOutput StandardOutput -PassThru
    $p.StandardInput.WriteLine('uci')
    Start-Sleep -Milliseconds 200
    $out = ""
    while (-not $p.StandardOutput.EndOfStream) {
        $line = $p.StandardOutput.ReadLine()
        $out += $line + [Environment]::NewLine
        if ($line -eq 'uciok') { break }
    }
    $p.StandardInput.WriteLine('quit')
    $p.WaitForExit()
    if ($out -match 'uciok') {
        Write-Host 'Smoke: Engine handshake OK'
    } else {
        throw 'uciok not received'
    }
} else {
    Write-Host 'stockfish.exe not found, running parser fixtures'
    $proj = Join-Path $PSScriptRoot '..\tests\Interop.Tests\Interop.Tests.csproj'
    dotnet test $proj --nologo --filter "UciParser_Fixtures_Should_ParseKnownLines" | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Parser smoke failed' }
    Write-Host 'Smoke: Parser OK'
}
