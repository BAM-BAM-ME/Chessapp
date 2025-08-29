using System;

namespace Interop
{
    public class InfoUpdate
    {
        public int Depth { get; set; }
        public int SelDepth { get; set; }
        public int ScoreCp { get; set; }
        public bool ScoreMate { get; set; }
        public int Nodes { get; set; }
        public int Nps { get; set; }
        public int TbHits { get; set; }
        public string Pv { get; set; } = string.Empty;
        public string RootMove { get; set; } = string.Empty;
        public int MultiPv { get; set; } = 1;
        public int TimeMs { get; set; }
    }

    public static class UciParser
    {
        public static InfoUpdate? TryParseInfo(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("info ")) return null;
            var upd = new InfoUpdate();
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < parts.Length; i++)
            {
                string t = parts[i];
                if (t == "depth" && i + 1 < parts.Length && int.TryParse(parts[i + 1], out var d)) { upd.Depth = d; i++; }
                else if (t == "seldepth" && i + 1 < parts.Length && int.TryParse(parts[i + 1], out var sd)) { upd.SelDepth = sd; i++; }
                else if (t == "nodes" && i + 1 < parts.Length && int.TryParse(parts[i + 1], out var n)) { upd.Nodes = n; i++; }
                else if (t == "nps" && i + 1 < parts.Length && int.TryParse(parts[i + 1], out var nps)) { upd.Nps = nps; i++; }
                else if (t == "tbhits" && i + 1 < parts.Length && int.TryParse(parts[i + 1], out var tb)) { upd.TbHits = tb; i++; }
                else if (t == "multipv" && i + 1 < parts.Length && int.TryParse(parts[i + 1], out var mpv)) { upd.MultiPv = mpv; i++; }
                else if (t == "time" && i + 1 < parts.Length && int.TryParse(parts[i + 1], out var tm)) { upd.TimeMs = tm; i++; }
                else if (t == "score" && i + 1 < parts.Length)
                {
                    if (parts[i + 1] == "cp" && i + 2 < parts.Length && int.TryParse(parts[i + 2], out var cp)) { upd.ScoreCp = cp; i += 2; }
                    else if (parts[i + 1] == "mate" && i + 2 < parts.Length && int.TryParse(parts[i + 2], out var mate)) { upd.ScoreMate = true; upd.ScoreCp = mate; i += 2; }
                }
                else if (t == "pv")
                {
                    upd.Pv = string.Join(' ', parts, i + 1, parts.Length - (i + 1));
                    var mvParts = upd.Pv.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (mvParts.Length > 0) upd.RootMove = mvParts[0];
                    break;
                }
            }
            return upd;
        }
    }
}
