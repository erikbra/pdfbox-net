/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDLineAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public sealed class PDLineAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDLineAppearanceHandler(PDAnnotationLine annotation)
        : this(annotation, null)
    {
    }

    public PDLineAppearanceHandler(PDAnnotationLine annotation, PDDocument? document)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationLine annotation = (PDAnnotationLine)Annotation;
        float[]? line = annotation.GetLine();
        if (line == null || line.Length < 4 || Color == null)
        {
            WriteDefaultNormalAppearance("PDLineAppearance");
            return;
        }

        float lineWidth = ResolveLineWidth(annotation);
        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SetStrokingColor(Color);
        contents.SetBorderLine(lineWidth, annotation.GetBorderStyle(), annotation.GetBorder());
        SetOpacity(contents, annotation.GetConstantOpacity());
        contents.MoveTo(line[0], line[1]);
        contents.LineTo(line[2], line[3]);
        contents.Stroke();
    }

    public override void GenerateRolloverAppearance()
    {
    }

    public override void GenerateDownAppearance()
    {
    }
}
