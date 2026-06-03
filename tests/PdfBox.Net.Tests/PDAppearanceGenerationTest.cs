using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Tests;

public class PDAppearanceGenerationTest
{
    [Fact]
    public void TextFieldConstructAppearancesCreatesParseableNormalAppearance()
    {
        using PDDocument document = new();
        PDAcroForm acroForm = CreateAcroFormWithHelvetica(document);
        PDTextField field = new(acroForm);
        field.SetPartialName("text");
        field.SetDefaultAppearance("/F1 11 Tf 0.2 0.3 0.4 rg");
        field.SetWidgets([CreateWidget(new PDRectangle(10, 10, 120, 24))]);

        field.SetValue("Hello appearance");

        PDAppearanceStream stream = field.GetWidgets().Single().GetNormalAppearanceStream()!;
        IList<object> tokens = ParseAppearance(stream!);

        Assert.Contains(tokens, token => token is COSString text && text.GetString().Contains("Hello", StringComparison.Ordinal));
        Assert.Contains(tokens, token => token is Operator op && op.GetName() == "BT");
        Assert.Contains(tokens, token => token is Operator op && op.GetName() == "ET");
    }

    [Fact]
    public void ComboBoxConstructAppearancesCreatesParseableNormalAppearance()
    {
        using PDDocument document = new();
        PDAcroForm acroForm = CreateAcroFormWithHelvetica(document);
        PDComboBox field = new(acroForm);
        field.SetPartialName("combo");
        field.SetDefaultAppearance("/F1 10 Tf 0 g");
        field.SetWidgets([CreateWidget(new PDRectangle(20, 20, 100, 22))]);
        field.SetOptions(["alpha", "beta"]);

        field.SetValue("beta");

        PDAppearanceStream stream = field.GetWidgets().Single().GetNormalAppearanceStream()!;
        IList<object> tokens = ParseAppearance(stream);

        Assert.Contains(tokens, token => token is COSString text && text.GetString() == "beta");
        Assert.Contains(tokens, token => token is Operator op && op.GetName() == "Tj");
    }

    [Fact]
    public void RepresentativeAnnotationHandlersGenerateParseableAppearanceStreams()
    {
        PDAnnotation[] annotations =
        [
            CreateTextAnnotation(),
            CreateHighlightAnnotation(),
            CreateUnderlineAnnotation(),
            CreateStrikeOutAnnotation(),
            CreateSquigglyAnnotation(),
            CreateSquareAnnotation(),
            CreateCircleAnnotation(),
            CreateLineAnnotation()
        ];

        foreach (PDAnnotation annotation in annotations)
        {
            annotation.ConstructAppearances();
            PDAppearanceStream? stream = annotation.GetNormalAppearanceStream();
            Assert.NotNull(stream);
            IList<object> tokens = ParseAppearance(stream);
            Assert.NotEmpty(tokens);
            Assert.Contains(tokens, token => token is Operator);
        }
    }

    [Fact]
    public void GeneratedAppearanceStreamsRemainStructurallyValidAcrossWidgetAndMarkupPaths()
    {
        using PDDocument document = new();
        PDAcroForm acroForm = CreateAcroFormWithHelvetica(document);

        PDTextField field = new(acroForm);
        field.SetDefaultAppearance("/F1 9 Tf 0 g");
        field.SetWidgets([CreateWidget(new PDRectangle(0, 0, 80, 18))]);
        field.SetValue("Value");

        PDAnnotationHighlight highlight = CreateHighlightAnnotation();
        highlight.ConstructAppearances();

        foreach (PDAppearanceStream stream in new[]
                 {
                     field.GetWidgets().Single().GetNormalAppearanceStream()!,
                     highlight.GetNormalAppearanceStream()!
                 })
        {
            IList<object> tokens = ParseAppearance(stream);
            Assert.DoesNotContain(tokens, token => token is COSNull);
        }
    }

    [Fact]
    public void FlattenMovesWidgetAppearanceIntoPageContentAndRemovesAcroForm()
    {
        byte[] serialized;
        using (PDDocument document = new())
        {
            PDPage page = new();
            document.AddPage(page);

            PDAcroForm acroForm = CreateAcroFormWithHelvetica(document);
            document.GetDocumentCatalog().SetAcroForm(acroForm);

            PDTextField field = new(acroForm);
            field.SetPartialName("text");
            field.SetDefaultAppearance("/F1 10 Tf 0 g");
            PDAnnotationWidget widget = CreateWidget(new PDRectangle(10, 10, 120, 24));
            widget.SetPage(page);
            field.SetWidgets([widget]);
            acroForm.GetFields().Add(field);
            page.SetAnnotations([widget]);

            field.SetValue("Flattened value");
            acroForm.Flatten();

            Assert.Null(document.GetDocumentCatalog().GetAcroForm(null));
            Assert.Empty(page.GetAnnotations());

            using Stream contentStream = ((PDContentStream)page).GetContents()!;
            using StreamReader reader = new(contentStream, Encoding.ASCII);
            string pageContent = reader.ReadToEnd();
            Assert.Contains("Do", pageContent, StringComparison.Ordinal);
            Assert.Contains("cm", pageContent, StringComparison.Ordinal);

            PDResources? resources = page.GetResources();
            Assert.NotNull(resources);
            Assert.Single(resources!.GetXObjectNames());

            using MemoryStream output = new();
            document.Save(output);
            serialized = output.ToArray();
        }

        using PDDocument loaded = PDDocument.Load(new MemoryStream(serialized));
        Assert.Null(loaded.GetDocumentCatalog().GetAcroForm(null));

        PDPage loadedPage = loaded.GetPage(0);
        Assert.Empty(loadedPage.GetAnnotations());

        using Stream loadedContentStream = ((PDContentStream)loadedPage).GetContents()!;
        using StreamReader loadedReader = new(loadedContentStream, Encoding.ASCII);
        Assert.Contains("Do", loadedReader.ReadToEnd(), StringComparison.Ordinal);
    }

    private static PDAcroForm CreateAcroFormWithHelvetica(PDDocument document)
    {
        PDAcroForm acroForm = new(document);
        COSDictionary fontDictionary = new();
        fontDictionary.SetName(COSName.SUBTYPE, "Type1");
        fontDictionary.SetName(COSName.GetPDFName("BaseFont"), "Helvetica");

        COSDictionary fontResources = new();
        fontResources.SetItem(COSName.GetPDFName("F1"), fontDictionary);

        COSDictionary resources = new();
        resources.SetItem(COSName.GetPDFName("Font"), fontResources);

        acroForm.SetDefaultResources(new PDResources(resources));
        acroForm.SetDefaultAppearance("/F1 10 Tf 0 g");
        return acroForm;
    }

    private static PDAnnotationWidget CreateWidget(PDRectangle rectangle)
    {
        PDAnnotationWidget widget = new();
        widget.SetRectangle(rectangle);
        return widget;
    }

    private static IList<object> ParseAppearance(PDAppearanceStream stream)
    {
        using Stream input = stream.GetContents();
        return PDFStreamParser.Parse(input);
    }

    private static PDAnnotationText CreateTextAnnotation()
    {
        PDAnnotationText annotation = new();
        annotation.SetRectangle(new PDRectangle(10, 10, 18, 20));
        annotation.SetColor(new PDColor(new COSArray { new COSFloat(1f), new COSFloat(1f), new COSFloat(0.6f) }, PDDeviceRGB.Instance));
        annotation.SetName(PDAnnotationText.NameNote);
        return annotation;
    }

    private static PDAnnotationHighlight CreateHighlightAnnotation()
    {
        PDAnnotationHighlight annotation = new();
        annotation.SetRectangle(new PDRectangle(10, 10, 50, 20));
        annotation.SetColor(new PDColor(new COSArray { new COSFloat(1f), new COSFloat(1f), new COSFloat(0f) }, PDDeviceRGB.Instance));
        annotation.SetQuadPoints([10, 30, 60, 30, 10, 10, 60, 10]);
        return annotation;
    }

    private static PDAnnotationUnderline CreateUnderlineAnnotation()
    {
        PDAnnotationUnderline annotation = new();
        annotation.SetRectangle(new PDRectangle(10, 10, 50, 20));
        annotation.SetColor(new PDColor(new COSArray { new COSFloat(0f) }, PDDeviceGray.Instance));
        annotation.SetQuadPoints([10, 30, 60, 30, 10, 10, 60, 10]);
        return annotation;
    }

    private static PDAnnotationStrikeOut CreateStrikeOutAnnotation()
    {
        PDAnnotationStrikeOut annotation = new();
        annotation.SetRectangle(new PDRectangle(10, 10, 50, 20));
        annotation.SetColor(new PDColor(new COSArray { new COSFloat(1f), new COSFloat(0f), new COSFloat(0f) }, PDDeviceRGB.Instance));
        annotation.SetQuadPoints([10, 30, 60, 30, 10, 10, 60, 10]);
        return annotation;
    }

    private static PDAnnotationSquiggly CreateSquigglyAnnotation()
    {
        PDAnnotationSquiggly annotation = new();
        annotation.SetRectangle(new PDRectangle(10, 10, 50, 20));
        annotation.SetColor(new PDColor(new COSArray { new COSFloat(0f), new COSFloat(0f), new COSFloat(1f) }, PDDeviceRGB.Instance));
        annotation.SetQuadPoints([10, 30, 60, 30, 10, 10, 60, 10]);
        return annotation;
    }

    private static PDAnnotationSquare CreateSquareAnnotation()
    {
        PDAnnotationSquare annotation = new();
        annotation.SetRectangle(new PDRectangle(10, 10, 30, 30));
        annotation.SetColor(new PDColor(new COSArray { new COSFloat(0f) }, PDDeviceGray.Instance));
        annotation.SetInteriorColor(new PDColor(new COSArray { new COSFloat(0.9f) }, PDDeviceGray.Instance));
        return annotation;
    }

    private static PDAnnotationCircle CreateCircleAnnotation()
    {
        PDAnnotationCircle annotation = new();
        annotation.SetRectangle(new PDRectangle(10, 10, 30, 30));
        annotation.SetColor(new PDColor(new COSArray { new COSFloat(0f), new COSFloat(0.5f), new COSFloat(0f) }, PDDeviceRGB.Instance));
        annotation.SetInteriorColor(new PDColor(new COSArray { new COSFloat(0.8f), new COSFloat(1f), new COSFloat(0.8f) }, PDDeviceRGB.Instance));
        return annotation;
    }

    private static PDAnnotationLine CreateLineAnnotation()
    {
        PDAnnotationLine annotation = new();
        annotation.SetRectangle(new PDRectangle(10, 10, 50, 20));
        annotation.SetColor(new PDColor(new COSArray { new COSFloat(0f) }, PDDeviceGray.Instance));
        annotation.SetLine([10, 10, 60, 30]);
        return annotation;
    }
}
