# ChessApp

ChessApp este un GUI de șah pentru Windows (WPF, .NET 8) care rulează un motor UCI extern (ex.: Stockfish 17.1) printr-un strat interop propriu. Ținta este stabilitatea, arhitectură curată și un flux de analiză util (MultiPV + grafic al evaluării), cu opțiune de stocare pentru „User Learning”.

## Funcționalități (curente / planificate)
- Găzduire motor UCI cu handshake determinist (`uci` → `uciok`, `isready` → `readyok`), aplicarea opțiunilor (Threads, Hash, Ponder, Syzygy, MultiPV).
- Joc om vs. motor și analiză continuă (`go infinite`) cu flux `info`/`bestmove` în timp real.
- Profiluri în JSON pentru comutare rapidă a opțiunilor și a politicii de mutare.
- Export PGN (schelet existent).
- (Plan) Insights pe SQLite și dashboard de analiză.
- (Plan) Pachetare MSIX și semnare cod.

## Cerințe
- Windows 10+
- .NET 8 SDK
- Un motor UCI (ex.: `stockfish.exe`) furnizat extern sau inclus în distribuție cu respectarea licențelor.

## Structura soluției
- **Gui** — interfață WPF (tablă, panou analiză, selector profil).
- **Core** — logică: `GameController`, `ConfigService`/`AppSettings`, `ProfileService`, `PgnService`.
- **Interop** — procese & protocol: `EngineHost`, `UciParser`, `ProcessSupervisor`.
- **Data/** — configurări (`appsettings.json`), profiluri (`profiles.json`).

## Quick Start
1. Instalează .NET 8 SDK.
2. Pune motorul la `Engines/stockfish.exe` **sau** setează calea în `Data/appsettings.json` (`EnginePath`).  
   **Dacă GUI este disponibil:** la prima lansare apare un dialog de alegere a motorului (sau se folosește `EnginePath` din config). Poți schimba ulterior în **Setări → Engine**.  
   **Rulările headless** folosesc mereu calea din fișierul de configurare.
3. Compilează soluția (Debug/Release).
4. Pornește aplicația și inițiază analiza: aplicația va trimite `go infinite` și va actualiza panoul de analiză în timp real.

### Setări (`Data/appsettings.json`)
```json
{
  "EnginePath": "Engines/stockfish.exe",
  "Threads": 8,
  "Hash": 1024,
  "Ponder": true,
  "MultiPV": 1,
  "SyzygyPath": "",
  "ProfilesPath": "Data/profiles.json",
  "LearnDb": "Data/learn.db",
  "engine": { "path": "Engines/stockfish.exe" }
}
```
> Notă: Scripturile PowerShell din `scripts/` folosesc aliasul `engine.path`. Păstrează-l sincron cu `EnginePath` sau păstrează ambele chei cu aceeași valoare.

### Profiluri (`Data/profiles.json`)
Câmpuri uzuale: `name`, `uci{...}`, `move_policy{deterministic, top_k}`, `book{enabled, allow[]}`. Vezi `PROFILES.md` și `EXAMPLES.md`.

## Blueprint & Roadmap
**F1 – EngineHost & UCI handshake:** lifecycle robust, timeouts deterministe, `setoption` corecte.  
**F2 – Panou analiză (MultiPV + chart):** listă candidați + grafic evaluare.  
**F3 – Selector profiluri:** încărcare/aplicare din JSON, reconfigurare live (MultiPV).  
**F4 – SQLite „User Learning”:** persistarea evaluărilor/pozițiilor, dashboard.  
**F5 – Packaging (MSIX) & code signing:** installer distribuibil, notificări terți.

### Verificări de acceptanță
- **F1:** `Analyze` pe `startpos`; respectarea `MultiPV`; export PGN compilează.
- **F2:** lista MultiPV și graficul se actualizează fluent în `go infinite`.
- **F3:** comutarea profilului reconfigurează motorul fără restart; `top_k` influențează alegerea mutării.
- **F4:** DB conține date de sesiune; dashboard cu „Worst 5” și „Depth coverage”.
- **F5:** MSIX se construiește; licențele terțe (și, dacă e cazul, Stockfish) sunt incluse.

## Depanare (scurt)
- **EnginePath invalid:** verifică executabilul sau pune `stockfish.exe` în `Engines/`.
- **Timeout la `uciok/readyok`:** verifică AV/sandbox; ajustează timeouts doar dacă e necesar.
- **Syzygy nu se încarcă:** setează `SyzygyPath` către `.rtbw/.rtbz` și verifică suportul motorului.

## Licențiere când pachetezi motoare
Codul proiectului poate fi sub o licență permisivă (ex.: MIT). Dacă **pachetezi** Stockfish (GPLv3) sau alt motor GPL în artefactul distribuit (ex.: MSIX/ZIP), **distribuibilul combinat** intră sub termenii GPLv3. Poți păstra sursa sub licență permisivă, dar artefactul care **conține** un motor GPL trebuie să respecte GPLv3 (ex.: sursă completă corespunzătoare și notificări). Ia în calcul o variantă „fără motor” pentru distribuții non-GPL.

— 2025-08-29
