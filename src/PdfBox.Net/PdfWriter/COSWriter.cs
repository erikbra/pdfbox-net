/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/COSWriter.java
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
using PdfBox.Net.COS;

namespace PdfBox.Net.PdfWriter;

/// <summary>
/// A class that can be used to write a document to a stream in PDF syntax.
/// Implements <see cref="ICOSVisitor"/> to route serialization through the visitor pattern.
/// </summary>
public sealed class COSWriter : ICOSVisitor
{
    public static readonly byte[] DICT_OPEN = Encoding.ASCII.GetBytes("<<");
    public static readonly byte[] DICT_CLOSE = Encoding.ASCII.GetBytes(">>");
    public static readonly byte[] SPACE = [(byte)' '];
    public static readonly byte[] ARRAY_OPEN = [(byte)'['];
    public static readonly byte[] ARRAY_CLOSE = [(byte)']'];

    private readonly COSStandardOutputStream _output;

    public COSWriter(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = new COSStandardOutputStream(output);
    }

    public void Write(COSBase? value)
    {
        if (value is null)
        {
            COSNull.NULL.Accept(this);
            return;
        }

        value.Accept(this);
    }

    /// <inheritdoc/>
    public void VisitFromArray(COSArray obj)
    {
        obj.WritePDF(_output);
    }

    /// <inheritdoc/>
    public void VisitFromBoolean(COSBoolean obj)
    {
        obj.WritePDF(_output);
    }

    /// <inheritdoc/>
    public void VisitFromDictionary(COSDictionary obj)
    {
        obj.WritePDF(_output);
    }

    /// <inheritdoc/>
    public void VisitFromDocument(COSDocument obj)
    {
        // Full document serialization is out of scope for this low-level writer.
    }

    /// <inheritdoc/>
    public void VisitFromFloat(COSFloat obj)
    {
        obj.WritePDF(_output);
    }

    /// <inheritdoc/>
    public void VisitFromInt(COSInteger obj)
    {
        obj.WritePDF(_output);
    }

    /// <inheritdoc/>
    public void VisitFromName(COSName obj)
    {
        obj.WritePDF(_output);
    }

    /// <inheritdoc/>
    public void VisitFromNull(COSNull obj)
    {
        obj.WritePDF(_output);
    }

    /// <inheritdoc/>
    public void VisitFromObject(COSObject obj)
    {
        COSBase? inner = obj.GetObject();
        if (inner is null)
        {
            COSNull.NULL.Accept(this);
        }
        else
        {
            inner.Accept(this);
        }
    }

    /// <inheritdoc/>
    public void VisitFromStream(COSStream obj)
    {
        // Write the stream dictionary portion only; the stream body
        // is handled by the full PDF document writer.
        obj.WritePDF(_output);
    }

    /// <inheritdoc/>
    public void VisitFromString(COSString obj)
    {
        obj.WritePDF(_output);
    }

    public static byte[] Serialize(COSBase? value)
    {
        using MemoryStream output = new();
        COSWriter writer = new(output);
        writer.Write(value);
        writer._output.Flush();
        return output.ToArray();
    }

    public static string SerializeToString(COSBase? value)
    {
        return Encoding.Latin1.GetString(Serialize(value));
    }

    public static void WriteString(COSString value, Stream output)
    {
        ArgumentNullException.ThrowIfNull(value);
        WriteString(value.GetBytes(), value.GetForceHexForm(), output);
    }

    public static void WriteString(byte[] bytes, Stream output)
    {
        WriteString(bytes, forceHex: false, output);
    }

    private static void WriteString(byte[] bytes, bool forceHex, Stream output)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentNullException.ThrowIfNull(output);

        bool isAscii = true;
        if (!forceHex)
        {
            foreach (byte b in bytes)
            {
                if (b >= 0x80 || b is 0x0d or 0x0a)
                {
                    isAscii = false;
                    break;
                }
            }
        }

        if (isAscii && !forceHex)
        {
            output.WriteByte((byte)'(');
            foreach (byte b in bytes)
            {
                switch (b)
                {
                    case (byte)'(':
                    case (byte)')':
                    case (byte)'\\':
                        output.WriteByte((byte)'\\');
                        output.WriteByte(b);
                        break;
                    default:
                        output.WriteByte(b);
                        break;
                }
            }

            output.WriteByte((byte)')');
        }
        else
        {
            output.WriteByte((byte)'<');
            output.Write(Encoding.ASCII.GetBytes(Convert.ToHexString(bytes)));
            output.WriteByte((byte)'>');
        }
    }
}
