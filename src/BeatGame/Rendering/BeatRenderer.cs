using BeatGame.Input;
using BeatGame.Models;
using Raylib_cs;

namespace BeatGame.Rendering;

public static class BeatRenderer
{
    public const int MaxVisibleBeats = 10;
    public const float TravelTimeMs = 1500f; // time it takes a beat to travel from top to hit zone

    private static readonly Color[] LaneColors =
    {
        new(255, 100, 100, 255),
        new(255, 200, 100, 255),
        new(100, 255, 150, 255),
        new(100, 200, 255, 255),
        new(200, 120, 255, 255),
    };

    public static void DrawLanes(int screenWidth, int screenHeight, KeyBindings bindings, float laneFadeAlpha = 1.0f)
    {
        (int laneX, int laneWidth, int hitZoneY) = ComputeLaneGeometry(screenWidth, screenHeight);
        int laneTop = 80;
        int laneBottom = hitZoneY + 60;

        for (int i = 0; i < KeyBindings.LaneCount; i++)
        {
            int x = laneX + i * laneWidth;
            Rectangle laneRect = new(x + 4, laneTop, laneWidth - 8, laneBottom - laneTop);
            Color laneBg = LaneColors[i];
            laneBg.A = (byte)(40 * laneFadeAlpha);
            Raylib.DrawRectangleRec(laneRect, laneBg);
        }

        // Hit zone bar
        Color hitZoneColor = UIRenderer.TextLight;
        hitZoneColor.A = (byte)(180 * laneFadeAlpha);
        Raylib.DrawRectangle(laneX, hitZoneY, laneWidth * KeyBindings.LaneCount, 4, hitZoneColor);

        // Per-lane indicators with bound key
        for (int i = 0; i < KeyBindings.LaneCount; i++)
        {
            int x = laneX + i * laneWidth;
            Color color = LaneColors[i];
            color.A = (byte)(255 * laneFadeAlpha);
            Raylib.DrawRectangle(x + 8, hitZoneY + 8, laneWidth - 16, 36, color);

            int textY = hitZoneY + 12;
            string keyLabel = bindings.GetKey(i).ToString();
            int textWidth = Raylib.MeasureText(keyLabel, 28);
            int textX = x + (laneWidth - textWidth) / 2;
            Color textColor = new(20, 20, 30, (byte)(255 * laneFadeAlpha));
            Raylib.DrawText(keyLabel, textX, textY, 28, textColor);
        }
    }

    public static void DrawUpcomingBeats(int screenWidth, int screenHeight, BeatMap map, int nextBeatIndex, double currentTimeMs)
    {
        (int laneX, int laneWidth, int hitZoneY) = ComputeLaneGeometry(screenWidth, screenHeight);
        int laneTop = 80;

        int end = Math.Min(nextBeatIndex + MaxVisibleBeats, map.Beats.Length);
        for (int i = nextBeatIndex; i < end; i++)
        {
            Beat beat = map.Beats[i];
            double offsetMs = beat.TimestampMs - currentTimeMs;
            if (offsetMs > TravelTimeMs) continue; // still off-screen above
            if (offsetMs < -200) continue;          // already past hit zone

            float normalized = 1.0f - (float)(offsetMs / TravelTimeMs);
            normalized = Math.Clamp(normalized, 0f, 1.5f);
            int y = laneTop + (int)((hitZoneY - laneTop) * normalized);

            int x = laneX + beat.Lane * laneWidth;
            Rectangle rect = new(x + 12, y - 12, laneWidth - 24, 24);
            Raylib.DrawRectangleRec(rect, LaneColors[beat.Lane]);
            Raylib.DrawRectangleLinesEx(rect, 2, UIRenderer.TextLight);
        }
    }

    private static (int laneX, int laneWidth, int hitZoneY) ComputeLaneGeometry(int screenWidth, int screenHeight)
    {
        int totalLaneWidth = Math.Min(640, screenWidth - 80);
        int laneWidth = totalLaneWidth / KeyBindings.LaneCount;
        int laneX = (screenWidth - laneWidth * KeyBindings.LaneCount) / 2;
        int hitZoneY = screenHeight - 140;
        return (laneX, laneWidth, hitZoneY);
    }
}
