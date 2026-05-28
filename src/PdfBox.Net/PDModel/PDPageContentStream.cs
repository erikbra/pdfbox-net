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
using PdfBox.Net.PDModel.Graphics.Form;
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

    /// <summary>
    /// Writes a save-graphics-state operator (q).
    /// </summary>
    public void SaveGraphicsState() => WriteOperator("q");

    /// <summary>
    /// Writes a restore-graphics-state operator (Q).
    /// </summary>
    public void RestoreGraphicsState() => WriteOperator("Q");

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
