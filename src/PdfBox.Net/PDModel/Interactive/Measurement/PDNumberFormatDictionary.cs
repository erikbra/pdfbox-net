/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/measurement/PDNumberFormatDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Measurement;

public partial class PDNumberFormatDictionary : COSObjectable
{
    public const string TYPE = "NumberFormat";
    public const string LABEL_SUFFIX_TO_VALUE = "S";
    public const string LABEL_PREFIX_TO_VALUE = "P";
    public const string FRACTIONAL_DISPLAY_DECIMAL = "D";
    public const string FRACTIONAL_DISPLAY_FRACTION = "F";
    public const string FRACTIONAL_DISPLAY_ROUND = "R";
    public const string FRACTIONAL_DISPLAY_TRUNCATE = "T";

    private readonly COSDictionary _dictionary;

    public PDNumberFormatDictionary()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetName(COSName.TYPE, TYPE);
    }

    public PDNumberFormatDictionary(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    public string GetTypeName() => TYPE;

    public string GetUnits() => _dictionary.GetString(COSName.GetPDFName("U"), string.Empty);
    public void SetUnits(string? units) => _dictionary.SetString(COSName.GetPDFName("U"), units);

    public float GetConversionFactor() => _dictionary.GetFloat(COSName.GetPDFName("C"));
    public void SetConversionFactor(float conversionFactor) => _dictionary.SetFloat(COSName.GetPDFName("C"), conversionFactor);

    public string GetFractionalDisplay() => _dictionary.GetString(COSName.GetPDFName("F"), FRACTIONAL_DISPLAY_DECIMAL);

    public void SetFractionalDisplay(string? fractionalDisplay)
    {
        if (fractionalDisplay is null || fractionalDisplay is FRACTIONAL_DISPLAY_DECIMAL or FRACTIONAL_DISPLAY_FRACTION or FRACTIONAL_DISPLAY_ROUND or FRACTIONAL_DISPLAY_TRUNCATE)
        {
            _dictionary.SetString(COSName.GetPDFName("F"), fractionalDisplay);
            return;
        }

        throw new ArgumentException("Value must be \"D\", \"F\", \"R\", or \"T\", (or null).", nameof(fractionalDisplay));
    }

    public int GetDenominator() => _dictionary.GetInt(COSName.GetPDFName("D"));
    public void SetDenominator(int denominator) => _dictionary.SetInt(COSName.GetPDFName("D"), denominator);

    public bool IsFD() => _dictionary.GetBoolean(COSName.GetPDFName("FD"), false);
    public void SetFD(bool fd) => _dictionary.SetBoolean(COSName.GetPDFName("FD"), fd);

    public string GetThousandsSeparator() => _dictionary.GetString(COSName.GetPDFName("RT"), ",");
    public void SetThousandsSeparator(string? value) => _dictionary.SetString(COSName.GetPDFName("RT"), value);

    public string GetDecimalSeparator() => _dictionary.GetString(COSName.GetPDFName("RD"), ".");
    public void SetDecimalSeparator(string? value) => _dictionary.SetString(COSName.GetPDFName("RD"), value);

    public string GetLabelPrefixString() => _dictionary.GetString(COSName.GetPDFName("PS"), " ");
    public void SetLabelPrefixString(string? value) => _dictionary.SetString(COSName.GetPDFName("PS"), value);

    public string GetLabelSuffixString() => _dictionary.GetString(COSName.GetPDFName("SS"), " ");
    public void SetLabelSuffixString(string? value) => _dictionary.SetString(COSName.GetPDFName("SS"), value);

    public string GetLabelPositionToValue() => _dictionary.GetString(COSName.GetPDFName("O"), LABEL_SUFFIX_TO_VALUE);

    public void SetLabelPositionToValue(string? labelPositionToValue)
    {
        if (labelPositionToValue is null || labelPositionToValue is LABEL_PREFIX_TO_VALUE or LABEL_SUFFIX_TO_VALUE)
        {
            _dictionary.SetString(COSName.GetPDFName("O"), labelPositionToValue);
            return;
        }

        throw new ArgumentException("Value must be \"S\", or \"P\" (or null).", nameof(labelPositionToValue));
    }
}
