using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FlappyBird;

internal sealed class GearButton : Button
{
    private const float HitAreaRatio = 0.94f;
    private readonly Bitmap sprite = PixelAssets.Load("gear");
    private readonly ImageAttributes highlight = new();
    private readonly Action<Graphics, Point> drawScene;
    private bool hovered;

    public GearButton(Action<Graphics, Point> drawScene)
    {
        this.drawScene = drawScene;
        highlight.SetColorMatrix(new ColorMatrix { Matrix00 = 1.12f, Matrix11 = 1.12f, Matrix22 = 1.12f });
        SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        UseVisualStyleBackColor = false;
        FlatStyle = FlatStyle.Flat;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        float size = Math.Min(ClientSize.Width, ClientSize.Height);
        if (size <= 0)
        {
            Region = null;
            return;
        }

        float diameter = size * HitAreaRatio;
        using var clickable = new GraphicsPath();
        clickable.AddEllipse((Width - diameter) / 2, (Height - diameter) / 2, diameter, diameter);
        Region = new Region(clickable);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        hovered = false;
        Invalidate();
    }

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);
        Invalidate();
    }

    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        int size = Math.Min(ClientSize.Width, ClientSize.Height);
        if (size <= 0)
        {
            return;
        }

        drawScene(e.Graphics, Location);
        var destination = new Rectangle((ClientSize.Width - size) / 2, (ClientSize.Height - size) / 2, size, size);
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.DrawImage(sprite, destination, 0, 0, sprite.Width, sprite.Height,
            GraphicsUnit.Pixel, hovered || Focused ? highlight : null);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            sprite.Dispose();
            highlight.Dispose();
        }

        base.Dispose(disposing);
    }
}
