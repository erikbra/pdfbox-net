using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationLine : FDFAnnotation
{
    private const string LineEndingNone = "None";
    private static readonly COSName LName = COSName.GetPDFName("L");
    private static readonly COSName LeName = COSName.GetPDFName("LE");
    private static readonly COSName IcName = COSName.GetPDFName("IC");
    private static readonly COSName CapName = COSName.GetPDFName("Cap");
    private static readonly COSName LlName = COSName.GetPDFName("LL");
    private static readonly COSName LleName = COSName.GetPDFName("LLE");
    private static readonly COSName LloName = COSName.GetPDFName("LLO");
    private static readonly COSName CpName = COSName.GetPDFName("CP");
    private static readonly COSName CoName = COSName.GetPDFName("CO");

    public const string Subtype = "Line";

    public FDFAnnotationLine()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationLine(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetLine(float[]? line) => Annot.SetItem(LName, line is null ? null : COSArray.Of(line));

    public float[]? GetLine() => Annot.GetCOSArray(LName)?.ToFloatArray();

    public void SetStartPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(LeName);
        if (array is null)
        {
            COSArray created = new();
            created.Add(COSName.GetPDFName(actualStyle));
            created.Add(COSName.GetPDFName(LineEndingNone));
            Annot.SetItem(LeName, created);
            return;
        }

        array.SetName(0, actualStyle);
    }

    public string GetStartPointEndingStyle() => Annot.GetCOSArray(LeName)?.GetName(0, LineEndingNone) ?? LineEndingNone;

    public void SetEndPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(LeName);
        if (array is null)
        {
            COSArray created = new();
            created.Add(COSName.GetPDFName(LineEndingNone));
            created.Add(COSName.GetPDFName(actualStyle));
            Annot.SetItem(LeName, created);
            return;
        }

        array.SetName(1, actualStyle);
    }

    public string GetEndPointEndingStyle() => Annot.GetCOSArray(LeName)?.GetName(1, LineEndingNone) ?? LineEndingNone;

    public void SetInteriorColor(float[]? color) => Annot.SetItem(IcName, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(IcName);

    public void SetCaption(bool cap) => Annot.SetBoolean(CapName, cap);

    public bool GetCaption() => Annot.GetBoolean(CapName, false);

    public float GetLeaderLength() => Annot.GetFloat(LlName);

    public void SetLeaderLength(float leaderLength) => Annot.SetFloat(LlName, leaderLength);

    public float GetLeaderExtend() => Annot.GetFloat(LleName);

    public void SetLeaderExtend(float leaderExtend) => Annot.SetFloat(LleName, leaderExtend);

    public float GetLeaderOffset() => Annot.GetFloat(LloName);

    public void SetLeaderOffset(float leaderOffset) => Annot.SetFloat(LloName, leaderOffset);

    public string? GetCaptionStyle() => Annot.GetString(CpName);

    public void SetCaptionStyle(string? captionStyle) => Annot.SetString(CpName, captionStyle);

    public void SetCaptionHorizontalOffset(float offset)
    {
        COSArray? array = Annot.GetCOSArray(CoName);
        if (array is null)
        {
            Annot.SetItem(CoName, COSArray.Of(offset, 0f));
            return;
        }

        array.Set(0, new COSFloat(offset));
    }

    public float GetCaptionHorizontalOffset()
    {
        float[]? values = Annot.GetCOSArray(CoName)?.ToFloatArray();
        return values is { Length: > 0 } ? values[0] : 0f;
    }

    public void SetCaptionVerticalOffset(float offset)
    {
        COSArray? array = Annot.GetCOSArray(CoName);
        if (array is null)
        {
            Annot.SetItem(CoName, COSArray.Of(0f, offset));
            return;
        }

        array.Set(1, new COSFloat(offset));
    }

    public float GetCaptionVerticalOffset()
    {
        float[]? values = Annot.GetCOSArray(CoName)?.ToFloatArray();
        return values is { Length: > 1 } ? values[1] : 0f;
    }
}
