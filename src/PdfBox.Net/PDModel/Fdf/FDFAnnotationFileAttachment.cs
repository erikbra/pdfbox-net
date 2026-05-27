using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationFileAttachment : FDFAnnotation
{
    public const string Subtype = "FileAttachment";

    public FDFAnnotationFileAttachment()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationFileAttachment(COSDictionary annotation)
        : base(annotation)
    {
    }
}
