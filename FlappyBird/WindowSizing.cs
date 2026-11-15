namespace FlappyBird;

internal static class WindowSizing
{
    private const int AspectWidth = 3;
    private const int AspectHeight = 4;
    private const int MinimumUnits = GameState.Width / AspectWidth;

    public static Rectangle LargestBounds(Rectangle workArea, Size frame)
    {
        Size outer = OuterSize(MaximumUnits(workArea, frame), frame);
        return new Rectangle(workArea.Left + Math.Max(0, (workArea.Width - outer.Width) / 2),
            workArea.Top + Math.Max(0, (workArea.Height - outer.Height) / 2), outer.Width, outer.Height);
    }

    public static Rectangle ConstrainDrag(Rectangle proposed, Rectangle current, Rectangle workArea,
        Size frame, DragEdge edge)
    {
        double widthUnits = (double)(proposed.Width - frame.Width) / AspectWidth;
        double heightUnits = (double)(proposed.Height - frame.Height) / AspectHeight;
        bool horizontalOnly = edge is DragEdge.Left or DragEdge.Right;
        bool verticalOnly = edge is DragEdge.Top or DragEdge.Bottom;
        double widthChange = Math.Abs(proposed.Width - current.Width) / (double)AspectWidth;
        double heightChange = Math.Abs(proposed.Height - current.Height) / (double)AspectHeight;
        bool useWidth = horizontalOnly || (!verticalOnly && widthChange >= heightChange);
        int units = Math.Clamp((int)Math.Round(useWidth ? widthUnits : heightUnits),
            MinimumUnits, MaximumUnits(workArea, frame));

        Size outer = OuterSize(units, frame);
        bool anchorRight = edge is DragEdge.Left or DragEdge.TopLeft or DragEdge.BottomLeft;
        bool anchorBottom = edge is DragEdge.Top or DragEdge.TopLeft or DragEdge.TopRight;
        int x = anchorRight ? proposed.Right - outer.Width : proposed.Left;
        int y = anchorBottom ? proposed.Bottom - outer.Height : proposed.Top;
        x = Math.Clamp(x, workArea.Left, Math.Max(workArea.Left, workArea.Right - outer.Width));
        y = Math.Clamp(y, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - outer.Height));
        return new Rectangle(x, y, outer.Width, outer.Height);
    }

    private static int MaximumUnits(Rectangle workArea, Size frame) =>
        Math.Max(MinimumUnits, Math.Min((workArea.Width - frame.Width) / AspectWidth,
            (workArea.Height - frame.Height) / AspectHeight));

    private static Size OuterSize(int units, Size frame) =>
        new(units * AspectWidth + frame.Width, units * AspectHeight + frame.Height);

    // Values passed by Windows in WM_SIZING.
    internal enum DragEdge
    {
        Left = 1,
        Right,
        Top,
        TopLeft,
        TopRight,
        Bottom,
        BottomLeft,
        BottomRight
    }
}
