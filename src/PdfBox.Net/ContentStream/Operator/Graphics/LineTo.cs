/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class LineTo : OperatorProcessor
{
    public LineTo(PDFStreamEngine context) : base(OperatorName.LINE_TO, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 2 || operands[0] is not COSNumber x || operands[1] is not COSNumber y) return;
        Context.LineTo(x.FloatValue(), y.FloatValue());
    }
}
