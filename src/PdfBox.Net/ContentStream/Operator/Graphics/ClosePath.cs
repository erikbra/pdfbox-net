/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class ClosePath : OperatorProcessor
{
    public ClosePath(PDFStreamEngine context) : base(OperatorName.CLOSE_PATH, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.ClosePath();
    }
}
