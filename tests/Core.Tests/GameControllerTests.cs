using System;
using System.IO;
using Xunit;
using Core;

namespace Core.Tests
{
    public class GameControllerTests
    {
        [Fact]
        public void GameController_Should_LoadStartpos_FEN()
        {
            var gc = new GameController();
            gc.NewGame();
            Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1", gc.Fen);
        }

        [Fact]
        public void GameController_Should_ApplyLegalMove_AndRejectIllegal()
        {
            var gc = new GameController();
            gc.NewGame();
            Assert.True(gc.TryApplyUserMove("e2", "e4", null));
            Assert.Equal("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR w - - 0 1", gc.Fen);
            Assert.False(gc.TryApplyUserMove("e2", "e4", null));
        }
    }

    public class PgnServiceTests
    {
        [Fact]
        public void PgnService_Should_EmitMinimalGameHeader()
        {
            var path = Path.GetTempFileName();
            try
            {
                PgnService.SavePgn(path, Array.Empty<string>());
                var lines = File.ReadAllLines(path);
                Assert.StartsWith("[Event \"Casual Game\"]", lines[0]);
                Assert.Contains("[Result \"*\"]", lines);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
