/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetStrokingDeviceCMYKColor.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetStrokingDeviceCMYKColor : OperatorProcessor
{
    public SetStrokingDeviceCMYKColor(PDFStreamEngine context) : base(OperatorName.STROKING_COLOR_CMYK, context) { }

    public override string GetName()
    {
        return Name;
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 4 || operands[0] is not COSNumber c || operands[1] is not COSNumber m || operands[2] is not COSNumber y || operands[3] is not COSNumber k) return;
        PDColorSpace colorSpace = PDDeviceCMYK.Instance;
        Context.SetStrokingColorSpace(colorSpace);
        Context.SetStrokingColor(new PDColor([c.FloatValue(), m.FloatValue(), y.FloatValue(), k.FloatValue()], colorSpace));
    }
}
