/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetNonStrokingDeviceRGBColor : OperatorProcessor
{
    public SetNonStrokingDeviceRGBColor(PDFStreamEngine context) : base(OperatorName.NON_STROKING_RGB, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 3 || operands[0] is not COSNumber r || operands[1] is not COSNumber g || operands[2] is not COSNumber b) return;
        PDColorSpace colorSpace = PDDeviceRGB.Instance;
        Context.SetNonStrokingColorSpace(colorSpace);
        Context.SetNonStrokingColor(new PDColor([r.FloatValue(), g.FloatValue(), b.FloatValue()], colorSpace));
    }
}
