/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/AppearanceGeneratorHelper.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using System.Text;

namespace PdfBox.Net.PDModel.Interactive.Form;

internal sealed class AppearanceGeneratorHelper
{
    private readonly PDVariableText _field;

    internal AppearanceGeneratorHelper(PDVariableText field)
    {
        _field = field;
    }

    internal void SetAppearanceValue(string? value)
    {
        foreach (PDAnnotationWidget widget in GetWidgets())
        {
            EnsureWidgetAppearance(widget, value);
        }
    }

    private IEnumerable<PDAnnotationWidget> GetWidgets()
    {
        COSDictionary fieldDictionary = _field.GetCOSDictionary();

        if (IsWidgetDictionary(fieldDictionary))
        {
            yield return new PDAnnotationWidget(fieldDictionary);
            yield break;
        }

        COSArray? kids = fieldDictionary.GetCOSArray(COSName.KIDS);
        if (kids == null)
        {
            yield break;
        }

        foreach (COSBase? kid in kids)
        {
            if (kid is COSDictionary kidDictionary && IsWidgetDictionary(kidDictionary))
            {
                yield return new PDAnnotationWidget(kidDictionary);
            }
        }
    }

    private static bool IsWidgetDictionary(COSDictionary dictionary)
    {
        return string.Equals(dictionary.GetNameAsString(COSName.SUBTYPE), PDAnnotationWidget.SUB_TYPE, StringComparison.Ordinal);
    }

    private void EnsureWidgetAppearance(PDAnnotationWidget widget, string? value)
    {
        PDAppearanceDictionary appearance = widget.GetAppearance() ?? new PDAppearanceDictionary();
        widget.SetAppearance(appearance);

        PDAppearanceStream stream;
        PDAppearanceEntry? entry = appearance.GetNormalAppearance();
        if (entry?.IsStream() == true)
        {
            stream = entry.GetAppearanceStream();
        }
        else
        {
            stream = new PDAppearanceStream(new COSStream());
            appearance.SetNormalAppearance(stream);
        }

        PDRectangle rect = widget.GetRectangle() ?? new PDRectangle(0, 0, 1, 1);
        if (Math.Abs(rect.GetWidth()) < float.Epsilon)
        {
            rect.SetUpperRightX(rect.GetLowerLeftX() + 1);
        }

        if (Math.Abs(rect.GetHeight()) < float.Epsilon)
        {
            rect.SetUpperRightY(rect.GetLowerLeftY() + 1);
        }

        stream.SetBBox(new PDRectangle(rect.GetLowerLeftX(), rect.GetLowerLeftY(), rect.GetWidth(), rect.GetHeight()));
        stream.SetMatrix(1, 0, 0, 1, -rect.GetLowerLeftX(), -rect.GetLowerLeftY());
        stream.SetResources(stream.GetResources() ?? new PDResources());

        using Stream output = stream.GetContentStream().CreateOutputStream();
        using StreamWriter writer = new(output, Encoding.ASCII, leaveOpen: true);
        writer.WriteLine("% WidgetAppearance");
        writer.WriteLine("BT");
        writer.WriteLine($"({EscapeLiteral(value ?? string.Empty)}) Tj");
        writer.WriteLine("ET");
        writer.Flush();
    }

    private static string EscapeLiteral(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("(", "\\(", StringComparison.Ordinal)
                    .Replace(")", "\\)", StringComparison.Ordinal);
    }
}
