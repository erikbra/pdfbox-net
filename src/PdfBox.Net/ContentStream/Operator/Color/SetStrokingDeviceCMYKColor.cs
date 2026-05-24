/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetStrokingDeviceCMYKColor : OperatorProcessor
{
    public SetStrokingDeviceCMYKColor(PDFStreamEngine context) : base(OperatorName.STROKING_COLOR_CMYK, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 4 || operands[0] is not COSNumber c || operands[1] is not COSNumber m || operands[2] is not COSNumber y || operands[3] is not COSNumber k) return;
        PDColorSpace colorSpace = new("DeviceCMYK", 4);
        Context.SetStrokingColorSpace(colorSpace);
        Context.SetStrokingColor(new PDColor([c.FloatValue(), m.FloatValue(), y.FloatValue(), k.FloatValue()], colorSpace));
    }
}
