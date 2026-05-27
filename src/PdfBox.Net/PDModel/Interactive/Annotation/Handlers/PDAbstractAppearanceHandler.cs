/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDAbstractAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public abstract class PDAbstractAppearanceHandler : PDAppearanceHandler
{
    private readonly PDAnnotation _annotation;
    private readonly PDDocument? _document;

    protected PDAbstractAppearanceHandler(PDAnnotation annotation, PDDocument? document = null)
    {
        _annotation = annotation ?? throw new ArgumentNullException(nameof(annotation));
        _document = document;
    }

    protected PDAnnotation Annotation => _annotation;

    public void GenerateAppearanceStreams()
    {
        GenerateNormalAppearance();
        GenerateRolloverAppearance();
        GenerateDownAppearance();
    }

    public abstract void GenerateNormalAppearance();

    public virtual void GenerateRolloverAppearance()
    {
    }

    public virtual void GenerateDownAppearance()
    {
    }

    protected void WriteDefaultNormalAppearance(string marker)
    {
        PDAppearanceStream stream = EnsureNormalAppearanceStream();
        EnsureAppearanceGeometry(stream);

        using Stream output = stream.GetContentStream().CreateOutputStream();
        using StreamWriter writer = new(output, System.Text.Encoding.ASCII, leaveOpen: true);
        writer.WriteLine($"% {marker}");
        writer.WriteLine("q");
        writer.WriteLine("Q");
        writer.Flush();
    }

    private PDAppearanceStream EnsureNormalAppearanceStream()
    {
        PDAppearanceDictionary appearance = _annotation.GetAppearance() ?? new PDAppearanceDictionary();
        _annotation.SetAppearance(appearance);

        PDAppearanceEntry? entry = appearance.GetNormalAppearance();
        if (entry?.IsStream() == true)
        {
            return entry.GetAppearanceStream();
        }

        PDAppearanceStream stream = _document != null ? new PDAppearanceStream(_document) : new PDAppearanceStream(new COSStream());
        appearance.SetNormalAppearance(stream);
        return stream;
    }

    private void EnsureAppearanceGeometry(PDAppearanceStream stream)
    {
        PDRectangle rect = _annotation.GetRectangle() ?? new PDRectangle(0, 0, 1, 1);
        if (Math.Abs(rect.GetWidth()) < float.Epsilon)
        {
            rect.SetUpperRightX(rect.GetLowerLeftX() + 1);
        }

        if (Math.Abs(rect.GetHeight()) < float.Epsilon)
        {
            rect.SetUpperRightY(rect.GetLowerLeftY() + 1);
        }

        stream.SetBBox(new PDRectangle(rect.GetLowerLeftX(), rect.GetLowerLeftY(), rect.GetWidth(), rect.GetHeight()));
        stream.SetMatrix(1, 0, 0, 1, -rect.GetLowerLeftX(), -rect.GetLowerLeftY());

        if (stream.GetResources() == null)
        {
            stream.SetResources(new PDResources());
        }
    }
}
