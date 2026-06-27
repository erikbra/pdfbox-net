/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetNonStrokingColorSpace.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

using System.IO;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetNonStrokingColorSpace : OperatorProcessor
{
    public SetNonStrokingColorSpace(PDFStreamEngine context) : base(OperatorName.NON_STROKING_COLORSPACE, context) { }

    public override string GetName()
    {
        return Name;
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1) return;
        try
        {
            Context.SetNonStrokingColorSpace(PDColorSpaceFactory.Create(operands[0], Context.GetResources()));
        }
        catch (IOException)
        {
            // ignore invalid color spaces
        }
    }
}
