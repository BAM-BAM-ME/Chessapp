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
}
