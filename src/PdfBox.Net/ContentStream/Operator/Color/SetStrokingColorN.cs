/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetStrokingColorN : SetStrokingColor
{
    public SetStrokingColorN(PDFStreamEngine context)
        : base(OperatorName.STROKING_COLOR_N, context)
    {
    }
}
