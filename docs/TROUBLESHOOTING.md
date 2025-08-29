# DEPANARE

## Timeouts la handshake
- Mărește ușor timeouts și verifică dacă un antivirus nu blochează procesul motorului.
- Rulează motorul manual pentru a confirma că pornește în mediul tău.

## Motor negăsit
- `EnginePath` trebuie să indice un executabil sau plasează `stockfish.exe` în `Engines/`.
- Pe unități de rețea, verifică politicile de execuție.

## Syzygy nu se încarcă
- `SyzygyPath` trebuie să indice un folder cu `.rtbw/.rtbz`. Unele motoare cer capitalizare exactă; recomandat SSD.

## Blocaje I/O / țevi rupte
- Ține operațiile costisitoare în afara UI thread; folosește I/O asincron cu backpressure.

### Particularități Windows
- Antivirusul blochează pornirea motorului → adaugă calea motorului în allowlist.
- MSIX nu poate porni motorul → rulează installerul o dată ca Administrator; verifică politicile „App Installer”.
- Căi cu spații → citează întotdeauna `EnginePath`.

### Capturarea unui log UCI
Activează o opțiune de logging UCI (dacă este suportată) **sau** rulează motorul printr-un wrapper ce redirecționează stdout în `Logs/uci-YYYYMMDD.txt`. Atașează acest log când deschizi un bug.
