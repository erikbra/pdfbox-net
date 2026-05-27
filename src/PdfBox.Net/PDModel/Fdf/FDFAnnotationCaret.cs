using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationCaret : FDFAnnotation
{
    private static readonly COSName RdName = COSName.GetPDFName("RD");
    private static readonly COSName SyName = COSName.GetPDFName("Sy");

    public const string Subtype = "Caret";

    public FDFAnnotationCaret()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationCaret(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetFringe(PDRectangle? fringe) => Annot.SetItem(RdName, fringe);

    public PDRectangle? GetFringe()
    {
        COSArray? array = Annot.GetCOSArray(RdName);
        return array is null ? null : new PDRectangle(array);
    }

    public void SetSymbol(string? symbol)
    {
        string newSymbol = symbol == "paragraph" ? "P" : "None";
        Annot.SetString(SyName, newSymbol);
    }

    public string? GetSymbol() => Annot.GetString(SyName);
}
