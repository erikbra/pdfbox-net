using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.Tests;

public class GraphicsBlendStateCloseoutTest
{
    [Fact]
    public void PDFontSetting_DefaultsToNullFontAndSizeOne()
    {
        PDFontSetting setting = new();

        Assert.Null(setting.GetFont());
        Assert.Equal(1f, setting.GetFontSize());

        setting.SetFontSize(12f);

        Assert.Equal(12f, setting.GetFontSize());
    }

    [Fact]
    public void CreateXObject_ReturnsPostScriptXObjectForPsSubtype()
    {
        COSStream stream = new();
        stream.SetName(COSName.SUBTYPE, "PS");

        PDXObject? xObject = PDXObject.CreateXObject(stream, null);

        Assert.IsType<PDPostScriptXObject>(xObject);
        Assert.Equal("PS", xObject!.GetSubtype());
    }

    [Fact]
    public void BlendMode_FromCosRecognizesExtendedModes()
    {
        Assert.Equal(BlendMode.COLOR_BURN, BlendModeExtensions.FromCos(COSName.GetPDFName("ColorBurn")));
        Assert.Equal(BlendMode.HUE, BlendModeExtensions.FromCos(COSName.GetPDFName("Hue")));

        COSArray array = new();
        array.Add(COSName.GetPDFName("Compatible"));
        array.Add(COSName.GetPDFName("SoftLight"));

        Assert.Equal(BlendMode.NORMAL, BlendModeExtensions.FromCos(array));
    }

    [Fact]
    public void BlendComposite_ComposesMultiplyChannelsWithConstantAlpha()
    {
        BlendComposite composite = BlendComposite.GetInstance(BlendMode.MULTIPLY, 0.5f);
        float[] result = new float[3];

        float alpha = composite.Compose(new[] { 0.8f, 0.4f, 0.2f }, 1f, new[] { 0.5f, 0.5f, 0.5f }, 0.25f, result);

        Assert.Equal(0.625f, alpha, 3);
        Assert.Equal(new[] { 0.44f, 0.32f, 0.26f }, result, new FloatArrayComparer(0.001f));
    }

    [Fact]
    public void PDTransparencyGroupAttributes_ExposesFlagsAndColorSpace()
    {
        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.CS, COSName.GetPDFName("DeviceRGB"));
        dictionary.SetBoolean(COSName.I, true);
        dictionary.SetBoolean(COSName.K, true);

        PDTransparencyGroupAttributes attributes = new(dictionary);

        Assert.NotNull(attributes.GetColorSpace());
        Assert.True(attributes.IsIsolated());
        Assert.True(attributes.IsKnockout());
    }

    [Fact]
    public void RenderingIntentAndModeHelpers_ReturnCanonicalValues()
    {
        Assert.Equal(RenderingIntent.RELATIVE_COLORIMETRIC, RenderingIntentExtensions.FromString("Unknown"));
        Assert.Equal("Perceptual", RenderingIntent.PERCEPTUAL.StringValue());

        RenderingMode mode = RenderingModeExtensions.FromInt(6);
        Assert.True(mode.IsFill());
        Assert.True(mode.IsStroke());
        Assert.True(mode.IsClip());
    }

    [Fact]
    public void GraphicsStateHelpers_UseRenderingEnums()
    {
        PDGraphicsState graphicsState = new();
        graphicsState.SetRenderingIntent(RenderingIntent.PERCEPTUAL);

        PDTextState textState = graphicsState.GetTextState();
        textState.SetRenderingMode(RenderingMode.FILL_STROKE_CLIP);

        PDExtendedGraphicsState extended = new();
        extended.SetRenderingIntent(RenderingIntent.SATURATION);

        Assert.Equal(RenderingIntent.PERCEPTUAL, graphicsState.GetRenderingIntentInstance());
        Assert.Equal(RenderingMode.FILL_STROKE_CLIP, textState.GetRenderingModeInstance());
        Assert.Equal(RenderingIntent.SATURATION, extended.GetRenderingIntentInstance());
    }

    private sealed class FloatArrayComparer : IEqualityComparer<float[]>
    {
        private readonly float _tolerance;

        public FloatArrayComparer(float tolerance)
        {
            _tolerance = tolerance;
        }

        public bool Equals(float[]? x, float[]? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.Length != y.Length)
            {
                return false;
            }

            for (int i = 0; i < x.Length; i++)
            {
                if (Math.Abs(x[i] - y[i]) > _tolerance)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(float[] obj) => obj.Length;
    }
}
