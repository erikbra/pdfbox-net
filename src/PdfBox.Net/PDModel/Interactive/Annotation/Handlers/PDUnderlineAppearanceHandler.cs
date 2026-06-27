/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDUnderlineAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDUnderlineAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDUnderlineAppearanceHandler(PDAnnotationUnderline annotation)
        : this(annotation, null)
    {
    }

    public PDUnderlineAppearanceHandler(PDAnnotationUnderline annotation, PDDocument? document)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        GenerateTextMarkup((PDAnnotationUnderline)Annotation, 0.1f);
    }

    private void GenerateTextMarkup(PDAnnotationTextMarkup annotation, float relativeY)
    {
        float[]? quadPoints = annotation.GetQuadPoints();
        if (quadPoints == null || quadPoints.Length < 8 || Color == null)
        {
            WriteDefaultNormalAppearance("PDUnderlineAppearance");
            return;
        }

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SetStrokingColor(Color);
        contents.SetLineWidth(1f);
        SetOpacity(contents, annotation.GetConstantOpacity());

        for (int i = 0; i + 7 < quadPoints.Length; i += 8)
        {
            float startX = quadPoints[i + 4];
            float endX = quadPoints[i + 6];
            float bottomY = Math.Min(quadPoints[i + 5], quadPoints[i + 7]);
            float topY = Math.Max(quadPoints[i + 1], quadPoints[i + 3]);
            float y = bottomY + (topY - bottomY) * relativeY;
            contents.MoveTo(startX, y);
            contents.LineTo(endX, y);
            contents.Stroke();
        }
    }

    public override void GenerateRolloverAppearance()
    {
    }

    public override void GenerateDownAppearance()
    {
    }
}
