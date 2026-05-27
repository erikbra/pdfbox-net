/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/AnnotationBorder.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

internal sealed class AnnotationBorder
{
    public float Width { get; init; } = 1;
    public COSArray? DashArray { get; init; }

    public static AnnotationBorder GetAnnotationBorder(PDAnnotation annotation, PDBorderStyleDictionary? borderStyle)
    {
        float width = borderStyle?.GetWidth() ?? 1;
        COSArray? dash = borderStyle?.GetDashStyle();
        return new AnnotationBorder
        {
            Width = width,
            DashArray = dash
        };
    }
}
