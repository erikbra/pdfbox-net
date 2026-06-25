/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/AppearanceGeneratorHelper.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using PdfBox.Net.Util;
using System.Text;

namespace PdfBox.Net.PDModel.Interactive.Form;

internal sealed class AppearanceGeneratorHelper
{
    private const float DefaultPadding = 0.5f;
    private static readonly Operator Bmc = Operator.GetOperator("BMC");
    private static readonly Operator Emc = Operator.GetOperator("EMC");
    private static readonly COSName TxName = COSName.GetPDFName("Tx");

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
        int rotation = ResolveRotation(widget);
        PDRectangle rectangle = widget.GetRectangle()!;
        Matrix rotationMatrix = Matrix.GetRotateInstance(Math.PI * rotation / 180.0, 0, 0);
        Vector transformedUpperRight = rotationMatrix.TransformPoint(rectangle.GetWidth(), rectangle.GetHeight());

        PDRectangle bbox = new(Math.Abs(transformedUpperRight.GetX()), Math.Abs(transformedUpperRight.GetY()));
        stream.SetBBox(bbox);
        if (rotation != 0)
        {
            stream.SetMatrix(CalculateMatrix(bbox, rotation));
        }
        stream.SetFormType(1);
        stream.SetResources(new PDResources());
        return stream;
    }

    private static int ResolveRotation(PDAnnotationWidget widget)
    {
        return widget.GetAppearanceCharacteristics()?.GetRotation() ?? 0;
    }

    private static Matrix CalculateMatrix(PDRectangle bbox, int rotation)
    {
        float tx = 0;
        float ty = 0;
        switch (rotation)
        {
            case 90:
                tx = bbox.GetUpperRightY();
                break;
            case 180:
                tx = bbox.GetUpperRightY();
                ty = bbox.GetUpperRightX();
                break;
            case 270:
                ty = bbox.GetUpperRightX();
                break;
        }

        return Matrix.GetRotateInstance(Math.PI * rotation / 180.0, tx, ty);
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
        ContentStreamWriter writer = new(buffer);
        IList<object> tokens = ParseAppearanceTokens(stream);
        int bmcIndex = IndexOfOperator(tokens, "BMC");
        if (bmcIndex == -1)
        {
            writer.WriteTokens(tokens);
            writer.WriteTokens(TxName, Bmc);
        }
        else
        {
            writer.WriteTokens(tokens.Take(bmcIndex + 1).ToList());
        }

        using (PDAppearanceContentStream contents = new(stream, buffer))
        {
            PDRectangle bbox = ResolveBoundingBox(widget, stream);
            float borderWidth = widget.GetBorderStyle()?.GetWidth() ?? 0f;
            float padding = Math.Max(1f, borderWidth);
            PDRectangle clipRect = ApplyPadding(bbox, padding);
            PDRectangle contentRect = ApplyPadding(clipRect, padding);

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

        int emcIndex = IndexOfOperator(tokens, "EMC");
        if (emcIndex == -1)
        {
            writer.WriteTokens(Emc);
        }
        else
        {
            writer.WriteTokens(tokens.Skip(emcIndex).ToList());
        }

        WriteToStream(buffer.ToArray(), stream);
    }

    private static IList<object> ParseAppearanceTokens(PDAppearanceStream stream)
    {
        if (stream.GetCOSObject()?.HasData() != true)
        {
            return [];
        }

        using Stream input = stream.GetContents();
        return PDFStreamParser.Parse(input);
    }

    private static int IndexOfOperator(IList<object> tokens, string name)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] is Operator op && string.Equals(op.GetName(), name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
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
