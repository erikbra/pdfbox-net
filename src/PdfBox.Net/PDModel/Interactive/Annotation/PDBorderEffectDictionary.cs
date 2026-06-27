/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDBorderEffectDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDBorderEffectDictionary : COSObjectable
{
    public const string STYLE_SOLID = "S";
    public const string STYLE_CLOUDY = "C";

    private readonly COSDictionary dictionary;

    public PDBorderEffectDictionary()
        : this(new COSDictionary())
    {
    }

    public PDBorderEffectDictionary(COSDictionary dictionary)
    {
        this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public string? GetStyle()
    {
        return dictionary.GetNameAsString(COSName.S);
    }

    public void SetStyle(string? style)
    {
        dictionary.SetName(COSName.S, style);
    }

    public float GetIntensity()
    {
        return dictionary.GetFloat(COSName.GetPDFName("I"), 0);
    }

    public void SetIntensity(float intensity)
    {
        dictionary.SetFloat(COSName.GetPDFName("I"), intensity);
    }

    public COSBase GetCOSObject()
    {
        return dictionary;
    }
}
