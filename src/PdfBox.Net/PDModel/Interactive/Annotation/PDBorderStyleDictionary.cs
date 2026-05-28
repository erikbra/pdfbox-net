using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDBorderStyleDictionary : COSObjectable
{
    private static readonly COSName WidthName = COSName.GetPDFName("W");
    private static readonly COSName StyleName = COSName.GetPDFName("S");
    private static readonly COSName DashName = COSName.GetPDFName("D");

    public const string STYLE_SOLID = "S";
    public const string STYLE_DASHED = "D";
    public const string STYLE_BEVELED = "B";
    public const string STYLE_INSET = "I";
    public const string STYLE_UNDERLINE = "U";

    private readonly COSDictionary _dictionary;

    public PDBorderStyleDictionary()
        : this(new COSDictionary())
    {
    }

    public PDBorderStyleDictionary(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    public float GetWidth() => _dictionary.GetFloat(WidthName, 1f);

    public void SetWidth(float width) => _dictionary.SetFloat(WidthName, width);

    public string GetStyle() => _dictionary.GetNameAsString(StyleName) ?? STYLE_SOLID;

    public void SetStyle(string style) => _dictionary.SetName(StyleName, style);

    public COSArray GetDashStyle()
    {
        return _dictionary.GetCOSArray(DashName) ?? new COSArray { new COSFloat(3f) };
    }

    public void SetDashStyle(float[] dashArray)
    {
        COSArray array = new();
        foreach (float value in dashArray)
        {
            array.Add(new COSFloat(value));
        }

        _dictionary.SetItem(DashName, array);
    }

    public void SetDashStyle(COSArray? dashStyle)
    {
        _dictionary.SetItem(DashName, dashStyle);
    }
}
