using BeatGame.Audio;
using BeatGame.Core;
using BeatGame.Input;
using BeatGame.Rendering;
using BeatGame.Storage;
using Raylib_cs;

namespace BeatGame.Screens;

public sealed class SettingsScreen : Screen
{
    private const int SlotSize = 72;
    private const int SlotSpacing = 16;

    private readonly KeyBindings _bindings;
    private readonly AudioManager _audio;
    private int? _listeningLane;

    public SettingsScreen(KeyBindings bindings, AudioManager audio)
    {
        _bindings = bindings;
        _audio = audio;
    }

    public override void OnEnter()
    {
        _listeningLane = null;
    }

    public override void Update(float deltaTime)
    {
        if (_listeningLane is not int lane) return;

        // Wait for next letter key press. If non-letter or escape, cancel.
        for (KeyboardKey key = KeyboardKey.A; key <= KeyboardKey.Z; key++)
        {
            if (Raylib.IsKeyPressed(key))
            {
                char letter = (char)('A' + (int)(key - KeyboardKey.A));
                if (_bindings.SetKey(lane, letter))
                {
                    SettingsStore.Save(_bindings);
                }
                _listeningLane = null;
                return;
            }
        }

        // Cancel on any non-letter key
        if (Raylib.GetKeyPressed() != 0)
        {
            _listeningLane = null;
        }
    }

    public override void Draw()
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        UIRenderer.DrawCenteredText("Settings", screenWidth / 2, 80, 56, UIRenderer.TextLight);

        // Input slots
        UIRenderer.DrawCenteredText("Input Keys", screenWidth / 2, 180, 28, UIRenderer.TextDim);

        int totalWidth = KeyBindings.LaneCount * SlotSize + (KeyBindings.LaneCount - 1) * SlotSpacing;
        int startX = (screenWidth - totalWidth) / 2;
        int slotY = 220;

        for (int i = 0; i < KeyBindings.LaneCount; i++)
        {
            Rectangle slot = new(startX + i * (SlotSize + SlotSpacing), slotY, SlotSize, SlotSize);
            bool isListening = _listeningLane == i;

            Color bg = isListening ? UIRenderer.Accent : UIRenderer.PanelBg;
            Raylib.DrawRectangleRec(slot, bg);
            Raylib.DrawRectangleLinesEx(slot, 2, UIRenderer.TextLight);

            string label = isListening ? "?" : _bindings.GetKey(i).ToString();
            UIRenderer.DrawCenteredText(label, (int)(slot.X + slot.Width / 2), (int)(slot.Y + slot.Height / 2 - 20), 40, UIRenderer.TextLight);

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), slot)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                _listeningLane = i;
            }
        }

        UIRenderer.DrawCenteredText(
            _listeningLane.HasValue ? "Press a letter key (A-Z) to assign, or any other key to cancel" : "Click a slot to remap, then press a letter",
            screenWidth / 2, slotY + SlotSize + 24, 16, UIRenderer.TextDim);

        // Volume slider
        UIRenderer.DrawCenteredText("Volume", screenWidth / 2, 420, 28, UIRenderer.TextDim);
        int sliderWidth = 400;
        Rectangle sliderRect = new((screenWidth - sliderWidth) / 2, 460, sliderWidth, 28);
        float newVolume = UIRenderer.DrawSlider(sliderRect, _audio.Volume);
        if (Math.Abs(newVolume - _audio.Volume) > 0.001f) _audio.Volume = newVolume;

        UIRenderer.DrawCenteredText($"{(int)(_audio.Volume * 100)}%", screenWidth / 2, 500, 18, UIRenderer.TextLight);

        // Back button
        Rectangle backRect = new(40, screenHeight - 88, 160, 48);
        if (UIRenderer.DrawButton(backRect, "Back", 22))
        {
            Manager.Transition(GameState.Menu);
        }
    }
}
