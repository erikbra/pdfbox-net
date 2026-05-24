/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.State;

public sealed class SetLineJoin : OperatorProcessor
{
    public SetLineJoin(PDFStreamEngine context) : base(OperatorName.SET_LINE_JOINSTYLE, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSNumber lineJoin) return;
        Context.SetLineJoin(lineJoin.IntValue());
    }
}
