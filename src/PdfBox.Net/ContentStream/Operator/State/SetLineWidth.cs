/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.State;

public sealed class SetLineWidth : OperatorProcessor
{
    public SetLineWidth(PDFStreamEngine context) : base(OperatorName.SET_LINE_WIDTH, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSNumber width) return;
        Context.SetLineWidth(width.FloatValue());
    }
}
