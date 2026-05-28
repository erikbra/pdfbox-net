using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Fdf;
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.Tests;

public class FDFAnnotationMirrorTypesTest
{
    [Fact]
    public void AnnotationFactoryMapsAllSupportedSubtypes()
    {
        Assert.IsType<FDFAnnotationHighlight>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationHighlight().GetCOSObject()));
        Assert.IsType<FDFAnnotationText>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationText().GetCOSObject()));
        Assert.IsType<FDFAnnotationCaret>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationCaret().GetCOSObject()));
        Assert.IsType<FDFAnnotationFreeText>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationFreeText().GetCOSObject()));
        Assert.IsType<FDFAnnotationFileAttachment>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationFileAttachment().GetCOSObject()));
        Assert.IsType<FDFAnnotationInk>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationInk().GetCOSObject()));
        Assert.IsType<FDFAnnotationLink>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationLink().GetCOSObject()));
        Assert.IsType<FDFAnnotationSquare>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationSquare().GetCOSObject()));
        Assert.IsType<FDFAnnotationCircle>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationCircle().GetCOSObject()));
        Assert.IsType<FDFAnnotationLine>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationLine().GetCOSObject()));
        Assert.IsType<FDFAnnotationPolygon>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationPolygon().GetCOSObject()));
        Assert.IsType<FDFAnnotationPolyline>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationPolyline().GetCOSObject()));
        Assert.IsType<FDFAnnotationSound>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationSound().GetCOSObject()));
        Assert.IsType<FDFAnnotationSquiggly>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationSquiggly().GetCOSObject()));
        Assert.IsType<FDFAnnotationStamp>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationStamp().GetCOSObject()));
        Assert.IsType<FDFAnnotationStrikeOut>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationStrikeOut().GetCOSObject()));
        Assert.IsType<FDFAnnotationUnderline>(FDFAnnotation.Create((PdfBox.Net.COS.COSDictionary)new FDFAnnotationUnderline().GetCOSObject()));
    }

    [Fact]
    public void AnnotationFactoryReturnsNullForUnknownSubtype()
    {
        PdfBox.Net.COS.COSDictionary dictionary = new();
        dictionary.SetName(PdfBox.Net.COS.COSName.SUBTYPE, "UnknownSubtype");

        Assert.Null(FDFAnnotation.Create(dictionary));
        Assert.Null(FDFAnnotation.Create(null));
    }

    [Fact]
    public void DictionaryAnnotationRoundTripPreservesRepresentativeProperties()
    {
        FDFAnnotationHighlight highlight = new();
        highlight.SetCoords([1, 2, 3, 4, 5, 6, 7, 8]);

        FDFAnnotationText text = new();
        text.SetIcon("Comment");
        text.SetState("Accepted");
        text.SetStateModel("Review");

        FDFAnnotationLink link = new();
        PDActionURI uri = new();
        uri.SetURI("https://example.com");
        link.SetAction(uri);

        FDFAnnotationSquare square = new();
        square.SetInteriorColor([0.1f, 0.2f, 0.3f]);
        square.SetFringe(new PDRectangle(1, 2, 3, 4));

        FDFAnnotationCircle circle = new();
        circle.SetInteriorColor([0.4f, 0.5f, 0.6f]);
        circle.SetFringe(new PDRectangle(2, 3, 4, 5));

        FDFAnnotationLine line = new();
        line.SetLine([10, 20, 30, 40]);
        line.SetStartPointEndingStyle("OpenArrow");
        line.SetEndPointEndingStyle("ClosedArrow");
        line.SetCaption(true);
        line.SetCaptionHorizontalOffset(6);
        line.SetCaptionVerticalOffset(7);

        FDFAnnotationSquiggly squiggly = new();
        squiggly.SetCoords([11, 12, 13, 14, 15, 16, 17, 18]);

        FDFDictionary dictionary = new();
        dictionary.SetAnnotations([highlight, text, link, square, circle, line, squiggly]);

        List<FDFAnnotation>? annotations = dictionary.GetAnnotations();

        Assert.NotNull(annotations);
        Assert.Collection(annotations!,
            a =>
            {
                float[]? coords = Assert.IsType<FDFAnnotationHighlight>(a).GetCoords();
                Assert.NotNull(coords);
                Assert.Equal([1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f], coords);
            },
            a =>
            {
                FDFAnnotationText loaded = Assert.IsType<FDFAnnotationText>(a);
                Assert.Equal("Comment", loaded.GetIcon());
                Assert.Equal("Accepted", loaded.GetState());
                Assert.Equal("Review", loaded.GetStateModel());
            },
            a =>
            {
                FDFAnnotationLink loaded = Assert.IsType<FDFAnnotationLink>(a);
                Assert.Equal("https://example.com", Assert.IsType<PDActionURI>(loaded.GetAction()).GetURI());
            },
            a =>
            {
                FDFAnnotationSquare loaded = Assert.IsType<FDFAnnotationSquare>(a);
                float[]? interiorColor = loaded.GetInteriorColor();
                Assert.NotNull(interiorColor);
                Assert.Equal([0.1f, 0.2f, 0.3f], interiorColor);
                Assert.NotNull(loaded.GetFringe());
            },
            a =>
            {
                FDFAnnotationCircle loaded = Assert.IsType<FDFAnnotationCircle>(a);
                float[]? interiorColor = loaded.GetInteriorColor();
                Assert.NotNull(interiorColor);
                Assert.Equal([0.4f, 0.5f, 0.6f], interiorColor);
                Assert.NotNull(loaded.GetFringe());
            },
            a =>
            {
                FDFAnnotationLine loaded = Assert.IsType<FDFAnnotationLine>(a);
                float[]? lineCoords = loaded.GetLine();
                Assert.NotNull(lineCoords);
                Assert.Equal([10f, 20f, 30f, 40f], lineCoords);
                Assert.Equal("OpenArrow", loaded.GetStartPointEndingStyle());
                Assert.Equal("ClosedArrow", loaded.GetEndPointEndingStyle());
                Assert.True(loaded.GetCaption());
                Assert.Equal(6, loaded.GetCaptionHorizontalOffset());
                Assert.Equal(7, loaded.GetCaptionVerticalOffset());
            },
            a =>
            {
                float[]? coords = Assert.IsType<FDFAnnotationSquiggly>(a).GetCoords();
                Assert.NotNull(coords);
                Assert.Equal([11f, 12f, 13f, 14f, 15f, 16f, 17f, 18f], coords);
            });
    }
}
