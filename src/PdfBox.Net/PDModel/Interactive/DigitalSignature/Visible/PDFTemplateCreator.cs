/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateCreator.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public class PDFTemplateCreator
{
    private static ILogger<PDFTemplateCreator> LOG => PdfBoxLogging.CreateLogger<PDFTemplateCreator>();

    private readonly PDFTemplateBuilder _pdfBuilder;

    public PDFTemplateCreator(PDFTemplateBuilder templateBuilder)
    {
        _pdfBuilder = templateBuilder;
    }

    public PDFTemplateStructure GetPdfStructure() => _pdfBuilder.GetStructure();

    public Stream BuildPDF(PDVisibleSignDesigner properties)
    {
        LOG.LogInformation("pdf building has been started");
        Stream stream = _pdfBuilder.BuildPDF(properties);
        LOG.LogInformation("stream returning started, size= {Size}", stream.CanSeek ? stream.Length : -1);
        return stream;
    }
}
