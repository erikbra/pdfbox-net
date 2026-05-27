using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Fdf;

public abstract class FDFAnnotation : COSObjectable
{
    private const int FlagInvisible = 1;
    private const int FlagHidden = 1 << 1;
    private const int FlagPrinted = 1 << 2;
    private const int FlagNoZoom = 1 << 3;
    private const int FlagNoRotate = 1 << 4;
    private const int FlagNoView = 1 << 5;
    private const int FlagReadOnly = 1 << 6;
    private const int FlagLocked = 1 << 7;
    private const int FlagToggleNoView = 1 << 8;
    private const int FlagLockedContents = 1 << 9;

    protected readonly COSDictionary Annot;

    protected FDFAnnotation()
    {
        Annot = new COSDictionary();
        Annot.SetItem(COSName.TYPE, COSName.ANNOT);
    }

    protected FDFAnnotation(COSDictionary annotation)
    {
        Annot = annotation ?? throw new ArgumentNullException(nameof(annotation));
    }

    public static FDFAnnotation? Create(COSDictionary? dictionary)
    {
        return dictionary?.GetNameAsString(COSName.SUBTYPE) switch
        {
            FDFAnnotationText.Subtype => new FDFAnnotationText(dictionary),
            FDFAnnotationCaret.Subtype => new FDFAnnotationCaret(dictionary),
            FDFAnnotationFreeText.Subtype => new FDFAnnotationFreeText(dictionary),
            FDFAnnotationFileAttachment.Subtype => new FDFAnnotationFileAttachment(dictionary),
            FDFAnnotationHighlight.Subtype => new FDFAnnotationHighlight(dictionary),
            FDFAnnotationInk.Subtype => new FDFAnnotationInk(dictionary),
            FDFAnnotationLine.Subtype => new FDFAnnotationLine(dictionary),
            FDFAnnotationLink.Subtype => new FDFAnnotationLink(dictionary),
            FDFAnnotationCircle.Subtype => new FDFAnnotationCircle(dictionary),
            FDFAnnotationSquare.Subtype => new FDFAnnotationSquare(dictionary),
            FDFAnnotationPolygon.Subtype => new FDFAnnotationPolygon(dictionary),
            FDFAnnotationPolyline.Subtype => new FDFAnnotationPolyline(dictionary),
            FDFAnnotationSound.Subtype => new FDFAnnotationSound(dictionary),
            FDFAnnotationSquiggly.Subtype => new FDFAnnotationSquiggly(dictionary),
            FDFAnnotationStamp.Subtype => new FDFAnnotationStamp(dictionary),
            FDFAnnotationStrikeOut.Subtype => new FDFAnnotationStrikeOut(dictionary),
            FDFAnnotationUnderline.Subtype => new FDFAnnotationUnderline(dictionary),
            _ => null
        };
    }

    public COSBase GetCOSObject() => Annot;

    public int? GetPage() => Annot.GetDictionaryObject(COSName.PAGE) is COSNumber page ? page.IntValue() : null;
    public void SetPage(int page) => Annot.SetInt(COSName.PAGE, page);

    public float[]? GetColor() => GetColor(COSName.C);

    protected float[]? GetColor(COSName colorName)
    {
        return Annot.GetCOSArray(colorName)?.ToFloatArray();
    }

    public void SetColor(float[]? color)
    {
        Annot.SetItem(COSName.C, color is null ? null : COSArray.Of(color));
    }

    public string? GetDate() => Annot.GetString(COSName.M);
    public void SetDate(string? date) => Annot.SetString(COSName.M, date);

    public bool IsInvisible() => Annot.GetFlag(COSName.F, FlagInvisible);
    public void SetInvisible(bool invisible) => Annot.SetFlag(COSName.F, FlagInvisible, invisible);

    public bool IsHidden() => Annot.GetFlag(COSName.F, FlagHidden);
    public void SetHidden(bool hidden) => Annot.SetFlag(COSName.F, FlagHidden, hidden);

    public bool IsPrinted() => Annot.GetFlag(COSName.F, FlagPrinted);
    public void SetPrinted(bool printed) => Annot.SetFlag(COSName.F, FlagPrinted, printed);

    public bool IsNoZoom() => Annot.GetFlag(COSName.F, FlagNoZoom);
    public void SetNoZoom(bool noZoom) => Annot.SetFlag(COSName.F, FlagNoZoom, noZoom);

    public bool IsNoRotate() => Annot.GetFlag(COSName.F, FlagNoRotate);
    public void SetNoRotate(bool noRotate) => Annot.SetFlag(COSName.F, FlagNoRotate, noRotate);

    public bool IsNoView() => Annot.GetFlag(COSName.F, FlagNoView);
    public void SetNoView(bool noView) => Annot.SetFlag(COSName.F, FlagNoView, noView);

    public bool IsReadOnly() => Annot.GetFlag(COSName.F, FlagReadOnly);
    public void SetReadOnly(bool readOnly) => Annot.SetFlag(COSName.F, FlagReadOnly, readOnly);

    public bool IsLocked() => Annot.GetFlag(COSName.F, FlagLocked);
    public void SetLocked(bool locked) => Annot.SetFlag(COSName.F, FlagLocked, locked);

    public bool IsToggleNoView() => Annot.GetFlag(COSName.F, FlagToggleNoView);
    public void SetToggleNoView(bool toggleNoView) => Annot.SetFlag(COSName.F, FlagToggleNoView, toggleNoView);

    public bool IsLockedContents() => Annot.GetFlag(COSName.F, FlagLockedContents);
    public void SetLockedContents(bool lockedContents) => Annot.SetFlag(COSName.F, FlagLockedContents, lockedContents);

    public string? GetName() => Annot.GetString(COSName.NM);
    public void SetName(string? name) => Annot.SetString(COSName.NM, name);

    public PDRectangle? GetRectangle()
    {
        COSArray? rect = Annot.GetCOSArray(COSName.RECT);
        return rect is null ? null : new PDRectangle(rect);
    }

    public void SetRectangle(PDRectangle? rectangle) => Annot.SetItem(COSName.RECT, rectangle);

    public string? GetContents() => Annot.GetString(COSName.CONTENTS);
    public void SetContents(string? contents) => Annot.SetString(COSName.CONTENTS, contents);

    public string? GetTitle() => Annot.GetString(COSName.T);
    public void SetTitle(string? title) => Annot.SetString(COSName.T, title);

    public DateTimeOffset? GetCreationDate() => Annot.GetDate(COSName.CREATION_DATE);
    public void SetCreationDate(DateTimeOffset? date) => Annot.SetDate(COSName.CREATION_DATE, date);

    public float GetOpacity() => Annot.GetFloat(COSName.CA, 1f);
    public void SetOpacity(float opacity) => Annot.SetFloat(COSName.CA, opacity);

    public string? GetSubject() => Annot.GetString(COSName.SUBJ);
    public void SetSubject(string? subject) => Annot.SetString(COSName.SUBJ, subject);

    public string? GetIntent() => Annot.GetNameAsString(COSName.IT);
    public void SetIntent(string? intent) => Annot.SetName(COSName.IT, intent);

    public string? GetRichContents() => GetStringOrStream(Annot.GetDictionaryObject(COSName.RC));
    public void SetRichContents(string? richContents)
    {
        Annot.SetItem(COSName.RC, richContents is null ? null : new COSString(richContents));
    }

    public PDBorderStyleDictionary? GetBorderStyle()
    {
        COSDictionary? dictionary = Annot.GetCOSDictionary(COSName.BS);
        return dictionary is null ? null : new PDBorderStyleDictionary(dictionary);
    }

    public void SetBorderStyle(PDBorderStyleDictionary? borderStyle) => Annot.SetItem(COSName.BS, borderStyle);

    public PDBorderEffectDictionary? GetBorderEffect()
    {
        COSDictionary? dictionary = Annot.GetCOSDictionary(COSName.BE);
        return dictionary is null ? null : new PDBorderEffectDictionary(dictionary);
    }

    public void SetBorderEffect(PDBorderEffectDictionary? borderEffect) => Annot.SetItem(COSName.BE, borderEffect);

    protected string? GetStringOrStream(COSBase? baseValue)
    {
        return baseValue switch
        {
            null => null,
            COSString cosString => cosString.GetString(),
            COSStream stream => stream.ToTextString(),
            _ => null
        };
    }
}
