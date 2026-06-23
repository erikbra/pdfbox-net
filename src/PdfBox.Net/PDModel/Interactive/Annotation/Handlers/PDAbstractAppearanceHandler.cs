/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDAbstractAppearanceHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

public abstract class PDAbstractAppearanceHandler : PDAppearanceHandler
{
    private readonly PDAnnotation _annotation;
    private readonly PDDocument? _document;

    protected PDAbstractAppearanceHandler(PDAnnotation annotation, PDDocument? document = null)
    {
        _annotation = annotation ?? throw new ArgumentNullException(nameof(annotation));
        _document = document;
    }

    protected PDAnnotation Annotation => _annotation;

    protected PDRectangle Rectangle => _annotation.GetRectangle()
        ?? throw new IOException("Annotation appearance generation requires a rectangle.");

    protected PDColor? Color => _annotation.GetColor();

    protected PDAppearanceStream GetOrCreateNormalAppearanceStream()
    {
        PDAppearanceDictionary appearance = _annotation.GetAppearance() ?? new PDAppearanceDictionary();
        _annotation.SetAppearance(appearance);

        PDAppearanceStream stream = _annotation.GetNormalAppearanceStream()
            ?? (_document != null ? new PDAppearanceStream(_document) : new PDAppearanceStream(new COSStream()));
        stream.SetBBox(Rectangle);
        stream.SetMatrix(Matrix.GetTranslateInstance(-Rectangle.GetLowerLeftX(), -Rectangle.GetLowerLeftY()));
        if (stream.GetResources() == null)
        {
            stream.SetResources(new PDModel.Resources.PDResources());
        }

        appearance.SetNormalAppearance(stream);
        return stream;
    }

    protected PDAppearanceContentStream OpenNormalAppearanceContentStream()
    {
        return new PDAppearanceContentStream(GetOrCreateNormalAppearanceStream());
    }

    protected void WriteDefaultNormalAppearance(string marker)
    {
        _ = marker;

        using PDAppearanceContentStream contents = OpenNormalAppearanceContentStream();
        contents.SaveGraphicsState();
        contents.RestoreGraphicsState();
    }

    protected void SetOpacity(PDAppearanceContentStream contents, float opacity)
    {
        if (opacity >= 1f)
        {
            return;
        }

        PDExtendedGraphicsState graphicsState = new();
        graphicsState.SetStrokingAlphaConstant(opacity);
        graphicsState.SetNonStrokingAlphaConstant(opacity);
        contents.SetGraphicsStateParameters(graphicsState);
    }

    protected static float ResolveLineWidth(PDAnnotationMarkup annotation)
    {
        PDBorderStyleDictionary? borderStyle = annotation.GetBorderStyle();
        if (borderStyle != null)
        {
            return borderStyle.GetWidth();
        }

        COSArray border = annotation.GetBorder();
        return border.Size() > 2 && border.GetObject(2) is COSNumber width ? width.FloatValue() : 1f;
    }

    protected static void DrawCircle(PDAppearanceContentStream contents, PDRectangle box)
    {
        float x0 = box.GetLowerLeftX();
        float y0 = box.GetLowerLeftY();
        float x1 = box.GetUpperRightX();
        float y1 = box.GetUpperRightY();
        float xm = x0 + box.GetWidth() / 2f;
        float ym = y0 + box.GetHeight() / 2f;
        float magic = 0.55555417f;
        float vOffset = box.GetHeight() / 2f * magic;
        float hOffset = box.GetWidth() / 2f * magic;

        contents.MoveTo(xm, y1);
        contents.CurveTo(xm + hOffset, y1, x1, ym + vOffset, x1, ym);
        contents.CurveTo(x1, ym - vOffset, xm + hOffset, y0, xm, y0);
        contents.CurveTo(xm - hOffset, y0, x0, ym - vOffset, x0, ym);
        contents.CurveTo(x0, ym + vOffset, xm - hOffset, y1, xm, y1);
        contents.ClosePath();
    }

    public virtual void GenerateRolloverAppearance()
    {
    }

    public virtual void GenerateDownAppearance()
    {
    }

    public abstract void GenerateNormalAppearance();
}
