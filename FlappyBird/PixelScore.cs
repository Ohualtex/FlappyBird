using System.Drawing.Drawing2D;

namespace FlappyBird;

internal static class PixelScore
{
    private const int GlyphWidth = 5;
    private const int GlyphHeight = 7;
    private const int GlyphAdvance = GlyphWidth + 1;

    public static void Draw(Graphics graphics, string text, Rectangle bounds, int pixelSize, int outlineWidth)
    {
        int width = (text.Length * GlyphAdvance - 1) * pixelSize;
        int x = bounds.X + (bounds.Width - width) / 2;
        int y = bounds.Y + (bounds.Height - GlyphHeight * pixelSize) / 2;
        var pixels = new List<Rectangle>();
        foreach (char character in text)
        {
            ReadOnlySpan<byte> glyph = GetGlyph(character);
            for (int row = 0; row < glyph.Length; row++)
            {
                for (int column = 0; column < GlyphWidth; column++)
                {
                    if ((glyph[row] & (1 << (GlyphWidth - 1 - column))) != 0)
                    {
                        pixels.Add(new Rectangle(x + column * pixelSize, y + row * pixelSize, pixelSize, pixelSize));
                    }
                }
            }

            x += GlyphAdvance * pixelSize;
        }

        if (pixels.Count == 0)
        {
            return;
        }

        Rectangle[] foreground = pixels.ToArray();
        var outline = new Rectangle[foreground.Length];
        for (int i = 0; i < foreground.Length; i++)
        {
            outline[i] = Rectangle.Inflate(foreground[i], outlineWidth, outlineWidth);
        }

        var state = graphics.Save();
        try
        {
            graphics.SmoothingMode = SmoothingMode.None;
            using var ink = new SolidBrush(ArcadeRenderer.Ink);
            using var fill = new SolidBrush(ArcadeRenderer.Cream);
            graphics.FillRectangles(ink, outline);
            graphics.FillRectangles(fill, foreground);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private static ReadOnlySpan<byte> GetGlyph(char character) => character switch
    {
        '0' => [0b01110, 0b11011, 0b11011, 0b11011, 0b11011, 0b11011, 0b01110],
        '1' => [0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110],
        '2' => [0b01110, 0b11011, 0b00011, 0b00110, 0b01100, 0b11000, 0b11111],
        '3' => [0b11110, 0b00011, 0b00011, 0b01110, 0b00011, 0b00011, 0b11110],
        '4' => [0b11011, 0b11011, 0b11011, 0b11111, 0b00011, 0b00011, 0b00011],
        '5' => [0b11111, 0b11000, 0b11000, 0b11110, 0b00011, 0b00011, 0b11110],
        '6' => [0b01110, 0b11000, 0b11000, 0b11110, 0b11011, 0b11011, 0b01110],
        '7' => [0b11111, 0b00011, 0b00110, 0b00110, 0b01100, 0b01100, 0b01100],
        '8' => [0b01110, 0b11011, 0b11011, 0b01110, 0b11011, 0b11011, 0b01110],
        '9' => [0b01110, 0b11011, 0b11011, 0b01111, 0b00011, 0b00011, 0b01110],
        'R' => [0b11110, 0b11011, 0b11011, 0b11110, 0b11100, 0b11010, 0b11011],
        'E' => [0b11111, 0b11000, 0b11000, 0b11110, 0b11000, 0b11000, 0b11111],
        'K' => [0b11011, 0b11011, 0b11110, 0b11100, 0b11110, 0b11011, 0b11011],
        'O' => [0b01110, 0b11011, 0b11011, 0b11011, 0b11011, 0b11011, 0b01110],
        _ => []
    };
}
