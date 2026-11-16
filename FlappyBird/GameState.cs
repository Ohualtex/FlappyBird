namespace FlappyBird;

internal sealed class GameState
{
    public const int Width = 480;
    public const int GroundY = 590;
    public const int BirdX = 95;
    public const int BirdSize = 28;
    public const int PipeWidth = 72;
    public const int PipeRimHeight = 22;
    public const int PipeBodyInset = 4;
    public const int GapHeight = 168;
    private const int MinimumPipeSpacing = 255;
    private const int MaximumPipeSpacing = 295;
    private const int MinimumGapTop = 125;
    private const int MaximumGapTop = 305;
    private const int ScrollSpeed = 4;

    private const int WingFlapDuration = 8;

    private int wingFlapTicks;

    public double BirdY { get; private set; }
    public double BirdVerticalSpeed { get; private set; }
    public bool IsBirdWingRaised => wingFlapTicks > 0;
    public int Score { get; private set; }
    public long DistanceTraveled { get; private set; }
    public bool IsGameOver { get; private set; }
    public IReadOnlyList<Obstacle> Obstacles { get; } = [new(), new()];

    public GameState() => Reset();

    public void Reset()
    {
        BirdY = 240;
        wingFlapTicks = 0;
        BirdVerticalSpeed = 0;
        Score = 0;
        DistanceTraveled = 0;
        IsGameOver = false;
        Place(Obstacles[0], Width + 35, Random.Shared.Next(145, 271));
        PlaceAfter(Obstacles[1], Obstacles[0]);
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

        DistanceTraveled += ScrollSpeed;

        foreach (Obstacle obstacle in Obstacles)
        {
            obstacle.X -= ScrollSpeed;
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
                Obstacle last = Obstacles.MaxBy(pipe => pipe.X)!;
                PlaceAfter(obstacle, last);
            }
        }
    }

    private bool HitsBird(Obstacle obstacle)
    {
        int right = obstacle.X + PipeWidth;
        if (BirdX >= right || BirdX + BirdSize <= obstacle.X)
        {
            return false;
        }

        double birdBottom = BirdY + BirdSize;
        int gapBottom = obstacle.GapTop + GapHeight;
        bool overlapsBody = BirdX < right - PipeBodyInset && BirdX + BirdSize > obstacle.X + PipeBodyInset;
        if (overlapsBody)
        {
            return BirdY < obstacle.GapTop || birdBottom > gapBottom;
        }

        return (BirdY < obstacle.GapTop && birdBottom > obstacle.GapTop - PipeRimHeight)
            || (BirdY < gapBottom + PipeRimHeight && birdBottom > gapBottom);
    }

    private static void PlaceAfter(Obstacle obstacle, Obstacle previous)
    {
        int spacing = Random.Shared.Next(MinimumPipeSpacing, MaximumPipeSpacing + 1);
        int gapTop = Random.Shared.Next(MinimumGapTop, MaximumGapTop + 1);
        Place(obstacle, previous.X + spacing, gapTop);
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
