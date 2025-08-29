# Releasing

Steps to cut a pre-release build.

1. Choose the next semantic version and create a tag:
   ```bash
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```
2. GitHub Actions builds the project and publishes an unsigned MSIX artifact.
3. Download the MSIX from the workflow run.
4. On Windows, enable Developer Mode and install the package manually:
   ```powershell
   Add-AppxPackage .\ChessApp.msix
   ```
   The package is unsigned and intended only for testing.
