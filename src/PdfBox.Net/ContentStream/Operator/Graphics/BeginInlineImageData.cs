/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class BeginInlineImageData : OperatorProcessor
{
    public BeginInlineImageData(PDFStreamEngine context) : base(OperatorName.BEGIN_INLINE_IMAGE_DATA, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.BeginInlineImageData();
    }
}
