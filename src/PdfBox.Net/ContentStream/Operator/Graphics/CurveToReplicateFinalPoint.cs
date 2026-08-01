/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/CurveToReplicateFinalPoint.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.COS;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class CurveToReplicateFinalPoint : OperatorProcessor
{
    private static ILogger<CurveToReplicateFinalPoint> LOG =>
        PdfBoxLogging.CreateLogger<CurveToReplicateFinalPoint>();

    public CurveToReplicateFinalPoint(PDFStreamEngine context) : base(OperatorName.CURVE_TO_REPLICATE_FINAL_POINT, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 4 ||
            operands[0] is not COSNumber x1 || operands[1] is not COSNumber y1 ||
            operands[2] is not COSNumber x3 || operands[3] is not COSNumber y3) return;

        if (Context.GetCurrentPoint() is null)
        {
            LOG.LogWarning("curveTo ({X},{Y}) without initial MoveTo", x3.FloatValue(),
                y3.FloatValue());
            return;
        }

        Context.CurveTo(
            x1.FloatValue(), y1.FloatValue(),
            x3.FloatValue(), y3.FloatValue(),
            x3.FloatValue(), y3.FloatValue());
    }
}
