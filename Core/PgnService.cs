using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core
{
    public static class PgnService
    {
        public static void SavePgn(string path, IEnumerable<string> uciMoves)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Event \"Casual Game\"]");
            sb.AppendLine("[Site \"?\"]");
            sb.AppendLine("[Date \"" + DateTime.UtcNow.ToString("yyyy.MM.dd") + "\"]");
            sb.AppendLine("[Round \"-\"]");
            sb.AppendLine("[White \"Human\"]");
            sb.AppendLine("[Black \"Engine\"]");
            sb.AppendLine("[Result \"*\"]");
            sb.AppendLine();
            int i = 1;
            foreach (var _ in uciMoves)
            {
                sb.Append(i + ". ... ");
                i++;
            }
            sb.AppendLine("*");
            File.WriteAllText(path, sb.ToString());
        }
    }
}
