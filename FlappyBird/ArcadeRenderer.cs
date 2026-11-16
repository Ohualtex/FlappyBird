using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace FlappyBird;

internal sealed class ArcadeRenderer : Component
{
    internal static readonly Color Ink = Color.FromArgb(23, 48, 65);
    internal static readonly Color Cream = Color.FromArgb(255, 248, 216);
    internal static readonly Color Gold = Color.FromArgb(255, 201, 66);

    private readonly Bitmap landscape = CreateLandscape();
    private readonly Bitmap birdIdle = PixelAssets.Load("bird-idle");
    private readonly Bitmap birdFlap = PixelAssets.Load("bird-flap");
    private readonly Bitmap pipeBody = PixelAssets.Load("pipe-body");
    private readonly Bitmap pipeCap = PixelAssets.Load("pipe-cap");
    private readonly Bitmap groundTile = PixelAssets.Load("ground-tile");
    private readonly Font titleFont = new(SystemFonts.MessageBoxFont!.FontFamily, 30, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font scoreFont = new(SystemFonts.MessageBoxFont!.FontFamily, 38, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font labelFont = new(SystemFonts.MessageBoxFont!.FontFamily, 14, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font hintFont = new(SystemFonts.MessageBoxFont!.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);

    public void Draw(Graphics graphics, Rectangle bounds, GameState game, bool running, int bestScore, string? scoreMessage)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var worldState = graphics.Save();
        try
        {
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            PixelAssets.Draw(graphics, landscape, new Rectangle(0, 0, bounds.Width, GameState.GroundY));
            foreach (GameState.Obstacle obstacle in game.Obstacles)
            {
                DrawPipePair(graphics, obstacle);
            }

            DrawBird(graphics, game);
            DrawGround(graphics, bounds, game.DistanceTraveled);
        }
        finally
        {
            graphics.Restore(worldState);
        }


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


    private void DrawPipePair(Graphics graphics, GameState.Obstacle obstacle)
    {
        int x = obstacle.X;
        int bodyX = x + GameState.PipeBodyInset;
        int bodyWidth = GameState.PipeWidth - GameState.PipeBodyInset * 2;
        int gapBottom = obstacle.GapTop + GameState.GapHeight;
        int rimHeight = GameState.PipeRimHeight;
        PixelAssets.Draw(graphics, pipeBody, new Rectangle(bodyX, 0, bodyWidth, obstacle.GapTop));
        PixelAssets.Draw(graphics, pipeBody,
            new Rectangle(bodyX, gapBottom, bodyWidth, GameState.GroundY - gapBottom));
        PixelAssets.Draw(graphics, pipeCap,
            new Rectangle(x, obstacle.GapTop - rimHeight, GameState.PipeWidth, rimHeight));
        PixelAssets.Draw(graphics, pipeCap, new Rectangle(x, gapBottom, GameState.PipeWidth, rimHeight));
    }

    private void DrawBird(Graphics graphics, GameState game)
    {
        const int center = GameState.BirdSize / 2;
        const int width = 26;
        Bitmap frame = game.IsBirdWingRaised ? birdFlap : birdIdle;
        int height = (int)Math.Round((float)width * frame.Height / frame.Width);
        float angle = (float)Math.Clamp(game.BirdVerticalSpeed * 3, -24, 30);
        GraphicsState transform = graphics.Save();
        try
        {
            graphics.TranslateTransform(GameState.BirdX + center, (float)game.BirdY + center);
            graphics.RotateTransform(angle);
            PixelAssets.Draw(graphics, frame, new Rectangle(-width / 2, -height / 2, width, height));
        }
        finally
        {
            graphics.Restore(transform);
        }
    }

    private void DrawGround(Graphics graphics, Rectangle bounds, long distanceTraveled)
    {
        int top = GameState.GroundY;
        int tileWidth = groundTile.Width * 2;
        int offset = (int)(distanceTraveled % tileWidth);
        for (int x = -offset; x < bounds.Width; x += tileWidth)
        {
            PixelAssets.Draw(graphics, groundTile, new Rectangle(x, top, tileWidth, bounds.Height - top));
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
            birdIdle.Dispose();
            birdFlap.Dispose();
            pipeBody.Dispose();
            pipeCap.Dispose();
            groundTile.Dispose();
            titleFont.Dispose();
            scoreFont.Dispose();
            labelFont.Dispose();
            hintFont.Dispose();
        }

        base.Dispose(disposing);
    }
}
