using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationPolyline : FDFAnnotation
{
    private const string LineEndingNone = "None";

    public const string Subtype = "Polyline";

    public FDFAnnotationPolyline()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationPolyline(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetVertices(float[]? vertices) => Annot.SetItem(COSName.VERTICES, vertices is null ? null : COSArray.Of(vertices));

    public float[]? GetVertices() => Annot.GetCOSArray(COSName.VERTICES)?.ToFloatArray();

    public void SetStartPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(COSName.LE);
        if (array is null)
        {
            Annot.SetItem(COSName.LE, COSArray.Of(COSName.GetPDFName(actualStyle), COSName.GetPDFName(LineEndingNone)));
            return;
        }

        array.SetName(0, actualStyle);
    }

    public string GetStartPointEndingStyle()
    {
        return Annot.GetCOSArray(COSName.LE)?.GetName(0, LineEndingNone) ?? LineEndingNone;
    }

    public void SetEndPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(COSName.LE);
        if (array is null)
        {
            Annot.SetItem(COSName.LE, COSArray.Of(COSName.GetPDFName(LineEndingNone), COSName.GetPDFName(actualStyle)));
            return;
        }

        array.SetName(1, actualStyle);
    }

    public string GetEndPointEndingStyle()
    {
        return Annot.GetCOSArray(COSName.LE)?.GetName(1, LineEndingNone) ?? LineEndingNone;
    }

    public void SetInteriorColor(float[]? color) => Annot.SetItem(COSName.IC, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(COSName.IC);
}
