using Interop;
using Xunit;

namespace Chessapp.Tests
{
    public class UciParserTests
    {
        [Theory]
        [InlineData("info depth 20 seldepth 36 multipv 2 score cp 34 pv e2e4 e7e5 g1f3", 2, "e2e4", 34)]
        [InlineData("info depth 15 multipv 1 score cp -12 pv d2d4 d7d5", 1, "d2d4", -12)]
        public void ParsesMultiPvAndRootMove(string line, int idx, string root, int score)
        {
            var upd = UciParser.TryParseInfo(line);
            Assert.NotNull(upd);
            Assert.Equal(idx, upd!.MultiPv);
            Assert.Equal(root, upd.RootMove);
            Assert.Equal(score, upd.ScoreCp);
        }
    }
}
