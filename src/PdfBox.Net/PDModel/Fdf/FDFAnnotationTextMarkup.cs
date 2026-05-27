using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public abstract class FDFAnnotationTextMarkup : FDFAnnotation
{
    protected FDFAnnotationTextMarkup()
    {
    }

    protected FDFAnnotationTextMarkup(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetCoords(float[]? coords)
    {
        Annot.SetItem(COSName.QUADPOINTS, coords is null ? null : COSArray.Of(coords));
    }

    public float[]? GetCoords()
    {
        return Annot.GetCOSArray(COSName.QUADPOINTS)?.ToFloatArray();
    }
}
