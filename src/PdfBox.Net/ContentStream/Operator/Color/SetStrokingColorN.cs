/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetStrokingColorN.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 */

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetStrokingColorN : SetStrokingColor
{
    public SetStrokingColorN(PDFStreamEngine context)
        : base(OperatorName.STROKING_COLOR_N, context)
    {
    }
    public override string GetName()
    {
        return Name;
    }

}
