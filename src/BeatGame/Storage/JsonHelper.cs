using System.Text.Json;

namespace BeatGame.Storage;

internal static class JsonHelper
{
    internal static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
}
