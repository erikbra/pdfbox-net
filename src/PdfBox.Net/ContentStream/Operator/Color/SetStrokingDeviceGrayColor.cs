/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetStrokingDeviceGrayColor : OperatorProcessor
{
    public SetStrokingDeviceGrayColor(PDFStreamEngine context) : base(OperatorName.STROKING_COLOR_GRAY, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSNumber g) return;
        PDColorSpace colorSpace = new("DeviceGray", 1);
        Context.SetStrokingColorSpace(colorSpace);
        Context.SetStrokingColor(new PDColor([g.FloatValue()], colorSpace));
    }
}
