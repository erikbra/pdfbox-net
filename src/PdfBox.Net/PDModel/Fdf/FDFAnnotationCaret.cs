using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationCaret : FDFAnnotation
{
    public const string Subtype = "Caret";

    public FDFAnnotationCaret()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationCaret(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetFringe(PDRectangle? fringe) => Annot.SetItem(COSName.RD, fringe);

    public PDRectangle? GetFringe()
    {
        COSArray? array = Annot.GetCOSArray(COSName.RD);
        return array is null ? null : new PDRectangle(array);
    }

    public void SetSymbol(string? symbol)
    {
        string newSymbol = symbol == "paragraph" ? "P" : "None";
        Annot.SetString(COSName.SY, newSymbol);
    }

    public string? GetSymbol() => Annot.GetString(COSName.SY);
}
