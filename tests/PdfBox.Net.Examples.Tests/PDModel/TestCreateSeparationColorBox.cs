// PORT_MODE: mechanical

using PdfBox.Net.Examples.PDModel;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Examples.Tests.PDModel;

public class TestCreateSeparationColorBox
{
    [Fact]
    public void TestCreateSeparationColorBoxExample()
    {
        string outputDir = Path.Combine(Path.GetTempPath(), "pdfbox-examples-separation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDir);
        string filename = Path.Combine(outputDir, "SeparationColorBox.pdf");

        CreateSeparationColorBox.Main([filename]);

        Assert.True(File.Exists(filename), "CreateSeparationColorBox should have created the PDF");

        using PDDocument document = PDDocument.Load(filename);
        Assert.Equal(1, document.GetNumberOfPages());
    }
}
