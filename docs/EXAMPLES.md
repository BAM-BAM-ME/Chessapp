# EXEMPLE

## 1) `Data/appsettings.json` (extins)
```json
{
  "EnginePath": "Engines/stockfish.exe",
  "Threads": 12,
  "Hash": 2048,
  "Ponder": true,
  "MultiPV": 3,
  "SyzygyPath": "D:/tb/syzygy",
  "ProfilesPath": "Data/profiles.json",
  "LearnDb": "Data/learn.db",
  "engine": { "path": "Engines/stockfish.exe" }
}
```

## 2) `Data/profiles.json` (mai multe profiluri)
```json
[
  {
    "name": "Titan Max",
    "uci": { "Threads": 12, "Hash": 4096, "Ponder": true, "MultiPV": 1 },
    "book": { "enabled": true, "allow": ["C60..C99", "D30..D69"] },
    "move_policy": { "deterministic": true, "top_k": 1 }
  },
  {
    "name": "Aggressive A",
    "uci": { "Threads": 8, "Hash": 2048, "Ponder": false, "MultiPV": 4 },
    "book": { "enabled": true, "allow": ["E20..E69"] },
    "move_policy": { "deterministic": false, "top_k": 3 }
  },
  {
    "name": "Human-Elo 2000",
    "uci": { "Threads": 6, "Hash": 1024, "Ponder": false, "MultiPV": 2 },
    "book": { "enabled": false, "allow": [] },
    "move_policy": { "deterministic": false, "top_k": 2 }
  }
]
```

## 3) Transcript UCI (handshake + căutare scurtă)
```
uci
id name Stockfish 17.1
id author the Stockfish developers
option name Hash type spin default 16 min 1 max 131072
option name Threads type spin default 1 min 1 max 1024
...
uciok
isready
readyok
setoption name Threads value 8
setoption name Hash value 1024
setoption name Ponder value true
setoption name MultiPV value 3
position startpos
go movetime 1000
info depth 18 seldepth 28 multipv 1 score cp 36 nodes 123456 nps 123000 tbhits 0 time 998 pv e2e4 e7e5 g1f3 ...
bestmove e2e4
```

## 4) Schemă SQLite (sugerată)
```sql
CREATE TABLE IF NOT EXISTS games (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  started_at TEXT NOT NULL,
  result TEXT,
  moves TEXT
);

CREATE TABLE IF NOT EXISTS positions (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  game_id INTEGER NOT NULL,
  ply INTEGER NOT NULL,
  fen TEXT NOT NULL,
  bestmove TEXT,
  score_cp INTEGER,
  score_mate INTEGER,
  depth INTEGER,
  nps INTEGER,
  tbhits INTEGER,
  time_ms INTEGER,
  pv TEXT,
  FOREIGN KEY(game_id) REFERENCES games(id)
);
```

### Interogări utile
```sql
-- Cele mai mari oscilații în ultimul joc
SELECT ply, fen, score_cp FROM positions
WHERE game_id = ?
ORDER BY ABS(score_cp) DESC
LIMIT 5;

-- Histograma adâncimii
SELECT depth, COUNT(*) AS cnt FROM positions GROUP BY depth ORDER BY depth;
```

## 5) Exemplu PGN
```
[Event "ChessApp Example"]
[Site "Local"]
[Date "2025.08.29"]
[Round "-"]
[White "You"]
[Black "Engine"]
[Result "*"]

1. e4 e5 2. Nf3 Nc6 3. Bb5 *
```
