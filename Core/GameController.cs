namespace Core;

/// <summary>
/// Minimal game controller that keeps only the FEN/state needed by tests/GUI.
/// NOTE: No dependency on Interop. Engine wiring should be done in the GUI layer.
/// </summary>
public class GameController
{
    /// <summary>
    /// Current position in Forsyth–Edwards Notation.
    /// </summary>
    public string Fen { get; private set; } = StartFen;

    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1";

    /// <summary>
    /// Resets to the initial chess position.
    /// </summary>
    public void NewGame()
    {
        Fen = StartFen;
    }

    /// <summary>
    /// Applies a UCI move to internal state. For now this is a no-op placeholder
    /// (kept for API compatibility). Real move application lives in a follow-up task.
    /// </summary>
    public void ApplyMove(string uciMove)
    {
        // TODO: update FEN from move; intentionally left as no-op in this MVP.
    }
}
