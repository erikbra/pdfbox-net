using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Image;

namespace PdfBox.Net.Tests;

public class PDImageXObjectTest
{
    [Fact]
    public void Interpolate_DefaultsFalseAndWritesDictionaryFlag()
    {
        PDStream stream = new();
        PDImageXObject image = new(stream, null);

        Assert.False(image.GetInterpolate());

        image.SetInterpolate(true);
        Assert.True(image.GetInterpolate());
        Assert.True(stream.GetCOSObject().GetBoolean(COSName.INTERPOLATE, false));

        image.SetInterpolate(false);
        Assert.False(image.GetInterpolate());
        Assert.False(stream.GetCOSObject().GetBoolean(COSName.INTERPOLATE, true));
    }

    [Fact]
    public void GetImageData_DecodesSimpleFlateEncodedDeviceRgbStream()
    {
        // 2x1 RGB pixels: red, green
        byte[] rgb = [255, 0, 0, 0, 255, 0];

        PDStream stream = new();
        using (Stream output = stream.CreateOutputStream(COSName.FLATE_DECODE))
        {
            output.Write(rgb, 0, rgb.Length);
        }

        COSStream cos = stream.GetCOSObject();
        cos.SetInt(COSName.WIDTH, 2);
        cos.SetInt(COSName.HEIGHT, 1);
        cos.SetInt(COSName.BITS_PER_COMPONENT, 8);
        cos.SetItem(COSName.COLORSPACE, COSName.GetPDFName("DeviceRGB"));

        PDImageXObject image = new(stream, null);

        Assert.Equal(2, image.GetWidth());
        Assert.Equal(1, image.GetHeight());
        Assert.Equal(8, image.GetBitsPerComponent());
        Assert.Equal(rgb, image.GetImageData());
    }
}
