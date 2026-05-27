using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationFreeText : FDFAnnotation
{
    public const string Subtype = "FreeText";

    public FDFAnnotationFreeText()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationFreeText(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetCallout(float[]? callout) => Annot.SetItem(COSName.CL, callout is null ? null : COSArray.Of(callout));

    public float[]? GetCallout() => Annot.GetCOSArray(COSName.CL)?.ToFloatArray();

    public void SetJustification(string? justification)
    {
        int quadding = justification switch
        {
            "centered" => 1,
            "right" => 2,
            _ => 0
        };
        Annot.SetInt(COSName.Q, quadding);
    }

    public string GetJustification() => Annot.GetInt(COSName.Q, 0).ToString();

    public void SetRotation(int rotation) => Annot.SetInt(COSName.ROTATE, rotation);

    public string? GetRotation() => Annot.GetString(COSName.ROTATE);

    public void SetDefaultAppearance(string? appearance) => Annot.SetString(COSName.DA, appearance);

    public string? GetDefaultAppearance() => Annot.GetString(COSName.DA);

    public void SetDefaultStyle(string? style) => Annot.SetString(COSName.DS, style);

    public string? GetDefaultStyle() => Annot.GetString(COSName.DS);

    public void SetFringe(PDRectangle? fringe) => Annot.SetItem(COSName.RD, fringe);

    public PDRectangle? GetFringe()
    {
        COSArray? array = Annot.GetCOSArray(COSName.RD);
        return array is null ? null : new PDRectangle(array);
    }

    public void SetLineEndingStyle(string? style) => Annot.SetName(COSName.LE, style);

    public string? GetLineEndingStyle() => Annot.GetNameAsString(COSName.LE);
}
