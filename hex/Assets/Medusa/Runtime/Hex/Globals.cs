using System;

public static class Globals
{
    public static float OffsetScale = 0.3f;
    public static float Radius = 1.5f;
    public static float Height = 2 * Radius;
    public static float RowHeight = 1.5f * Radius;
    public static float HalfWidth = (float)Math.Sqrt((Radius * Radius) - ((Radius / 2) * (Radius / 2)));
    public static float Width = 2 * HalfWidth;
    public static float ExtraHeight = Height - RowHeight;
    public static float Edge = RowHeight - ExtraHeight;
}
