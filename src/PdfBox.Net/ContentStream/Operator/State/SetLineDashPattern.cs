/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.State;

public sealed class SetLineDashPattern : OperatorProcessor
{
    public SetLineDashPattern(PDFStreamEngine context) : base(OperatorName.SET_LINE_DASHPATTERN, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 2 || operands[0] is not COSArray array || operands[1] is not COSNumber phase) return;
        Context.SetLineDashPattern(array.ToFloatArray(), phase.IntValue());
    }
}
