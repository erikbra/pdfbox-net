/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetNonStrokingDeviceGrayColor.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetNonStrokingDeviceGrayColor : OperatorProcessor
{
    public SetNonStrokingDeviceGrayColor(PDFStreamEngine context) : base(OperatorName.NON_STROKING_GRAY, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSNumber g) return;
        PDColorSpace colorSpace = PDDeviceGray.Instance;
        Context.SetNonStrokingColorSpace(colorSpace);
        Context.SetNonStrokingColor(new PDColor([g.FloatValue()], colorSpace));
    }
}
