/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using System.IO;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetStrokingColorSpace : OperatorProcessor
{
    public SetStrokingColorSpace(PDFStreamEngine context) : base(OperatorName.STROKING_COLORSPACE, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1) return;
        try
        {
            Context.SetStrokingColorSpace(PDColorSpaceFactory.Create(operands[0], Context.GetCurrentPage()?.GetResources()));
        }
        catch (IOException)
        {
            // ignore invalid color spaces
        }
    }
}
