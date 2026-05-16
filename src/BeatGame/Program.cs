using BeatGame.Audio;
using BeatGame.Core;
using BeatGame.Input;
using BeatGame.Models;
using BeatGame.Rendering;
using BeatGame.Screens;
using BeatGame.Storage;
using Raylib_cs;

const int WindowWidth = 1280;
const int WindowHeight = 720;
const int TargetFps = 60;

Raylib.InitWindow(WindowWidth, WindowHeight, "BeatGame");
Raylib.SetTargetFPS(TargetFps);
Raylib.SetExitKey(KeyboardKey.Null); // disable the default ESC-to-quit; we handle Quit via menu

// Settings: key bindings loaded from %APPDATA%/BeatGame/settings.json
KeyBindings bindings = SettingsStore.Load();

// Audio
using AudioManager audio = new();
audio.Initialize();

// Active song (set by Selection screen, consumed by Play screen)
Song? activeSong = null;

string contentDir = Path.Combine(AppContext.BaseDirectory, "Content");

// Wire up screens
GameStateManager manager = new();
manager.RegisterScreen(GameState.Menu, new MenuScreen(audio));
manager.RegisterScreen(GameState.Settings, new SettingsScreen(bindings, audio));
manager.RegisterScreen(GameState.Selection, new SelectionScreen(
    songLoader: () =>
    {
        try { return Song.LoadFromContent(contentDir, "first-song.json"); }
        catch { return null; }
    },
    onSongSelected: song => activeSong = song));

PlayScreen playScreen = new(bindings, audio, () => activeSong);
manager.RegisterScreen(GameState.PlayCountdown, playScreen);

manager.SetInitialState(GameState.Menu);

// Main loop
while (!Raylib.WindowShouldClose() && !manager.ExitRequested)
{
    float deltaTime = Raylib.GetFrameTime();
    manager.Update(deltaTime);

    Raylib.BeginDrawing();
    Raylib.ClearBackground(UIRenderer.Background);
    manager.Draw();
    Raylib.EndDrawing();
}

audio.Dispose();
Raylib.CloseWindow();
