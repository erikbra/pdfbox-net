/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDCircleAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDCircleAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDCircleAppearanceHandler(PDAnnotationCircle annotation)
        : this(annotation, null)
    {
    }

    public PDCircleAppearanceHandler(PDAnnotationCircle annotation, PDDocument? document)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationCircle annotation = (PDAnnotationCircle)Annotation;
        float lineWidth = ResolveLineWidth(annotation);

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        bool hasStroke = contents.SetStrokingColorOnDemand(Color);
        bool hasFill = contents.SetNonStrokingColorOnDemand(annotation.GetInteriorColor());
        contents.SetBorderLine(lineWidth, annotation.GetBorderStyle(), annotation.GetBorder());
        SetOpacity(contents, annotation.GetConstantOpacity());

        PDRectangle box = new(
            Rectangle.GetLowerLeftX() + lineWidth / 2f,
            Rectangle.GetLowerLeftY() + lineWidth / 2f,
            Math.Max(0, Rectangle.GetWidth() - lineWidth),
            Math.Max(0, Rectangle.GetHeight() - lineWidth));
        DrawCircle(contents, box);
        contents.DrawShape(lineWidth, hasStroke, hasFill);
    }

    public override void GenerateRolloverAppearance()
    {
    }

    public override void GenerateDownAppearance()
    {
    }
}
