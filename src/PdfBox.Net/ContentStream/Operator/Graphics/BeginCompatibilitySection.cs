/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator.Graphics;

public sealed class BeginCompatibilitySection : OperatorProcessor
{
    public BeginCompatibilitySection(PDFStreamEngine context) : base(OperatorName.BEGIN_COMPATIBILITY_SECTION, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        Context.BeginCompatibilitySection();
    }
}
