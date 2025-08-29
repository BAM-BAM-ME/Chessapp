# Releasing Chessapp

This project uses tag-driven releases. Creating an annotated tag that matches `v*` triggers
an automated workflow on GitHub Actions which builds the application, packages the
Windows binaries, and publishes a GitHub Release with the zip attached.

## Cutting a release

1. Ensure the `main` branch is up to date and all changes are committed.
2. Choose the next semantic version. Versions follow the form `vMAJOR.MINOR.PATCH`.
3. Create an annotated tag and push it:
   ```bash
   git tag -a vX.Y.Z -m "Chessapp vX.Y.Z"
   git push origin vX.Y.Z
   ```
4. GitHub Actions builds the solution and uploads an artifact named
   `Chessapp-vX.Y.Z-win64.zip` to a new GitHub Release titled `Chessapp vX.Y.Z`.

No signing or MSIX packaging is performed. Releases currently target Windows x64 only.
