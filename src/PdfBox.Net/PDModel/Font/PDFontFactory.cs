/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted PDFont factory for creating concrete PDModel font implementations.
 *
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Font;

public static class PDFontFactory
{
    private static readonly COSName SubtypeKey = COSName.GetPDFName("Subtype");

    public static PDFont CreateFont(COSDictionary dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        string? subtype = dictionary.GetNameAsString(SubtypeKey);
        return subtype switch
        {
            "Type0" => PDType0Font.Load(dictionary),
            "Type1" or "MMType1" => PDType1Font.Load(dictionary),
            "TrueType" => PDTrueTypeFont.Load(dictionary) ?? (PDFont)PDDictionaryFont.Create(dictionary),
            "CIDFontType0" => new PDCIDFontType0(dictionary),
            "CIDFontType2" => PDCIDFontType2.Load(dictionary),
            _ => PDDictionaryFont.Create(dictionary),
        };
    }

    internal static PDCIDFont CreateDescendantFont(COSDictionary dictionary)
    {
        string? subtype = dictionary.GetNameAsString(SubtypeKey);
        return subtype switch
        {
            "CIDFontType2" => PDCIDFontType2.Load(dictionary),
            _ => new PDCIDFontType0(dictionary),
        };
    }
}
