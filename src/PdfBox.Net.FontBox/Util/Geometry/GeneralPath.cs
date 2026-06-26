namespace PdfBox.Net.Util.Geometry;

/// <summary>
/// "Port" of the Java java.awt.geom.GeneralPath class.
/// </summary>
public class GeneralPath
{
    public enum SegmentType
    {
        MoveTo,
        LineTo,
        QuadTo,
        CurveTo,
        Close
    }

    public readonly record struct Segment(
        SegmentType Type,
        float X1,
        float Y1,
        float X2 = 0,
        float Y2 = 0,
        float X3 = 0,
        float Y3 = 0);

    private readonly List<Segment> _segments = [];

    public IReadOnlyList<Segment> Segments => _segments;

    public void MoveTo(float x, float y) => _segments.Add(new Segment(SegmentType.MoveTo, x, y, 0, 0));

    public void LineTo(float x, float y) => _segments.Add(new Segment(SegmentType.LineTo, x, y, 0, 0));

    public void QuadTo(float x1, float y1, float x2, float y2) => _segments.Add(new Segment(SegmentType.QuadTo, x1, y1, x2, y2));

    public void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3) =>
        _segments.Add(new Segment(SegmentType.CurveTo, x1, y1, x2, y2, x3, y3));

    public void ClosePath() => _segments.Add(new Segment(SegmentType.Close, 0, 0, 0, 0));
}
