using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationHighlight : FDFAnnotationTextMarkup
{
    public const string Subtype = "Highlight";

    public FDFAnnotationHighlight()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationHighlight(COSDictionary annotation)
        : base(annotation)
    {
    }
}
