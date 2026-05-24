using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.Tests;

public class PDStreamTest
{
    [Fact]
    public void CreateOutputStream_WithFlateFilter_EncodesAndDecodesRoundtrip()
    {
        byte[] source = Encoding.ASCII.GetBytes("BT /F1 12 Tf 72 700 Td (PDStream Flate) Tj ET");
        PDStream stream = new();

        using (Stream output = stream.CreateOutputStream(COSName.FLATE_DECODE))
        {
            output.Write(source, 0, source.Length);
        }

        using Stream input = stream.CreateInputStream();
        using MemoryStream decoded = new();
        input.CopyTo(decoded);

        Assert.Equal(source, decoded.ToArray());
        Assert.Single(stream.GetFilters());
        Assert.Equal(COSName.FLATE_DECODE, stream.GetFilters()[0]);
    }
}
