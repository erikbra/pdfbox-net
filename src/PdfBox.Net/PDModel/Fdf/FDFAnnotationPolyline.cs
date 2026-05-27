using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationPolyline : FDFAnnotation
{
    private const string LineEndingNone = "None";
    private static readonly COSName VerticesName = COSName.GetPDFName("Vertices");
    private static readonly COSName LeName = COSName.GetPDFName("LE");
    private static readonly COSName IcName = COSName.GetPDFName("IC");

    public const string Subtype = "Polyline";

    public FDFAnnotationPolyline()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationPolyline(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetVertices(float[]? vertices) => Annot.SetItem(VerticesName, vertices is null ? null : COSArray.Of(vertices));

    public float[]? GetVertices() => Annot.GetCOSArray(VerticesName)?.ToFloatArray();

    public void SetStartPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(LeName);
        if (array is null)
        {
            COSArray created = new();
            created.Add(COSName.GetPDFName(actualStyle));
            created.Add(COSName.GetPDFName(LineEndingNone));
            Annot.SetItem(LeName, created);
            return;
        }

        array.SetName(0, actualStyle);
    }

    public string GetStartPointEndingStyle() => Annot.GetCOSArray(LeName)?.GetName(0, LineEndingNone) ?? LineEndingNone;

    public void SetEndPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(LeName);
        if (array is null)
        {
            COSArray created = new();
            created.Add(COSName.GetPDFName(LineEndingNone));
            created.Add(COSName.GetPDFName(actualStyle));
            Annot.SetItem(LeName, created);
            return;
        }

        array.SetName(1, actualStyle);
    }

    public string GetEndPointEndingStyle() => Annot.GetCOSArray(LeName)?.GetName(1, LineEndingNone) ?? LineEndingNone;

    public void SetInteriorColor(float[]? color) => Annot.SetItem(IcName, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(IcName);
}
