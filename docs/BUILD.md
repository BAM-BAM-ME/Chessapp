# BUILD

## Prerequisites
- Windows 10 or later
- .NET 8 SDK
- Optional UCI engine (`stockfish.exe`)

## Setup
Copy `stockfish.exe` to `Engines/` **or** set `AppSettings.EnginePath` in `Data/appsettings.json`.

## Visual Studio
1. Open `ChessApp.sln` in Visual Studio 2022.
2. Build and run the `Gui` project.

## CLI
```bash
dotnet build -c Release
dotnet run --project Gui
```

### Headless / smoke run
The GUI is required for normal use, but CI performs a small headless smoke check using `Scripts/prepare-engine.ps1`. The script prepares the engine folder and can validate `AppSettings.EnginePath`.

## Notes
- `SyzygyPath` should point to `.rtbw/.rtbz` files.
- Profiles live in `Data/profiles.json`; see [PROFILES.md](PROFILES.md).

