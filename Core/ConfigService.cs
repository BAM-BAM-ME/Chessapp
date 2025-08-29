using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Core
{
    public class AppSettings
    {
        public string EnginePath { get; set; } = "Engines/stockfish.exe";
        public int Threads { get; set; } = Environment.ProcessorCount - 1 > 0 ? Environment.ProcessorCount - 1 : 1;
        public int Hash { get; set; } = 1024;
        public bool Ponder { get; set; } = true;
        public int MultiPV { get; set; } = 1;
        public string SyzygyPath { get; set; } = "";
        public string ProfilesPath { get; set; } = "Data/profiles.json";
        public string LearnDb { get; set; } = "Data/learn.db";
    }

    public class Profile
    {
        public string Name { get; set; } = "Default";
        public Dictionary<string, object> Uci { get; set; } = new();
        public MovePolicy MovePolicy { get; set; } = new();
        public BookConfig Book { get; set; } = new();
    }

    public class MovePolicy
    {
        public bool Deterministic { get; set; } = true;
        public int TopK { get; set; } = 1;
    }

    public class BookConfig
    {
        public string Path { get; set; } = "";
        public List<string> Allow { get; set; } = new();
    }

    public static class ConfigService
    {
        private static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "Data", "appsettings.json");

        public static AppSettings LoadAppSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }
            return new AppSettings();
        }

        public static void SaveAppSettings(AppSettings settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
    }
}
