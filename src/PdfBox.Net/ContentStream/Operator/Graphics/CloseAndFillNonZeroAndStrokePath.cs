/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class CloseAndFillNonZeroAndStrokePath : OperatorProcessor
{
    private readonly ClosePath _closePath;
    private readonly FillNonZeroAndStrokePath _fillNonZeroAndStrokePath;

    public CloseAndFillNonZeroAndStrokePath(PDFStreamEngine context) : base(OperatorName.CLOSE_FILL_NON_ZERO_AND_STROKE, context)
    {
        _closePath = new ClosePath(context);
        _fillNonZeroAndStrokePath = new FillNonZeroAndStrokePath(context);
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        _closePath.Process(op, operands);
        _fillNonZeroAndStrokePath.Process(op, operands);
    }
}
