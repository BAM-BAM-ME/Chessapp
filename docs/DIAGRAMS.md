# DIAGRAME

## Arhitectură (component overview)
```mermaid
flowchart TD
  GUI[WPF GUI] --> Core[Core]
  Core --> Interop[Interop]
  Interop <-->|stdin/stdout (UCI)| Engine[UCI Engine (Stockfish)]
  Core -->|read/write| Data[(Data/ appsettings.json, profiles.json)]
```

## Handshake UCI (sequence)
```mermaid
sequenceDiagram
  participant GUI
  participant Core
  participant Host as EngineHost
  participant Eng as Engine
  GUI->>Core: Start Analyze
  Core->>Host: StartAndHandshake()
  Host->>Eng: uci
  Eng-->>Host: id, option, ... uciok
  Host->>Eng: isready
  Eng-->>Host: readyok
  Host->>Eng: setoption (Threads/Hash/Ponder/MultiPV/Syzygy)
  Host-->>Core: EngineReady
  Core->>Host: position startpos [moves ...]
  Core->>Host: go infinite
  Eng-->>Host: info (depth, nps, score, multipv, pv...)
  Host-->>Core: InfoUpdate
  Core-->>GUI: actualizează listă/grafic
```

## Process Supervisor (state)
```mermaid
stateDiagram-v2
  [*] --> Stopped
  Stopped --> Starting: start process
  Starting --> Running: uciok && readyok
  Running --> Restarting: crash/exit
  Restarting --> Starting: retries < MAX
  Restarting --> Failed: retries == MAX
  Failed --> [*]
```

## Analiza — pipeline
```mermaid
flowchart TD
  Moves[Lista mutări] --> Position[UCI 'position' + FEN]
  Position --> Go[go infinite / go movetime]
  Go --> Info[flux info]
  Info --> Parse[UciParser → InfoUpdate]
  Parse --> UI[VM: listă MultiPV + grafic]
  Parse --> DB[(SQLite positions & evals)]
```
