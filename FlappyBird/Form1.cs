using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace FlappyBird;

public partial class Form1 : Form
{
    private const int GameHeight = 640;
    private static readonly Rectangle PauseButtonBounds = new(16, 16, 44, 44);
    private static readonly Rectangle ResumeButtonBounds = new(120, 278, 240, 44);
    private static readonly Rectangle RestartButtonBounds = new(120, 334, 240, 44);

    private static readonly Rectangle SoundButtonBounds = new(120, 390, 240, 44);

    private readonly GameState game = new();
    private readonly ArcadeRenderer renderer = new();
    private readonly GameAudio audio = new();
    private readonly System.Windows.Forms.Timer gameTimer;
    private readonly ArcadeButton pauseButton;
    private readonly ArcadeButton retryButton;
    private readonly Button resumeButton;
    private readonly Button restartButton;
    private readonly Button soundButton;
    private bool paused;
    private Rectangle? restoreWindowBounds;

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
        Size frame = GetFrameSize();
        MinimumSize = SizeFromClientSize(new Size(GameState.Width, GameHeight));
        Rectangle workArea = Screen.FromRectangle(Bounds).WorkingArea;
        const int preferredWidth = 576;
        int startupWidth = Math.Min(preferredWidth, WindowSizing.LargestBounds(workArea, frame).Width - frame.Width);
        ClientSize = new Size(startupWidth, startupWidth * GameHeight / GameState.Width);
        BackColor = ArcadeRenderer.Ink;
        DoubleBuffered = true;
        components ??= new System.ComponentModel.Container();
        components.Add(renderer);
        components.Add(audio);

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

        pauseButton = new ArcadeButton(DrawScene)
        {
            AccessibleName = "Duraklat",
            Visible = false
        };
        pauseButton.Click += (_, _) => TogglePause();
        resumeButton = CreatePauseAction("Devam et", ArcadeRenderer.Gold);
        resumeButton.Click += (_, _) => TogglePause();
        restartButton = CreatePauseAction("Yeni oyun", ArcadeRenderer.Cream);
        restartButton.Click += (_, _) => ResetGame();
        retryButton = new ArcadeButton(DrawScene)
        {
            Text = "Tekrar oyna",
            Visible = false
        };
        retryButton.Click += (_, _) => FlapOrRestart();
        soundButton = CreatePauseAction("Ses: Açık", ArcadeRenderer.Cream);
        soundButton.Click += (_, _) =>
        {
            audio.ToggleMute();
            soundButton.Text = audio.Muted ? "Ses: Kapalı" : "Ses: Açık";
        };
        Controls.AddRange([pauseButton, resumeButton, restartButton, soundButton, retryButton]);

        ClientSizeChanged += (_, _) => LayoutGame();
        LayoutGame();
    }

    protected override void WndProc(ref Message message)
    {
        const int sizingMessage = 0x0214;
        const int systemCommand = 0x0112;
        const int maximizeCommand = 0xF030;

        if (message.Msg == sizingMessage)
        {
            restoreWindowBounds = null;
            var drag = Marshal.PtrToStructure<NativeRectangle>(message.LParam);
            Rectangle proposed = Rectangle.FromLTRB(drag.Left, drag.Top, drag.Right, drag.Bottom);
            Rectangle workArea = Screen.FromRectangle(proposed).WorkingArea;
            Rectangle constrained = WindowSizing.ConstrainDrag(proposed, Bounds, workArea, GetFrameSize(),
                (WindowSizing.DragEdge)message.WParam.ToInt32());
            Marshal.StructureToPtr(new NativeRectangle(constrained), message.LParam, false);
            message.Result = 1;
            return;
        }

        if (message.Msg == systemCommand && (message.WParam.ToInt64() & 0xFFF0) == maximizeCommand)
        {
            if (restoreWindowBounds is Rectangle restore)
            {
                restoreWindowBounds = null;
                Bounds = restore;
            }
            else
            {
                restoreWindowBounds = Bounds;
                Bounds = WindowSizing.LargestBounds(Screen.FromControl(this).WorkingArea, GetFrameSize());
            }

            return;
        }

        base.WndProc(ref message);
    }

    private Size GetFrameSize() => SizeFromClientSize(new Size(GameState.Width, GameHeight))
        - new Size(GameState.Width, GameHeight);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle(Rectangle rectangle)
    {
        public int Left = rectangle.Left;
        public int Top = rectangle.Top;
        public int Right = rectangle.Right;
        public int Bottom = rectangle.Bottom;
    }

    protected override bool ProcessCmdKey(ref Message message, Keys keyData)
    {
        Keys key = keyData & Keys.KeyCode;
        if (key is not (Keys.Space or Keys.Escape))
        {
            return base.ProcessCmdKey(ref message, keyData);
        }

        // Windows sets bit 30 when the key was already down before this message.
        const long previousKeyState = 1L << 30;
        if ((message.LParam.ToInt64() & previousKeyState) == 0)
        {
            if (key == Keys.Escape)
            {
                TogglePause();
            }
            else
            {
                FlapOrRestart();
            }
        }

        return true;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left
            && GetGameViewport().Contains(e.X, e.Y))
        {
            FlapOrRestart();
        }
    }

    private static Button CreatePauseAction(string text, Color background)
    {
        var button = new Button
        {
            Text = text,
            Visible = false,
            BackColor = background,
            ForeColor = ArcadeRenderer.Ink,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = ArcadeRenderer.Ink;
        return button;
    }

    private void TogglePause()
    {
        if (!paused && !gameTimer.Enabled)
        {
            return;
        }

        paused = !paused;
        gameTimer.Enabled = !paused;
        UpdateGameControls();
        if (paused)
        {
            resumeButton.Select();
        }
        else
        {
            pauseButton.Select();
        }
    }

    private void UpdateGameControls()
    {
        pauseButton.ShowPlayIcon(paused);
        pauseButton.Visible = paused || gameTimer.Enabled;
        resumeButton.Visible = paused;
        restartButton.Visible = paused;
        soundButton.Visible = paused;
        retryButton.Visible = game.IsGameOver;
        Invalidate(invalidateChildren: true);
    }


    private void FlapOrRestart()
    {
        if (paused)
        {
            return;
        }

        if (game.IsGameOver)
        {
            game.Reset(fromReadyScreen: false);
        }

        game.Flap();
        audio.PlayFlap();
        gameTimer.Start();
        UpdateGameControls();
    }

    private void AdvanceGame(object? sender, EventArgs e)
    {
        if (paused)
        {
            return;
        }

        int previousScore = game.Score;
        game.Step();
        if (game.IsGameOver)
        {
            gameTimer.Stop();
            audio.PlayHit();
            SaveHighScore();
            UpdateGameControls();
            return;
        }

        if (game.Score > previousScore)
        {
            audio.PlayPoint();
        }

        Invalidate(invalidateChildren: true);
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
        paused = false;
        game.Reset();
        UpdateGameControls();
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
        pauseButton.Bounds = ScaleBounds(PauseButtonBounds);
        resumeButton.Bounds = ScaleBounds(ResumeButtonBounds);
        restartButton.Bounds = ScaleBounds(RestartButtonBounds);
        soundButton.Bounds = ScaleBounds(SoundButtonBounds);
        retryButton.Bounds = ScaleBounds(ResultScreen.RetryBounds);

        Rectangle ScaleBounds(Rectangle bounds) => Rectangle.Round(new RectangleF(
            viewport.Left + bounds.X * scale, viewport.Top + bounds.Y * scale,
            bounds.Width * scale, bounds.Height * scale));

        Invalidate(invalidateChildren: true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        DrawScene(e.Graphics, Point.Empty);
    }

    private void DrawScene(Graphics graphics, Point clientOffset)
    {
        RectangleF viewport = GetGameViewport();
        if (viewport.Width <= 0)
        {
            return;
        }

        float scale = viewport.Width / GameState.Width;
        viewport.Offset(-clientOffset.X, -clientOffset.Y);
        var state = graphics.Save();
        try
        {
            graphics.SetClip(viewport, CombineMode.Intersect);
            graphics.TranslateTransform(viewport.Left, viewport.Top);
            graphics.ScaleTransform(scale, scale);
            renderer.Draw(graphics, new Rectangle(0, 0, GameState.Width, GameHeight),
                game, gameTimer.Enabled, paused, bestScore, scoreMessage);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

}
