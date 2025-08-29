# Architecture

```
GUI (WPF)
   │ moves
   ▼
GameController (Core)
   │ FEN & move list
   ▼
EngineHost (Interop) ⇄ UciParser
   │ UCI lines
   ▼
Stockfish (UCI engine)
```

## Layers
- **GUI (WPF)** – board, analysis panel and profile selector.
- **Core** – maintains game state through `GameController` and issues engine commands.
- **Interop** – `EngineHost` spawns the engine process while `UciParser` translates plain-text protocol lines.

## Runtime sequence
1. GUI reports a user move to `GameController`.
2. `GameController` updates the move list and FEN, then calls `EngineHost` with `position` and `go`.
3. `EngineHost` writes UCI commands and reads responses from the engine.
4. Stockfish returns `info` and `bestmove` lines that `UciParser` parses and propagates back to `GameController` and the GUI.

## Data
- **UCI lines** – commands and responses over stdin/stdout.
- **FEN** – board snapshot built after each move.
- **Move list** – sequential moves that reproduce the position.

## Extensibility
- Swap any UCI engine by configuring its path.
- Extend `GameController` for custom move policies or state persistence.
- Add new GUI panels that subscribe to `GameController` events.
- Expand `UciParser` to support additional protocol options.
