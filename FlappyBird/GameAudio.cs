using System.ComponentModel;
using System.Media;

namespace FlappyBird;

internal sealed class GameAudio : Component
{
    private readonly SoundPlayer? flap = Load("flap");
    private readonly WaveEffect point = new("point");
    private readonly WaveEffect hit = new("hit");
    private SoundPlayer? active;

    public bool Muted { get; private set; }
    public bool IsHitPlaying => hit.IsPlaying;

    public void PlayFlap() => Play(flap);
    public void PlayPoint()
    {
        if (!Muted)
        {
            point.Play();
        }
    }

    public void PlayHit()
    {
        if (!Muted)
        {
            hit.Play();
        }
    }

    public void ToggleMute()
    {
        Muted = !Muted;
        if (Muted)
        {
            active?.Stop();
            point.Stop();
            hit.Stop();
            active = null;
        }
    }

    private static SoundPlayer? Load(string name)
    {
        Stream? stream = typeof(GameAudio).Assembly.GetManifestResourceStream($"FlappyBird.Assets.{name}.wav");
        if (stream is null)
        {
            return null;
        }

        var player = new SoundPlayer(stream);
        try
        {
            player.Load();
            return player;
        }
        catch (Exception error) when (error is IOException or InvalidOperationException or TimeoutException)
        {
            player.Dispose();
            stream.Dispose();
            return null;
        }
    }

    private void Play(SoundPlayer? player)
    {
        if (Muted || player is null)
        {
            return;
        }

        try
        {
            player.Play();
            active = player;
        }
        catch (Exception error) when (error is InvalidOperationException or TimeoutException)
        {
            // Audio failures must not interrupt play or scoring.
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            active?.Stop();
            point.Dispose();
            DisposePlayer(flap);
            hit.Dispose();
        }

        base.Dispose(disposing);
    }

    private static void DisposePlayer(SoundPlayer? player)
    {
        Stream? stream = player?.Stream;
        player?.Dispose();
        stream?.Dispose();
    }
}
