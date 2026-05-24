/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class SetType3GlyphWidthAndBoundingBox : OperatorProcessor
{
    public SetType3GlyphWidthAndBoundingBox(PDFStreamEngine context) : base(OperatorName.TYPE3_D1, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 6 ||
            operands[0] is not COSNumber wx || operands[1] is not COSNumber wy ||
            operands[2] is not COSNumber llx || operands[3] is not COSNumber lly ||
            operands[4] is not COSNumber urx || operands[5] is not COSNumber ury) return;

        Context.SetType3GlyphWidthAndBoundingBox(
            wx.FloatValue(), wy.FloatValue(),
            llx.FloatValue(), lly.FloatValue(),
            urx.FloatValue(), ury.FloatValue());
    }
}
