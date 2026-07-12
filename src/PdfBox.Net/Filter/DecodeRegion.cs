/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

namespace PdfBox.Net.Filter;

/// <summary>
/// Integer rectangle describing the source image region requested during filter decoding.
/// </summary>
public readonly record struct DecodeRegion(int X, int Y, int Width, int Height)
{
    public static DecodeRegion Empty { get; } = new(0, 0, 0, 0);

    public int Left => X;

    public int Top => Y;

    public int Right => X + Width;

    public int Bottom => Y + Height;

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public DecodeRegion Intersect(DecodeRegion other)
    {
        int left = Math.Max(Left, other.Left);
        int top = Math.Max(Top, other.Top);
        int right = Math.Min(Right, other.Right);
        int bottom = Math.Min(Bottom, other.Bottom);
        if (right <= left || bottom <= top)
        {
            return Empty;
        }

        return new DecodeRegion(left, top, right - left, bottom - top);
    }
}
