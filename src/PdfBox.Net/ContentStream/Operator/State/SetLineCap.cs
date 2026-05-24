/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.State;

public sealed class SetLineCap : OperatorProcessor
{
    public SetLineCap(PDFStreamEngine context) : base(OperatorName.SET_LINE_CAPSTYLE, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSNumber lineCap) return;
        Context.SetLineCap(lineCap.IntValue());
    }
}
