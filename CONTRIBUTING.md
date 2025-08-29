# Contributing to ChessApp

Thanks for your interest in improving ChessApp! This guide explains how to work with the repository and submit changes.

## Tooling
- [ .NET 8 SDK](https://dotnet.microsoft.com/) (required)
- Windows 10/11 or recent Linux/macOS for development
- Optional: Visual Studio 2022, Rider, or VS Code

## Branch and PR model
1. Create a topic branch from `main`.
2. Keep commits focused; use [Conventional Commits](https://www.conventionalcommits.org/):
   - `feat:` new feature
   - `fix:` bug fix
   - `docs:` documentation only
   - `chore:` tooling or maintenance
3. Push the branch and open a Pull Request.
4. Run tests and a GUI smoke test before requesting review.

## Code style
- C# projects follow the default `.NET` conventions.
- Run `dotnet format` to apply consistent styling.

## Testing
Run the full test suite from the repo root:
```bash
dotnet test
```

## Smoke test
Verify that the GUI launches:
```bash
dotnet run --project Gui
```

## Need help?
Open an issue using the templates in `.github/ISSUE_TEMPLATE` or start a discussion in the PR.

