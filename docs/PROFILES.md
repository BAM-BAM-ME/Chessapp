# PROFILES

Profiles are stored in `Data/profiles.json`.

## Schema
- `name`: string
- `uci`: object (engine `setoption` key/value pairs)
- `move_policy`: object
  - `deterministic`: bool
  - `top_k`: int ≥ 1
- `book`: object
  - `path`: string (optional, **deprecated**)
  - `allow`: string[] — list of ECO codes or inclusive ranges

### Opening Book (`book`)
Controls the allowlist of ECO codes. When `allow` is empty, no ECO filtering is applied. `path` is optional and deprecated; it will be removed once engine book loading is implemented. An entry can be a single ECO code (e.g. `C65`) or an inclusive range with `..` (e.g. `C60..C99`).

```json
{
  "book": {
    "path": "Books/titan.bin",
    "allow": ["C60..C99", "D30", "B50..B99"]
  }
}
```

**Notes**
- `allow` **must** be an array of strings. An empty list means "no restriction".
- Recommended validation pattern: `^[A-E][0-9]{2}(\.\.[A-E][0-9]{2})?$`.
- Recommended: keep `MultiPV ≥ top_k` in profiles where you select from the top-k list.

## Example profile
```json
{
  "name": "Titan Max",
  "uci": { "Threads": 8, "Hash": 2048, "Ponder": true, "MultiPV": 1 },
  "book": { "path": "Books/titan.bin", "allow": ["C60..C99", "D30..D69"] },
  "move_policy": { "deterministic": true, "top_k": 1 }
}
```

## Applying a profile
- For every key in `uci{...}`: `setoption name <Key> value <Value>`.
- If `move_policy.top_k > 1`, ensure `MultiPV ≥ top_k` and choose a move among the first K (stochastic when `deterministic=false`).

## Best practices
- Use the exact option names accepted by the engine.
- Adjust Threads/Hash/MultiPV/Ponder according to hardware and purpose (play vs. analysis).

## Validation
```bash
npx ajv validate -s docs/schemas/profiles.schema.json -d Data/profiles.json
```

```csharp
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

var schema = JSchema.Parse(File.ReadAllText("docs/schemas/profiles.schema.json"));
var profiles = JArray.Parse(File.ReadAllText("Data/profiles.json"));
profiles.Validate(schema);
```
