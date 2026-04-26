using System;
using System.IO;
using System.Text.Json;

namespace XstReader
{
    internal sealed class AppSettings
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private static readonly string SettingsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XstReader");

        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

        public string LastFolder { get; set; } = string.Empty;
        public string LastAttachmentFolder { get; set; } = string.Empty;
        public string LastExportFolder { get; set; } = string.Empty;
        public double Height { get; set; }
        public double Width { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    return new AppSettings();

                string json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(SettingsDirectory);
            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(this, JsonOptions));
        }
    }
}
