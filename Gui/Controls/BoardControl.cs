using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Gui.Controls
{
    public class BoardControl : Canvas
    {
        public event Action<string, string, string?>? MoveRequested;
        private char[] _board = new char[64]; // a8..h1
        private int _selected = -1;
        private readonly Typeface _typeface = new Typeface("Segoe UI Symbol");

        public BoardControl()
        {
            ClipToBounds = true;
            Focusable = true;
            Background = Brushes.Beige;
            Width = 640;
            Height = 640;
            ResetToStart();
            MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        public void ResetToStart()
        {
            SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1");
        }

        public void SetPosition(string fen)
        {
            try
            {
                var ranks = fen.Split(' ')[0];
                var squares = new char[64];
                int idx = 0;
                foreach (var ch in ranks)
                {
                    if (ch == '/') continue;
                    if (char.IsDigit(ch)) idx += (int)char.GetNumericValue(ch);
                    else squares[idx++] = ch;
                }
                _board = squares;
                InvalidateVisual();
            }
            catch { }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            double size = Math.Min(ActualWidth, ActualHeight);
            double sq = size / 8.0;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    bool dark = ((r + c) % 2) == 1;
                    var rect = new Rect(c * sq, r * sq, sq, sq);
                    dc.DrawRectangle(dark ? Brushes.Sienna : Brushes.Bisque, null, rect);
                    int i = r * 8 + c;
                    if (i == _selected)
                    {
                        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(64, 0, 128, 255)), null, rect);
                    }
                    char p = _board[i];
                    if (p != '\0')
                    {
                        string symbol = ToUnicodePiece(p);
                        var formatted = new FormattedText(symbol, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _typeface, sq * 0.8, dark ? Brushes.White : Brushes.Black, 96);
                        dc.DrawText(formatted, new Point(c * sq + sq * 0.1, r * sq + sq * 0.05));
                    }
                }
            }
        }

        private static string ToUnicodePiece(char p) => p switch
        {
            'K' => "♔", 'Q' => "♕", 'R' => "♖", 'B' => "♗", 'N' => "♘", 'P' => "♙",
            'k' => "♚", 'q' => "♛", 'r' => "♜", 'b' => "♝", 'n' => "♞", 'p' => "♟", _ => ""
        };

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            Point p = e.GetPosition(this);
            double size = Math.Min(ActualWidth, ActualHeight);
            double sq = size / 8.0;
            int c = Math.Clamp((int)(p.X / sq), 0, 7);
            int r = Math.Clamp((int)(p.Y / sq), 0, 7);
            int index = r * 8 + c;

            if (_selected < 0) _selected = index;
            else
            {
                var from = IndexToCoord(_selected);
                var to = IndexToCoord(index);
                _selected = -1;
                MoveRequested?.Invoke(from, to, null);
            }
            InvalidateVisual();
        }

        private static string IndexToCoord(int i)
        {
            int file = i % 8;
            int rankFromTop = i / 8;
            int rank = 7 - rankFromTop;
            char f = (char)('a' + file);
            char r = (char)('1' + rank);
            return $"{f}{r}";
        }
    }
}
