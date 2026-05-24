/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class BeginInlineImage : OperatorProcessor
{
    public BeginInlineImage(PDFStreamEngine context) : base(OperatorName.BEGIN_INLINE_IMAGE, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.BeginInlineImage();
    }
}
