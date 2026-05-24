/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class CurveToReplicateInitialPoint : OperatorProcessor
{
    public CurveToReplicateInitialPoint(PDFStreamEngine context) : base(OperatorName.CURVE_TO_REPLICATE_INITIAL_POINT, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 4 ||
            operands[0] is not COSNumber x2 || operands[1] is not COSNumber y2 ||
            operands[2] is not COSNumber x3 || operands[3] is not COSNumber y3) return;

        var currentPoint = Context.GetCurrentPoint();
        if (!currentPoint.HasValue) return;

        Context.CurveTo(
            currentPoint.Value.X, currentPoint.Value.Y,
            x2.FloatValue(), y2.FloatValue(),
            x3.FloatValue(), y3.FloatValue());
    }
}
