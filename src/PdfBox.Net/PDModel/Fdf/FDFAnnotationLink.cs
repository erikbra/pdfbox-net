using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationLink : FDFAnnotation
{
    public const string Subtype = "Link";

    public FDFAnnotationLink()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationLink(COSDictionary annotation)
        : base(annotation)
    {
    }

    public PDAction? GetAction() => PDActionFactory.CreateAction(Annot.GetCOSDictionary(COSName.A));

    public void SetAction(PDAction? action) => Annot.SetItem(COSName.A, action);
}
