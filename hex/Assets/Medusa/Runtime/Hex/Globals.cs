using System;

public static class Globals
{
    /// <summary>
    /// 事件上移高度
    /// </summary>
    public static float OffsetScale = 0.3f;
    public static float Radius = 1.5f;
    public static float Height = 2 * Radius;
    public static float RowHeight = 1.5f * Radius;
    public static float HalfWidth = (float)Math.Sqrt((Radius * Radius) - ((Radius / 2) * (Radius / 2)));
    public static float Width = 2 * HalfWidth;
    public static float ExtraHeight = 0.5f * Radius;//Height - RowHeight;
    public static float Edge = Radius;//RowHeight - ExtraHeight;


    //    p4_________p5
    //     /         \            |
    //    /           \           |
    //   /             \ p0       | width
    //p3 \             /          |
    //    \           /           |
    //  p2 \_________/p1          |
    //  |  |        |   |
    //  |  |        |   |
    //  |  |        |   |
    //  |   --rowheight-|
    //  |   --edge---   | 
    //  |   ---r-----   |
    //  |---height------|
    //
    //    ---------------> y
    //    |
    //    |
    //    |
    //    |
    //     x
}
