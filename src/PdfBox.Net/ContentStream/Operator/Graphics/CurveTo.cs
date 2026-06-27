/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/CurveTo.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class CurveTo : OperatorProcessor
{
    public CurveTo(PDFStreamEngine context) : base(OperatorName.CURVE_TO, context) { }

    public override string GetName()
    {
        return Name;
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 6 ||
            operands[0] is not COSNumber x1 || operands[1] is not COSNumber y1 ||
            operands[2] is not COSNumber x2 || operands[3] is not COSNumber y2 ||
            operands[4] is not COSNumber x3 || operands[5] is not COSNumber y3) return;

        Context.CurveTo(
            x1.FloatValue(), y1.FloatValue(),
            x2.FloatValue(), y2.FloatValue(),
            x3.FloatValue(), y3.FloatValue());
    }
}
