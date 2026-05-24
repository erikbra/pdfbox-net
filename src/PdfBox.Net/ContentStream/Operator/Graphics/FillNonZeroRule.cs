/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class FillNonZeroRule : OperatorProcessor
{
    public FillNonZeroRule(PDFStreamEngine context, string? name = null) : base(name ?? OperatorName.FILL_NON_ZERO, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.FillPath(1);
    }
}
