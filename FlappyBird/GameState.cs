namespace FlappyBird;

internal sealed class GameState
{
    public const int Width = 480;
    public const int GroundY = 590;
    public const int BirdX = 95;
    public const int BirdSize = 28;
    public const int PipeWidth = 72;
    public const int GapHeight = 168;
    private const int PipeSpacing = 275;

    private const int WingFlapDuration = 8;

    private int wingFlapTicks;

    public double BirdY { get; private set; }
    public double BirdVerticalSpeed { get; private set; }
    public bool IsBirdWingRaised => wingFlapTicks > 0;
    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }
    public IReadOnlyList<Obstacle> Obstacles { get; } = [new(), new()];

    public GameState() => Reset();

    public void Reset()
    {
        BirdY = 240;
        wingFlapTicks = 0;
        BirdVerticalSpeed = 0;
        Score = 0;
        IsGameOver = false;
        Place(Obstacles[0], Width + 35, 185);
        Place(Obstacles[1], Width + 35 + PipeSpacing, 255);
    }

    public void Flap()
    {
        BirdVerticalSpeed = -7.8;
        wingFlapTicks = WingFlapDuration;
    }

    public void Step()
    {
        if (IsGameOver)
        {
            return;
        }

        if (wingFlapTicks > 0)
        {
            wingFlapTicks--;
        }

        BirdVerticalSpeed = Math.Min(BirdVerticalSpeed + 0.42, 10);
        BirdY = Math.Max(0, BirdY + BirdVerticalSpeed);
        if (BirdY == 0)
        {
            BirdVerticalSpeed = 0;
        }

        foreach (Obstacle obstacle in Obstacles)
        {
            obstacle.X -= 4;
        }

        IsGameOver = BirdY + BirdSize >= GroundY || Obstacles.Any(HitsBird);
        if (IsGameOver)
        {
            return;
        }

        foreach (Obstacle obstacle in Obstacles)
        {
            if (!obstacle.Scored && obstacle.X + PipeWidth <= BirdX)
            {
                obstacle.Scored = true;
                Score++;
            }

            if (obstacle.X + PipeWidth <= 0)
            {
                int nextX = Obstacles.Max(pipe => pipe.X) + PipeSpacing;
                Place(obstacle, nextX, Random.Shared.Next(105, 321));
            }
        }
    }

    private bool HitsBird(Obstacle obstacle)
    {
        bool overlapsHorizontally = BirdX < obstacle.X + PipeWidth
            && BirdX + BirdSize > obstacle.X;
        return overlapsHorizontally
            && (BirdY < obstacle.GapTop || BirdY + BirdSize > obstacle.GapTop + GapHeight);
    }

    private static void Place(Obstacle obstacle, int x, int gapTop)
    {
        obstacle.X = x;
        obstacle.GapTop = gapTop;
        obstacle.Scored = false;
    }

    internal sealed class Obstacle
    {
        public int X { get; set; }
        public int GapTop { get; set; }
        public bool Scored { get; set; }
    }
}
