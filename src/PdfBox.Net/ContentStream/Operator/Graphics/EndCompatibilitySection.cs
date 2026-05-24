/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class EndCompatibilitySection : OperatorProcessor
{
    public EndCompatibilitySection(PDFStreamEngine context) : base(OperatorName.END_COMPATIBILITY_SECTION, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.EndCompatibilitySection();
    }
}
