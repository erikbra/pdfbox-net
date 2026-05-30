/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetNonStrokingColorN : SetNonStrokingColor
{
    public SetNonStrokingColorN(PDFStreamEngine context)
        : base(OperatorName.NON_STROKING_COLOR_N, context)
    {
    }
}
