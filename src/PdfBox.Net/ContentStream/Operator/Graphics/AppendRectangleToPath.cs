/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/AppendRectangleToPath.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class AppendRectangleToPath : OperatorProcessor
{
    public AppendRectangleToPath(PDFStreamEngine context) : base(OperatorName.APPEND_RECT, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 4 ||
            operands[0] is not COSNumber x || operands[1] is not COSNumber y ||
            operands[2] is not COSNumber width || operands[3] is not COSNumber height) return;

        Context.AppendRectangle(x.FloatValue(), y.FloatValue(), width.FloatValue(), height.FloatValue());
    }
}
