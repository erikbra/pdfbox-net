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
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;
using System.Text;

namespace PdfBox.Net.PDModel.Interactive.Form;

internal sealed class AppearanceGeneratorHelper
{
    private const float DefaultPadding = 1f;

    private readonly PDVariableText _field;
    private readonly PDDefaultAppearanceString? _defaultAppearance;

    internal AppearanceGeneratorHelper(PDVariableText field)
    {
        _field = field ?? throw new ArgumentNullException(nameof(field));
        try
        {
            _defaultAppearance = field.GetDefaultAppearanceString();
        }
        catch (ArgumentException)
        {
            _defaultAppearance = null;
        }
        catch (IOException)
        {
            _defaultAppearance = null;
        }
    }

    internal void SetAppearanceValue(string value)
    {
        string appearanceValue = NormalizeValue(value);
        foreach (PDAnnotationWidget widget in _field.GetWidgets())
        {
            PDRectangle? rect = widget.GetRectangle();
            if (rect == null || rect.GetWidth() <= 0 || rect.GetHeight() <= 0)
            {
                continue;
            }

            PDAppearanceDictionary appearance = widget.GetAppearance() ?? new PDAppearanceDictionary();
            widget.SetAppearance(appearance);

            PDAppearanceStream stream = widget.GetNormalAppearanceStream() ?? PrepareNormalAppearanceStream(widget);
            appearance.SetNormalAppearance(stream);
            if (_defaultAppearance?.Font == null)
            {
                WriteFallbackValue(stream, appearanceValue);
                continue;
            }

            InitializeWidgetAppearance(widget, stream);
            WriteValue(widget, stream, appearanceValue);
        }
    }

    private string NormalizeValue(string value)
    {
        if (_field is PDTextField textField && !textField.IsMultiline())
        {
            return value
                .Replace("\r\n", " ", StringComparison.Ordinal)
                .Replace('\n', ' ')
                .Replace('\r', ' ');
        }

        return value ?? string.Empty;
    }

    private PDAppearanceStream PrepareNormalAppearanceStream(PDAnnotationWidget widget)
    {
        PDAppearanceStream stream = new(_field.GetAcroForm().GetDocument());
        PDRectangle bbox = widget.GetRectangle()!.CreateRetranslatedRectangle();
        stream.SetBBox(bbox);
        stream.SetMatrix(Matrix.GetTranslateInstance(-widget.GetRectangle()!.GetLowerLeftX(), -widget.GetRectangle()!.GetLowerLeftY()));
        stream.SetFormType(1);
        stream.SetResources(new PDResources());
        return stream;
    }

    private void InitializeWidgetAppearance(PDAnnotationWidget widget, PDAppearanceStream stream)
    {
        PDAppearanceCharacteristicsDictionary? characteristics = widget.GetAppearanceCharacteristics();
        if (characteristics == null)
        {
            return;
        }

        using MemoryStream buffer = new();
        using (PDAppearanceContentStream contents = new(stream, buffer))
        {
            PDRectangle bbox = ResolveBoundingBox(widget, stream);
            if (characteristics.GetBackground() is { } background)
            {
                contents.SetNonStrokingColor(background);
                contents.AddRect(bbox.GetLowerLeftX(), bbox.GetLowerLeftY(), bbox.GetWidth(), bbox.GetHeight());
                contents.Fill();
            }

            if (characteristics.GetBorderColour() is { } borderColor)
            {
                float lineWidth = widget.GetBorderStyle()?.GetWidth() ?? 1f;
                contents.SetStrokingColor(borderColor);
                contents.SetLineWidthOnDemand(lineWidth);
                PDRectangle inner = ApplyPadding(bbox, Math.Max(DefaultPadding, lineWidth / 2f));
                contents.AddRect(inner.GetLowerLeftX(), inner.GetLowerLeftY(), inner.GetWidth(), inner.GetHeight());
                contents.CloseAndStroke();
            }
        }

        WriteToStream(buffer.ToArray(), stream);
    }

    private void WriteValue(PDAnnotationWidget widget, PDAppearanceStream stream, string value)
    {
        PDDefaultAppearanceString defaultAppearance = _defaultAppearance!;
        using MemoryStream buffer = new();
        using (PDAppearanceContentStream contents = new(stream, buffer))
        {
            PDRectangle bbox = ResolveBoundingBox(widget, stream);
            PDRectangle clipRect = ApplyPadding(bbox, DefaultPadding);
            PDRectangle contentRect = ApplyPadding(clipRect, DefaultPadding);

            defaultAppearance.CopyNeededResourcesTo(stream);

            PDFont font = defaultAppearance.Font
                ?? throw new IOException("Widget appearance generation requires a resolved font.");

            float fontSize = defaultAppearance.FontSize == 0 ? 12f : defaultAppearance.FontSize;
            float boundingHeight = font.GetBoundingBox().GetHeight();
            float leading = boundingHeight > 0
                ? boundingHeight * fontSize / 1000f
                : fontSize * 1.2f;

            contents.SaveGraphicsState();
            contents.AddRect(clipRect.GetLowerLeftX(), clipRect.GetLowerLeftY(), clipRect.GetWidth(), clipRect.GetHeight());
            contents.Clip();
            contents.BeginText();
            defaultAppearance.WriteTo(contents, fontSize);

            float baseline = contentRect.GetUpperRightY() - fontSize;
            AppearanceStyle style = new();
            style.SetFont(font);
            style.SetFontSize(fontSize);
            style.SetLeading(leading);

            PlainTextFormatter formatter = new PlainTextFormatter.Builder(contents)
                .Style(style)
                .Text(new PlainText(value))
                .Width(contentRect.GetWidth())
                .WrapLines(_field is PDTextField textField && textField.IsMultiline())
                .InitialOffset(contentRect.GetLowerLeftX(), baseline)
                .TextAlign(widget.GetCOSDictionary().GetInt(COSName.GetPDFName("Q"), _field.GetQ()))
                .Build();
            formatter.Format();
            contents.EndText();
            contents.RestoreGraphicsState();
        }

        WriteToStream(buffer.ToArray(), stream);
    }

    private static void WriteFallbackValue(PDAppearanceStream stream, string value)
    {
        using Stream output = stream.GetCOSObject()!.CreateOutputStream();
        using StreamWriter writer = new(output, Encoding.ASCII, leaveOpen: true);
        writer.WriteLine("q");
        writer.WriteLine("BT");
        writer.Write('(');
        writer.Write(EscapeLiteral(value));
        writer.WriteLine(") Tj");
        writer.WriteLine("ET");
        writer.WriteLine("Q");
        writer.Flush();
    }

    private static string EscapeLiteral(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static void WriteToStream(byte[] data, PDAppearanceStream stream)
    {
        using Stream output = stream.GetCOSObject()!.CreateOutputStream();
        output.Write(data);
    }

    private static PDRectangle ResolveBoundingBox(PDAnnotationWidget widget, PDAppearanceStream stream)
    {
        return stream.GetBBox() ?? widget.GetRectangle()!.CreateRetranslatedRectangle();
    }

    private static PDRectangle ApplyPadding(PDRectangle rectangle, float padding)
    {
        return new PDRectangle(
            rectangle.GetLowerLeftX() + padding,
            rectangle.GetLowerLeftY() + padding,
            Math.Max(0, rectangle.GetWidth() - 2 * padding),
            Math.Max(0, rectangle.GetHeight() - 2 * padding));
    }
}
