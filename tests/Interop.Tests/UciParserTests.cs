using System;
using System.IO;
using Xunit;
using Interop;

namespace Interop.Tests
{
    static class Fixture
    {
        public static string[] ReadLines(string name)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
            return File.ReadAllLines(path);
        }
    }

    public class UciParserTests
    {
        [Fact]
        public void UciParser_Should_ParseCentipawnAndMateScores()
        {
            var cpLine = Fixture.ReadLines("info_cp_multipv.txt")[0];
            var cp = UciParser.TryParseInfo(cpLine)!;
            Assert.False(cp.ScoreMate);
            Assert.Equal(34, cp.ScoreCp);

            var mateLines = Fixture.ReadLines("info_mate_short.txt");
            var matePlus = UciParser.TryParseInfo(mateLines[0])!;
            Assert.True(matePlus.ScoreMate);
            Assert.Equal(3, matePlus.ScoreCp);
            var mateNeg = UciParser.TryParseInfo(mateLines[2])!;
            Assert.True(mateNeg.ScoreMate);
            Assert.Equal(-2, mateNeg.ScoreCp);
        }

        [Fact]
        public void UciParser_Should_ParseMultiPV_And_PVs()
        {
            var lines = Fixture.ReadLines("info_cp_multipv.txt");
            var u1 = UciParser.TryParseInfo(lines[0])!;
            Assert.Equal(1, u1.MultiPv);
            Assert.Equal("e2e4 e7e5 g1f3", u1.Pv);
            var u2 = UciParser.TryParseInfo(lines[1])!;
            Assert.Equal(2, u2.MultiPv);
            Assert.Equal("d2d4 d7d5 c2c4", u2.Pv);
            var u3 = UciParser.TryParseInfo(lines[2])!;
            Assert.Equal(3, u3.MultiPv);
            Assert.Equal("g1f3 g8f6", u3.Pv);
        }

        [Fact]
        public void UciParser_Should_HandleWhitespace_And_UnknownTokens()
        {
            var lines = Fixture.ReadLines("info_whitespace_variants.txt");
            var u1 = UciParser.TryParseInfo(lines[0])!;
            Assert.Equal(20, u1.Depth);
            Assert.Equal(42, u1.ScoreCp);
            Assert.Equal("e2e4 e7e5", u1.Pv);

            var u2 = UciParser.TryParseInfo(lines[1])!;
            Assert.True(u2.ScoreMate);
            Assert.Equal(-1, u2.ScoreCp);

            var u3 = UciParser.TryParseInfo(lines[2])!;
            Assert.Equal(7, u3.TbHits);
            Assert.Equal(100, u3.Nodes);
            Assert.Equal("d2d4", u3.Pv);
        }

        [Fact]
        public void UciParser_Should_ParsePerformanceCounters()
        {
            var line = Fixture.ReadLines("info_perf_counters.txt")[0];
            var upd = UciParser.TryParseInfo(line)!;
            Assert.Equal(123456, upd.Nodes);
            Assert.Equal(789012, upd.Nps);
            Assert.Equal(3, upd.TbHits);
            Assert.Equal(50, upd.TimeMs);
        }
    }
}
