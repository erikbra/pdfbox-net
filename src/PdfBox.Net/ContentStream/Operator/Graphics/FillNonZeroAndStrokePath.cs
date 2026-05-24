/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class FillNonZeroAndStrokePath : OperatorProcessor
{
    public FillNonZeroAndStrokePath(PDFStreamEngine context) : base(OperatorName.FILL_NON_ZERO_AND_STROKE, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.FillAndStrokePath(1);
    }
}
