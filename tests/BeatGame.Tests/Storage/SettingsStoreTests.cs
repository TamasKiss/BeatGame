using BeatGame.Input;
using BeatGame.Storage;
using Xunit;

namespace BeatGame.Tests.Storage;

public class SettingsStoreTests : IDisposable
{
    private readonly string _tempPath;

    public SettingsStoreTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"beatgame-settings-test-{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempPath))
        {
            try { File.Delete(_tempPath); } catch { /* best effort */ }
        }
    }

    [Fact]
    public void Save_WritesValidJson()
    {
        KeyBindings bindings = new(new[] { 'Q', 'W', 'E', 'R', 'T' });
        SettingsStore.Save(bindings, _tempPath);

        Assert.True(File.Exists(_tempPath));
        string json = File.ReadAllText(_tempPath);
        Assert.Contains("\"Q\"", json);
        Assert.Contains("\"T\"", json);
    }

    [Fact]
    public void Load_RoundTripsBindings()
    {
        KeyBindings original = new(new[] { 'Q', 'W', 'E', 'R', 'T' });
        SettingsStore.Save(original, _tempPath);

        KeyBindings loaded = SettingsStore.Load(_tempPath);

        for (int i = 0; i < KeyBindings.LaneCount; i++)
        {
            Assert.Equal(original.GetKey(i), loaded.GetKey(i));
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        string nonexistent = Path.Combine(Path.GetTempPath(), $"beatgame-missing-{Guid.NewGuid():N}.json");
        KeyBindings loaded = SettingsStore.Load(nonexistent);

        Assert.Equal('A', loaded.GetKey(0));
        Assert.Equal('G', loaded.GetKey(4));
    }

    [Fact]
    public void Load_CorruptFile_ReturnsDefaults()
    {
        File.WriteAllText(_tempPath, "{ this is not valid json !!!");
        KeyBindings loaded = SettingsStore.Load(_tempPath);

        Assert.Equal('A', loaded.GetKey(0));
        Assert.Equal('G', loaded.GetKey(4));
    }

    [Fact]
    public void Load_FileWithWrongShape_ReturnsDefaults()
    {
        File.WriteAllText(_tempPath, "{ \"unrelated\": 42 }");
        KeyBindings loaded = SettingsStore.Load(_tempPath);

        Assert.Equal('A', loaded.GetKey(0));
    }

    [Fact]
    public void Save_CreatesDirectoryIfMissing()
    {
        string nestedPath = Path.Combine(Path.GetTempPath(), $"beatgame-nest-{Guid.NewGuid():N}", "settings.json");
        try
        {
            KeyBindings bindings = new();
            SettingsStore.Save(bindings, nestedPath);
            Assert.True(File.Exists(nestedPath));
        }
        finally
        {
            string? dir = Path.GetDirectoryName(nestedPath);
            if (dir != null && Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }
}
