using Core;
using Xunit;

namespace Core.Tests;

public class GameControllerTests
{
    [Fact]
    public void GameController_Should_LoadStartPos()
    {
        var gc = new GameController();
        gc.NewGame();
        Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1", gc.Fen);
    }

    [Fact]
    public void ToUciPositionCommand_Should_ReturnStartPos_When_NoMoves()
    {
        var gc = new GameController();
        gc.NewGame();
        Assert.Equal("position startpos", gc.ToUciPositionCommand());
    }

    [Fact]
    public void ToUciPositionCommand_Should_ListMoves_When_Present()
    {
        var gc = new GameController();
        gc.NewGame();
        gc.TryApplyUserMove("e2e4");
        gc.TryApplyUserMove("c7c5");
        Assert.Equal("position startpos moves e2e4 c7c5", gc.ToUciPositionCommand());
    }

    [Fact]
    public void ApplyEngineMove_Should_RecordMove()
    {
        var gc = new GameController();
        gc.NewGame();
        bool applied = gc.ApplyEngineMove("g8f6");
        Assert.True(applied);
        Assert.Equal("position startpos moves g8f6", gc.ToUciPositionCommand());
    }

    [Fact]
    public void ApplyEngineMove_Should_RaiseEvent()
    {
        var gc = new GameController();
        gc.NewGame();
        string? notified = null;
        gc.EngineMoveApplied += m => notified = m;
        bool applied = gc.ApplyEngineMove("g8f6");
        Assert.True(applied);
        Assert.Equal("g8f6", notified);
    }
}
