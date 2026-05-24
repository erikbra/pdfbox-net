/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class CurveTo : OperatorProcessor
{
    public CurveTo(PDFStreamEngine context) : base(OperatorName.CURVE_TO, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 6 ||
            operands[0] is not COSNumber x1 || operands[1] is not COSNumber y1 ||
            operands[2] is not COSNumber x2 || operands[3] is not COSNumber y2 ||
            operands[4] is not COSNumber x3 || operands[5] is not COSNumber y3) return;

        Context.CurveTo(
            x1.FloatValue(), y1.FloatValue(),
            x2.FloatValue(), y2.FloatValue(),
            x3.FloatValue(), y3.FloatValue());
    }
}
