/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.State;

public sealed class SetMiterLimit : OperatorProcessor
{
    public SetMiterLimit(PDFStreamEngine context) : base(OperatorName.SET_LINE_MITERLIMIT, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSNumber miterLimit) return;
        Context.SetMiterLimit(miterLimit.FloatValue());
    }
}
