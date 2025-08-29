# SETTINGS

Documents the keys in `Data/appsettings.json`.

## Keys & defaults
- `EnginePath`: `string` — default `Engines/stockfish.exe`
- `Threads`: `int` — default `8`
- `Hash`: `int` — default `1024` (MB)
- `Ponder`: `bool` — default `true`
- `MultiPV`: `int` — default `1`
- `SyzygyPath`: `string` — default `""`
- `ProfilesPath`: `string` — default `Data/profiles.json`
- `LearnDb`: `string` — default `Data/learn.db`

> Alias for scripts: `engine.path` (equivalent to `EnginePath`). Keep them in sync.

## Extended example
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

## Validation & errors
- `EnginePath` must point to an existing executable; if missing, prompt the user or fall back to `Engines/stockfish.exe`.
- `Threads` ≥ 1; `Hash` in MB; `MultiPV` ≥ 1.
- If `SyzygyPath` is set but the engine lacks tablebase support, ignore gracefully.
