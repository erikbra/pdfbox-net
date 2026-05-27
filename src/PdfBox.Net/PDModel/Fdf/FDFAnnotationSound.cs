using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationSound : FDFAnnotation
{
    public const string Subtype = "Sound";

    public FDFAnnotationSound()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationSound(COSDictionary annotation)
        : base(annotation)
    {
    }
}
