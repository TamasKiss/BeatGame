using System.Text.Json;

namespace BeatGame.Storage;

public static class ScoreStore
{
    private const string FolderName = "BeatGame";
    private const string FileName   = "scores.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static string GetScoresPath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, FolderName, FileName);
    }

    public static int LoadHighScore() => LoadHighScore(GetScoresPath());

    public static int LoadHighScore(string path)
    {
        if (!File.Exists(path)) return 0;
        try
        {
            string json = File.ReadAllText(path);
            ScoreDto? dto = JsonSerializer.Deserialize<ScoreDto>(json, JsonOptions);
            return Math.Max(0, dto?.HighScore ?? 0);
        }
        catch { return 0; }
    }

    /// <summary>
    /// Saves <paramref name="score"/> only if it exceeds the current stored high score.
    /// </summary>
    public static void SaveIfHighScore(int score) => SaveIfHighScore(score, GetScoresPath());

    public static void SaveIfHighScore(int score, string path)
    {
        if (score <= LoadHighScore(path)) return;  // not a new record

        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, JsonSerializer.Serialize(new ScoreDto { HighScore = score }, JsonOptions));
    }

    private sealed class ScoreDto
    {
        public int HighScore { get; set; }
    }
}
