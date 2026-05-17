using System.Text.Json;
using BeatGame.Input;

namespace BeatGame.Storage;

public static class SettingsStore
{
    private const string FolderName = "BeatGame";
    private const string FileName = "settings.json";

    public static string GetSettingsPath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, FolderName, FileName);
    }

    public static KeyBindings Load() => Load(GetSettingsPath());

    public static KeyBindings Load(string path)
    {
        if (!File.Exists(path)) return new KeyBindings();

        try
        {
            string json = File.ReadAllText(path);
            SettingsDto? dto = JsonSerializer.Deserialize<SettingsDto>(json, JsonHelper.WriteOptions);
            if (dto?.KeyBindings is null || dto.KeyBindings.Length != KeyBindings.LaneCount)
            {
                return new KeyBindings();
            }

            char[] chars = new char[KeyBindings.LaneCount];
            for (int i = 0; i < KeyBindings.LaneCount; i++)
            {
                string s = dto.KeyBindings[i];
                if (string.IsNullOrEmpty(s) || !KeyBindings.IsValidKey(s[0]))
                {
                    return new KeyBindings();
                }
                chars[i] = char.ToUpperInvariant(s[0]);
            }
            return new KeyBindings(chars);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[SettingsStore] Failed to load settings from '{path}': {ex.Message}");
            return new KeyBindings();
        }
    }

    public static void Save(KeyBindings bindings) => Save(bindings, GetSettingsPath());

    public static void Save(KeyBindings bindings, string path)
    {
        ArgumentNullException.ThrowIfNull(bindings);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        char[] chars = bindings.ToArray();
        string[] strings = new string[chars.Length];
        for (int i = 0; i < chars.Length; i++) strings[i] = chars[i].ToString();

        SettingsDto dto = new() { KeyBindings = strings };
        string json = JsonSerializer.Serialize(dto, JsonHelper.WriteOptions);
        File.WriteAllText(path, json);
    }

    private sealed class SettingsDto
    {
        public string[]? KeyBindings { get; set; }
    }
}
