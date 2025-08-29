# PACKAGING (MSIX)

## MSIX pentru WPF
- Definește identitatea pachetului (Name, Publisher, Version).
- Include `Data/` (setări și profiluri).
- La prima pornire, dacă `EnginePath` nu este valid și `Engines/stockfish.exe` lipsește, cere utilizatorului calea către motor.

## Notificări & licențe terțe
- Include `ThirdPartyNotices.txt` în installer.
- **Dacă distribui un motor** (ex.: Stockfish), **trebuie** să incluzi fișierele oficiale `LICENSE` și `AUTHORS` din distribuția motorului, alături de notele tale.

## Semnare cod
- Certificat & thumbprint: `<to be supplied externally>`
- Semnează în CI sau înainte de publicarea artefactelor.

### Semnare în CI (GitHub Actions)
Furnizează certificatul PFX ca secret de repo (`SIGNING_CERT` în base64) și parola (`SIGNING_PASS`). Decodează în CI și rulează `signtool`:

```powershell
# Scripts/sign-msix.ps1
$certPath = "$env:RUNNER_TEMP\cert.pfx"
[IO.File]::WriteAllBytes($certPath, [Convert]::FromBase64String($env:SIGNING_CERT))
signtool sign /fd SHA256 /a /f $certPath /p $env:SIGNING_PASS out/installer/Chessapp.msix
Remove-Item $certPath -Force
```

## Artefacte & testare
- Produ un `.msix` și validează instalarea/dezinstalarea.
- Smoke test: descoperire motor + căutare 1s pe `startpos`.
