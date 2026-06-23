/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDHighlightAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDHighlightAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDHighlightAppearanceHandler(PDAnnotationHighlight annotation, PDDocument? document = null)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationHighlight annotation = (PDAnnotationHighlight)Annotation;
        float[]? quadPoints = annotation.GetQuadPoints();
        if (quadPoints == null || quadPoints.Length < 8 || Color == null || Color.GetComponents().Length == 0)
        {
            WriteDefaultNormalAppearance("PDHighlightAppearance");
            return;
        }

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SetNonStrokingColor(Color!);
        SetOpacity(contents, annotation.GetConstantOpacity());

        for (int i = 0; i + 7 < quadPoints.Length; i += 8)
        {
            contents.MoveTo(quadPoints[i + 4], quadPoints[i + 5]);
            contents.LineTo(quadPoints[i + 0], quadPoints[i + 1]);
            contents.LineTo(quadPoints[i + 2], quadPoints[i + 3]);
            contents.LineTo(quadPoints[i + 6], quadPoints[i + 7]);
            contents.ClosePath();
            contents.Fill();
        }
    }
}
