# ChessApp

ChessApp is a Windows (.NET 8 WPF) GUI that drives an external UCI engine such as Stockfish 17.1. It currently offers a 2D board, human vs. computer play, basic analysis, JSON configuration and profiles, and is structured for future insights and learning features.

## Quickstart

### Requirements

* Windows 10/11 x64
* [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/)
* [Visual Studio 2022](https://visualstudio.microsoft.com/)
* Optional UCI engine such as [`stockfish.exe`](https://stockfishchess.org/)

### Run

1. Clone the repo:

   ```powershell
   git clone <repo-url>
   cd ChessApp
   ```

   Open `ChessApp.sln` in Visual Studio 2022.

2. Configure the engine path (choose one):

   * **Place the engine** at `Engines/stockfish.exe`, or use the helper script:

     ```powershell
     pwsh Scripts/prepare-engine.ps1 -Path C:\path\to\stockfish.exe
     ```
   * **Or edit** `Data/appsettings.json` and set `AppSettings.EnginePath`.

3. Build & run:

codex/align-namespaces-in-gui-files
   * **Visual Studio:** set **Gui** as the startup project and run (F5).
   * **CLI (optional):**

```powershell
Scripts/build-msix.ps1
```
main

     ```powershell
     dotnet build
     dotnet run --project Gui
     ```

See [BUILD.md](docs/BUILD.md) for detailed instructions and [PROFILES.md](docs/PROFILES.md) for profile definitions.

## Packaging

For distribution guidance, see [RELEASING.md](RELEASING.md). If you plan to ship an engine alongside the GUI, run `Scripts/prepare-engine.ps1` first and ensure the license files are included (see below).

## Troubleshooting

* Install the .NET 8 Desktop Runtime if the app fails to launch.
* Ensure `AppSettings.EnginePath` points to a valid engine.
* Visual Studio must include the ".NET desktop development" workload.
* If PowerShell blocks scripts, run `Set-ExecutionPolicy -Scope Process RemoteSigned`.

## Key Files

* `Interop/EngineHost.cs` – starts and communicates with the engine via UCI.
* `Core/GameController.cs` – applies UCI moves and maintains FEN.
* `Data/appsettings.json` – base settings. `Data/profiles.json` – sample profiles.

## Insights

ChessApp stores recent engine evaluations in a local SQLite database at `Data/insights.db`. The latest 50 entries (timestamp, depth, score, NPS and PV) are visible on the **Insights** tab. This feature is enabled by default; set `"Insights": false` in `Data/appsettings.json` to opt out.

## Blueprint

The roadmap and architecture plan live in [BLUEPRINT.md](docs/BLUEPRINT.md).

## License and Stockfish

If you ship the engine with the app, include the Stockfish `LICENSE` and `AUTHORS`. A simple approach is to distribute only the GUI and ask the user for the engine path on first launch.
