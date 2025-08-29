using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class GameController
    {
        private readonly List<string> _uciMoves = new();
        private readonly char[] _board = new char[64]; // a8..h1
        private readonly List<Candidate> _candidates = new();
        public string Fen { get; private set; } = string.Empty;

        public void NewGame()
        {
            LoadStartPosition();
            _uciMoves.Clear();
            Fen = BuildFen();
        }

        public string ToUciPositionCommand()
        {
            if (_uciMoves.Count == 0) return "position startpos";
            return "position startpos moves " + string.Join(' ', _uciMoves);
        }

        public bool TryApplyUserMove(string from, string to, string? promo)
        {
            var uci = (from + to + (promo ?? "")).Trim();
            if (!ApplyUciMove(uci)) return false;
            _uciMoves.Add(uci);
            Fen = BuildFen();
            return true;
        }

        public bool ApplyEngineMove(string bestmove, MovePolicy? policy = null, int seed = 0)
        {
            var uci = bestmove.Trim();
            if (policy != null && policy.TopK > 1 && _candidates.Count > 0)
            {
                var sel = CandidateSelector.Select(_candidates, policy, seed);
                if (sel != null) uci = sel.Move;
            }
            if (string.IsNullOrWhiteSpace(uci) || uci == "(none)") return false;
            if (!ApplyUciMove(uci)) return false;
            _uciMoves.Add(uci);
            Fen = BuildFen();
            return true;
        }

        public void IngestInfo(Interop.InfoUpdate info)
        {
            if (info.MultiPv <= 0 || string.IsNullOrWhiteSpace(info.RootMove)) return;
            var cand = new Candidate(info.RootMove, info.ScoreCp, info.MultiPv);
            int idx = _candidates.FindIndex(c => c.Rank == info.MultiPv);
            if (idx >= 0) _candidates[idx] = cand; else _candidates.Add(cand);
            _candidates.Sort((a, b) => a.Rank.CompareTo(b.Rank));
        }

        private static int CoordToIndex(string coord)
        {
            int file = coord[0] - 'a';
            int rank = coord[1] - '1';
            int rowFromTop = 7 - rank;
            return rowFromTop * 8 + file;
        }

        private bool ApplyUciMove(string uci)
        {
            if (uci.Length < 4) return false;
            string from = uci.Substring(0, 2);
            string to = uci.Substring(2, 2);
            char promo = uci.Length >= 5 ? uci[4] : '\0';

            int iFrom = CoordToIndex(from);
            int iTo = CoordToIndex(to);
            if (iFrom < 0 || iFrom >= 64 || iTo < 0 || iTo >= 64) return false;

            char piece = _board[iFrom];
            if (piece == '\0') return false;

            // Castling
            if ((piece == 'K' && from == "e1" && to == "g1") || (piece == 'k' && from == "e8" && to == "g8"))
            {
                _board[iTo] = piece; _board[iFrom] = '\0';
                int rookFrom = piece == 'K' ? CoordToIndex("h1") : CoordToIndex("h8");
                int rookTo   = piece == 'K' ? CoordToIndex("f1") : CoordToIndex("f8");
                _board[rookTo] = _board[rookFrom]; _board[rookFrom] = '\0';
                return true;
            }
            if ((piece == 'K' && from == "e1" && to == "c1") || (piece == 'k' && from == "e8" && to == "c8"))
            {
                _board[iTo] = piece; _board[iFrom] = '\0';
                int rookFrom = piece == 'K' ? CoordToIndex("a1") : CoordToIndex("a8");
                int rookTo   = piece == 'K' ? CoordToIndex("d1") : CoordToIndex("d8");
                _board[rookTo] = _board[rookFrom]; _board[rookFrom] = '\0';
                return true;
            }

            // En passant heuristic: pawn moves diagonally, destination empty
            bool isPawn = piece == 'P' || piece == 'p';
            if (isPawn && _board[iTo] == '\0')
            {
                int fileDiff = Math.Abs((iFrom % 8) - (iTo % 8));
                int rankDiff = Math.Abs((iFrom / 8) - (iTo / 8));
                if (fileDiff == 1 && rankDiff == 1)
                {
                    int dir = piece == 'P' ? 1 : -1;
                    int capturedIndex = iTo - dir * 8;
                    if (capturedIndex >= 0 && capturedIndex < 64)
                    {
                        char cap = _board[capturedIndex];
                        if (cap == 'p' || cap == 'P') _board[capturedIndex] = '\0';
                    }
                }
            }

            _board[iTo] = piece;
            _board[iFrom] = '\0';

            // Promotion
            if (promo != '\0' && (piece == 'P' || piece == 'p'))
            {
                bool white = piece == 'P';
                char pp = char.ToLowerInvariant(promo);
                char promoted = pp switch
                {
                    'q' => white ? 'Q' : 'q',
                    'r' => white ? 'R' : 'r',
                    'b' => white ? 'B' : 'b',
                    'n' => white ? 'N' : 'n',
                    _ => white ? 'Q' : 'q'
                };
                _board[iTo] = promoted;
            }

            return true;
        }

        private void LoadStartPosition()
        {
            string start = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
            for (int i = 0; i < 64; i++) _board[i] = '\0';
            int idx = 0;
            foreach (var ch in start)
            {
                if (ch == '/') continue;
                if (char.IsDigit(ch)) idx += (int)char.GetNumericValue(ch);
                else _board[idx++] = ch;
            }
        }

        private string BuildFen()
        {
            var sb = new StringBuilder();
            for (int r = 0; r < 8; r++)
            {
                int empty = 0;
                for (int c = 0; c < 8; c++)
                {
                    int i = r * 8 + c;
                    char p = _board[i];
                    if (p == '\0') empty++;
                    else
                    {
                        if (empty > 0) { sb.Append(empty); empty = 0; }
                        sb.Append(p);
                    }
                }
                if (empty > 0) sb.Append(empty);
                if (r != 7) sb.Append('/');
            }
            return sb.ToString() + " w - - 0 1";
        }
    }
}
