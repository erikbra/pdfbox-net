/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetNonStrokingColorN : OperatorProcessor
{
    public SetNonStrokingColorN(PDFStreamEngine context) : base(OperatorName.NON_STROKING_COLOR_N, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        float[] components = new float[operands.Count];
        for (int i = 0; i < operands.Count; i++)
        {
            if (operands[i] is not COSNumber number) return;
            components[i] = number.FloatValue();
        }

        if (components.Length == 0) return;
        PDColorSpace colorSpace = Context.GetGraphicsState().GetNonStrokingColorSpace();
        Context.SetNonStrokingColor(new PDColor(components, colorSpace));
    }
}
