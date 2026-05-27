/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/CloudyBorder.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

internal sealed class CloudyBorder
{
    public CloudyBorder(float intensity, float lineWidth, PDRectangle rectangle)
    {
        Intensity = intensity;
        LineWidth = lineWidth;
        Rectangle = rectangle;
    }

    public float Intensity { get; }
    public float LineWidth { get; }
    public PDRectangle Rectangle { get; }
}
