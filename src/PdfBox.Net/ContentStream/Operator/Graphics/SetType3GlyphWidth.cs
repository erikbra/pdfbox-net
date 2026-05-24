/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class SetType3GlyphWidth : OperatorProcessor
{
    public SetType3GlyphWidth(PDFStreamEngine context) : base(OperatorName.TYPE3_D0, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 2 || operands[0] is not COSNumber wx || operands[1] is not COSNumber wy) return;
        Context.SetType3GlyphWidth(wx.FloatValue(), wy.FloatValue());
    }
}
