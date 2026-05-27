using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationFreeText : FDFAnnotation
{
    private static readonly COSName ClName = COSName.GetPDFName("CL");
    private static readonly COSName QName = COSName.GetPDFName("Q");
    private static readonly COSName DaName = COSName.GetPDFName("DA");
    private static readonly COSName DsName = COSName.GetPDFName("DS");
    private static readonly COSName RdName = COSName.GetPDFName("RD");
    private static readonly COSName LeName = COSName.GetPDFName("LE");

    public const string Subtype = "FreeText";

    public FDFAnnotationFreeText()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationFreeText(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetCallout(float[]? callout) => Annot.SetItem(ClName, callout is null ? null : COSArray.Of(callout));

    public float[]? GetCallout() => Annot.GetCOSArray(ClName)?.ToFloatArray();

    public void SetJustification(string? justification)
    {
        int quadding = justification switch
        {
            "centered" => 1,
            "right" => 2,
            _ => 0
        };
        Annot.SetInt(QName, quadding);
    }

    public string GetJustification()
    {
        return Annot.GetInt(QName, 0) switch
        {
            1 => "centered",
            2 => "right",
            _ => "left"
        };
    }

    public void SetRotation(int rotation) => Annot.SetInt(COSName.ROTATE, rotation);

    public int? GetRotation()
    {
        return Annot.GetDictionaryObject(COSName.ROTATE) is COSNumber rotation ? rotation.IntValue() : null;
    }

    public void SetDefaultAppearance(string? appearance) => Annot.SetString(DaName, appearance);

    public string? GetDefaultAppearance() => Annot.GetString(DaName);

    public void SetDefaultStyle(string? style) => Annot.SetString(DsName, style);

    public string? GetDefaultStyle() => Annot.GetString(DsName);

    public void SetFringe(PDRectangle? fringe) => Annot.SetItem(RdName, fringe);

    public PDRectangle? GetFringe()
    {
        COSArray? array = Annot.GetCOSArray(RdName);
        return array is null ? null : new PDRectangle(array);
    }

    public void SetLineEndingStyle(string? style) => Annot.SetName(LeName, style);

    public string? GetLineEndingStyle() => Annot.GetNameAsString(LeName);
}
