using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Tests;

public class UpstreamRenderingJiraFixturesTest
{
    public static TheoryData<string, string, int, bool> RenderFixtures => new()
    {
        { "PDFBOX-2359", "PDFBOX-2359-3.pdf", 0, true },
        { "PDFBOX-5953", "PDFBOX-5953-mail-test2-repaired.pdf", 0, true },
        { "PDFBOX-5953", "PDFBOX-5953-mail-test2-repaired.pdf", 6, true },
        { "PDFBOX-6024", "PDFBOX-6024-gs-bugzilla689309-reduced-bc1_RGB.pdf", 0, true },
        { "PDFBOX-6024", "PDFBOX-6024-gs-bugzilla689931-reduced-Multiply.pdf", 0, false },
        { "PDFBOX-6024", "PDFBOX-6024-gs-bugzilla689931-reduced-Screen.pdf", 0, false },
    };

    [Theory]
    [MemberData(nameof(RenderFixtures))]
    public void UpstreamRenderingFixture_RendersExpectedCurrentPageShape(string jiraKey, string pdfName, int pageIndex, bool expectVisible)
    {
        string pdfPath = FixturePath(pdfName);
        Assert.True(File.Exists(pdfPath), $"{jiraKey} fixture missing: {pdfPath}");

        using PDDocument document = Loader.LoadPDF(pdfPath);
        Assert.True(document.GetNumberOfPages() > pageIndex,
            $"{jiraKey} fixture {pdfName} has only {document.GetNumberOfPages()} pages.");

        using BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(pageIndex, 36f, ImageType.RGB);
        (bool nearBlank, int nonBackground, int dominant) = MeasureNearBlank(image);

        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
        Assert.Equal(expectVisible, !nearBlank);
    }

    private static (bool NearBlank, int NonBackground, int Dominant) MeasureNearBlank(BufferedImage image)
    {
        Dictionary<int, int> histogram = new();
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int rgb = image.GetRgb(x, y) & 0x00FFFFFF;
                histogram[rgb] = histogram.GetValueOrDefault(rgb) + 1;
            }
        }

        int dominant = histogram.Values.Max();
        int total = image.Width * image.Height;
        int nonBackground = total - dominant;
        return (nonBackground < Math.Max(16, total / 200), nonBackground, dominant);
    }

    private static string FixturePath(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "Rendering", "UpstreamJira", name);
    }
}
