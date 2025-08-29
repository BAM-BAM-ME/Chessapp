# SETTINGS

Documentează cheile din `Data/appsettings.json`.

## Chei & valori implicite
- `EnginePath`: `string` — implicit: `Engines/stockfish.exe`
- `Threads`: `int` — implicit: `8`
- `Hash`: `int` — implicit: `1024` (MB)
- `Ponder`: `bool` — implicit: `true`
- `MultiPV`: `int` — implicit: `1`
- `SyzygyPath`: `string` — implicit: `""`
- `ProfilesPath`: `string` — implicit: `Data/profiles.json`
- `LearnDb`: `string` — implicit: `Data/learn.db`

> Alias pentru scripturi: `engine.path` (echivalent cu `EnginePath`). Păstrează-le sincron.

## Exemplu extins
```json
{
  "EnginePath": "Engines/stockfish.exe",
  "Threads": 8,
  "Hash": 1024,
  "Ponder": true,
  "MultiPV": 1,
  "SyzygyPath": "",
  "ProfilesPath": "Data/profiles.json",
  "LearnDb": "Data/learn.db"
}
```

## Validare & erori
- `EnginePath` trebuie să indice un executabil existent; dacă lipsește, cere utilizatorului calea sau folosește fallback `Engines/stockfish.exe`.
- `Threads` ≥ 1; `Hash` în MB; `MultiPV` ≥ 1.
- Dacă `SyzygyPath` este setat dar motorul nu are suport TB, ignoră grațios.
