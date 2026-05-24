/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted WinAnsi encoding for PDModel fonts.
 *
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.PDModel.Font.Encoding;

public sealed class WinAnsiEncoding : Encoding
{
    public static readonly WinAnsiEncoding INSTANCE = new();

    private WinAnsiEncoding()
    {
        // ASCII printable range.
        for (int code = 32; code <= 126; code++)
        {
            AddCharacterEncoding(code, PdfBox.Net.FontBox.Encoding.StandardEncoding.INSTANCE.GetName(code));
        }

        // Common WinAnsi high-byte mappings required by PDF standard fonts.
        AddCharacterEncoding(128, "Euro");
        AddCharacterEncoding(130, "quotesinglbase");
        AddCharacterEncoding(131, "florin");
        AddCharacterEncoding(132, "quotedblbase");
        AddCharacterEncoding(133, "ellipsis");
        AddCharacterEncoding(134, "dagger");
        AddCharacterEncoding(135, "daggerdbl");
        AddCharacterEncoding(136, "circumflex");
        AddCharacterEncoding(137, "perthousand");
        AddCharacterEncoding(138, "Scaron");
        AddCharacterEncoding(139, "guilsinglleft");
        AddCharacterEncoding(140, "OE");
        AddCharacterEncoding(145, "quoteleft");
        AddCharacterEncoding(146, "quoteright");
        AddCharacterEncoding(147, "quotedblleft");
        AddCharacterEncoding(148, "quotedblright");
        AddCharacterEncoding(149, "bullet");
        AddCharacterEncoding(150, "endash");
        AddCharacterEncoding(151, "emdash");
        AddCharacterEncoding(152, "tilde");
        AddCharacterEncoding(153, "trademark");
        AddCharacterEncoding(154, "scaron");
        AddCharacterEncoding(155, "guilsinglright");
        AddCharacterEncoding(156, "oe");
        AddCharacterEncoding(159, "Ydieresis");

        // ISO-8859-1 aligned range in WinAnsi.
        string[] latin1Glyphs =
        [
            "space", "exclamdown", "cent", "sterling", "currency", "yen", "brokenbar", "section",
            "dieresis", "copyright", "ordfeminine", "guillemotleft", "logicalnot", "hyphen", "registered", "macron",
            "degree", "plusminus", "twosuperior", "threesuperior", "acute", "mu", "paragraph", "periodcentered",
            "cedilla", "onesuperior", "ordmasculine", "guillemotright", "onequarter", "onehalf", "threequarters", "questiondown",
            "Agrave", "Aacute", "Acircumflex", "Atilde", "Adieresis", "Aring", "AE", "Ccedilla",
            "Egrave", "Eacute", "Ecircumflex", "Edieresis", "Igrave", "Iacute", "Icircumflex", "Idieresis",
            "Eth", "Ntilde", "Ograve", "Oacute", "Ocircumflex", "Otilde", "Odieresis", "multiply",
            "Oslash", "Ugrave", "Uacute", "Ucircumflex", "Udieresis", "Yacute", "Thorn", "germandbls",
            "agrave", "aacute", "acircumflex", "atilde", "adieresis", "aring", "ae", "ccedilla",
            "egrave", "eacute", "ecircumflex", "edieresis", "igrave", "iacute", "icircumflex", "idieresis",
            "eth", "ntilde", "ograve", "oacute", "ocircumflex", "otilde", "odieresis", "divide",
            "oslash", "ugrave", "uacute", "ucircumflex", "udieresis", "yacute", "thorn", "ydieresis",
        ];

        for (int i = 0; i < latin1Glyphs.Length; i++)
        {
            AddCharacterEncoding(160 + i, latin1Glyphs[i]);
        }
    }
}
