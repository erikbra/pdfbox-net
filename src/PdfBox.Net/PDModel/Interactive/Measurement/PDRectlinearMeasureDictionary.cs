/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/measurement/PDRectlinearMeasureDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Measurement;

public class PDRectlinearMeasureDictionary : PDMeasureDictionary
{
    public const string SUBTYPE = "RL";

    public PDRectlinearMeasureDictionary()
    {
        SetSubtype(SUBTYPE);
    }

    public PDRectlinearMeasureDictionary(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public string GetScaleRatio() => Dictionary.GetString(COSName.GetPDFName("R"), string.Empty);
    public void SetScaleRatio(string? scaleRatio) => Dictionary.SetString(COSName.GetPDFName("R"), scaleRatio);

    public PDNumberFormatDictionary[]? GetChangeXs() => GetNumberFormats(COSName.GetPDFName("X"));
    public void SetChangeXs(PDNumberFormatDictionary[] values) => SetNumberFormats(COSName.GetPDFName("X"), values);

    public PDNumberFormatDictionary[]? GetChangeYs() => GetNumberFormats(COSName.GetPDFName("Y"));
    public void SetChangeYs(PDNumberFormatDictionary[] values) => SetNumberFormats(COSName.GetPDFName("Y"), values);

    public PDNumberFormatDictionary[]? GetDistances() => GetNumberFormats(COSName.GetPDFName("D"));
    public void SetDistances(PDNumberFormatDictionary[] values) => SetNumberFormats(COSName.GetPDFName("D"), values);

    public PDNumberFormatDictionary[]? GetAreas() => GetNumberFormats(COSName.GetPDFName("A"));
    public void SetAreas(PDNumberFormatDictionary[] values) => SetNumberFormats(COSName.GetPDFName("A"), values);

    public PDNumberFormatDictionary[]? GetAngles() => GetNumberFormats(COSName.GetPDFName("T"));
    public void SetAngles(PDNumberFormatDictionary[] values) => SetNumberFormats(COSName.GetPDFName("T"), values);

    public PDNumberFormatDictionary[]? GetLineSloaps() => GetNumberFormats(COSName.GetPDFName("S"));
    public void SetLineSloaps(PDNumberFormatDictionary[] values) => SetNumberFormats(COSName.GetPDFName("S"), values);
    public PDNumberFormatDictionary[]? GetLineSlopes() => GetLineSloaps();
    public void SetLineSlopes(PDNumberFormatDictionary[] values) => SetLineSloaps(values);

    public float[]? GetCoordSystemOrigin() => Dictionary.GetCOSArray(COSName.GetPDFName("O"))?.ToFloatArray();
    public void SetCoordSystemOrigin(float[] coordSystemOrigin) => Dictionary.SetItem(COSName.GetPDFName("O"), COSArray.Of(coordSystemOrigin));

    public float GetCYX() => Dictionary.GetFloat(COSName.GetPDFName("CYX"));
    public void SetCYX(float cyx) => Dictionary.SetFloat(COSName.GetPDFName("CYX"), cyx);

    private PDNumberFormatDictionary[]? GetNumberFormats(COSName key)
    {
        COSArray? array = Dictionary.GetCOSArray(key);
        if (array == null)
        {
            return null;
        }

        List<PDNumberFormatDictionary> result = [];
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary dict)
            {
                result.Add(new PDNumberFormatDictionary(dict));
            }
        }

        return result.ToArray();
    }

    private void SetNumberFormats(COSName key, IEnumerable<PDNumberFormatDictionary> values)
    {
        COSArray array = new();
        foreach (PDNumberFormatDictionary value in values)
        {
            array.Add(value);
        }
        Dictionary.SetItem(key, array);
    }
}
