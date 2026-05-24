using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.Tests;

public class PDExtendedGraphicsStateTest
{
    [Fact]
    public void CopyIntoGraphicsState_AppliesAlphaBlendAndLineJoin()
    {
        PDExtendedGraphicsState ext = new();
        ext.SetStrokingAlphaConstant(0.25f);
        ext.SetNonStrokingAlphaConstant(0.75f);
        ext.SetBlendMode(BlendMode.MULTIPLY);
        ext.SetLineJoinStyle(2);

        PDGraphicsState gs = new();
        ext.CopyIntoGraphicsState(gs);

        Assert.Equal(0.25f, gs.GetAlphaConstant());
        Assert.Equal(0.75f, gs.GetNonStrokeAlphaConstant());
        Assert.Equal(BlendMode.MULTIPLY, gs.GetBlendMode());
        Assert.Equal(2, gs.GetLineJoin());
    }

    [Fact]
    public void GetLineDashPattern_ReadsDashArrayAndPhase()
    {
        COSArray dash = new();
        dash.Add(COSArray.Of(3f, 1f));
        dash.Add(COSInteger.Get(2));

        COSDictionary dict = new();
        dict.SetItem(COSName.GetPDFName("D"), dash);

        PDExtendedGraphicsState ext = new(dict);
        PDLineDashPattern? pattern = ext.GetLineDashPattern();

        Assert.NotNull(pattern);
        Assert.Equal(2, pattern!.GetPhase());
        Assert.Equal(new[] { 3f, 1f }, pattern.GetDashArray());
    }
}
