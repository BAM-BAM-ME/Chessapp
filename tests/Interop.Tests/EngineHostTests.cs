using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Interop;
using Xunit;

namespace Interop.Tests
{
    public class EngineHostTests
    {
        [Fact]
        public async Task StartHandshakeAndDisposeStopsProcess()
        {
            if (OperatingSystem.IsWindows())
            {
                // scripting via bash is unavailable; skip on Windows.
                return;
            }

            var script = "#!/usr/bin/env bash\n" +
                         "while read line; do\n" +
                         "  if [[ \"$line\" == \"uci\" ]]; then echo uciok; fi\n" +
                         "  if [[ \"$line\" == \"quit\" ]]; then break; fi\n" +
                         "done\n";

            var path = Path.Combine(Path.GetTempPath(), $"fake_engine_{Guid.NewGuid():N}.sh");
            await File.WriteAllTextAsync(path, script);
            Process.Start("chmod", $"+x {path}")!.WaitForExit();

            await using var host = new EngineHost();
            host.Start(path);
            await host.UciHandshakeAsync(TimeSpan.FromSeconds(1));

            await host.DisposeAsync();
            Assert.False(host.IsRunning);
        }
    }
}
