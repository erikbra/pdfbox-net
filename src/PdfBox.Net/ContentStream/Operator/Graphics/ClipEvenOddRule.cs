/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class ClipEvenOddRule : OperatorProcessor
{
    public ClipEvenOddRule(PDFStreamEngine context) : base(OperatorName.CLIP_EVEN_ODD, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.Clip(0);
    }
}
