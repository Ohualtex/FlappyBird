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

        DrawBird(graphics, game);
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
            var notice = new Rectangle(323, 16, 141, 42);
            using var background = new SolidBrush(Ink);
            graphics.FillRectangle(background, notice);
            DrawCentered(graphics, scoreMessage, hintFont, Cream, notice);
        }
    }

    public static void DrawMenuGear(Graphics graphics, Rectangle bounds)
    {
        float size = Math.Min(bounds.Width, bounds.Height);
        float centerX = bounds.Left + bounds.Width / 2f;
        float centerY = bounds.Top + bounds.Height / 2f;
        const int teeth = 8;
        var points = new PointF[teeth * 4];
        for (int i = 0; i < points.Length; i++)
        {
            double angle = 2 * Math.PI * i / points.Length;
            float radius = size * (i % 4 is 1 or 2 ? 0.34f : 0.25f);
            points[i] = new PointF(centerX + radius * (float)Math.Cos(angle),
                centerY + radius * (float)Math.Sin(angle));
        }

        var state = graphics.Save();
        try
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var shape = new GraphicsPath(FillMode.Alternate);
            shape.AddPolygon(points);
            float hole = size * 0.20f;
            shape.AddEllipse(centerX - hole / 2, centerY - hole / 2, hole, hole);
            using var fill = new SolidBrush(Gold);
            graphics.FillPath(fill, shape);
        }
        finally
        {
            graphics.Restore(state);
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

    private static void DrawBird(Graphics graphics, GameState game)
    {
        const int center = GameState.BirdSize / 2;
        float angle = (float)Math.Clamp(game.BirdVerticalSpeed * 3, -24, 30);
        GraphicsState transform = graphics.Save();
        try
        {
            graphics.TranslateTransform(GameState.BirdX + center, (float)game.BirdY + center);
            graphics.RotateTransform(angle);

            using var outline = new SolidBrush(Ink);
            using var body = new SolidBrush(Gold);
            using var wing = new SolidBrush(Color.FromArgb(238, 143, 39));
            using var beak = new SolidBrush(Color.FromArgb(245, 108, 56));
            graphics.FillEllipse(outline, -14, -13, 26, 26);
            graphics.FillEllipse(body, -12, -11, 22, 22);
            graphics.FillEllipse(wing, -11, game.IsBirdWingRaised ? -7 : -1, 12, 8);
            graphics.FillPolygon(beak, [new Point(6, -1), new Point(13, 2), new Point(8, 6)]);
            graphics.FillEllipse(Brushes.White, 1, -8, 8, 8);
            graphics.FillEllipse(outline, 5, -6, 3, 4);
        }
        finally
        {
            graphics.Restore(transform);
        }
    }

    private static void DrawGround(Graphics graphics, Rectangle bounds)
    {
        int top = GameState.GroundY;
        var ground = new Rectangle(0, top, bounds.Width, bounds.Height - top);
        using var soil = new LinearGradientBrush(ground, Color.FromArgb(192, 127, 69),
            Color.FromArgb(133, 77, 50), LinearGradientMode.Vertical);
        using var shade = new SolidBrush(Color.FromArgb(127, 74, 47));
        using var gravel = new SolidBrush(Color.FromArgb(229, 163, 99));
        using var grass = new SolidBrush(Color.FromArgb(112, 166, 66));
        using var grassLight = new SolidBrush(Color.FromArgb(179, 213, 97));

        graphics.FillRectangle(soil, ground);
        for (int x = 0; x < bounds.Width; x += 31)
        {
            graphics.FillEllipse(shade, x + 4, top + 22, 8, 4);
            graphics.FillEllipse(gravel, x + 17, top + 35, 5, 3);
            graphics.FillEllipse(gravel, x + 25, top + 18, 3, 3);
            graphics.FillEllipse(shade, x + 24, top + 42, 4, 2);
        }

        graphics.FillRectangle(grass, 0, top, bounds.Width, 11);
        for (int x = 0; x < bounds.Width; x += 18)
        {
            graphics.FillEllipse(grass, x, top + 7, 14, 6);
        }

        graphics.FillRectangle(grassLight, 0, top, bounds.Width, 3);
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
        using var textBrush = new SolidBrush(color);
        using var alignment = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        graphics.DrawString(text, font, textBrush, bounds, alignment);
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
