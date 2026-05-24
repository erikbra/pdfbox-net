/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class CloseAndFillEvenOddAndStrokePath : OperatorProcessor
{
    private readonly ClosePath _closePath;
    private readonly FillEvenOddAndStrokePath _fillEvenOddAndStrokePath;

    public CloseAndFillEvenOddAndStrokePath(PDFStreamEngine context) : base(OperatorName.CLOSE_FILL_EVEN_ODD_AND_STROKE, context)
    {
        _closePath = new ClosePath(context);
        _fillEvenOddAndStrokePath = new FillEvenOddAndStrokePath(context);
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        _closePath.Process(op, operands);
        _fillEvenOddAndStrokePath.Process(op, operands);
    }
}
