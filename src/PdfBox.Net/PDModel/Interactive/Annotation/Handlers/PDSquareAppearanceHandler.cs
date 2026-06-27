/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDSquareAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDSquareAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDSquareAppearanceHandler(PDAnnotationSquare annotation)
        : this(annotation, null)
    {
    }

    public PDSquareAppearanceHandler(PDAnnotationSquare annotation, PDDocument? document)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationSquare annotation = (PDAnnotationSquare)Annotation;
        float lineWidth = ResolveLineWidth(annotation);

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        bool hasStroke = contents.SetStrokingColorOnDemand(Color);
        bool hasFill = contents.SetNonStrokingColorOnDemand(annotation.GetInteriorColor());
        contents.SetBorderLine(lineWidth, annotation.GetBorderStyle(), annotation.GetBorder());
        SetOpacity(contents, annotation.GetConstantOpacity());

        PDRectangle box = Shrink(Rectangle, Math.Max(0.5f, lineWidth / 2f));
        contents.AddRect(box.GetLowerLeftX(), box.GetLowerLeftY(), box.GetWidth(), box.GetHeight());
        contents.DrawShape(lineWidth, hasStroke, hasFill);
    }

    private static PDRectangle Shrink(PDRectangle rect, float inset)
    {
        return new PDRectangle(rect.GetLowerLeftX() + inset, rect.GetLowerLeftY() + inset,
            Math.Max(0, rect.GetWidth() - inset * 2), Math.Max(0, rect.GetHeight() - inset * 2));
    }

    public override void GenerateRolloverAppearance()
    {
    }

    public override void GenerateDownAppearance()
    {
    }
}
