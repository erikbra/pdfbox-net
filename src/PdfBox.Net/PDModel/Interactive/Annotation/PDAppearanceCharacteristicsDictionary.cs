/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAppearanceCharacteristicsDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAppearanceCharacteristicsDictionary : COSObjectable
{
    private static readonly COSName BorderColorName = COSName.GetPDFName("BC");
    private static readonly COSName BackgroundColorName = COSName.GetPDFName("BG");
    private static readonly COSName RotationName = COSName.GetPDFName("R");
    private static readonly COSName NormalIconName = COSName.GetPDFName("I");
    private static readonly COSName RolloverIconName = COSName.GetPDFName("RI");
    private static readonly COSName AlternateIconName = COSName.GetPDFName("IX");

    private readonly COSDictionary _dictionary;

    public PDAppearanceCharacteristicsDictionary()
        : this(new COSDictionary())
    {
    }

    public PDAppearanceCharacteristicsDictionary(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    public PDColor? GetBorderColour() => GetColor(BorderColorName);

    public void SetBorderColour(PDColor? color) => SetColor(BorderColorName, color);

    public PDColor? GetBackground() => GetColor(BackgroundColorName);

    public void SetBackground(PDColor? color) => SetColor(BackgroundColorName, color);

    public int GetRotation() => _dictionary.GetInt(RotationName, 0);

    public void SetRotation(int rotation) => _dictionary.SetInt(RotationName, rotation);

    public string? GetNormalCaption() => _dictionary.GetString(COSName.GetPDFName("CA"));

    public void SetNormalCaption(string? caption) => _dictionary.SetString(COSName.GetPDFName("CA"), caption);

    public string? GetRolloverCaption() => _dictionary.GetString(COSName.GetPDFName("RC"));

    public void SetRolloverCaption(string? caption) => _dictionary.SetString(COSName.GetPDFName("RC"), caption);

    public string? GetAlternateCaption() => _dictionary.GetString(COSName.GetPDFName("AC"));

    public void SetAlternateCaption(string? caption) => _dictionary.SetString(COSName.GetPDFName("AC"), caption);

    public PDFormXObject? GetNormalIcon()
    {
        return _dictionary.GetCOSStream(NormalIconName) is COSStream stream ? new PDFormXObject(stream) : null;
    }

    public PDFormXObject? GetRolloverIcon()
    {
        return _dictionary.GetCOSStream(RolloverIconName) is COSStream stream ? new PDFormXObject(stream) : null;
    }

    public PDFormXObject? GetAlternateIcon()
    {
        return _dictionary.GetCOSStream(AlternateIconName) is COSStream stream ? new PDFormXObject(stream) : null;
    }

    private PDColor? GetColor(COSName key)
    {
        if (_dictionary.GetCOSArray(key) is not COSArray array)
        {
            return null;
        }

        return array.Size() switch
        {
            1 => new PDColor(array, PDDeviceGray.Instance),
            3 => new PDColor(array, PDDeviceRGB.Instance),
            4 => new PDColor(array, PDDeviceCMYK.Instance),
            _ => null
        };
    }

    private void SetColor(COSName key, PDColor? color)
    {
        _dictionary.SetItem(key, color?.ToCOSArray());
    }
}
