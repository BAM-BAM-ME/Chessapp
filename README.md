# ChessApp for Windows with Stockfish 17.1

Aplicatie Windows (.NET 8 WPF) care ruleaza Stockfish 17.1 ca engine extern prin UCI. Include tabla 2D, joc om vs computer, analiza de baza, profiluri si configurare in JSON. Proiectul este pregatit pentru extindere cu Insights si invatare locala.

## Cerinte
- Windows 10 sau 11 x64
- .NET 8 Desktop Runtime
- Executabilul motorului in `Engines/stockfish.exe`

## Instalare rapida
1. Copiaza `stockfish.exe` in folderul `Engines`.
2. Deschide `ChessApp.sln` in Visual Studio 2022 si ruleaza proiectul `Gui`.
3. Daca vrei sa schimbi calea engine-ului, editeaza `Data/appsettings.json` sau foloseste butonul Engine din UI.

## Control joc
- Start game trimite `position startpos` si `go movetime 1000`.
- Analyze porneste `go infinite`, Stop trimite `stop`.
- Tabla se actualizeaza la fiecare mutare.

## Fisiere cheie
- `Interop/EngineHost.cs` porneste si comunica cu `stockfish.exe` prin UCI.
- `Interop/UciParser.cs` parseaza liniile `info` pentru viitorul panou de analiza.
- `Core/GameController.cs` aplica mutarile UCI si mentine FEN pentru randare.
- `Gui/Controls/BoardControl.cs` randare 2D cu piese Unicode.
- `Data/appsettings.json` setari de baza. `Data/profiles.json` contine 20 de profiluri.
- `Engines/README.txt` explicatii pentru motor.

## Insights
ChessApp stores recent engine evaluations in a local SQLite database at `Data/insights.db`. The latest 50 entries (timestamp, depth, score, NPS and PV) are visible on the **Insights** tab. This feature is enabled by default; set `"Insights": false` in `Data/appsettings.json` to opt out.

## Blueprint
Planul detaliat pentru evoluția aplicației, incluzând engine propriu, NNUE, optimizări Windows și GUI avansat, este descris în `docs/BLUEPRINT.md`.

## Licenta si Stockfish
Daca distribui motorul impreuna cu pachetul tau, include licenta si autorii Stockfish. Cea mai simpla varianta este sa livrezi doar GUI-ul si sa soliciti calea catre executabil la primul start.
