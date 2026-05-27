/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDFileAttachmentAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDFileAttachmentAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDFileAttachmentAppearanceHandler(PDAnnotation annotation, PDDocument? document = null)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        WriteDefaultNormalAppearance("PDFileAttachmentAppearance");
    }
}
