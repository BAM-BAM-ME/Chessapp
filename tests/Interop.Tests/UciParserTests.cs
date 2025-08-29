using System.IO;
using System.Reflection;
using Interop;
using Xunit;

namespace Interop.Tests;

public class UciParserTests
{
    [Fact]
    public void UciParser_Fixtures_Should_ParseKnownLines()
    {
        var asm = Assembly.GetExecutingAssembly();
        var dir = Path.Combine(Path.GetDirectoryName(asm.Location)!, "Fixtures");
        var lines = File.ReadAllLines(Path.Combine(dir, "info.txt"));
        foreach (var line in lines)
        {
            var upd = UciParser.TryParseInfo(line);
            Assert.NotNull(upd);
        }
    }
}
