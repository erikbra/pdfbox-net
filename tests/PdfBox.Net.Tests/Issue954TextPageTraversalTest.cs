using PdfBox.Net.PDModel;
using PdfBox.Net.Text;

namespace PdfBox.Net.Tests;

public class Issue954TextPageTraversalTest
{
    [Fact]
    public void ExtractText_VisitsEmptyPagesAndPreservesPageNumbers()
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        document.AddPage(new PDPage());
        PageTrackingStripper stripper = new();

        stripper.GetText(document);

        Assert.Equal(2, stripper.PagesStarted);
        Assert.Equal(2, stripper.PagesEnded);
    }

    [Fact]
    public void ExtractRegions_ProcessesEmptyPage()
    {
        PageTrackingAreaStripper stripper = new();
        stripper.ExtractRegions(new PDPage());
        Assert.Equal(1, stripper.PagesProcessed);
    }

    private sealed class PageTrackingStripper : PDFTextStripper
    {
        public int PagesStarted { get; private set; }
        public int PagesEnded { get; private set; }
        protected override void StartPage(PDPage page) => PagesStarted++;
        protected override void EndPage(PDPage page) => PagesEnded++;
    }

    private sealed class PageTrackingAreaStripper : PDFTextStripperByArea
    {
        public int PagesProcessed { get; private set; }
        public override void ProcessPage(PDPage page) => PagesProcessed++;
    }
}
