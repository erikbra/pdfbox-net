using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationCircle : FDFAnnotation
{
    private static readonly COSName IcName = COSName.GetPDFName("IC");
    private static readonly COSName RdName = COSName.GetPDFName("RD");

    public const string Subtype = "Circle";

    public FDFAnnotationCircle()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationCircle(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetInteriorColor(float[]? color) => Annot.SetItem(IcName, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(IcName);

    public void SetFringe(PDRectangle? fringe) => Annot.SetItem(RdName, fringe);

    public PDRectangle? GetFringe()
    {
        COSArray? array = Annot.GetCOSArray(RdName);
        return array is null ? null : new PDRectangle(array);
    }
}
