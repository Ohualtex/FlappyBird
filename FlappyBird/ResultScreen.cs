using System.Drawing.Drawing2D;
using System.Globalization;

namespace FlappyBird;

internal static class ResultScreen
{
    internal static readonly Rectangle RetryBounds = new(154, 325, 172, 46);
    private static readonly Rectangle ScorePanel = new(140, 235, 200, 72);

    public static void Draw(Graphics graphics, int score, int bestScore, Font labelFont, Font hintFont)
    {
        PixelHeading.Draw(graphics, "GAME OVER", 180);
        using var ink = new SolidBrush(ArcadeRenderer.Ink);
        using var cream = new SolidBrush(ArcadeRenderer.Cream);

        SmoothingMode smoothing = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.None;

        Rectangle shadow = ScorePanel;
        shadow.Offset(4, 5);
        graphics.FillRectangle(ink, shadow);
        graphics.FillRectangle(ink, ScorePanel);
        graphics.FillRectangle(cream, Rectangle.Inflate(ScorePanel, -2, -2));
        graphics.SmoothingMode = smoothing;

        DrawScore(graphics, "SKOR", score, ScorePanel.Left + ScorePanel.Width / 4, labelFont);
        DrawScore(graphics, "REKOR", bestScore, ScorePanel.Right - ScorePanel.Width / 4, labelFont);
        var hint = new Rectangle(RetryBounds.X, RetryBounds.Bottom + 4, RetryBounds.Width, 22);
        ArcadeRenderer.DrawCentered(graphics, "Boşluk", hintFont, ArcadeRenderer.Ink, hint);
    }

    private static void DrawScore(Graphics graphics, string label, int score, int center, Font labelFont)
    {
        var labelBounds = new Rectangle(center - 46, ScorePanel.Top + 7, 92, 18);
        ArcadeRenderer.DrawCentered(graphics, label, labelFont, ArcadeRenderer.Ink, labelBounds);
        ScoreDigits.Draw(graphics, score.ToString(CultureInfo.InvariantCulture), center, ScorePanel.Top + 29, 1.5f, 88);
    }
}
