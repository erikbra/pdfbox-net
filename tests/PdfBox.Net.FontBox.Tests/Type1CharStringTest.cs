using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.FontBox.Type1;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.Tests;

public class Type1CharStringTest
{
    [Fact]
    public void RmovetoUsesLastTwoOperandsAndRendersTheGlyph()
    {
        Type1CharString glyph = new(null, "TestFont", "A", [
            10, 500, CharStringCommand.HSBW,
            1, 2, 3, 4, CharStringCommand.RMOVETO,
            5, 6, CharStringCommand.RLINETO,
            CharStringCommand.CLOSEPATH, CharStringCommand.ENDCHAR]);
        Assert.Equal(500, glyph.GetWidth());
        Assert.Collection(glyph.GetPath().Segments,
            move => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.MoveTo, 13, 4), move),
            line => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.LineTo, 18, 10), line),
            close => Assert.Equal(GeneralPath.SegmentType.Close, close.Type),
            move => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.MoveTo, 18, 10), move));
        Assert.Same(glyph.GetPath(), glyph.GetPath());
        Assert.Equal(13, glyph.GetBounds().GetLowerLeftX());
        Assert.Equal(10, glyph.GetBounds().GetUpperRightY());
    }

    [Fact]
    public void RawByteCompatibilityConstructorParsesWidthsCurvesAndDivision()
    {
        // 0 500 hsbw; 8 2 div 7 rmoveto; 1 2 3 4 5 6 rrcurveto; endchar
        Type1CharString glyph = new("TestFont", "A", [139, 248, 136, 13,
            147, 141, 12, 12, 146, 21, 140, 141, 142, 143, 144, 145, 8, 14]);
        Assert.Equal(500, glyph.GetWidth());
        Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.MoveTo, 4, 7), glyph.GetPath().Segments[0]);
        Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.CurveTo, 5, 9, 8, 13, 13, 19), glyph.GetPath().Segments[1]);
    }

    [Fact]
    public void FontEntryPointExpandsSubroutinesAndRendersTheParsedSequence()
    {
        Type1Font font = new();
        font.AddSubr(0, [140, 141, 21, 11]); // 1 2 rmoveto; return
        font.AddCharString("A", [139, 248, 136, 13, 139, 10, 142, 143, 5, 14]);
        Assert.Equal(font.GetCharStringsDict()["A"], font.GetType1CharString("A").Bytes);
        Assert.Equal(500, font.GetWidth("A"));
        Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.MoveTo, 1, 2), font.GetPath("A").Segments[0]);
        Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.LineTo, 4, 6), font.GetPath("A").Segments[1]);
    }

    [Fact]
    public void SeacComposesBaseAndTranslatedAccentPaths()
    {
        Type1Font font = new();
        font.AddCharString("A", [139, 248, 136, 13, 140, 141, 21, 142, 143, 5, 14]);
        font.AddCharString("acute", [139, 248, 136, 13, 144, 145, 21, 146, 147, 5, 14]);
        // asb=0 adx=10 ady=20 bchar=65(A) achar=194(acute)
        font.AddCharString("Aacute", [139, 248, 136, 13, 139, 149, 159, 204, 247, 86, 12, 6, 14]);
        var path = font.GetPath("Aacute");
        Assert.Equal(4, path.Segments.Count);
        Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.MoveTo, 1, 2), path.Segments[0]);
        Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.MoveTo, 15, 26), path.Segments[2]);
    }
}
