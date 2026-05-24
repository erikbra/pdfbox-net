/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class EndInlineImage : OperatorProcessor
{
    public EndInlineImage(PDFStreamEngine context) : base(OperatorName.END_INLINE_IMAGE, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.EndInlineImage();
    }
}
