/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDBorderStyleDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDBorderStyleDictionary : COSObjectable
{
    public const string STYLE_SOLID = "S";
    public const string STYLE_DASHED = "D";
    public const string STYLE_BEVELED = "B";
    public const string STYLE_INSET = "I";
    public const string STYLE_UNDERLINE = "U";

    private readonly COSDictionary dictionary;

    public PDBorderStyleDictionary()
        : this(new COSDictionary())
    {
    }

    public PDBorderStyleDictionary(COSDictionary dictionary)
    {
        this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public float GetWidth()
    {
        return dictionary.GetFloat(COSName.W, 1);
    }

    public void SetWidth(float width)
    {
        dictionary.SetFloat(COSName.W, width);
    }

    public string? GetStyle()
    {
        return dictionary.GetNameAsString(COSName.S);
    }

    public void SetStyle(string? style)
    {
        dictionary.SetName(COSName.S, style);
    }

    public COSArray? GetDashStyle()
    {
        return dictionary.GetCOSArray(COSName.D);
    }

    public void SetDashStyle(COSArray? dashStyle)
    {
        dictionary.SetItem(COSName.D, dashStyle);
    }

    public COSBase GetCOSObject()
    {
        return dictionary;
    }
}
