using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Core
{
    public static class ProfileService
    {
        public static List<Profile> LoadProfiles(string path)
        {
            if (!File.Exists(path)) return new List<Profile>();
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
            }
            catch { return new List<Profile>(); }
        }
    }
}
