using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationPolygon : FDFAnnotation
{
    private static readonly COSName VerticesName = COSName.GetPDFName("Vertices");
    private static readonly COSName IcName = COSName.GetPDFName("IC");

    public const string Subtype = "Polygon";

    public FDFAnnotationPolygon()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationPolygon(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetVertices(float[]? vertices) => Annot.SetItem(VerticesName, vertices is null ? null : COSArray.Of(vertices));

    public float[]? GetVertices() => Annot.GetCOSArray(VerticesName)?.ToFloatArray();

    public void SetInteriorColor(float[]? color) => Annot.SetItem(IcName, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(IcName);
}
