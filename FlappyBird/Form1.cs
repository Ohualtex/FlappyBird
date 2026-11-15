namespace FlappyBird;

public partial class Form1 : Form
{
    private readonly GameState game = new();
    private readonly System.Windows.Forms.Timer gameTimer;

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
        ClientSize = new Size(GameState.Width, 640);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;

        this.highScoreStore = highScoreStore;
        if (!highScoreStore.TryLoad(out bestScore))
        {
            scoreMessage = "Rekor okunamadı.";
        }

        gameTimer = new System.Windows.Forms.Timer(components ??= new System.ComponentModel.Container())
        {
            Interval = 16
        };
        gameTimer.Tick += AdvanceGame;

        var restartButton = new Button
        {
            Text = "Yeni oyun",
            Bounds = new Rectangle(355, 601, 110, 29),
            TabStop = false
        };
        restartButton.Click += (_, _) => ResetGame();
        Controls.Add(restartButton);
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

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics graphics = e.Graphics;
        graphics.Clear(Color.FromArgb(163, 222, 249));

        foreach (GameState.Obstacle obstacle in game.Obstacles)
        {
            int gapBottom = obstacle.GapTop + GameState.GapHeight;
            graphics.FillRectangle(Brushes.SeaGreen, obstacle.X, 0, GameState.PipeWidth, obstacle.GapTop);
            graphics.FillRectangle(Brushes.SeaGreen, obstacle.X, gapBottom,
                GameState.PipeWidth, GameState.GroundY - gapBottom);
        }

        graphics.FillRectangle(Brushes.Wheat, 0, GameState.GroundY, GameState.Width, 50);
        int birdY = (int)game.BirdY;
        graphics.FillEllipse(Brushes.Gold, GameState.BirdX, birdY, GameState.BirdSize, GameState.BirdSize);
        graphics.FillEllipse(Brushes.Black, GameState.BirdX + 18, birdY + 7, 4, 4);
        TextRenderer.DrawText(graphics, $"Skor: {game.Score}     Rekor: {bestScore}",
            Font, new Point(15, 600), Color.Black);
        if (scoreMessage is not null)
        {
            TextRenderer.DrawText(graphics, scoreMessage, Font, new Point(15, 619), Color.Maroon);
        }

        if (!gameTimer.Enabled)
        {
            string message = game.IsGameOver ? "Game Over — Yeniden denemek için Boşluk" : "Başlamak için Boşluk tuşuna bas";
            var banner = new Rectangle(35, 80, GameState.Width - 70, 45);
            graphics.FillRectangle(Brushes.WhiteSmoke, banner);
            TextRenderer.DrawText(graphics, message, Font, banner, Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
