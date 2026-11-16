using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace FlappyBird;

internal sealed class ArcadeRenderer : Component
{
    internal static readonly Color Ink = Color.FromArgb(23, 48, 65);
    internal static readonly Color Cream = Color.FromArgb(255, 248, 216);
    internal static readonly Color Gold = Color.FromArgb(255, 201, 66);

    private readonly Font titleFont = new(SystemFonts.MessageBoxFont!.FontFamily, 30, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font scoreFont = new(SystemFonts.MessageBoxFont!.FontFamily, 38, FontStyle.Bold, GraphicsUnit.Pixel);
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
        DrawGround(graphics, bounds, game.DistanceTraveled);
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

    private static void DrawGround(Graphics graphics, Rectangle bounds, long distanceTraveled)
    {
        const int tileWidth = 28;
        int top = GameState.GroundY;
        using var sand = new SolidBrush(Color.FromArgb(229, 206, 148));
        using var edge = new SolidBrush(Ink);
        using var grass = new SolidBrush(Color.FromArgb(169, 207, 69));
        using var grassShadow = new SolidBrush(Color.FromArgb(104, 140, 57));
        using var stripe = new SolidBrush(Color.FromArgb(201, 225, 97));

        graphics.FillRectangle(sand, 0, top, bounds.Width, bounds.Height - top);
        graphics.FillRectangle(edge, 0, top, bounds.Width, 4);
        graphics.FillRectangle(grass, 0, top + 4, bounds.Width, 14);
        graphics.FillRectangle(grassShadow, 0, top + 18, bounds.Width, 4);

        int offset = (int)(distanceTraveled % tileWidth);
        for (int x = -offset; x < bounds.Width; x += tileWidth)
        {
            graphics.FillRectangle(stripe, x + 16, top + 6, 10, 4);
            graphics.FillRectangle(stripe, x + 12, top + 10, 10, 4);
            graphics.FillRectangle(stripe, x + 8, top + 14, 10, 4);
        }
    }

    private void DrawScore(Graphics graphics, int score, int bestScore)
    {
        DrawOutlinedText(graphics, score.ToString(), scoreFont, 5, new Rectangle(150, 10, 180, 42));
        DrawOutlinedText(graphics, $"Rekor {bestScore}", labelFont, 3, new Rectangle(150, 53, 180, 22));
    }

    private static void DrawOutlinedText(Graphics graphics, string text, Font font, float outlineWidth, Rectangle bounds)
    {
        using var glyphs = new GraphicsPath();
        using var format = StringFormat.GenericTypographic;
        glyphs.AddString(text, font.FontFamily, (int)font.Style, font.Size, PointF.Empty, format);
        RectangleF glyphBounds = glyphs.GetBounds();
        using var position = new Matrix();
        position.Translate(bounds.X + (bounds.Width - glyphBounds.Width) / 2 - glyphBounds.X,
            bounds.Y + (bounds.Height - glyphBounds.Height) / 2 - glyphBounds.Y);
        glyphs.Transform(position);

        using var outline = new Pen(Ink, outlineWidth) { LineJoin = LineJoin.Round };
        using var fill = new SolidBrush(Cream);
        graphics.DrawPath(outline, glyphs);
        graphics.FillPath(fill, glyphs);
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
        string action = gameOver ? "Yeniden dene: Tıkla / Boşluk" : "Başla: Tıkla / Boşluk";
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
