/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
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
