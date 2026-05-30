/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
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

    protected override void SetColorValue(PDColor color)
    {
        Context.SetStrokingColor(color);
    }
}
