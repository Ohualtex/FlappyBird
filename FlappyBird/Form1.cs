using System.Drawing.Drawing2D;

namespace FlappyBird;

public partial class Form1 : Form
{
    private const int GameHeight = 640;
    private static readonly Rectangle RestartButtonBounds = new(346, 602, 116, 29);

    private readonly GameState game = new();
    private readonly ArcadeRenderer renderer = new();
    private readonly System.Windows.Forms.Timer gameTimer;
    private readonly Button restartButton;
    private Font? scaledRestartFont;

    private readonly HighScoreStore highScoreStore;
    private int bestScore;
    private string? scoreMessage;

    public Form1() : this(new HighScoreStore(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FlappyBird", "high-score.txt")))
    {
    }

    internal Form1(HighScoreStore highScoreStore)
    {
        InitializeComponent();
        Text = "Flappy Bird";
        FormBorderStyle = FormBorderStyle.Sizable;
        ClientSize = new Size(GameState.Width, GameHeight);
        MinimumSize = SizeFromClientSize(ClientSize);
        BackColor = ArcadeRenderer.Ink;
        DoubleBuffered = true;
        (components ??= new System.ComponentModel.Container()).Add(renderer);

        this.highScoreStore = highScoreStore;
        if (!highScoreStore.TryLoad(out bestScore))
        {
            scoreMessage = "Rekor okunamadı.";
        }

        gameTimer = new System.Windows.Forms.Timer(components)
        {
            Interval = 16
        };
        gameTimer.Tick += AdvanceGame;

        restartButton = new Button
        {
            Text = "Yeni oyun",
            Bounds = RestartButtonBounds,
            BackColor = ArcadeRenderer.Gold,
            ForeColor = ArcadeRenderer.Ink,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            TabStop = true
        };
        restartButton.FlatAppearance.BorderColor = ArcadeRenderer.Cream;
        restartButton.Click += (_, _) => ResetGame();
        restartButton.Disposed += (_, _) => scaledRestartFont?.Dispose();
        Controls.Add(restartButton);
        ClientSizeChanged += (_, _) => LayoutGame();
        LayoutGame();
    }

    protected override bool ProcessCmdKey(ref Message message, Keys keyData)
    {
        if ((keyData & Keys.KeyCode) != Keys.Space)
        {
            return base.ProcessCmdKey(ref message, keyData);
        }

        // Windows sets bit 30 when the key was already down before this message.
        const long previousKeyState = 1L << 30;
        if ((message.LParam.ToInt64() & previousKeyState) != 0)
        {
            return true;
        }

        if (game.IsGameOver)
        {
            ResetGame();
        }

        game.Flap();
        gameTimer.Start();
        Invalidate();
        return true;
    }

    private void AdvanceGame(object? sender, EventArgs e)
    {
        game.Step();
        if (game.IsGameOver)
        {
            gameTimer.Stop();
            SaveHighScore();
        }

        Invalidate();
    }

    private void SaveHighScore()
    {
        if (game.Score <= bestScore)
        {
            return;
        }

        bestScore = game.Score;
        if (highScoreStore.TrySave(bestScore, out int savedScore))
        {
            bestScore = savedScore;
            scoreMessage = null;
        }
        else
        {
            scoreMessage = "Rekor kaydedilemedi.";
        }
    }

    private void ResetGame()
    {
        gameTimer.Stop();
        game.Reset();
        Invalidate();
    }

    private RectangleF GetGameViewport()
    {
        float scale = Math.Min((float)ClientSize.Width / GameState.Width,
            (float)ClientSize.Height / GameHeight);
        float width = GameState.Width * scale;
        float height = GameHeight * scale;
        return new RectangleF((ClientSize.Width - width) / 2,
            (ClientSize.Height - height) / 2, width, height);
    }

    private void LayoutGame()
    {
        RectangleF viewport = GetGameViewport();
        if (viewport.Width <= 0)
        {
            return;
        }

        float scale = viewport.Width / GameState.Width;
        restartButton.Bounds = Rectangle.Round(new RectangleF(
            viewport.Left + RestartButtonBounds.X * scale,
            viewport.Top + RestartButtonBounds.Y * scale,
            RestartButtonBounds.Width * scale,
            RestartButtonBounds.Height * scale));

        float scaledFontSize = Font.Size * scale;
        if (Math.Abs(restartButton.Font.Size - scaledFontSize) >= 0.1f)
        {
            Font? previousFont = scaledRestartFont;
            scaledRestartFont = scale == 1 ? null : new Font(Font.FontFamily, scaledFontSize, Font.Style, Font.Unit);
            restartButton.Font = scaledRestartFont;
            previousFont?.Dispose();
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        RectangleF viewport = GetGameViewport();
        if (viewport.Width <= 0)
        {
            return;
        }

        float scale = viewport.Width / GameState.Width;
        var graphics = e.Graphics;
        var state = graphics.Save();
        try
        {
            graphics.SetClip(viewport, CombineMode.Intersect);
            graphics.TranslateTransform(viewport.Left, viewport.Top);
            graphics.ScaleTransform(scale, scale);
            renderer.Draw(graphics, new Rectangle(0, 0, GameState.Width, GameHeight),
                game, gameTimer.Enabled, bestScore, scoreMessage);
        }
        finally
        {
            graphics.Restore(state);
        }
    }
}
