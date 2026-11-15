using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace FlappyBird;

internal sealed class ArcadeRenderer : Component
{
    internal static readonly Color Ink = Color.FromArgb(23, 48, 65);
    internal static readonly Color Cream = Color.FromArgb(255, 248, 216);
    internal static readonly Color Gold = Color.FromArgb(255, 201, 66);

    private readonly Font titleFont = new(SystemFonts.MessageBoxFont!.FontFamily, 30, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font scoreFont = new(SystemFonts.MessageBoxFont!.FontFamily, 32, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font labelFont = new(SystemFonts.MessageBoxFont!.FontFamily, 14, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font hintFont = new(SystemFonts.MessageBoxFont!.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);

    public void Draw(Graphics graphics, Rectangle bounds, GameState game, bool running, int bestScore, string? scoreMessage)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        DrawSky(graphics, bounds.Width);
        foreach (GameState.Obstacle obstacle in game.Obstacles)
        {
            DrawPipePair(graphics, obstacle);
        }

        DrawBird(graphics, (int)game.BirdY);
        DrawGround(graphics, bounds);
        if (running)
        {
            DrawScore(graphics, game.Score, bestScore);
        }
        else
        {
            DrawMessage(graphics, game.IsGameOver, game.Score, bestScore);
        }

        if (scoreMessage is not null)
        {
            TextRenderer.DrawText(graphics, scoreMessage, hintFont, new Point(18, 608), Cream);
        }
    }

    private static void DrawSky(Graphics graphics, int width)
    {
        var area = new Rectangle(0, 0, width, GameState.GroundY);
        using var sky = new LinearGradientBrush(area, Color.FromArgb(58, 152, 210),
            Color.FromArgb(194, 239, 226), LinearGradientMode.Vertical);
        graphics.FillRectangle(sky, area);

        using var sun = new SolidBrush(Color.FromArgb(225, 255, 239, 168));
        graphics.FillEllipse(sun, 335, 93, 68, 68);

        using var cloud = new SolidBrush(Color.FromArgb(200, 247, 251, 239));
        DrawCloud(graphics, cloud, 38, 139);
        DrawCloud(graphics, cloud, 252, 257);
        DrawCloud(graphics, cloud, 109, 404);

        using var farHill = new SolidBrush(Color.FromArgb(117, 193, 169));
        using var nearHill = new SolidBrush(Color.FromArgb(80, 157, 133));
        graphics.FillEllipse(farHill, -90, 486, 335, 172);
        graphics.FillEllipse(farHill, 171, 491, 435, 170);
        graphics.FillEllipse(nearHill, -62, 535, 390, 127);
        graphics.FillEllipse(nearHill, 246, 524, 320, 130);
    }

    private static void DrawCloud(Graphics graphics, Brush brush, int x, int y)
    {
        graphics.FillEllipse(brush, x, y + 13, 72, 22);
        graphics.FillEllipse(brush, x + 13, y + 3, 27, 25);
        graphics.FillEllipse(brush, x + 30, y, 27, 28);
    }

    private static void DrawPipePair(Graphics graphics, GameState.Obstacle obstacle)
    {
        int gapBottom = obstacle.GapTop + GameState.GapHeight;
        using var outline = new SolidBrush(Ink);
        using var light = new SolidBrush(Color.FromArgb(160, 223, 92));
        using var dark = new SolidBrush(Color.FromArgb(41, 114, 78));
        DrawPipe(0, obstacle.GapTop, obstacle.GapTop - 19);
        DrawPipe(gapBottom, GameState.GroundY - gapBottom, gapBottom);

        void DrawPipe(int top, int height, int rimY)
        {
            int x = obstacle.X;
            int width = GameState.PipeWidth;
            var body = new Rectangle(x, top, width, height);
            using var fill = new LinearGradientBrush(body, Color.FromArgb(142, 213, 78),
                Color.FromArgb(49, 136, 76), LinearGradientMode.Horizontal);
            graphics.FillRectangle(outline, body);
            graphics.FillRectangle(fill, x + 3, top, width - 6, height);
            graphics.FillRectangle(light, x + 8, top, 6, height);
            graphics.FillRectangle(outline, x, rimY, width, 19);
            graphics.FillRectangle(dark, x + 3, rimY + 3, width - 6, 13);
            graphics.FillRectangle(light, x + 5, rimY + 4, width - 10, 3);
        }
    }

    private static void DrawBird(Graphics graphics, int y)
    {
        int x = GameState.BirdX;
        using var outline = new SolidBrush(Ink);
        using var body = new SolidBrush(Gold);
        using var wing = new SolidBrush(Color.FromArgb(238, 143, 39));
        using var beak = new SolidBrush(Color.FromArgb(245, 108, 56));
        graphics.FillEllipse(outline, x, y + 1, 26, 26);
        graphics.FillEllipse(body, x + 2, y + 3, 22, 22);
        graphics.FillEllipse(wing, x + 3, y + 13, 12, 8);
        graphics.FillPolygon(beak, [new Point(x + 20, y + 13), new Point(x + 28, y + 16),
            new Point(x + 22, y + 20)]);
        graphics.FillEllipse(Brushes.White, x + 15, y + 6, 8, 8);
        graphics.FillEllipse(outline, x + 19, y + 8, 3, 4);
    }

    private static void DrawGround(Graphics graphics, Rectangle bounds)
    {
        int top = GameState.GroundY;
        using var soil = new SolidBrush(Ink);
        using var grass = new SolidBrush(Color.FromArgb(144, 210, 107));
        using var shade = new SolidBrush(Color.FromArgb(42, 76, 79));
        graphics.FillRectangle(soil, 0, top, bounds.Width, bounds.Height - top);
        graphics.FillRectangle(grass, 0, top, bounds.Width, 5);
        for (int x = 0; x < bounds.Width; x += 24)
        {
            graphics.FillPolygon(shade, [new Point(x, top + 5), new Point(x + 11, top + 5),
                new Point(x + 5, top + 12), new Point(x, top + 12)]);
        }
    }

    private void DrawScore(Graphics graphics, int score, int bestScore)
    {
        var panel = new Rectangle(170, 15, 140, 66);
        using var background = new SolidBrush(Ink);
        graphics.FillRectangle(background, panel);
        DrawCentered(graphics, score.ToString(), scoreFont, Gold, new Rectangle(170, 18, 140, 38));
        DrawCentered(graphics, $"Rekor {bestScore}", labelFont, Cream, new Rectangle(170, 54, 140, 20));
    }

    private void DrawMessage(Graphics graphics, bool gameOver, int score, int bestScore)
    {
        var panel = new Rectangle(55, 162, 370, 194);
        using var shadow = new SolidBrush(Color.FromArgb(95, Ink));
        using var border = new SolidBrush(Ink);
        using var interior = new SolidBrush(Cream);
        using var accent = new SolidBrush(Gold);
        graphics.FillRectangle(shadow, panel.X + 6, panel.Y + 7, panel.Width, panel.Height);
        graphics.FillRectangle(border, panel);
        graphics.FillRectangle(interior, Rectangle.Inflate(panel, -3, -3));
        graphics.FillRectangle(accent, panel.X + 3, panel.Y + 3, panel.Width - 6, 6);

        string title = gameOver ? "Game Over" : "Flappy Bird";
        string details = gameOver ? $"Skor {score}     Rekor {bestScore}" : $"Rekor {bestScore}";
        string action = gameOver ? "Yeniden denemek için Boşluk" : "Başlamak için Boşluk";
        DrawCentered(graphics, title, titleFont, Ink, new Rectangle(55, 191, 370, 42));
        DrawCentered(graphics, details, labelFont, Ink, new Rectangle(55, 238, 370, 23));
        graphics.FillRectangle(border, 107, 279, 266, 42);
        DrawCentered(graphics, action, hintFont, Cream, new Rectangle(107, 279, 266, 42));
    }

    private static void DrawCentered(Graphics graphics, string text, Font font, Color color, Rectangle bounds)
    {
        TextRenderer.DrawText(graphics, text, font, bounds, color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            titleFont.Dispose();
            scoreFont.Dispose();
            labelFont.Dispose();
            hintFont.Dispose();
        }

        base.Dispose(disposing);
    }
}
