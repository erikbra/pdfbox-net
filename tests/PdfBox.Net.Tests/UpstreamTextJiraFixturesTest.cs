using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.Text;

namespace PdfBox.Net.Tests;

public class UpstreamTextJiraFixturesTest
{
    public static TheoryData<string, string, string> Fixtures => new()
    {
        {
            "PDFBOX-2138",
            "PDFBOX-2138.pdf",
            "PDFBOX-2138-java-output.txt"
        },
        {
            "PDFBOX-2532",
            "PDFBOX-2532-PDFBOX2247-701542.pdf",
            "PDFBOX-2532-acrobat-reference.txt"
        },
        {
            "PDFBOX-6188",
            "PDFBOX-6188-A151_src.pdf",
            "PDFBOX-6188-A151_src-content-stream.txt"
        },
    };

    [Theory]
    [MemberData(nameof(Fixtures))]
    public void UpstreamTextFixture_LoadsAndExtractsText(string jiraKey, string pdfName, string referenceName)
    {
        string pdfPath = FixturePath(pdfName);
        string referencePath = FixturePath(referenceName);

        Assert.True(File.Exists(pdfPath), $"{jiraKey} fixture missing: {pdfPath}");
        Assert.True(File.Exists(referencePath), $"{jiraKey} reference attachment missing: {referencePath}");

        using PDDocument document = Loader.LoadPDF(pdfPath);
        string extracted = new PDFTextStripper().GetText(document);

        Assert.False(string.IsNullOrWhiteSpace(extracted), $"{jiraKey} extracted no text.");
        Assert.False(string.IsNullOrWhiteSpace(File.ReadAllText(referencePath)), $"{jiraKey} reference attachment is empty.");
    }

    [Fact]
    public void PDFBOX6188_SortByPositionProbe_DocumentsDefaultAndVisualOrderOutputs()
    {
        using PDDocument document = Loader.LoadPDF(FixturePath("PDFBOX-6188-A151_src.pdf"));

        string streamOrderText = new PDFTextStripper().GetText(document);
        PDFTextStripper visualOrderStripper = new();
        visualOrderStripper.SetSortByPosition(true);
        string visualOrderText = visualOrderStripper.GetText(document);

        Assert.False(string.IsNullOrWhiteSpace(streamOrderText));
        Assert.False(string.IsNullOrWhiteSpace(visualOrderText));
        Assert.NotEqual(streamOrderText, visualOrderText);
    }

    private static string FixturePath(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "TextExtraction", "UpstreamJira", name);
    }
}
