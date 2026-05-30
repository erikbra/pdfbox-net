/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/color/SetColor.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.ContentStream.Operator.Color;

public abstract class SetColor : OperatorProcessor
{
    protected SetColor(string name, PDFStreamEngine context)
        : base(name, context)
    {
    }

    public override void Process(Operator op, IList<COSBase> operands)
    {
        PDColorSpace colorSpace = GetColorSpace();
        if (colorSpace is null)
        {
            return;
        }

        if (colorSpace is not PDPattern && operands.Count < colorSpace.GetNumberOfComponents())
        {
            throw new MissingOperandException(op, operands);
        }

        float[] components = new float[operands.Count];
        for (int i = 0; i < operands.Count; i++)
        {
            if (operands[i] is not COSNumber number)
            {
                SetColorValue(new PDColor(Array.Empty<float>(), colorSpace));
                return;
            }

            components[i] = number.FloatValue();
        }

        SetColorValue(new PDColor(components, colorSpace));
    }

    protected abstract PDColorSpace GetColorSpace();

    protected abstract void SetColorValue(PDColor color);
}
