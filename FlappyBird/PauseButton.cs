using System.Drawing.Drawing2D;

namespace FlappyBird;

internal sealed class PauseButton : Button
{
    private const int ArtWidth = 22;
    private const int ArtHeight = 23;
    private readonly Action<Graphics, Point> drawScene;
    private bool showingPlayIcon;
    private bool hovered;
    private bool pressed;

    public PauseButton(Action<Graphics, Point> drawScene)
    {
        this.drawScene = drawScene;
        SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        UseVisualStyleBackColor = false;
        FlatStyle = FlatStyle.Flat;
    }

    public void ShowPlayIcon(bool showPlayIcon)
    {
        if (showingPlayIcon == showPlayIcon)
        {
            return;
        }

        showingPlayIcon = showPlayIcon;
        AccessibleName = showPlayIcon ? "Devam et" : "Duraklat";
        Invalidate();
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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            pressed = true;
            Refresh();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (!Capture && pressed)
        {
            pressed = false;
            Invalidate();
        }
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

        Graphics graphics = e.Graphics;
        drawScene(graphics, Location);
        var state = graphics.Save();
        try
        {
            graphics.TranslateTransform((ClientSize.Width - size) / 2f, (ClientSize.Height - size) / 2f);
            graphics.ScaleTransform((float)size / ArtWidth, (float)size / ArtHeight);
            graphics.SmoothingMode = SmoothingMode.None;
            DrawFace(graphics);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private void DrawFace(Graphics graphics)
    {
        bool sunken = pressed && hovered;
        using var ink = new SolidBrush(Color.FromArgb(52, 42, 31));
        using var cream = new SolidBrush(Color.FromArgb(255, 242, 209));
        using var orange = new SolidBrush(sunken ? Color.FromArgb(196, 81, 23)
            : hovered || Focused ? Color.FromArgb(243, 122, 37) : Color.FromArgb(229, 106, 31));
        using var highlight = new SolidBrush(Color.FromArgb(251, 149, 55));
        using var shadow = new SolidBrush(Color.FromArgb(152, 67, 21));

        graphics.FillRectangle(ink, 0, 0, ArtWidth, ArtHeight);
        graphics.FillRectangle(cream, 1, 1, 20, 20);
        graphics.FillRectangle(shadow, 3, 3, 16, 16);
        graphics.FillRectangle(orange, 3, sunken ? 5 : 3, 16, 14);
        graphics.FillRectangle(highlight, 3, sunken ? 18 : 3, 16, 1);

        int iconTop = sunken ? 7 : 6;
        if (showingPlayIcon)
        {
            for (int step = 0; step < 5; step++)
            {
                graphics.FillRectangle(cream, 7 + step * 2, iconTop + step, 2, 9 - step * 2);
            }
        }
        else
        {
            graphics.FillRectangle(cream, 7, iconTop, 3, 9);
            graphics.FillRectangle(cream, 12, iconTop, 3, 9);
        }
    }
}
