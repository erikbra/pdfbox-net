/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetStrokingColor.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public class SetStrokingColor : SetColor
{
    protected SetStrokingColor(string name, PDFStreamEngine context)
        : base(name, context)
    {
    }

    public SetStrokingColor(PDFStreamEngine context)
        : this(OperatorName.STROKING_COLOR, context)
    {
    }

    protected override PDColorSpace GetColorSpace()
    {
        return Context.GetGraphicsState().GetStrokingColorSpace();
    }

    protected override PDColor GetColor()
    {
        return Context.GetGraphicsState().GetStrokingColor();
    }

    protected virtual void SetColor(PDColor color)
    {
        Context.SetStrokingColor(color);
    }

    protected override void SetColorValue(PDColor color)
    {
        SetColor(color);
    }

    public override string GetName()
    {
        return Name;
    }
}
