# ChessApp

ChessApp is a Windows (.NET 8 WPF) GUI that drives an external UCI engine such as Stockfish 17.1. It currently offers a 2D board, human vs. computer play, basic analysis, JSON configuration and profiles, and is structured for future insights and learning features.

## Quick Start

**Prerequisites:** Windows 10/11 x64, [.NET 8 SDK](https://dotnet.microsoft.com/), optional [`stockfish.exe`](https://stockfishchess.org/).

1. Optionally place the engine at `Engines/stockfish.exe` **or** edit `Data/appsettings.json` to set `AppSettings.EnginePath`.
2. Build the GUI:

   ```bash
   dotnet build
   # or open ChessApp.sln in Visual Studio 2022 and run the Gui project
   ```
3. Run:

   ```bash
   dotnet run --project Gui
   ```

The engine path is configured via `AppSettings.EnginePath`.

See [BUILD.md](docs/BUILD.md) for detailed instructions and [PROFILES.md](docs/PROFILES.md) for profile definitions.

## Key Files

- `Interop/EngineHost.cs` – starts and communicates with the engine via UCI.
- `Core/GameController.cs` – applies UCI moves and maintains FEN.
- `Data/appsettings.json` – base settings. `Data/profiles.json` – sample profiles.

## Blueprint

The roadmap and architecture plan live in [BLUEPRINT.md](docs/BLUEPRINT.md).

## License and Stockfish

If you ship the engine with the app, include the Stockfish `LICENSE` and `AUTHORS`. A simple approach is to distribute only the GUI and ask the user for the engine path on first launch.

