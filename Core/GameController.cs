using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Core;

/// <summary>
/// Minimal game controller that tracks basic game state (FEN + move list) and
/// provides a tiny UCI-facing API for the GUI. No dependency on Interop.
/// </summary>
public class GameController
{
    private readonly ILogger _logger;
    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1";

    /// <summary>Current position in Forsyth–Edwards Notation.</summary>
    public string Fen { get; private set; } = StartFen;

    // Very small move list so the GUI can build a UCI "position" command.
    private readonly List<string> _moves = new();
    /// <summary>Raised whenever an engine move is applied.</summary>
    public event Action<string>? EngineMoveApplied;

    public GameController(ILogger<GameController>? logger = null)
        => _logger = logger ?? Logging.Factory.CreateLogger<GameController>();

    /// <summary>Resets to the initial chess position.</summary>
    public void NewGame()
    {
        Fen = StartFen;
        _moves.Clear();
        _logger.LogInformation("New game started");
    }

    /// <summary>
    /// Returns a UCI "position" command reflecting the current state.
    /// Returns "position startpos" when no moves have been recorded;
    /// otherwise "position startpos moves" followed by the move list.
    /// </summary>
    public string ToUciPositionCommand()
    {
        if (_moves.Count == 0) return "position startpos";
        return "position startpos moves " + string.Join(" ", _moves);
    }

    /// <summary>
    /// Tries to apply a user move given in UCI (e.g., "e2e4" or "e7e8q").
    /// Accepts 4/5-character strings. Returns true if accepted.
    /// </summary>
    public bool TryApplyUserMove(string uci)
    {
        if (string.IsNullOrWhiteSpace(uci)) return false;
        var s = uci.Trim();
        if (s.Length is 4 or 5)
        {
            _moves.Add(s);
            // TODO: Update FEN based on moves (out of current scope)
            return true;
        }
        return false;
    }

    /// <summary>
    /// Overload used by GUI callers that supply separate components (from, to, promotion).
    /// Types are kept wide (object) to accommodate either strings (e.g., "e2") or indices (0..63).
    /// </summary>
    public bool TryApplyUserMove(object from, object to, object? promotion = null)
        => TryApplyUserMove(BuildUci(from, to, promotion));

    /// <summary>
    /// Attempts to apply an engine move in UCI form. Uses the same validation as
    /// user moves and updates the internal move list. Raises
    /// <see cref="EngineMoveApplied"/> when successful. Returns true when the
    /// move is accepted.
    /// </summary>
    public bool TryApplyEngineMove(string uci)
    {
        if (TryApplyUserMove(uci))
        {
            EngineMoveApplied?.Invoke(uci.Trim());
            return true;
        }
        return false;
    }

    /// <summary>
    /// Overload for engine move using separate components (from, to,
    /// promotion).
    /// </summary>
    public bool TryApplyEngineMove(object from, object to, object? promotion = null)
        => TryApplyEngineMove(BuildUci(from, to, promotion));

    // Helpers
    private static string BuildUci(object from, object to, object? promotion)
    {
        // If indices are provided (0..63), convert to algebraic (a1..h8)
        string fromStr = ConvertSquare(from);
        string toStr   = ConvertSquare(to);
        string promo   = promotion?.ToString()?.Trim() ?? string.Empty;
        return fromStr + toStr + promo;
    }

    private static string ConvertSquare(object value)
    {
        // Accept either algebraic like "e2" or zero-based index 0..63
        var s = value?.ToString()?.Trim() ?? string.Empty;
        if (int.TryParse(s, out var idx) && idx >= 0 && idx < 64)
        {
            int file = idx % 8;        // 0..7
            int rank = idx / 8;        // 0..7
            char f = (char)('a' + file);
            char r = (char)('1' + rank);
            return $"{f}{r}";
        }
        // fall back to original string (assumed like "e2")
        return s;
    }
}
