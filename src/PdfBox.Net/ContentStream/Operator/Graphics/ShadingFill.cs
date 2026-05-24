/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class ShadingFill : OperatorProcessor
{
    public ShadingFill(PDFStreamEngine context) : base(OperatorName.SHADING_FILL, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSName shadingName) return;
        Context.ShadingFill(shadingName);
    }
}
