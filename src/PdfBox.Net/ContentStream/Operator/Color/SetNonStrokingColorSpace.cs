/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public sealed class SetNonStrokingColorSpace : OperatorProcessor
{
    public SetNonStrokingColorSpace(PDFStreamEngine context) : base(OperatorName.NON_STROKING_COLORSPACE, context) { }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        if (operands.Count < 1 || operands[0] is not COSName colorSpaceName) return;
        Context.SetNonStrokingColorSpace(CreateColorSpace(colorSpaceName.GetName()));
    }

    private static PDColorSpace CreateColorSpace(string name) => name switch
    {
        "DeviceGray" => new PDColorSpace(name, 1),
        "DeviceRGB" => new PDColorSpace(name, 3),
        "DeviceCMYK" => new PDColorSpace(name, 4),
        _ => new PDColorSpace(name)
    };
}
