using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationUnderline : FDFAnnotationTextMarkup
{
    public const string Subtype = "Underline";

    public FDFAnnotationUnderline()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationUnderline(COSDictionary annotation)
        : base(annotation)
    {
    }
}
