using System.Drawing.Drawing2D;

namespace FlappyBird;

internal static class ScoreDigits
{
    private const int DigitHeight = 24;
    private const int Top = 26;
    private const float ScoreScale = 1.5f;

    public static void Draw(Graphics graphics, string digits) =>
        Draw(graphics, digits, GameState.Width / 2, Top, ScoreScale, GameState.Width);

    public static void Draw(Graphics graphics, string digits, float centerX, int top, float scale, int maxWidth)
    {
        int digitWidth = digits.Sum(DigitWidth);
        scale = Math.Min(scale, (float)maxWidth / (digitWidth + 1));
        float width = digitWidth * scale;
        int x = 0;
        var state = graphics.Save();
        try
        {
            graphics.TranslateTransform(centerX - width / 2, top);
            graphics.ScaleTransform(scale, scale);
            graphics.SmoothingMode = SmoothingMode.None;
            using var shadow = new SolidBrush(Color.FromArgb(28, 63, 67));
            foreach (char digit in digits)
            {
                DrawSilhouette(graphics, shadow, digit, x + 1, 1);
                DrawSilhouette(graphics, Brushes.Black, digit, x, 0);
                DrawFace(graphics, digit, x);
                x += DigitWidth(digit);
            }
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private static int DigitWidth(char digit) => digit == '1' ? 11 : 16;

    private static void DrawSilhouette(Graphics graphics, Brush brush, char digit, int x, int y)
    {
        switch (digit)
        {
            case '1':
                graphics.FillRectangle(brush, x, y, 11, 10);
                graphics.FillRectangle(brush, x + 2, y + 10, 9, DigitHeight - 10);
                break;
            case '4':
                graphics.FillRectangle(brush, x, y, 16, 17);
                graphics.FillRectangle(brush, x + 8, y + 17, 8, DigitHeight - 17);
                break;
            case '7':
                graphics.FillRectangle(brush, x, y, 16, 10);
                graphics.FillRectangle(brush, x + 8, y + 10, 8, DigitHeight - 10);
                break;
            default:
                graphics.FillRectangle(brush, x, y, DigitWidth(digit), DigitHeight);
                break;
        }
    }

    private static void DrawFace(Graphics graphics, char digit, int x)
    {
        void White(int left, int top, int width, int height) =>
            graphics.FillRectangle(Brushes.White, x + left, top, width, height);

        void Black(int left, int top, int width, int height) =>
            graphics.FillRectangle(Brushes.Black, x + left, top, width, height);

        switch (digit)
        {
            case '1':
                White(2, 2, 7, 6);
                White(4, 8, 5, 14);
                return;
            case '4':
                White(2, 2, 12, 13);
                White(10, 15, 4, 7);
                Black(6, 2, 2, 8);
                return;
            case '7':
                White(2, 2, 12, 6);
                White(10, 8, 4, 14);
                return;
        }

        White(2, 2, 12, 20);
        switch (digit)
        {
            case '0':
                Black(7, 7, 2, 10);
                break;
            case '2':
                Black(2, 7, 8, 3);
                Black(7, 14, 7, 2);
                break;
            case '3':
                Black(2, 7, 8, 3);
                Black(2, 14, 8, 3);
                break;
            case '5':
                Black(7, 7, 7, 3);
                Black(2, 14, 8, 3);
                break;
            case '6':
                Black(7, 7, 7, 3);
                Black(7, 15, 2, 2);
                break;
            case '8':
                Black(7, 7, 2, 3);
                Black(7, 14, 2, 3);
                break;
            case '9':
                Black(7, 7, 2, 2);
                Black(2, 15, 7, 2);
                break;
        }
    }
}
