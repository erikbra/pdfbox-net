/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDLinkAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDLinkAppearanceHandler : PDAbstractAppearanceHandler
{
    public PDLinkAppearanceHandler(PDAnnotation annotation)
        : this(annotation, null)
    {
    }

    public PDLinkAppearanceHandler(PDAnnotation annotation, PDDocument? document)
        : base(annotation, document)
    {
    }

    public override void GenerateNormalAppearance()
    {
        PDAnnotationLink annotation = (PDAnnotationLink)Annotation;
        PDRectangle? rect = annotation.GetRectangle();
        if (rect == null)
        {
            return;
        }

        float lineWidth = GetLineWidth(annotation);
        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        PDColor color = annotation.GetColor() ?? new PDColor([0f], PDDeviceGray.Instance);
        bool hasStroke = contents.SetStrokingColorOnDemand(color);
        contents.SetBorderLine(lineWidth, annotation.GetBorderStyle(), annotation.GetBorder());

        PDRectangle borderEdge = GetPaddedRectangle(Rectangle, lineWidth / 2f);
        float[]? pathsArray = annotation.GetQuadPoints();
        if (pathsArray != null)
        {
            for (int i = 0; i < pathsArray.Length / 2; i++)
            {
                if (!rect.Contains(pathsArray[i * 2], pathsArray[i * 2 + 1]))
                {
                    pathsArray = null;
                    break;
                }
            }
        }

        if (pathsArray == null)
        {
            pathsArray =
            [
                borderEdge.GetLowerLeftX(),
                borderEdge.GetLowerLeftY(),
                borderEdge.GetUpperRightX(),
                borderEdge.GetLowerLeftY(),
                borderEdge.GetUpperRightX(),
                borderEdge.GetUpperRightY(),
                borderEdge.GetLowerLeftX(),
                borderEdge.GetUpperRightY()
            ];
        }

        bool underlined = false;
        if (pathsArray.Length >= 8 && annotation.GetBorderStyle() is { } borderStyle)
        {
            underlined = string.Equals(borderStyle.GetStyle(), PDBorderStyleDictionary.STYLE_UNDERLINE, StringComparison.Ordinal);
        }

        int offset = 0;
        while (offset + 7 < pathsArray.Length)
        {
            contents.MoveTo(pathsArray[offset], pathsArray[offset + 1]);
            contents.LineTo(pathsArray[offset + 2], pathsArray[offset + 3]);
            if (!underlined)
            {
                contents.LineTo(pathsArray[offset + 4], pathsArray[offset + 5]);
                contents.LineTo(pathsArray[offset + 6], pathsArray[offset + 7]);
                contents.ClosePath();
            }

            offset += 8;
        }

        contents.DrawShape(lineWidth, hasStroke, hasFill: false);
    }

    public override void GenerateRolloverAppearance()
    {
    }

    public override void GenerateDownAppearance()
    {
    }

    private static float GetLineWidth(PDAnnotationLink annotation)
    {
        PDBorderStyleDictionary? borderStyle = annotation.GetBorderStyle();
        if (borderStyle != null)
        {
            return borderStyle.GetWidth();
        }

        COSArray border = annotation.GetBorder();
        return border.Size() >= 3 && border.GetObject(2) is COSNumber width ? width.FloatValue() : 1f;
    }

    private static PDRectangle GetPaddedRectangle(PDRectangle rectangle, float padding)
    {
        return new PDRectangle(
            rectangle.GetLowerLeftX() + padding,
            rectangle.GetLowerLeftY() + padding,
            Math.Max(0, rectangle.GetWidth() - 2 * padding),
            Math.Max(0, rectangle.GetHeight() - 2 * padding));
    }

}
