using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationPolygon : FDFAnnotation
{
    public const string Subtype = "Polygon";

    public FDFAnnotationPolygon()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationPolygon(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetVertices(float[]? vertices) => Annot.SetItem(COSName.VERTICES, vertices is null ? null : COSArray.Of(vertices));

    public float[]? GetVertices() => Annot.GetCOSArray(COSName.VERTICES)?.ToFloatArray();

    public void SetInteriorColor(float[]? color) => Annot.SetItem(COSName.IC, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(COSName.IC);
}
