using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAppearanceCharacteristicsDictionary : COSObjectable
{
    private static readonly COSName BorderColorName = COSName.GetPDFName("BC");
    private static readonly COSName BackgroundColorName = COSName.GetPDFName("BG");
    private static readonly COSName RotationName = COSName.GetPDFName("R");

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
