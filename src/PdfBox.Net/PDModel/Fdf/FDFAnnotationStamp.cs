using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationStamp : FDFAnnotation
{
    public const string Subtype = "Stamp";

    public FDFAnnotationStamp()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationStamp(COSDictionary annotation)
        : base(annotation)
    {
    }
}
