using System.ComponentModel;
using System.Media;

namespace FlappyBird;

internal sealed class GameAudio : Component
{
    private readonly SoundPlayer? flap = Load("flap");
    private readonly SoundPlayer? point = Load("point");
    private readonly SoundPlayer? hit = Load("hit");
    private SoundPlayer? active;

    public bool Muted { get; private set; }

    public void PlayFlap() => Play(flap);
    public void PlayPoint() => Play(point);
    public void PlayHit() => Play(hit);

    public void ToggleMute()
    {
        Muted = !Muted;
        if (Muted)
        {
            active?.Stop();
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
            DisposePlayer(flap);
            DisposePlayer(point);
            DisposePlayer(hit);
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
