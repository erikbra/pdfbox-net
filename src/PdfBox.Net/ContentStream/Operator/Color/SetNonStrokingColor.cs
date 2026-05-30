/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public class SetNonStrokingColor : SetColor
{
    protected SetNonStrokingColor(string name, PDFStreamEngine context)
        : base(name, context)
    {
    }

    public SetNonStrokingColor(PDFStreamEngine context)
        : this(OperatorName.NON_STROKING_COLOR, context)
    {
    }

    protected override PDColorSpace GetColorSpace()
    {
        return Context.GetGraphicsState().GetNonStrokingColorSpace();
    }

    protected override void SetColorValue(PDColor color)
    {
        Context.SetNonStrokingColor(color);
    }
}
