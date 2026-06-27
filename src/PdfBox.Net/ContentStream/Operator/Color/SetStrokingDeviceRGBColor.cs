/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetStrokingDeviceRGBColor.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetStrokingDeviceRGBColor : OperatorProcessor
{
    public SetStrokingDeviceRGBColor(PDFStreamEngine context) : base(OperatorName.STROKING_COLOR_RGB, context) { }

    public override string GetName()
    {
        return Name;
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 3 || operands[0] is not COSNumber r || operands[1] is not COSNumber g || operands[2] is not COSNumber b) return;
        PDColorSpace colorSpace = PDDeviceRGB.Instance;
        Context.SetStrokingColorSpace(colorSpace);
        Context.SetStrokingColor(new PDColor([r.FloatValue(), g.FloatValue(), b.FloatValue()], colorSpace));
    }
}
