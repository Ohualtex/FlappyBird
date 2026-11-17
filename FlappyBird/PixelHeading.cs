using System.Drawing.Drawing2D;

namespace FlappyBird;

internal static class PixelHeading
{
    // Each bit represents one pixel in a five-column row.
    private static readonly Dictionary<char, byte[]> Glyphs = new()
    {
        ['A'] = [14, 17, 17, 31, 17, 17, 17],
        ['D'] = [30, 17, 17, 17, 17, 17, 30],
        ['E'] = [31, 16, 16, 30, 16, 16, 31],
        ['G'] = [14, 17, 16, 23, 17, 17, 15],
        ['P'] = [30, 17, 17, 30, 16, 16, 16],
        ['S'] = [15, 16, 16, 14, 1, 1, 30],
        ['Y'] = [17, 17, 10, 4, 4, 4, 4],
        ['M'] = [17, 27, 31, 21, 17, 17, 17],
        ['O'] = [14, 17, 17, 17, 17, 17, 14],
        ['R'] = [30, 17, 17, 30, 20, 18, 17],
        ['T'] = [31, 4, 4, 4, 4, 4, 4],
        ['U'] = [17, 17, 17, 17, 17, 17, 14],
        ['V'] = [17, 17, 17, 17, 17, 10, 4],
        [' '] = []
    };

    public static void Draw(Graphics graphics, string text, int top)
    {
        using var ink = new SolidBrush(ArcadeRenderer.Ink);
        using var cream = new SolidBrush(ArcadeRenderer.Cream);
        using var orange = new SolidBrush(Color.FromArgb(255, 133, 25));
        SmoothingMode smoothing = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.None;
        DrawLayer(graphics, text, top, cream, 4);
        DrawLayer(graphics, text, top, ink, 2);
        DrawLayer(graphics, text, top, orange, 0);
        graphics.SmoothingMode = smoothing;
    }

    private static void DrawLayer(Graphics graphics, string text, int top, Brush brush, int outline)
    {
        const int pixel = 5;
        const int advance = 6;
        int left = (GameState.Width - (text.Length * advance - 1) * pixel) / 2;

        foreach (char letter in text)
        {
            byte[] glyph = Glyphs[letter];
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
