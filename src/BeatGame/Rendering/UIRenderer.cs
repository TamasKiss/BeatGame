using System.Numerics;
using Raylib_cs;

namespace BeatGame.Rendering;

public static class UIRenderer
{
    public static readonly Color Background = new(18, 18, 28, 255);
    public static readonly Color Primary = new(80, 180, 255, 255);
    public static readonly Color PrimaryHover = new(120, 210, 255, 255);
    public static readonly Color Accent = new(255, 100, 150, 255);
    public static readonly Color TextLight = new(240, 240, 245, 255);
    public static readonly Color TextDim = new(160, 160, 175, 255);
    public static readonly Color PanelBg = new(28, 28, 42, 255);

    public static bool DrawButton(Rectangle rect, string text, int fontSize = 28)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        bool hovered = Raylib.CheckCollisionPointRec(mouse, rect);
        bool clicked = hovered && Raylib.IsMouseButtonPressed(MouseButton.Left);

        Color bg = hovered ? PrimaryHover : Primary;
        Raylib.DrawRectangleRec(rect, bg);
        Raylib.DrawRectangleLinesEx(rect, 2, TextLight);

        int textWidth = Raylib.MeasureText(text, fontSize);
        int textX = (int)(rect.X + (rect.Width - textWidth) / 2);
        int textY = (int)(rect.Y + (rect.Height - fontSize) / 2);
        Raylib.DrawText(text, textX, textY, fontSize, TextLight);

        return clicked;
    }

    public static void DrawCenteredText(string text, int centerX, int y, int fontSize, Color color)
    {
        int textWidth = Raylib.MeasureText(text, fontSize);
        Raylib.DrawText(text, centerX - textWidth / 2, y, fontSize, color);
    }

    public static float DrawSlider(Rectangle rect, float value, float min = 0f, float max = 1f)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        bool hovered = Raylib.CheckCollisionPointRec(mouse, rect);
        bool active = hovered && Raylib.IsMouseButtonDown(MouseButton.Left);

        Raylib.DrawRectangleRec(rect, PanelBg);
        Raylib.DrawRectangleLinesEx(rect, 1, TextDim);

        float normalized = (value - min) / (max - min);
        normalized = Math.Clamp(normalized, 0f, 1f);

        Rectangle fill = new(rect.X, rect.Y, rect.Width * normalized, rect.Height);
        Raylib.DrawRectangleRec(fill, Primary);

        if (active)
        {
            float t = (mouse.X - rect.X) / rect.Width;
            t = Math.Clamp(t, 0f, 1f);
            value = min + t * (max - min);
        }

        return value;
    }
}
