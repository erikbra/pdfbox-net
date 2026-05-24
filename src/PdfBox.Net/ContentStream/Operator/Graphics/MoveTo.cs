/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class MoveTo : OperatorProcessor
{
    public MoveTo(PDFStreamEngine context) : base(OperatorName.MOVE_TO, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 2 || operands[0] is not COSNumber x || operands[1] is not COSNumber y) return;
        Context.MoveTo(x.FloatValue(), y.FloatValue());
    }
}
