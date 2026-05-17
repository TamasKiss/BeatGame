using Raylib_cs;

namespace BeatGame.Rendering;

public static class AnimationHelper
{
    /// <summary>
    /// Draws "3, 2, 1, GO!" countdown centered. countdownElapsedSec ranges 0..3.
    /// Each digit appears for ~1 second and fades.
    /// </summary>
    public static void DrawCountdown(int screenWidth, int screenHeight, double countdownElapsedSec)
    {
        string text;
        double phaseT;
        if (countdownElapsedSec < 1.0) { text = "3"; phaseT = countdownElapsedSec; }
        else if (countdownElapsedSec < 2.0) { text = "2"; phaseT = countdownElapsedSec - 1.0; }
        else if (countdownElapsedSec < 3.0) { text = "1"; phaseT = countdownElapsedSec - 2.0; }
        else { text = "GO!"; phaseT = Math.Min(countdownElapsedSec - 3.0, 0.5) * 2.0; }

        // Fade: starts opaque, fades to transparent over the second
        float alpha = (float)(1.0 - phaseT);
        alpha = Math.Clamp(alpha, 0f, 1f);

        Color color = UIRenderer.TextLight;
        color.A = (byte)(255 * alpha);

        int fontSize = text == "GO!" ? 120 : 160;
        int textWidth = Raylib.MeasureText(text, fontSize);
        Raylib.DrawText(text, (screenWidth - textWidth) / 2, screenHeight / 2 - fontSize / 2, fontSize, color);
    }

    /// <summary>
    /// 0 at start, 1 after duration. Used for fading-in lanes after countdown.
    /// </summary>
    public static float LinearFadeIn(double elapsedSec, double durationSec)
    {
        if (durationSec <= 0) return 1f;
        return (float)Math.Clamp(elapsedSec / durationSec, 0.0, 1.0);
    }

    /// <summary>
    /// Returns a 1→0 fade value based on time elapsed since a press was recorded.
    /// Returns 0 if never pressed (pressedAtSec &lt; 0) or duration has fully elapsed.
    /// </summary>
    public static float GetFlashAlpha(double pressedAtSec, double currentSec, double durationSec)
    {
        if (pressedAtSec < 0) return 0f;
        double elapsed = currentSec - pressedAtSec;
        if (elapsed >= durationSec) return 0f;
        return (float)(1.0 - elapsed / durationSec);
    }
}
