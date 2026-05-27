using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationLine : FDFAnnotation
{
    private const string LineEndingNone = "None";

    public const string Subtype = "Line";

    public FDFAnnotationLine()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationLine(COSDictionary annotation)
        : base(annotation)
    {
    }

    public void SetLine(float[]? line) => Annot.SetItem(COSName.L, line is null ? null : COSArray.Of(line));

    public float[]? GetLine() => Annot.GetCOSArray(COSName.L)?.ToFloatArray();

    public void SetStartPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(COSName.LE);
        if (array is null)
        {
            Annot.SetItem(COSName.LE, COSArray.Of(COSName.GetPDFName(actualStyle), COSName.GetPDFName(LineEndingNone)));
            return;
        }

        array.SetName(0, actualStyle);
    }

    public string GetStartPointEndingStyle()
    {
        return Annot.GetCOSArray(COSName.LE)?.GetName(0, LineEndingNone) ?? LineEndingNone;
    }

    public void SetEndPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(COSName.LE);
        if (array is null)
        {
            Annot.SetItem(COSName.LE, COSArray.Of(COSName.GetPDFName(LineEndingNone), COSName.GetPDFName(actualStyle)));
            return;
        }

        array.SetName(1, actualStyle);
    }

    public string GetEndPointEndingStyle()
    {
        return Annot.GetCOSArray(COSName.LE)?.GetName(1, LineEndingNone) ?? LineEndingNone;
    }

    public void SetInteriorColor(float[]? color) => Annot.SetItem(COSName.IC, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(COSName.IC);

    public void SetCaption(bool cap) => Annot.SetBoolean(COSName.CAP, cap);

    public bool GetCaption() => Annot.GetBoolean(COSName.CAP, false);

    public float GetLeaderLength() => Annot.GetFloat(COSName.LL);

    public void SetLeaderLength(float leaderLength) => Annot.SetFloat(COSName.LL, leaderLength);

    public float GetLeaderExtend() => Annot.GetFloat(COSName.LLE);

    public void SetLeaderExtend(float leaderExtend) => Annot.SetFloat(COSName.LLE, leaderExtend);

    public float GetLeaderOffset() => Annot.GetFloat(COSName.LLO);

    public void SetLeaderOffset(float leaderOffset) => Annot.SetFloat(COSName.LLO, leaderOffset);

    public string? GetCaptionStyle() => Annot.GetString(COSName.CP);

    public void SetCaptionStyle(string? captionStyle) => Annot.SetString(COSName.CP, captionStyle);

    public void SetCaptionHorizontalOffset(float offset)
    {
        COSName co = COSName.GetPDFName("CO");
        COSArray? array = Annot.GetCOSArray(co);
        if (array is null)
        {
            Annot.SetItem(co, COSArray.Of(offset, 0f));
            return;
        }

        array.Set(0, new COSFloat(offset));
    }

    public float GetCaptionHorizontalOffset()
    {
        COSArray? array = Annot.GetCOSArray(COSName.GetPDFName("CO"));
        return array?.ToFloatArray().ElementAtOrDefault(0) ?? 0f;
    }

    public void SetCaptionVerticalOffset(float offset)
    {
        COSName co = COSName.GetPDFName("CO");
        COSArray? array = Annot.GetCOSArray(co);
        if (array is null)
        {
            Annot.SetItem(co, COSArray.Of(0f, offset));
            return;
        }

        array.Set(1, new COSFloat(offset));
    }

    public float GetCaptionVerticalOffset()
    {
        COSArray? array = Annot.GetCOSArray(COSName.GetPDFName("CO"));
        return array?.ToFloatArray().ElementAtOrDefault(1) ?? 0f;
    }
}
