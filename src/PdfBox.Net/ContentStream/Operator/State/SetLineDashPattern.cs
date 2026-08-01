/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/state/SetLineDashPattern.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.COS;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.ContentStream.Operator.State;

public sealed class SetLineDashPattern : OperatorProcessor
{
    private static ILogger<SetLineDashPattern> LOG =>
        PdfBoxLogging.CreateLogger<SetLineDashPattern>();

    public SetLineDashPattern(PDFStreamEngine context) : base(OperatorName.SET_LINE_DASHPATTERN, context) { }

    public override string GetName()
    {
        return Name;
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 2 || operands[0] is not COSArray array || operands[1] is not COSNumber phase) return;
        float[] dashArray = array.ToFloatArray();
        foreach (COSBase? item in array)
        {
            if (item is COSNumber number)
            {
                if (number.FloatValue() != 0)
                {
                    break;
                }
            }
            else
            {
                LOG.LogWarning("dash array has non number element {Element}, ignored", item);
                dashArray = [];
                break;
            }
        }
        Context.SetLineDashPattern(dashArray, phase.IntValue());
    }
}
