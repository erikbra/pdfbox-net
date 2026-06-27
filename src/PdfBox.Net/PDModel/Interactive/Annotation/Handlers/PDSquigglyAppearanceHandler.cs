/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDSquigglyAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDSquigglyAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDSquigglyAppearanceHandler(PDAnnotationSquiggly annotation)
        : this(annotation, null)
    {
    }

    public PDSquigglyAppearanceHandler(PDAnnotationSquiggly annotation, PDDocument? document)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        float[]? quadPoints = ((PDAnnotationSquiggly)Annotation).GetQuadPoints();
        if (quadPoints == null || quadPoints.Length < 8 || Color == null)
        {
            WriteDefaultNormalAppearance("PDSquigglyAppearance");
            return;
        }

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SetStrokingColor(Color);
        contents.SetLineWidth(1f);

        for (int i = 0; i + 7 < quadPoints.Length; i += 8)
        {
            float startX = quadPoints[i + 4];
            float endX = quadPoints[i + 6];
            float baseY = Math.Min(quadPoints[i + 5], quadPoints[i + 7]) + 1f;
            float step = Math.Max(2f, (endX - startX) / 8f);

            contents.MoveTo(startX, baseY);
            bool up = true;
            for (float x = startX + step; x <= endX; x += step)
            {
                contents.LineTo(Math.Min(x, endX), baseY + (up ? 1.5f : -1.5f));
                up = !up;
            }
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
