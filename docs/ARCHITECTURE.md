# ARHITECTURĂ

```
GUI (WPF)
   │
   ▼
Core (GameController, ConfigService, ProfileService, PgnService)
   │        ▲
   │        │ Setări (appsettings.json), Profiluri (profiles.json)
   ▼
Interop (EngineHost ⇄ UciParser ⇄ ProcessSupervisor)
   │
   ▼  (stdin/stdout, protocol UCI)
Motor UCI (ex.: Stockfish)
```

## Proiecte din soluție
- **Gui** — prezentare WPF (tablă, panou analiză, selector profil).
- **Core** — logică domeniu:
  - `GameController`: menține lista de mutări, construiește `position startpos [moves ...]`, actualizează FEN; rocade, promoții, euristică en passant.
  - `ConfigService` / `AppSettings`: încărcare `Data/appsettings.json`.
  - `ProfileService`: încărcare `Data/profiles.json`.
  - `PgnService`: utilitare de scriere PGN (schelet).
- **Interop** — proces & protocol:
  - `EngineHost`: start/stop motor, comenzi (`uci`, `isready`, `setoption`, `position`, `go`), citire linii, evenimente.
  - `UciParser`: parsează `info` (cp/mate, depth/seldepth, multipv, nps, tbhits, time, pv).
  - `ProcessSupervisor`: repornește motorul la crash cu retry limitat.

## Fire & evenimente
- I/O de motor pe task-uri de fundal; UI primește evenimente marshal-uite pe Dispatcher (WPF).
- Evenimente tip: `EngineReady`, `InfoReceived(InfoUpdate)`, `BestMoveReceived`, `EngineCrashed`.

## Logging
- Logging minim structurat la granița interop: start/exit proces, pașii handshake, erori de parse, setoption, `bestmove`.

## Presupuneri & limite
- Motorul respectă UCI și este accesibil prin `EnginePath`.
- `SyzygyPath` este opțional; când este setat, motorul suportă TB.
- Profilurile pot include `deterministic/top_k`; selecția stocastică se face în Core.
