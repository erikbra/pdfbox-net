/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class StrokePath : OperatorProcessor
{
    public StrokePath(PDFStreamEngine context) : base(OperatorName.STROKE_PATH, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.StrokePath();
    }
}
