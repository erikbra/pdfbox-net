/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageContentStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfWriter;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel;

/// <summary>
/// Represents a content stream writer for a PDF page.
/// Supports overwrite, append, and prepend modes.
/// </summary>
public sealed class PDPageContentStream : IDisposable
{
    /// <summary>
    /// Specifies how the new content stream is placed relative to any existing content.
    /// </summary>
    public enum AppendMode
    {
        /// <summary>Overwrite all existing content.</summary>
        OVERWRITE,

        /// <summary>Append new content after existing content.</summary>
        APPEND,

        /// <summary>Prepend new content before existing content.</summary>
        PREPEND,
    }

    private readonly PDDocument _document;
    private readonly PDPage _page;
    private readonly MemoryStream _buffer;
    private readonly ContentStreamWriter _writer;
    private readonly AppendMode _appendMode;
    private readonly bool _compress;
    private bool _disposed;
    private int _graphicsStateCounter;
    private readonly Stack<PDColorSpace?> _strokingColorSpaceStack = new();
    private readonly Stack<PDColorSpace?> _nonStrokingColorSpaceStack = new();

    /// <summary>
    /// Creates a content stream for the given page using <see cref="AppendMode.OVERWRITE"/>.
    /// </summary>
    public PDPageContentStream(PDDocument document, PDPage page)
        : this(document, page, AppendMode.OVERWRITE, true)
    {
    }

    /// <summary>
    /// Creates a content stream for the given page with the specified append mode.
    /// </summary>
    public PDPageContentStream(PDDocument document, PDPage page, AppendMode appendMode, bool compress)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _page = page ?? throw new ArgumentNullException(nameof(page));
        _appendMode = appendMode;
        _compress = compress;
        _buffer = new MemoryStream();
        _writer = new ContentStreamWriter(_buffer);
    }

    public void BeginText() => WriteOperator("BT");

    public void EndText() => WriteOperator("ET");

    public void ShowText(string text) => WriteOperator("Tj", new COSString(text ?? string.Empty));

    public void NewLineAtOffset(float tx, float ty) => WriteOperator("Td", tx, ty);

    public void SetLeading(float leading) => WriteOperator("TL", leading);

    public void NewLine() => WriteOperator("T*");

    public void SetFont(PDFont font, float fontSize)
    {
        ArgumentNullException.ThrowIfNull(font);
        PDResources resources = _page.GetResources() ?? new PDResources();
        _page.SetResources(resources);
        COSName fontName = AddFontResource(resources, font);
        WriteOperator("Tf", fontName, fontSize);
    }

    public void MoveTo(float x, float y) => WriteOperator("m", x, y);

    public void LineTo(float x, float y) => WriteOperator("l", x, y);

    public void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3) =>
        WriteOperator("c", x1, y1, x2, y2, x3, y3);

    public void AddRect(float x, float y, float width, float height) => WriteOperator("re", x, y, width, height);

    public void ClosePath() => WriteOperator("h");

    public void Stroke() => WriteOperator("S");

    public void Fill() => WriteOperator("f");

    public void FillAndStroke() => WriteOperator("B");

    public void CloseAndStroke() => WriteOperator("s");

    public void CloseAndFillAndStroke() => WriteOperator("b");

    /// <summary>
    /// Registers the shading resource on the page and fills the current clipping region with it.
    /// </summary>
    /// <param name="shading">The shading resource to invoke.</param>
    public void ShadingFill(PDShading shading)
    {
        ArgumentNullException.ThrowIfNull(shading);
        PDResources resources = _page.GetResources() ?? new PDResources();
        _page.SetResources(resources);
        COSName shadingName = resources.Add(shading, "Sh");
        WriteOperator("sh", shadingName);
    }

    public void Clip()
    {
        WriteOperator("W");
        WriteOperator("n");
    }

    /// <summary>
    /// Writes a save-graphics-state operator (q).
    /// </summary>
    public void SaveGraphicsState()
    {
        if (_strokingColorSpaceStack.Count > 0)
        {
            _strokingColorSpaceStack.Push(_strokingColorSpaceStack.Peek());
        }

        if (_nonStrokingColorSpaceStack.Count > 0)
        {
            _nonStrokingColorSpaceStack.Push(_nonStrokingColorSpaceStack.Peek());
        }

        WriteOperator("q");
    }

    /// <summary>
    /// Writes a restore-graphics-state operator (Q).
    /// </summary>
    public void RestoreGraphicsState()
    {
        if (_strokingColorSpaceStack.Count > 0)
        {
            _strokingColorSpaceStack.Pop();
        }

        if (_nonStrokingColorSpaceStack.Count > 0)
        {
            _nonStrokingColorSpaceStack.Pop();
        }

        WriteOperator("Q");
    }

    /// <summary>
    /// Writes a set-text-matrix operator (Tm) for the given matrix.
    /// </summary>
    public void SetTextMatrix(Matrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        WriteOperator("Tm",
            matrix.GetScaleX(),
            matrix.GetShearY(),
            matrix.GetShearX(),
            matrix.GetScaleY(),
            matrix.GetTranslateX(),
            matrix.GetTranslateY());
    }

    /// <summary>
    /// Set the non-stroking (fill) color in the DeviceGray color space. Range is 0..1.
    /// </summary>
    /// <param name="gray">The gray value.</param>
    public void SetNonStrokingColor(float gray)
    {
        if (IsOutsideOneInterval(gray))
        {
            throw new ArgumentOutOfRangeException(nameof(gray), "Parameter must be within 0..1, but is " + gray);
        }

        WriteOperator("g", gray);
        UpdateNonStrokingColorSpaceStack(PDDeviceGray.Instance);
    }

    /// <summary>
    /// Set the stroking color in the DeviceGray color space. Range is 0..1.
    /// </summary>
    /// <param name="gray">The gray value.</param>
    public void SetStrokingColor(float gray)
    {
        if (IsOutsideOneInterval(gray))
        {
            throw new ArgumentOutOfRangeException(nameof(gray), "Parameter must be within 0..1, but is " + gray);
        }

        WriteOperator("G", gray);
        UpdateStrokingColorSpaceStack(PDDeviceGray.Instance);
    }

    /// <summary>
    /// Set the non-stroking (fill) color in the DeviceRGB color space. Range is 0..1.
    /// </summary>
    public void SetNonStrokingColor(float r, float g, float b)
    {
        if (IsOutsideOneInterval(r) || IsOutsideOneInterval(g) || IsOutsideOneInterval(b))
        {
            throw new ArgumentOutOfRangeException(nameof(r),
                $"Parameters must be within 0..1, but are ({r:F2},{g:F2},{b:F2})");
        }

        WriteOperator("rg", r, g, b);
        UpdateNonStrokingColorSpaceStack(PDDeviceRGB.Instance);
    }

    /// <summary>
    /// Set the stroking color in the DeviceRGB color space. Range is 0..1.
    /// </summary>
    public void SetStrokingColor(float r, float g, float b)
    {
        if (IsOutsideOneInterval(r) || IsOutsideOneInterval(g) || IsOutsideOneInterval(b))
        {
            throw new ArgumentOutOfRangeException(nameof(r),
                $"Parameters must be within 0..1, but are ({r:F2},{g:F2},{b:F2})");
        }

        WriteOperator("RG", r, g, b);
        UpdateStrokingColorSpaceStack(PDDeviceRGB.Instance);
    }

    /// <summary>
    /// Set the non-stroking (fill) color in the DeviceCMYK color space. Range is 0..1.
    /// </summary>
    public void SetNonStrokingColor(float c, float m, float y, float k)
    {
        if (IsOutsideOneInterval(c) || IsOutsideOneInterval(m) || IsOutsideOneInterval(y) || IsOutsideOneInterval(k))
        {
            throw new ArgumentOutOfRangeException(nameof(c),
                $"Parameters must be within 0..1, but are ({c:F2},{m:F2},{y:F2},{k:F2})");
        }

        WriteOperator("k", c, m, y, k);
        UpdateNonStrokingColorSpaceStack(PDDeviceCMYK.Instance);
    }

    /// <summary>
    /// Set the stroking color in the DeviceCMYK color space. Range is 0..1.
    /// </summary>
    public void SetStrokingColor(float c, float m, float y, float k)
    {
        if (IsOutsideOneInterval(c) || IsOutsideOneInterval(m) || IsOutsideOneInterval(y) || IsOutsideOneInterval(k))
        {
            throw new ArgumentOutOfRangeException(nameof(c),
                $"Parameters must be within 0..1, but are ({c:F2},{m:F2},{y:F2},{k:F2})");
        }

        WriteOperator("K", c, m, y, k);
        UpdateStrokingColorSpaceStack(PDDeviceCMYK.Instance);
    }

    /// <summary>
    /// Sets the non-stroking (fill) color and, if necessary, the non-stroking color space.
    /// </summary>
    /// <param name="color">Color in a specific color space.</param>
    public void SetNonStrokingColor(PDColor color)
    {
        ArgumentNullException.ThrowIfNull(color);
        PDColorSpace? colorSpace = color.GetColorSpace();
        if (colorSpace is null)
        {
            return;
        }

        if (_nonStrokingColorSpaceStack.Count == 0 ||
            _nonStrokingColorSpaceStack.Peek() != colorSpace)
        {
            WriteOperator("cs", GetColorSpaceName(colorSpace));
            UpdateNonStrokingColorSpaceStack(colorSpace);
        }

        List<object> operands = BuildColorOperands(color, colorSpace);
        bool useN = colorSpace is PDPattern || colorSpace is PDSeparation ||
                    colorSpace is PDDeviceN || colorSpace is PDICCBased;
        WriteOperator(useN ? "scn" : "sc", operands.ToArray());
    }

    /// <summary>
    /// Sets the stroking color and, if necessary, the stroking color space.
    /// </summary>
    /// <param name="color">Color in a specific color space.</param>
    public void SetStrokingColor(PDColor color)
    {
        ArgumentNullException.ThrowIfNull(color);
        PDColorSpace? colorSpace = color.GetColorSpace();
        if (colorSpace is null)
        {
            return;
        }

        if (_strokingColorSpaceStack.Count == 0 ||
            _strokingColorSpaceStack.Peek() != colorSpace)
        {
            WriteOperator("CS", GetColorSpaceName(colorSpace));
            UpdateStrokingColorSpaceStack(colorSpace);
        }

        List<object> operands = BuildColorOperands(color, colorSpace);
        bool useN = colorSpace is PDPattern || colorSpace is PDSeparation ||
                    colorSpace is PDDeviceN || colorSpace is PDICCBased;
        WriteOperator(useN ? "SCN" : "SC", operands.ToArray());
    }

    /// <summary>
    /// Sets the non-stroking (fill) color space.
    /// </summary>
    /// <param name="colorSpace">The color space.</param>
    public void SetNonStrokingColorSpace(PDColorSpace colorSpace)
    {
        ArgumentNullException.ThrowIfNull(colorSpace);
        WriteOperator("cs", GetColorSpaceName(colorSpace));
        UpdateNonStrokingColorSpaceStack(colorSpace);
    }

    /// <summary>
    /// Sets the stroking color space.
    /// </summary>
    /// <param name="colorSpace">The color space.</param>
    public void SetStrokingColorSpace(PDColorSpace colorSpace)
    {
        ArgumentNullException.ThrowIfNull(colorSpace);
        WriteOperator("CS", GetColorSpaceName(colorSpace));
        UpdateStrokingColorSpaceStack(colorSpace);
    }

    /// <summary>
    /// Sets the graphics state parameters from a named extended graphics state resource (gs).
    /// </summary>
    public void SetGraphicsStateParameters(PDExtendedGraphicsState graphicsState)
    {
        ArgumentNullException.ThrowIfNull(graphicsState);

        PDResources resources = _page.GetResources() ?? new PDResources();
        _page.SetResources(resources);
        COSName resourceName = COSName.GetPDFName($"GS{_graphicsStateCounter++}");
        resources.Put(resourceName, graphicsState);
        WriteOperator("gs", resourceName);
    }

    /// <summary>
    /// Writes a concatenate-matrix operator (cm) for the given matrix.
    /// </summary>
    public void Transform(Matrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        WriteOperator("cm",
            matrix.GetScaleX(),
            matrix.GetShearY(),
            matrix.GetShearX(),
            matrix.GetScaleY(),
            matrix.GetTranslateX(),
            matrix.GetTranslateY());
    }

    /// <summary>
    /// Writes a begin-marked-content operator (BDC) with the given tag and inline property list.
    /// </summary>
    public void BeginMarkedContent(COSName tag, PDPropertyList properties)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(properties);

        PDResources resources = _page.GetResources() ?? new PDResources();
        _page.SetResources(resources);
        COSName propName = resources.Add(properties, "MC");

        WriteOperator("BDC", tag, propName);
    }

    /// <summary>
    /// Writes an end-marked-content operator (EMC).
    /// </summary>
    public void EndMarkedContent() => WriteOperator("EMC");

    /// <summary>
    /// Writes a draw-form-XObject operator (Do) for the given form.
    /// </summary>
    public void DrawForm(PDFormXObject form)
    {
        ArgumentNullException.ThrowIfNull(form);

        PDResources resources = _page.GetResources() ?? new PDResources();
        _page.SetResources(resources);
        COSName formName = resources.Add(form, "Form");

        WriteOperator("Do", formName);
    }

    /// <summary>
    /// Draws an image at the given coordinates using the image's intrinsic size.
    /// </summary>
    public void DrawImage(PDImageXObject image, float x, float y)
    {
        ArgumentNullException.ThrowIfNull(image);
        DrawImage(image, x, y, image.GetWidth(), image.GetHeight());
    }

    /// <summary>
    /// Draws an image at the given coordinates and size.
    /// </summary>
    public void DrawImage(PDImageXObject image, float x, float y, float width, float height)
    {
        ArgumentNullException.ThrowIfNull(image);
        DrawImage(image, new Matrix(width, 0, 0, height, x, y));
    }

    /// <summary>
    /// Draws an image at the origin using the provided transformation matrix.
    /// </summary>
    public void DrawImage(PDImageXObject image, Matrix matrix)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(matrix);

        PDResources resources = _page.GetResources() ?? new PDResources();
        _page.SetResources(resources);
        COSName imageName = resources.Add(image, "Im");

        SaveGraphicsState();
        Transform(matrix);
        WriteOperator("Do", imageName);
        RestoreGraphicsState();
    }

    /// <summary>
    /// Flushes and commits the buffered content to the page's content stream(s).
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _buffer.Flush();
        byte[] newBytes = _buffer.ToArray();
        _buffer.Dispose();

        if (newBytes.Length == 0)
        {
            return;
        }

        COSStream newStream = CreateStream(newBytes);
        COSBase? existingContents = _page.GetContents();

        switch (_appendMode)
        {
            case AppendMode.OVERWRITE:
            {
                COSArray array = new();
                array.Add(newStream);
                ((COSDictionary)_page.GetCOSObject()).SetItem(COSName.CONTENTS, array);
                break;
            }
            case AppendMode.APPEND:
            {
                COSArray array = BuildContentArray(existingContents);
                array.Add(newStream);
                ((COSDictionary)_page.GetCOSObject()).SetItem(COSName.CONTENTS, array);
                break;
            }
            case AppendMode.PREPEND:
            {
                COSArray array = BuildContentArray(existingContents);
                array.Add(0, newStream);
                ((COSDictionary)_page.GetCOSObject()).SetItem(COSName.CONTENTS, array);
                break;
            }
        }
    }

    private static COSArray BuildContentArray(COSBase? existing)
    {
        if (existing is COSArray existingArray)
        {
            return existingArray;
        }

        COSArray array = new();
        if (existing is COSStream stream)
        {
            array.Add(stream);
        }

        return array;
    }

    private COSStream CreateStream(byte[] bytes)
    {
        COSStream stream = new();
        if (_compress)
        {
            using Stream output = stream.CreateOutputStream(COSName.FLATE_DECODE);
            output.Write(bytes);
        }
        else
        {
            using Stream output = stream.CreateOutputStream();
            output.Write(bytes);
        }

        return stream;
    }

    private static COSName AddFontResource(PDResources resources, PDFont font)
    {
        foreach (COSName existingName in resources.GetFontNames())
        {
            PDFont? existingFont = resources.GetFont(existingName);
            if (existingFont is not null && ReferenceEquals(existingFont.GetCOSObject(), font.GetCOSObject()))
            {
                return existingName;
            }
        }

        HashSet<string> existingNames = resources.GetFontNames().Select(name => name.GetName()).ToHashSet(StringComparer.Ordinal);
        int counter = 1;
        string name;
        do
        {
            name = $"F{counter++}";
        } while (existingNames.Contains(name));

        COSName fontName = COSName.GetPDFName(name);
        resources.Put(fontName, font);
        return fontName;
    }

    private COSName GetColorSpaceName(PDColorSpace colorSpace)
    {
        if (colorSpace is PDDeviceGray || colorSpace is PDDeviceRGB || colorSpace is PDDeviceCMYK)
        {
            return COSName.GetPDFName(colorSpace.GetName());
        }

        PDResources resources = _page.GetResources() ?? new PDResources();
        _page.SetResources(resources);
        return resources.Add(colorSpace, "cs");
    }

    private void UpdateStrokingColorSpaceStack(PDColorSpace colorSpace)
    {
        if (_strokingColorSpaceStack.Count > 0)
        {
            _strokingColorSpaceStack.Pop();
        }
        _strokingColorSpaceStack.Push(colorSpace);
    }

    private void UpdateNonStrokingColorSpaceStack(PDColorSpace colorSpace)
    {
        if (_nonStrokingColorSpaceStack.Count > 0)
        {
            _nonStrokingColorSpaceStack.Pop();
        }
        _nonStrokingColorSpaceStack.Push(colorSpace);
    }

    private static List<object> BuildColorOperands(PDColor color, PDColorSpace colorSpace)
    {
        List<object> operands = new();
        foreach (float value in color.GetComponents())
        {
            operands.Add(value);
        }

        if (colorSpace is PDPattern)
        {
            COSName? patternName = color.GetPatternName();
            if (patternName is not null)
            {
                operands.Add(patternName);
            }
        }

        return operands;
    }

    private static bool IsOutsideOneInterval(float value) => value < 0f || value > 1f;

    private void WriteOperator(string name, params object[] operands)
    {
        List<object> tokens = new(operands.Length + 1);
        foreach (object operand in operands)
        {
            tokens.Add(ToCosOperand(operand));
        }

        tokens.Add(Operator.GetOperator(name));
        _writer.WriteTokens(tokens);
    }

    private static object ToCosOperand(object operand) =>
        operand switch
        {
            COSBase cosBase => cosBase,
            COSObjectable objectable => objectable.GetCOSObject(),
            int intValue => COSInteger.Get(intValue),
            float floatValue => new COSFloat(floatValue),
            double doubleValue => new COSFloat((float)doubleValue),
            long longValue => COSInteger.Get(longValue),
            string text => new COSString(text),
            _ => throw new ArgumentException(
                $"Unsupported content stream operand type {operand.GetType().FullName}",
                nameof(operand))
        };
}
