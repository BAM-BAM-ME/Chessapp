# PROFILES

Profilurile sunt stocate în `Data/profiles.json`.

## Schemă
- `name`: string
- `uci`: obiect (perechi pentru `setoption` ale motorului)
- `move_policy`: obiect
  - `deterministic`: bool
  - `top_k`: int ≥ 1
- `book`: obiect
  - `enabled`: bool
  - `allow`: string[] — listă de coduri ECO sau intervale

### Opening Book (`book`)
Controlează utilizarea unei allowlist ECO. Când `enabled=true`, motorul primește doar deschideri ale căror coduri ECO se potrivesc allowlist-ului. O intrare poate fi un cod ECO singular (ex.: `C65`) sau un interval inclusiv cu `..` (ex.: `C60..C99`).

```json
{
  "book": {
    "enabled": true,
    "allow": ["C60..C99", "D30", "B50..B99"]
  }
}
```

**Note**
- `allow` **trebuie** să fie un array de string-uri. Lista goală înseamnă „nicio restricție” (similar cu `enabled=false`).
- Validare recomandată: `^[A-E][0-9]{2}(\.\.[A-E][0-9]{2})?$`.
- Recomandat: menține `MultiPV ≥ top_k` în profilurile unde selectezi din top-k.

## Exemplu profil
```json
{
  "name": "Titan Max",
  "uci": { "Threads": 8, "Hash": 2048, "Ponder": true, "MultiPV": 1 },
  "book": { "enabled": true, "allow": ["C60..C99", "D30..D69"] },
  "move_policy": { "deterministic": true, "top_k": 1 }
}
```

## Aplicarea profilului
- Pentru fiecare cheie din `uci{...}`: `setoption name <Cheie> value <Valoare>`.
- Dacă `move_policy.top_k > 1`, asigură `MultiPV ≥ top_k` și alege o mutare dintre primele K (stocastic când `deterministic=false`).

## Bune practici
- Respectă numele exact al opțiunilor acceptate de motor.
- Ajustează Threads/Hash/MultiPV/ponder în funcție de hardware și scop (joc vs. analiză).
