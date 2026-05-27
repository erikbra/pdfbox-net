using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationSquare : FDFAnnotation
{
    public const string Subtype = "Square";

    public FDFAnnotationSquare()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationSquare(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetInteriorColor(float[]? color) => Annot.SetItem(COSName.IC, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(COSName.IC);

    public void SetFringe(PDRectangle? fringe) => Annot.SetItem(COSName.RD, fringe);

    public PDRectangle? GetFringe()
    {
        COSArray? array = Annot.GetCOSArray(COSName.RD);
        return array is null ? null : new PDRectangle(array);
    }
}
