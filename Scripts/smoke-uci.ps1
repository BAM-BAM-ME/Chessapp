$ErrorActionPreference = 'Stop'

# Look for a local engine for a quick handshake; otherwise run parser smoke via tests
$engine = Join-Path $PSScriptRoot '..' | Join-Path 'Engines/stockfish.exe'
if (Test-Path $engine) {
    Write-Host 'Engine found, performing UCI handshake...'
    $p = Start-Process -FilePath $engine -RedirectStandardInput StandardInput -RedirectStandardOutput StandardOutput -PassThru
    try {
        $p.StandardInput.WriteLine('uci')
        Start-Sleep -Milliseconds 200
        $ok = $false
        while (-not $p.StandardOutput.EndOfStream) {
            $line = $p.StandardOutput.ReadLine()
            if ($line -eq 'uciok') { $ok = $true; break }
        }
        $p.StandardInput.WriteLine('quit')
        $p.WaitForExit()
        if (-not $ok) { throw 'uciok not received' }
        Write-Host 'Smoke: Engine handshake OK'
    }
    finally {
        if (-not $p.HasExited) { $p.Kill() }
    }
}
else {
    Write-Host 'stockfish.exe not found, running parser fixtures instead'
    $proj = Join-Path $PSScriptRoot '..\tests\Interop.Tests\Interop.Tests.csproj'
    dotnet test $proj --nologo --filter "UciParser_Fixtures_Should_ParseKnownLines" | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Parser smoke failed' }
    Write-Host 'Smoke: Parser OK'
}
