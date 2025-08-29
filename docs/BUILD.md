# BUILD

## Prerechizite
- Windows 10 sau mai nou
- .NET 8 SDK
- Executabil de motor UCI (ex.: `stockfish.exe`)

## Configurare
Editează `Data/appsettings.json` și setează `EnginePath` **sau** pune motorul la `Engines/stockfish.exe`.
- **GUI disponibil:** la prima lansare, aplicația afișează un dialog de alegere a motorului și persistă calea în setări.
- **Headless:** se folosește mereu calea din config.

### Exemplar minim `appsettings.json`
```json
{
  "EnginePath": "Engines/stockfish.exe",
  "Threads": 8,
  "Hash": 1024,
  "Ponder": true,
  "MultiPV": 1
}
```

## Build
- Deschide `ChessApp.sln` în Visual Studio 2022 sau folosește CLI:
  ```bash
  dotnet build -c Release
  ```

## Run & Quick Test
1. Pornește aplicația.
2. Verifică handshake-ul:
   - `uci` → `uciok`
   - `isready` → `readyok`
3. Test scurt:
   - `position startpos`
   - `go movetime 1000` (sau `go infinite` + `stop`)

## Note
- **Syzygy:** setează `SyzygyPath` către folderul cu `.rtbw/.rtbz`.
- **Profiluri:** asigură `Data/profiles.json`; vezi `PROFILES.md`.
- **Analiză:** pentru actualizări continue, folosește `go infinite`.
