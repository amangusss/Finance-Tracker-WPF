using System.IO;
using System.Text.Json;
using Serilog;

namespace Finance_Tracker_WPF_API.Core.Configuration;

public class AppSettings
{
    private const string SettingsFileName = "appsettings.json";
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FinanceTracker",
        SettingsFileName);

    public string ApiKey { get; set; } = string.Empty;

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load AppSettings from {SettingsPath}", SettingsPath);
        }

        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save AppSettings to {SettingsPath}", SettingsPath);
        }
    }
} 