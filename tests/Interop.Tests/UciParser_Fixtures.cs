using System.IO;
using System.Reflection;
using Interop;
using Xunit;

namespace Interop.Tests;

public class UciParser_Fixtures
{
    private static string Fixture(string name)
    {
        var asm = Assembly.GetExecutingAssembly();
        var dir = Path.Combine(Path.GetDirectoryName(asm.Location)!, "Fixtures");
        return Path.Combine(dir, name);
    }

    [Fact]
    public void Info_Should_Parse_All_Common_Fields()
    {
        var lines = File.ReadAllLines(Fixture("info_hashfull.txt"));
        var upd = UciParser.TryParseInfo(lines[0])!;
        Assert.Equal(25, upd.Depth);
        Assert.Equal(32, upd.SelDepth);
        Assert.Equal(13, upd.ScoreCp);
        Assert.False(upd.ScoreMate);
        Assert.Equal(123456, upd.Nodes);
        Assert.Equal(765432, upd.Nps);
        Assert.Equal(512, upd.HashFull);
        Assert.Equal(2, upd.MultiPv);
        Assert.Equal("e2e4", upd.RootMove);
        Assert.Equal("e2e4 e7e5 g1f3", upd.Pv);

        var upd2 = UciParser.TryParseInfo(lines[1])!;
        Assert.Equal(15, upd2.Depth);
        Assert.Equal(22, upd2.SelDepth);
        Assert.Equal(-3, upd2.ScoreCp);
        Assert.True(upd2.ScoreMate);
        Assert.Equal(2048, upd2.Nodes);
        Assert.Equal(8192, upd2.Nps);
        Assert.Equal(873, upd2.HashFull);
        Assert.Equal(1, upd2.MultiPv);
        Assert.Equal("d2d4", upd2.RootMove);
        Assert.Equal("d2d4 d7d5", upd2.Pv);
    }

    [Fact]
    public void BestMove_Should_Parse_Move_And_Ponder()
    {
        var lines = File.ReadAllLines(Fixture("bestmove.txt"));
        var bm1 = UciParser.TryParseBestMove(lines[0])!;
        Assert.Equal("e2e4", bm1.Move);
        Assert.Equal(string.Empty, bm1.Ponder);

        var bm2 = UciParser.TryParseBestMove(lines[1])!;
        Assert.Equal("g1f3", bm2.Move);
        Assert.Equal("e7e5", bm2.Ponder);
    }
}
