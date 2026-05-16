using System.Text.Json;

namespace BeatGame.Models;

public sealed class Song
{
    public string Title { get; }
    public string AudioFilePath { get; }
    public BeatMap BeatMap { get; }
    public int Bpm { get; }

    public Song(string title, string audioFilePath, BeatMap beatMap, int bpm)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(audioFilePath)) throw new ArgumentException("AudioFilePath cannot be empty.", nameof(audioFilePath));
        ArgumentNullException.ThrowIfNull(beatMap);

        Title = title;
        AudioFilePath = audioFilePath;
        BeatMap = beatMap;
        Bpm = bpm;
    }

    public static Song LoadFromContent(string contentDirectory, string jsonFileName)
    {
        string jsonPath = Path.Combine(contentDirectory, jsonFileName);
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Beat map file not found: {jsonPath}", jsonPath);
        }

        string json = File.ReadAllText(jsonPath);
        BeatMapDto? dto = JsonSerializer.Deserialize<BeatMapDto>(json, JsonOptions);
        if (dto is null)
        {
            throw new InvalidDataException($"Beat map JSON could not be parsed: {jsonPath}");
        }
        if (dto.Beats is null || dto.Beats.Length == 0)
        {
            throw new InvalidDataException($"Beat map contains no beats: {jsonPath}");
        }

        Beat[] beats = new Beat[dto.Beats.Length];
        for (int i = 0; i < dto.Beats.Length; i++)
        {
            beats[i] = new Beat(dto.Beats[i].TimestampMs, dto.Beats[i].Lane);
        }

        BeatMap beatMap = new(beats);
        string audioPath = Path.Combine(contentDirectory, dto.AudioFile ?? throw new InvalidDataException("audioFile missing in beat map JSON."));
        return new Song(dto.Title ?? "Untitled", audioPath, beatMap, dto.Bpm);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private sealed class BeatMapDto
    {
        public string? Title { get; set; }
        public string? AudioFile { get; set; }
        public int Bpm { get; set; }
        public BeatDto[]? Beats { get; set; }
    }

    private sealed class BeatDto
    {
        public int TimestampMs { get; set; }
        public int Lane { get; set; }
    }
}
