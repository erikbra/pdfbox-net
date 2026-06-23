/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/FillNonZeroAndStrokePath.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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
