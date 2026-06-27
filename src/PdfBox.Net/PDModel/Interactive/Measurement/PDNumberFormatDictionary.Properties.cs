/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/measurement/PDNumberFormatDictionary.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Measurement;

public partial class PDNumberFormatDictionary
{
    public float ConversionFactor
    {
        get => GetConversionFactor();
        set => SetConversionFactor(value);
    }

    public string DecimalSeparator
    {
        get => GetDecimalSeparator();
        set => SetDecimalSeparator(value);
    }

    public int Denominator
    {
        get => GetDenominator();
        set => SetDenominator(value);
    }

    public string FractionalDisplay
    {
        get => GetFractionalDisplay();
        set => SetFractionalDisplay(value);
    }

    public string LabelPositionToValue
    {
        get => GetLabelPositionToValue();
        set => SetLabelPositionToValue(value);
    }

    public string LabelPrefixString
    {
        get => GetLabelPrefixString();
        set => SetLabelPrefixString(value);
    }

    public string LabelSuffixString
    {
        get => GetLabelSuffixString();
        set => SetLabelSuffixString(value);
    }

    public string ThousandsSeparator
    {
        get => GetThousandsSeparator();
        set => SetThousandsSeparator(value);
    }

    public string Units
    {
        get => GetUnits();
        set => SetUnits(value);
    }
}
