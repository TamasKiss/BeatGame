using BeatGame.Core;
using BeatGame.Models;
using BeatGame.Rendering;
using Raylib_cs;

namespace BeatGame.Screens;

public sealed class SelectionScreen : Screen
{
    private readonly Func<Song?> _songLoader;
    private readonly Action<Song> _onSongSelected;
    private string? _loadError;

    public SelectionScreen(Func<Song?> songLoader, Action<Song> onSongSelected)
    {
        _songLoader = songLoader;
        _onSongSelected = onSongSelected;
    }

    public override void OnEnter()
    {
        _loadError = null;
    }

    public override void Update(float deltaTime) { }

    public override void Draw()
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        UIRenderer.DrawCenteredText("Select a Song", screenWidth / 2, 80, 48, UIRenderer.TextLight);

        // Single song entry
        int songWidth = 480;
        int songHeight = 80;
        Rectangle songRect = new((screenWidth - songWidth) / 2, 220, songWidth, songHeight);
        if (UIRenderer.DrawButton(songRect, "First song", 28))
        {
            try
            {
                Song? song = _songLoader();
                if (song is not null)
                {
                    _onSongSelected(song);
                    Manager.Transition(GameState.PlayCountdown);
                }
                else
                {
                    _loadError = "Failed to load song.";
                }
            }
            catch (Exception ex)
            {
                _loadError = $"Could not load song: {ex.Message}";
            }
        }

        if (_loadError is not null)
        {
            UIRenderer.DrawCenteredText(_loadError, screenWidth / 2, 340, 18, UIRenderer.Accent);
        }

        // Back button
        Rectangle backRect = new(40, screenHeight - 88, 160, 48);
        if (UIRenderer.DrawButton(backRect, "Back", 22))
        {
            Manager.Transition(GameState.Menu);
        }
    }
}
