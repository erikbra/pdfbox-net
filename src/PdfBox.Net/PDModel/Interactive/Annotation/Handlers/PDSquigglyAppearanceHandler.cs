/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDSquigglyAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDSquigglyAppearanceHandler(PDAnnotation annotation, PDDocument? document = null)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        WriteDefaultNormalAppearance("PDSquigglyAppearance");
    }
}
