using BeatGame.Audio;
using BeatGame.Core;
using BeatGame.Rendering;
using Raylib_cs;

namespace BeatGame.Screens;

public sealed class MenuScreen : Screen
{
    private const int ButtonWidth = 320;
    private const int ButtonHeight = 64;
    private const int ButtonSpacing = 20;

    private readonly AudioManager? _audio;

    public MenuScreen(AudioManager? audio = null)
    {
        _audio = audio;
    }

    public override void Update(float deltaTime)
    {
        // Input + drawing both happen in Draw() because UIRenderer.DrawButton
        // both renders and detects clicks in a single call.
    }

    public override void Draw()
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        UIRenderer.DrawCenteredText("BeatGame", screenWidth / 2, screenHeight / 4, 72, UIRenderer.TextLight);
        UIRenderer.DrawCenteredText("A rhythm game", screenWidth / 2, screenHeight / 4 + 80, 22, UIRenderer.TextDim);

        int buttonX = (screenWidth - ButtonWidth) / 2;
        int firstButtonY = screenHeight / 2 - (ButtonHeight + ButtonSpacing);

        Rectangle playRect = new(buttonX, firstButtonY, ButtonWidth, ButtonHeight);
        Rectangle settingsRect = new(buttonX, firstButtonY + ButtonHeight + ButtonSpacing, ButtonWidth, ButtonHeight);
        Rectangle quitRect = new(buttonX, firstButtonY + 2 * (ButtonHeight + ButtonSpacing), ButtonWidth, ButtonHeight);

        if (UIRenderer.DrawButton(playRect, "Play")) Manager.Transition(GameState.Selection);
        if (UIRenderer.DrawButton(settingsRect, "Settings")) Manager.Transition(GameState.Settings);
        if (UIRenderer.DrawButton(quitRect, "Quit")) Manager.Transition(GameState.Exit);

        if (_audio is not null && !_audio.DeviceAvailable)
        {
            UIRenderer.DrawCenteredText(
                "Warning: no audio device detected. Gameplay will run without sound.",
                screenWidth / 2, screenHeight - 60, 16, UIRenderer.Accent);
        }
    }
}
