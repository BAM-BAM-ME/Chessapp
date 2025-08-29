using System;
using System.IO;
using Core;
using Xunit;

namespace Core.Tests
{
    public class ConfigServiceTests
    {
        [Fact]
        public void LoadAppSettings_InvalidJson_LogsErrorAndReturnsDefaults()
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            var settingsPath = Path.Combine(dataDir, "appsettings.json");
            File.WriteAllText(settingsPath, "{ invalid json }");

            var writer = new StringWriter();
            var originalErr = Console.Error;
            Console.SetError(writer);
            try
            {
                var cfg = ConfigService.LoadAppSettings();
                Assert.Equal("Engines/stockfish.exe", cfg.EnginePath);
                Assert.Contains("Failed to load app settings", writer.ToString());
            }
            finally
            {
                Console.SetError(originalErr);
                File.Delete(settingsPath);
            }
        }
    }
}

