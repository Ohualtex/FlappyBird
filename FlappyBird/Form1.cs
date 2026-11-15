namespace FlappyBird;

public partial class Form1 : Form
{
    private readonly GameState game = new();
    private readonly ArcadeRenderer renderer = new();
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

        var restartButton = new Button
        {
            Text = "Yeni oyun",
            Bounds = new Rectangle(346, 602, 116, 29),
            BackColor = ArcadeRenderer.Gold,
            ForeColor = ArcadeRenderer.Ink,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            TabStop = true
        };
        restartButton.FlatAppearance.BorderColor = ArcadeRenderer.Cream;
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
        renderer.Draw(e.Graphics, ClientRectangle, game, gameTimer.Enabled, bestScore, scoreMessage);
    }
}
