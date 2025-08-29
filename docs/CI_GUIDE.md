# GHID CI

Workflows-urile canonice trăiesc în `.github/workflows/`:
- `ci.yml` — build, teste, validare scheme și smoke run
- `release.yml` — release pe tag-uri `v*` cu publicare artefacte

Copiază aceste fișiere când bootstrap-ezi un repo nou.

## Secrete (dacă faci packaging/semnare)
- `SIGNING_CERT` — PFX în base64
- `SIGNING_PASS` — parola PFX

## Cache
Se cache-ază pachetele NuGet pentru build mai rapid.

## Mediu
Rulează pe `windows-latest` cu .NET 8.
