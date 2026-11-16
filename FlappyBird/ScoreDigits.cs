using System.Drawing.Drawing2D;

namespace FlappyBird;

internal static class ScoreDigits
{
    private const int Advance = 16;
    private const float Scale = 1.4f;

    public static GraphicsPath Create(string digits)
    {
        var path = new GraphicsPath(FillMode.Alternate);
        int x = 0;
        foreach (char digit in digits)
        {
            switch (digit)
            {
                case '0':
                    path.AddRectangle(new Rectangle(x, 0, 12, 20));
                    path.AddRectangle(new Rectangle(x + 4, 4, 4, 12));
                    break;
                case '1':
                    Add([(2, 2), (6, 0), (10, 0), (10, 20), (6, 20), (6, 5), (2, 5)]);
                    break;
                case '2':
                    Add([(0, 0), (12, 0), (12, 8), (4, 15), (4, 16), (12, 16), (12, 20),
                        (0, 20), (0, 13), (8, 6), (8, 4), (0, 4)]);
                    break;
                case '3':
                    Add([(0, 0), (12, 0), (12, 20), (0, 20), (0, 16), (8, 16), (8, 12),
                        (3, 12), (3, 8), (8, 8), (8, 4), (0, 4)]);
                    break;
                case '4':
                    Add([(0, 0), (4, 0), (4, 9), (8, 9), (8, 0), (12, 0), (12, 20),
                        (8, 20), (8, 13), (0, 13)]);
                    break;
                case '5':
                    Add([(0, 0), (12, 0), (12, 4), (4, 4), (4, 8), (12, 8), (12, 20),
                        (0, 20), (0, 16), (8, 16), (8, 12), (0, 12)]);
                    break;
                case '6':
                    Add([(0, 0), (12, 0), (12, 4), (4, 4), (4, 8), (12, 8), (12, 20), (0, 20)]);
                    path.AddRectangle(new Rectangle(x + 4, 12, 4, 4));
                    break;
                case '7':
                    Add([(0, 0), (12, 0), (12, 4), (6, 20), (2, 20), (8, 4), (0, 4)]);
                    break;
                case '8':
                    path.AddRectangle(new Rectangle(x, 0, 12, 20));
                    path.AddRectangle(new Rectangle(x + 4, 4, 4, 4));
                    path.AddRectangle(new Rectangle(x + 4, 12, 4, 4));
                    break;
                case '9':
                    Add([(0, 0), (12, 0), (12, 20), (0, 20), (0, 16), (8, 16), (8, 12), (0, 12)]);
                    path.AddRectangle(new Rectangle(x + 4, 4, 4, 4));
                    break;
            }

            x += Advance;
        }

        using var scale = new Matrix();
        scale.Scale(Scale, Scale);
        path.Transform(scale);
        return path;

        void Add((int X, int Y)[] vertices) =>
            path.AddPolygon(Array.ConvertAll(vertices, point => new Point(x + point.X, point.Y)));
    }
}
