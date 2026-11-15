using System.Drawing.Drawing2D;

namespace FlappyBird;

internal sealed class GearButton : Button
{
    private const float OuterRadius = 0.44f;
    private const float OutlineWidth = 0.055f;
    private bool hovered;

    public GearButton()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.Transparent;
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

        float diameter = size * (2 * OuterRadius + OutlineWidth);
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
        float size = Math.Min(ClientSize.Width, ClientSize.Height);
        if (size <= 0)
        {
            return;
        }

        float centerX = ClientSize.Width / 2f;
        float centerY = ClientSize.Height / 2f;
        var points = new PointF[32];
        for (int i = 0; i < points.Length; i++)
        {
            double angle = 2 * Math.PI * i / points.Length;
            float radius = size * (i % 4 is 1 or 2 ? OuterRadius : 0.32f);
            points[i] = new PointF(centerX + radius * (float)Math.Cos(angle),
                centerY + radius * (float)Math.Sin(angle));
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var icon = new GraphicsPath(FillMode.Alternate);
        icon.AddPolygon(points);
        float hole = size * 0.27f;
        icon.AddEllipse(centerX - hole / 2, centerY - hole / 2, hole, hole);
        using var outline = new Pen(ArcadeRenderer.Ink, size * OutlineWidth) { LineJoin = LineJoin.Round };
        using var fill = new SolidBrush(hovered || Focused ? ArcadeRenderer.Gold : ArcadeRenderer.Cream);
        e.Graphics.DrawPath(outline, icon);
        e.Graphics.FillPath(fill, icon);
    }
}
