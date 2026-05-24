/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using System.IO;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetNonStrokingColorSpace : OperatorProcessor
{
    public SetNonStrokingColorSpace(PDFStreamEngine context) : base(OperatorName.NON_STROKING_COLORSPACE, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1) return;
        try
        {
            Context.SetNonStrokingColorSpace(PDColorSpaceFactory.Create(operands[0], Context.GetCurrentPage()?.GetResources()));
        }
        catch (IOException)
        {
            // ignore invalid color spaces
        }
    }
}
