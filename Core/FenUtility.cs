using System;
using System.Text;

namespace Core;

/// <summary>
/// Minimal FEN helper capable of applying UCI moves and deriving new FEN.
/// It does <b>not</b> validate move legality, but updates basic state such as
/// en-passant, castling rights, move counters and side to move.
/// </summary>
internal static class FenUtility
{
    private const char Empty = '.';

    private sealed class Position
    {
        public char[,] Board = new char[8,8];
        public bool WhiteToMove;
        public string Castling = "-";
        public string EnPassant = "-";
        public int HalfmoveClock;
        public int FullmoveNumber;
    }

    public static bool TryApplyMove(string fen, string uci, out string newFen)
    {
        try
        {
            var pos = ParseFen(fen);
            ApplyMove(pos, uci);
            newFen = FormatFen(pos);
            return true;
        }
        catch
        {
            newFen = fen;
            return false;
        }
    }

    private static Position ParseFen(string fen)
    {
        var parts = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 6) throw new ArgumentException("Invalid FEN", nameof(fen));
        var pos = new Position();

        // board
        var rows = parts[0].Split('/');
        if (rows.Length != 8) throw new ArgumentException("Invalid board in FEN", nameof(fen));
        for (int r = 0; r < 8; r++)
        {
            int file = 0;
            foreach (char c in rows[r])
            {
                if (char.IsDigit(c))
                {
                    int empty = c - '0';
                    for (int i = 0; i < empty; i++) pos.Board[r, file++] = Empty;
                }
                else
                {
                    pos.Board[r, file++] = c;
                }
            }
            if (file != 8) throw new ArgumentException("Invalid board row", nameof(fen));
        }

        pos.WhiteToMove = parts[1] == "w";
        pos.Castling = parts[2];
        pos.EnPassant = parts[3];
        pos.HalfmoveClock = int.Parse(parts[4]);
        pos.FullmoveNumber = int.Parse(parts[5]);
        return pos;
    }

    private static string FormatFen(Position pos)
    {
        var sb = new StringBuilder();
        for (int r = 0; r < 8; r++)
        {
            int empty = 0;
            for (int f = 0; f < 8; f++)
            {
                char p = pos.Board[r, f];
                if (p == Empty || p == '\0')
                {
                    empty++;
                }
                else
                {
                    if (empty > 0)
                    {
                        sb.Append(empty);
                        empty = 0;
                    }
                    sb.Append(p);
                }
            }
            if (empty > 0) sb.Append(empty);
            if (r < 7) sb.Append('/');
        }
        string castling = pos.Castling;
        if (string.IsNullOrEmpty(castling) || castling == "-") castling = "-";
        string ep = pos.EnPassant;
        if (string.IsNullOrEmpty(ep)) ep = "-";
        return string.Join(' ', sb.ToString(), pos.WhiteToMove ? "w" : "b", castling, ep,
            pos.HalfmoveClock.ToString(), pos.FullmoveNumber.ToString());
    }

    private static void ApplyMove(Position pos, string move)
    {
        if (move.Length < 4) throw new ArgumentException("Invalid move", nameof(move));
        int fromFile = move[0] - 'a';
        int fromRank = move[1] - '1';
        int toFile = move[2] - 'a';
        int toRank = move[3] - '1';
        int fromRow = 7 - fromRank;
        int fromCol = fromFile;
        int toRow = 7 - toRank;
        int toCol = toFile;

        char piece = pos.Board[fromRow, fromCol];
        if (piece == Empty || piece == '\0') throw new ArgumentException("No piece on from square", nameof(move));
        bool white = char.IsUpper(piece);
        bool pawn = piece is 'P' or 'p';
        bool capture = pos.Board[toRow, toCol] != Empty;

        // Castling
        if (piece == 'K' && fromRow == 7 && fromCol == 4)
        {
            if (toCol == 6) // white king side
            {
                pos.Board[7, 4] = Empty; pos.Board[7, 6] = 'K';
                pos.Board[7, 7] = Empty; pos.Board[7, 5] = 'R';
            }
            else if (toCol == 2) // white queen side
            {
                pos.Board[7, 4] = Empty; pos.Board[7, 2] = 'K';
                pos.Board[7, 0] = Empty; pos.Board[7, 3] = 'R';
            }
            else
            {
                MovePiece(pos, fromRow, fromCol, toRow, toCol);
            }
            pos.Castling = pos.Castling.Replace("K", string.Empty).Replace("Q", string.Empty);
        }
        else if (piece == 'k' && fromRow == 0 && fromCol == 4)
        {
            if (toCol == 6) // black king side
            {
                pos.Board[0, 4] = Empty; pos.Board[0, 6] = 'k';
                pos.Board[0, 7] = Empty; pos.Board[0, 5] = 'r';
            }
            else if (toCol == 2)
            {
                pos.Board[0, 4] = Empty; pos.Board[0, 2] = 'k';
                pos.Board[0, 0] = Empty; pos.Board[0, 3] = 'r';
            }
            else
            {
                MovePiece(pos, fromRow, fromCol, toRow, toCol);
            }
            pos.Castling = pos.Castling.Replace("k", string.Empty).Replace("q", string.Empty);
        }
        else
        {
            // en passant capture
            if (pawn && !capture && fromCol != toCol && pos.EnPassant != "-" &&
                SquareToCoords(pos.EnPassant, out int epRow, out int epCol) && epRow == toRow && epCol == toCol)
            {
                capture = true;
                pos.Board[white ? toRow + 1 : toRow - 1, toCol] = Empty;
            }

            // move piece or promotion
            char promo = move.Length == 5 ? move[4] : '\0';
            if (promo != '\0') promo = white ? char.ToUpper(promo) : char.ToLower(promo);
            MovePiece(pos, fromRow, fromCol, toRow, toCol, promo);

            // update castling rights on rook or king moves/captures
            if (piece == 'K') pos.Castling = pos.Castling.Replace("K", string.Empty).Replace("Q", string.Empty);
            if (piece == 'k') pos.Castling = pos.Castling.Replace("k", string.Empty).Replace("q", string.Empty);
            // rook moves
            if (piece == 'R')
            {
                if (fromRow == 7 && fromCol == 0) pos.Castling = pos.Castling.Replace("Q", string.Empty);
                if (fromRow == 7 && fromCol == 7) pos.Castling = pos.Castling.Replace("K", string.Empty);
            }
            if (piece == 'r')
            {
                if (fromRow == 0 && fromCol == 0) pos.Castling = pos.Castling.Replace("q", string.Empty);
                if (fromRow == 0 && fromCol == 7) pos.Castling = pos.Castling.Replace("k", string.Empty);
            }
            // rook captured
            if (capture)
            {
                if (toRow == 7 && toCol == 0) pos.Castling = pos.Castling.Replace("Q", string.Empty);
                if (toRow == 7 && toCol == 7) pos.Castling = pos.Castling.Replace("K", string.Empty);
                if (toRow == 0 && toCol == 0) pos.Castling = pos.Castling.Replace("q", string.Empty);
                if (toRow == 0 && toCol == 7) pos.Castling = pos.Castling.Replace("k", string.Empty);
            }
        }

        // en passant target
        if (pawn && Math.Abs(toRow - fromRow) == 2)
        {
            int epRow = white ? toRow + 1 : toRow - 1;
            int epRank = 8 - epRow;
            pos.EnPassant = ToSquare(epRank, toCol);
        }
        else
        {
            pos.EnPassant = "-";
        }

        // halfmove clock
        if (pawn || capture)
            pos.HalfmoveClock = 0;
        else
            pos.HalfmoveClock++;

        // switch side and update fullmove
        pos.WhiteToMove = !pos.WhiteToMove;
        if (pos.WhiteToMove)
            pos.FullmoveNumber++;

        if (string.IsNullOrEmpty(pos.Castling)) pos.Castling = "-";
    }

    private static void MovePiece(Position pos, int fromRow, int fromCol, int toRow, int toCol, char promo = '\0')
    {
        char piece = pos.Board[fromRow, fromCol];
        pos.Board[fromRow, fromCol] = Empty;
        pos.Board[toRow, toCol] = promo != '\0' ? promo : piece;
    }

    private static bool SquareToCoords(string square, out int row, out int col)
    {
        row = col = 0;
        if (square.Length != 2) return false;
        col = square[0] - 'a';
        int rank = square[1] - '1';
        if (col < 0 || col > 7 || rank < 0 || rank > 7) return false;
        row = 7 - rank;
        return true;
    }

    private static string ToSquare(int rank, int file)
    {
        // rank: 1..8
        char f = (char)('a' + file);
        char r = (char)('0' + rank);
        return string.Concat(f, r);
    }
}
