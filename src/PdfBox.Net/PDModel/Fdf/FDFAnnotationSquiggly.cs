using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationSquiggly : FDFAnnotationTextMarkup
{
    public const string Subtype = "Squiggly";

    public FDFAnnotationSquiggly()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationSquiggly(COSDictionary annotation)
        : base(annotation)
    {
    }
}
