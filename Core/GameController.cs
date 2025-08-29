using System.Collections.Generic;

namespace Core;

/// <summary>
/// Minimal game controller that keeps only board state and simple UCI plumbing
/// required by the GUI. No dependency on Interop.
/// </summary>
public class GameController
{
    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1";

    /// <summary>Current position in Forsyth–Edwards Notation.</summary>
    public string Fen { get; private set; } = StartFen;

    // Keep a very small move list so the GUI can build a UCI "position" command.
    private readonly List<string> _moves = new();

    /// <summary>Resets to the initial chess position.</summary>
    public void NewGame()
    {
        Fen = StartFen;
        _moves.Clear();
    }

    /// <summary>
    /// Builds the UCI position command from current state.
    /// Example outputs: "position startpos" or "position startpos moves e2e4 e7e5".
    /// </summary>
    public string ToUciPositionCommand()
    {
        if (_moves.Count == 0) return "position startpos";
        return "position startpos moves " + string.Join(" ", _moves);
    }

    /// <summary>
    /// Tries to apply a user move given in UCI (e.g., "e2e4" or "e7e8q").
    /// For now, accepts basic 4/5-char forms and records it without recomputing FEN.
    /// Returns true if accepted.
    /// </summary>
    public bool TryApplyUserMove(string uci)
    {
        if (string.IsNullOrWhiteSpace(uci)) return false;
        var s = uci.Trim();
        if (s.Length is 4 or 5)
        {
            _moves.Add(s);
            // TODO: Update FEN based on moves (out of scope for this build fix)
            return true;
        }
        return false;
    }

    /// <summary>
    /// Applies an engine move in UCI form. Currently records the move only.
    /// </summary>
    public void ApplyEngineMove(string uci)
    {
        if (!string.IsNullOrWhiteSpace(uci))
        {
            _moves.Add(uci.Trim());
            // TODO: Update FEN based on moves (out of scope for this build fix)
        }
    }
}
