$ErrorActionPreference = 'Stop'

# Resolve repo root and engine path without using pipeline into Join-Path (PS on runners may not bind it)
$root   = Join-Path -Path $PSScriptRoot -ChildPath '..'
$engine = Join-Path -Path $root        -ChildPath 'Engines\stockfish.exe'

if (Test-Path -LiteralPath $engine) {
    Write-Host 'Engine found, performing handshake...'
    $p = Start-Process -FilePath $engine -NoNewWindow -RedirectStandardInput StandardInput -RedirectStandardOutput StandardOutput -PassThru
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
    $proj = Join-Path -Path $root -ChildPath 'tests\Interop.Tests\Interop.Tests.csproj'
    dotnet test $proj --nologo --filter "UciParser_Fixtures_Should_ParseKnownLines" | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Parser smoke failed' }
    Write-Host 'Smoke: Parser OK'
}
