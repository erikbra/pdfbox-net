using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Tests;

public sealed class PDModelFixupTest
{
    [Fact]
    public void GetAcroFormWithoutFixupPreservesOriginalValues()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();
        PDAcroForm acroForm = new(document);
        catalog.SetAcroForm(acroForm);

        PDAcroForm? restored = catalog.GetAcroForm(null);

        Assert.NotNull(restored);
        Assert.Equal(string.Empty, restored!.GetDefaultAppearance());
        Assert.Null(restored.GetDefaultResources());
    }

    [Fact]
    public void GetAcroFormAppliesDefaultFixup()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();
        PDAcroForm acroForm = new(document);
        catalog.SetAcroForm(acroForm);

        PDAcroForm? restored = catalog.GetAcroForm();

        Assert.NotNull(restored);
        Assert.Equal("/Helv 0 Tf 0 g ", restored!.GetDefaultAppearance());
        Assert.NotNull(restored.GetDefaultResources());
        Assert.NotNull(restored.GetDefaultResources()!.GetFont(COSName.GetPDFName("Helv")));
        Assert.NotNull(restored.GetDefaultResources()!.GetFont(COSName.GetPDFName("ZaDb")));
    }

    [Fact]
    public void GetAcroFormRebuildsOrphanWidgetsAndGeneratesAppearance()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();
        PDAcroForm acroForm = new(document);
        acroForm.SetNeedAppearances(true);
        catalog.SetAcroForm(acroForm);

        PDPage page = new();
        document.AddPage(page);

        COSDictionary widgetDictionary = new();
        widgetDictionary.SetName(COSName.SUBTYPE, PDAnnotationWidget.SUB_TYPE);
        widgetDictionary.SetName(COSName.GetPDFName("FT"), "Tx");
        widgetDictionary.SetString(COSName.T, "orphan");
        widgetDictionary.SetString(COSName.V, "Recovered value");
        widgetDictionary.SetItem(COSName.RECT, new PDRectangle(5, 5, 120, 20).GetCOSArray());
        page.SetAnnotations([new PDAnnotationWidget(widgetDictionary)]);

        PDAcroForm? restored = catalog.GetAcroForm();

        Assert.NotNull(restored);
        Assert.False(restored!.GetNeedAppearances());
        PDTextField textField = Assert.IsType<PDTextField>(Assert.Single(restored.GetFields()));
        Assert.Equal("Recovered value", textField.GetValue());

        PDAnnotationWidget widget = Assert.IsType<PDAnnotationWidget>(Assert.Single(page.GetAnnotations()));
        Assert.NotNull(widget.GetNormalAppearanceStream());
        string appearance = System.Text.Encoding.ASCII.GetString(widget.GetNormalAppearanceStream()!.GetContentStream().ToByteArray());
        Assert.Contains("Recovered value", appearance, StringComparison.Ordinal);
    }
}
