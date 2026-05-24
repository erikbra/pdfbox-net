/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class FillEvenOddRule : OperatorProcessor
{
    public FillEvenOddRule(PDFStreamEngine context) : base(OperatorName.FILL_EVEN_ODD, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.FillPath(0);
    }
}
