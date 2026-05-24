/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.State;

public sealed class SetFlatness : OperatorProcessor
{
    public SetFlatness(PDFStreamEngine context) : base(OperatorName.SET_FLATNESS, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSNumber flatness) return;
        Context.SetFlatness(flatness.FloatValue());
    }
}
