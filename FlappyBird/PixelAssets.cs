namespace FlappyBird;

internal static class PixelAssets
{
    public static Bitmap Load(string name)
    {
        string resource = $"FlappyBird.Assets.{name}.png";
        using Stream stream = typeof(PixelAssets).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Missing embedded sprite: {resource}");
        using var image = new Bitmap(stream);
        return new Bitmap(image);
    }

    public static void Draw(Graphics graphics, Bitmap sprite, Rectangle destination)
    {
        graphics.DrawImage(sprite, destination, new Rectangle(Point.Empty, sprite.Size), GraphicsUnit.Pixel);
    }
}
