using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace FlappyBird;

internal sealed class ArcadeRenderer : Component
{
    internal static readonly Color Ink = Color.FromArgb(23, 48, 65);
    internal static readonly Color Cream = Color.FromArgb(255, 248, 216);
    internal static readonly Color Gold = Color.FromArgb(255, 201, 66);

    private readonly Bitmap landscape = CreateLandscape();
    private readonly Font titleFont = new(SystemFonts.MessageBoxFont!.FontFamily, 30, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font scoreFont = new(SystemFonts.MessageBoxFont!.FontFamily, 38, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font labelFont = new(SystemFonts.MessageBoxFont!.FontFamily, 14, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font hintFont = new(SystemFonts.MessageBoxFont!.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);

    public void Draw(Graphics graphics, Rectangle bounds, GameState game, bool running, int bestScore, string? scoreMessage)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var backgroundState = graphics.Save();
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.DrawImage(landscape, new Rectangle(0, 0, bounds.Width, GameState.GroundY),
            new Rectangle(Point.Empty, landscape.Size), GraphicsUnit.Pixel);
        graphics.Restore(backgroundState);
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

    private static Bitmap CreateLandscape()
    {
        var bitmap = new Bitmap(GameState.Width, GameState.GroundY);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(109, 198, 208));

        int top = GameState.GroundY - 122;
        using var cloud = new SolidBrush(Color.FromArgb(236, 246, 223));
        using var cloudShade = new SolidBrush(Color.FromArgb(215, 235, 218));
        (int X, int Y, int Width)[] clouds =
        [
            (-28, 12, 98), (64, 27, 102), (140, 3, 112),
            (251, 23, 90), (319, 0, 120), (419, 20, 106)
        ];
        foreach (var (x, y, width) in clouds)
        {
            graphics.FillRectangle(cloud, x, top + y + 18, width, 65);
            graphics.FillRectangle(cloud, x + 12, top + y + 8, width - 24, 52);
            graphics.FillRectangle(cloud, x + 28, top + y, width - 56, 35);
            graphics.FillRectangle(cloudShade, x, top + y + 58, width, 12);
        }

        int skylineBottom = GameState.GroundY - 16;
        using var wall = new SolidBrush(Color.FromArgb(159, 205, 196));
        using var roof = new SolidBrush(Color.FromArgb(192, 229, 213));
        using var window = new SolidBrush(Color.FromArgb(220, 235, 211));
        (int X, int Width, int Height)[] buildings =
        [
            (0, 29, 49), (35, 31, 66), (72, 24, 42), (102, 43, 73),
            (153, 25, 53), (183, 24, 40), (213, 43, 70), (262, 30, 51),
            (297, 28, 66), (332, 37, 44), (375, 23, 61), (405, 36, 49), (446, 34, 64)
        ];
        foreach (var (x, width, height) in buildings)
        {
            int buildingTop = skylineBottom - height;
            graphics.FillRectangle(wall, x, buildingTop, width, height);
            graphics.FillRectangle(roof, x + 4, buildingTop + 4, width - 8, 4);
            for (int row = buildingTop + 12; row < skylineBottom - 7; row += 12)
            {
                for (int column = x + 6; column < x + width - 5; column += 10)
                {
                    graphics.FillRectangle(window, column, row, 4, 6);
                }
            }
        }

        using var shrubs = new SolidBrush(Color.FromArgb(132, 190, 83));
        using var leafLight = new SolidBrush(Color.FromArgb(166, 208, 102));
        graphics.FillRectangle(shrubs, 0, skylineBottom - 2, GameState.Width, 18);
        for (int x = -12; x < GameState.Width; x += 35)
        {
            graphics.FillRectangle(shrubs, x, skylineBottom - 9, 32, 12);
            graphics.FillRectangle(shrubs, x + 6, skylineBottom - 15, 18, 8);
            graphics.FillRectangle(leafLight, x + 5, skylineBottom - 8, 22, 3);
        }

        return bitmap;
    }


    private static void DrawPipePair(Graphics graphics, GameState.Obstacle obstacle)
    {
        const int rimHeight = GameState.PipeRimHeight;
        int gapBottom = obstacle.GapTop + GameState.GapHeight;
        using var outline = new SolidBrush(Color.FromArgb(37, 51, 34));
        using var body = new SolidBrush(Color.FromArgb(120, 189, 53));
        using var light = new SolidBrush(Color.FromArgb(190, 226, 96));
        using var shadow = new SolidBrush(Color.FromArgb(73, 133, 42));
        using var rim = new SolidBrush(Color.FromArgb(147, 207, 62));

        DrawPipe(0, obstacle.GapTop, obstacle.GapTop - rimHeight);
        DrawPipe(gapBottom, GameState.GroundY - gapBottom, gapBottom);

        void DrawPipe(int top, int height, int rimY)
        {
            int x = obstacle.X;
            int width = GameState.PipeWidth;
            int bodyX = x + GameState.PipeBodyInset;
            int bodyWidth = width - GameState.PipeBodyInset * 2;
            graphics.FillRectangle(outline, bodyX, top, bodyWidth, height);
            graphics.FillRectangle(body, bodyX + 4, top, bodyWidth - 8, height);
            graphics.FillRectangle(light, bodyX + 8, top, 8, height);
            graphics.FillRectangle(shadow, bodyX + bodyWidth - 13, top, 9, height);

            graphics.FillRectangle(outline, x, rimY, width, rimHeight);
            graphics.FillRectangle(rim, x + 4, rimY + 4, width - 8, rimHeight - 8);
            graphics.FillRectangle(light, x + 8, rimY + 4, 10, rimHeight - 8);
            graphics.FillRectangle(shadow, x + width - 15, rimY + 4, 11, rimHeight - 8);
            graphics.FillRectangle(light, x + 18, rimY + 4, width - 33, 3);
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
            landscape.Dispose();
            titleFont.Dispose();
            scoreFont.Dispose();
            labelFont.Dispose();
            hintFont.Dispose();
        }

        base.Dispose(disposing);
    }
}
