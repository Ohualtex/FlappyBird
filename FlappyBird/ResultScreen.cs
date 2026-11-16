using System.Drawing.Drawing2D;
using System.Globalization;

namespace FlappyBird;

internal static class ResultScreen
{
    internal static readonly Rectangle RetryBounds = new(154, 325, 172, 46);
    private static readonly Rectangle ScorePanel = new(140, 235, 200, 72);

    // Each bit represents one pixel in a five-column row of the heading.
    private static readonly byte[][] TitleGlyphs =
    [
        [14, 17, 16, 23, 17, 17, 15], // G
        [14, 17, 17, 31, 17, 17, 17], // A
        [17, 27, 31, 21, 17, 17, 17], // M
        [31, 16, 16, 30, 16, 16, 31], // E
        [],                           // space
        [14, 17, 17, 17, 17, 17, 14], // O
        [17, 17, 17, 17, 17, 10, 4],  // V
        [31, 16, 16, 30, 16, 16, 31], // E
        [30, 17, 17, 30, 20, 18, 17]  // R
    ];

    public static void Draw(Graphics graphics, int score, int bestScore, Font labelFont, Font hintFont)
    {
        using var ink = new SolidBrush(ArcadeRenderer.Ink);
        using var cream = new SolidBrush(ArcadeRenderer.Cream);
        using var orange = new SolidBrush(Color.FromArgb(255, 133, 25));
        SmoothingMode smoothing = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.None;

        DrawTitle(graphics, cream, 4);
        DrawTitle(graphics, ink, 2);
        DrawTitle(graphics, orange, 0);
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

    private static void DrawTitle(Graphics graphics, Brush brush, int outline)
    {
        const int pixel = 5;
        const int advance = 6;
        const int top = 180;
        int left = (GameState.Width - (TitleGlyphs.Length * advance - 1) * pixel) / 2;

        foreach (byte[] glyph in TitleGlyphs)
        {
            for (int row = 0; row < glyph.Length; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    if ((glyph[row] & (16 >> column)) == 0)
                    {
                        continue;
                    }

                    var cell = new Rectangle(left + column * pixel, top + row * pixel, pixel, pixel);
                    graphics.FillRectangle(brush, Rectangle.Inflate(cell, outline, outline));
                }
            }

            left += advance * pixel;
        }
    }
}
