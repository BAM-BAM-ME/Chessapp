# ChessApp for Windows with Stockfish 17.1

Windows app (.NET 8 WPF) that runs Stockfish 17.1 as an external UCI engine. It includes a 2D board, human vs. computer play, basic analysis, profiles, and JSON-based configuration. The project is prepared for future extensions (Insights, local learning).

## Requirements

* Windows 10 or 11 x64
* .NET 8 Desktop Runtime
* Engine executable at `Engines/stockfish.exe`

## Quick Start

1. Copy `stockfish.exe` into the `Engines` folder.
2. Open `ChessApp.sln` in Visual Studio 2022 and run the `Gui` project.
3. To change the engine path, edit `Data/appsettings.json` or use the Engine button in the UI.

## Gameplay

* **Start game** sends `position startpos` then `go movetime 1000`.
* **Analyze** starts `go infinite`; **Stop** sends `stop`.
* The board updates after each move.

## Key files

* `Interop/EngineHost.cs` — spawns and talks to `stockfish.exe` via UCI.
* `Interop/UciParser.cs` — parses `info` lines used by the analysis panel.
* `Core/GameController.cs` — applies UCI moves and maintains FEN for rendering.
* `Gui/Controls/BoardControl.cs` — 2D rendering with Unicode pieces.
* `Data/appsettings.json` — base settings. `Data/profiles.json` contains profile presets.
* `Engines/README.txt` — engine notes.

## Insights
ChessApp stores recent engine evaluations in a local SQLite database at `Data/insights.db`. The latest 50 entries (timestamp, depth, score, NPS and PV) are visible on the **Insights** tab. This feature is enabled by default; set `"Insights": false` in `Data/appsettings.json` to opt out.

## Blueprint

The detailed plan for the app evolution (engine, NNUE, Windows optimizations, advanced GUI) is described in `docs/BLUEPRINT.md`.

## License and Stockfish

If you distribute the engine together with your package, include Stockfish license & authors. The simplest option is to ship only the GUI and request the engine path on first launch.

## Packaging

The repository includes an experimental Windows Application Packaging Project. It is **not signed** and is intended for local testing only.

### Local build

1. Enable **Developer Mode** or allow sideloading on your Windows machine.
2. Open a **Developer PowerShell** and run `Scripts\build-msix.ps1`.
   The script restores the solution, builds the WPF project, and places an unsigned `.msix` in `packaging\Artifacts`.
   Placeholder logos are generated automatically during this step.
3. Install the package by launching the generated `.msix`. Windows will warn about the unsigned package.

### Troubleshooting

* The script expects **`msbuild`** from Visual Studio to be available in `PATH`.
* Make sure Developer Mode is enabled; otherwise Windows will refuse to install the package.
* Code signing and Store submission will be added in later tasks.
