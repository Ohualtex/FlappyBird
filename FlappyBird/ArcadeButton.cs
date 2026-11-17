using System.Drawing.Drawing2D;

namespace FlappyBird;

internal sealed class ArcadeButton : Button
{
    private const int ArtWidth = 22;
    private const int ArtHeight = 23;
    private readonly Action<Graphics, Point> drawScene;
    private readonly bool creamFace;
    private bool showingPlayIcon;
    private bool hovered;
    private bool pressed;

    private readonly bool showsSpeaker;
    private bool speakerMuted;

    public ArcadeButton(Action<Graphics, Point> drawScene, bool creamFace = false, bool showsSpeaker = false)
    {
        this.drawScene = drawScene;
        this.creamFace = creamFace;
        this.showsSpeaker = showsSpeaker;
        SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        UseVisualStyleBackColor = false;
        FlatStyle = FlatStyle.Flat;
    }

    public void SetSpeakerMuted(bool muted)
    {
        speakerMuted = muted;
        Text = muted ? "Sound: Off" : "Sound: On";
    }

    public void ShowPlayIcon(bool showPlayIcon)
    {
        if (showingPlayIcon == showPlayIcon)
        {
            return;
        }

        showingPlayIcon = showPlayIcon;
        AccessibleName = showPlayIcon ? "Resume" : "Pause";
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
        bool iconOnly = Text.Length == 0;
        if (iconOnly)
        {
            drawScene(graphics, Location);
        }
        var state = graphics.Save();
        try
        {
            int width = iconOnly ? ArtWidth : ClientSize.Width * ArtHeight / size;
            if (iconOnly)
            {
                graphics.TranslateTransform((ClientSize.Width - size) / 2f, (ClientSize.Height - size) / 2f);
            }

            graphics.ScaleTransform((float)(iconOnly ? size : ClientSize.Width) / width,
                (float)(iconOnly ? size : ClientSize.Height) / ArtHeight);
            graphics.SmoothingMode = SmoothingMode.None;
            DrawFace(graphics, width);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private void DrawSpeaker(Graphics graphics, Brush brush, int x, int y)
    {
        graphics.FillRectangle(brush, x, y + 2, 2, 4);
        graphics.FillRectangle(brush, x + 2, y + 1, 1, 6);
        graphics.FillRectangle(brush, x + 3, y, 1, 8);
        if (speakerMuted)
        {
            graphics.FillRectangle(brush, x + 5, y + 2, 1, 1);
            graphics.FillRectangle(brush, x + 6, y + 3, 1, 2);
            graphics.FillRectangle(brush, x + 5, y + 5, 1, 1);
            graphics.FillRectangle(brush, x + 7, y + 2, 1, 1);
            graphics.FillRectangle(brush, x + 7, y + 5, 1, 1);
        }
        else
        {
            graphics.FillRectangle(brush, x + 5, y + 2, 1, 4);
            graphics.FillRectangle(brush, x + 7, y + 1, 1, 6);
        }
    }

    private void DrawFace(Graphics graphics, int width)
    {
        bool sunken = pressed && hovered;
        using var ink = new SolidBrush(Color.FromArgb(52, 42, 31));
        using var cream = new SolidBrush(Color.FromArgb(255, 242, 209));
        using var orange = new SolidBrush(sunken ? Color.FromArgb(196, 81, 23)
            : hovered || Focused ? Color.FromArgb(243, 122, 37) : Color.FromArgb(229, 106, 31));
        using var highlight = new SolidBrush(Color.FromArgb(251, 149, 55));
        using var shadow = new SolidBrush(Color.FromArgb(152, 67, 21));
        using var neutralFace = new SolidBrush(sunken ? Color.FromArgb(234, 214, 164)
            : hovered || Focused ? Color.FromArgb(255, 231, 165) : ArcadeRenderer.Cream);
        using var neutralShadow = new SolidBrush(Color.FromArgb(165, 137, 85));

        graphics.FillRectangle(ink, 0, 0, width, ArtHeight);
        graphics.FillRectangle(cream, 1, 1, width - 2, 20);
        graphics.FillRectangle(creamFace ? neutralShadow : shadow, 3, 3, width - 6, 16);
        graphics.FillRectangle(creamFace ? neutralFace : orange, 3, sunken ? 5 : 3, width - 6, 14);
        graphics.FillRectangle(creamFace ? cream : highlight, 3, sunken ? 18 : 3, width - 6, 1);

        if (Text.Length > 0)
        {
            using var font = new Font(SystemFonts.MessageBoxFont!.FontFamily, 9, FontStyle.Bold, GraphicsUnit.Pixel);
            int top = sunken ? 5 : 3;
            int textInset = showsSpeaker ? 13 : 3;
            if (showsSpeaker)
            {
                int iconX = (int)((width - graphics.MeasureString(Text, font).Width) / 2) - 10;
                DrawSpeaker(graphics, ink, iconX, top + 4);
            }

            ArcadeRenderer.DrawCentered(graphics, Text, font, creamFace ? ArcadeRenderer.Ink : cream.Color,
                new Rectangle(textInset, top, width - textInset - 3, 14));
            return;
        }

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
