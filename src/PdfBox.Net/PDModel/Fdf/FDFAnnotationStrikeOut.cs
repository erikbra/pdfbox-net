using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationStrikeOut : FDFAnnotationTextMarkup
{
    public const string Subtype = "StrikeOut";

    public FDFAnnotationStrikeOut()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationStrikeOut(COSDictionary annotation)
        : base(annotation)
    {
    }
}
