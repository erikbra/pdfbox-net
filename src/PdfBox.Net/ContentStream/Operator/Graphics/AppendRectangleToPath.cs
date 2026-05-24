/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class AppendRectangleToPath : OperatorProcessor
{
    public AppendRectangleToPath(PDFStreamEngine context) : base(OperatorName.APPEND_RECT, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 4 ||
            operands[0] is not COSNumber x || operands[1] is not COSNumber y ||
            operands[2] is not COSNumber width || operands[3] is not COSNumber height) return;

        Context.AppendRectangle(x.FloatValue(), y.FloatValue(), width.FloatValue(), height.FloatValue());
    }
}
