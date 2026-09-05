using PdfBox.Net.COS;
using PdfBox.Net.Debugger.Streampane.Tooltip;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Tests;

public class Issue954AnnotationAndTooltipTest
{
    [Theory]
    [InlineData(0f)]
    [InlineData(0.25f)]
    [InlineData(1f)]
    public void EqualRgbAnnotationComponentsUseDeviceGray(float value)
    {
        PDAnnotationSquare annotation = new();
        annotation.SetColor(new PDColor([value, value, value], PDDeviceRGB.Instance));
        annotation.SetInteriorColor(new PDColor([value, value, value], PDDeviceRGB.Instance));

        PDColor color = Assert.IsType<PDColor>(annotation.GetColor());
        Assert.Same(PDDeviceGray.Instance, color.GetColorSpace());
        Assert.Equal([value], color.GetComponents());
        Assert.Equal(3, annotation.GetCOSDictionary().GetCOSArray(COSName.C)!.Size());
        Assert.Same(PDDeviceGray.Instance, annotation.GetInteriorColor()!.GetColorSpace());
    }

    [Fact]
    public void DifferentRgbAnnotationComponentsRemainRgb()
    {
        PDAnnotationSquare annotation = new();
        annotation.SetColor(new PDColor([0.25f, 0.5f, 0.25f], PDDeviceRGB.Instance));

        PDColor color = Assert.IsType<PDColor>(annotation.GetColor());
        Assert.Same(PDDeviceRGB.Instance, color.GetColorSpace());
        Assert.Equal([0.25f, 0.5f, 0.25f], color.GetComponents());
    }

    [Fact]
    public void FontTooltipTreatsDocumentFontNameAsText()
    {
        COSDictionary font = new();
        font.SetName(COSName.TYPE, "Font");
        font.SetName(COSName.SUBTYPE, "Type1");
        font.SetName(COSName.GetPDFName("BaseFont"), "<img src=\"https://example.invalid/a&b\">'font'");
        COSDictionary fonts = new();
        fonts.SetItem(COSName.GetPDFName("F1"), font);
        COSDictionary resources = new();
        resources.SetItem(COSName.GetPDFName("Font"), fonts);

        FontToolTip tooltip = new(new PDResources(resources), "/F1 12 Tf");

        Assert.Equal("<html>&lt;img src=&quot;https://example.invalid/a&amp;b&quot;&gt;&#39;font&#39;</html>", tooltip.ToolTipText);
    }
}
