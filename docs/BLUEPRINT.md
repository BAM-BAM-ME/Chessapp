# Blueprint consolidat

## Obiectiv
Construirea unei aplicații de șah pentru Windows cu un engine de clasă mondială, foarte greu de învins de către un om, cu timp de răspuns minim și UX impecabil.

## KPI
- Forță >3600 Elo în testele interne.
- Viteză: 8–12M NPS pe CPU AVX2, scalare până la 32 threaduri.
- Stabilitate: zero crash-uri în 72h, determinism la seed fix.
- UX: inițializare <500 ms, mutări afișate sub 16 ms, animare 60 FPS.
- Pachetare: instalator MSIX semnat, actualizări diferențiale.

## Stack tehnologic
- **Engine**: C++20/23, MSVC, CMake, AVX2/AVX‑512, LTO+PGO, alocatori tcmalloc/mimalloc.
- **Evaluare**: NNUE int8 cu SIMD.
- **Protocol**: UCI pentru separarea GUI–engine.
- **GUI**: .NET 8 C# WPF/WinUI, DirectX 11, MVVM.
- **Persistență**: fișiere JSON.
- **Pachetare**: MSIX cu semnare și telemetrie locală opțională.

## Arhitectură
```
[WPF GUI] ←UCI→ [Engine C++]
    |                |
 [Settings]       [Search]
 [Skins]          [NNUE]
 [Game]           [MoveGen]
 [Book]           [TT]
 [PGN]            [TB]
```
- Engine în proces separat; comunicare UCI prin pipe.
- Resurse (TB, cărți) gestionate central.
- Logare pe componente.

## Engine – mutări și căutare
- Bitboarduri 64‑biți, magic bitboards, hashing Zobrist.
- Generare mutări legale cu filtrare de șah, SEE pentru capturi.
- Căutare: Iterative Deepening, PVS cu Aspiration windows, Quiescence, Null move, LMR, killer & history heuristics.
- Transposition Table bucketed (implicit 1 GiB), SMP Lazy cu split points, prefetch TT.

## Evaluare NNUE
- Actualizare incrementală, feature-uri piece‑square.
- Inferență int8 AVX2, fallback SSE4.2, reload dinamic al rețelei.

## Deschideri și finaluri
- Book Poliglot + auto‑joc filtrat, ponderi după performanță.
- Tablebase Syzygy WDL/DTZ până la 6 piese (opțional 7).

## Managementul timpului
Alocare dinamică în funcție de faza jocului, detectare crize și tăieri agresive.

## Stabilitate și securitate
Validare strictă FEN/UCI/PGN, limitare memorie, thread‑pool cu oprire ordonată, watchdog pentru blocaje.

## Optimizări Windows
ETW & WPA, PGO, detectare capabilități CPU, alocatori rapizi, pagini mari pentru TT.

## GUI Windows
- MVVM, bindinguri reactive.
- Randare Direct2D 60 FPS, drag & drop, pre‑move, highlight.
- Panou analiză cu multipv și grafic evaluare.
- Editor FEN/PGN, setări (nivel, TT, threaduri, cărți, TB, temă), localizare.

## API GUI–Engine
- UCI standard asincron pe pipe cu buffer ring și parser robust.

## Testare și calitate
- Perft până la adâncime 7–8; suite EPD.
- Auto‑joc cutechess‑cli cu SPRT; tuning Texel.
- Stres 72 h, leak detection, teste UI automate.

## Pipeline antrenare NNUE
- Auto‑joc multi‑thread pentru date, etichete din căutare adâncă.
- Training PyTorch (AdamW, LR ciclic, regularizare), export binar cu cuantizare int8.

## Licențiere
- Engine propriu (comercial) sau integrare Stockfish cu respectarea GPLv3.

## Structura repo
```
/engine
/gui
/tools
/packaging
```

## Plan incremental
0. Setup CI Windows, CMake, MSVC, PGO, skeleton WPF.
1. Bitboard, movegen, perft, UCI minim.
2. Căutare PVS, TT, ordonare, pruning.
3. Evaluare NNUE incrementală și convertor.
4. SMP, management timp, Syzygy, book, profilare.
5. GUI complet, multipv, export PGN, theming.
6. Teste Elo, tuning, hardening, MSIX, semnare.

## Performanță vizată
- 10M NPS pe Ryzen 7 7800X3D (16 threaduri).
- TT hit rate >35% la adâncimi medii.
- Adâncime 30 plies în 2 min.

## Diferențiatori
NNUE hot‑swap, politici anti‑blunder în criză, vizualizare variații în GUI.

## Riscuri și mitigări
Regresii de forță (SPRT continuu), probleme GPL (separare/rescriere), latențe IO (pipe asincron cu buffer).

## Next steps
1. Decide sursa engine‑ului.
2. Inițializează repo cu schelet C++ și WPF.
3. Implementare perft și framework test.
4. Stabilirea țintelor hardware și TB.

## Anexă: pseudocod PVS
```
function search(node, depth, alpha, beta):
  if depth <= 0:
    return qsearch(node, alpha, beta)
  if tt.has(node) and tt.depth >= depth:
    return tt.value
  best = -INF
  moves = order(generate(node))
  for i, m in enumerate(moves):
    child = apply(node, m)
    if i == 0:
      score = -search(child, depth-1, -beta, -alpha)
    else:
      score = -search(child, depth-1-R(m), -alpha-1, -alpha)
      if score > alpha and score < beta:
        score = -search(child, depth-1, -beta, -alpha)
    if score > best: best = score
    if best > alpha: alpha = best
    if alpha >= beta: break
  tt.store(node, best, depth)
  return best
```
