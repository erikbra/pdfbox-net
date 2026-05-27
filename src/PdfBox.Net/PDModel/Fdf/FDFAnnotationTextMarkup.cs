using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public abstract class FDFAnnotationTextMarkup : FDFAnnotation
{
    private static readonly COSName QuadPointsName = COSName.GetPDFName("QuadPoints");
    protected FDFAnnotationTextMarkup()
    {
    }

    protected FDFAnnotationTextMarkup(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetCoords(float[]? coords)
    {
        Annot.SetItem(QuadPointsName, coords is null ? null : COSArray.Of(coords));
    }

    public float[]? GetCoords()
    {
        return Annot.GetCOSArray(QuadPointsName)?.ToFloatArray();
    }
}
