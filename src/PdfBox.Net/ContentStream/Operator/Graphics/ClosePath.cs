/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/graphics/ClosePath.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.COS;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class ClosePath : OperatorProcessor
{
    private static ILogger<ClosePath> LOG => PdfBoxLogging.CreateLogger<ClosePath>();

    public ClosePath(PDFStreamEngine context) : base(OperatorName.CLOSE_PATH, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (Context.GetCurrentPoint() is null)
        {
            LOG.LogWarning("ClosePath without initial MoveTo");
            return;
        }
        Context.ClosePath();
    }
}
