/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/ContentStreamWriter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.PdfWriter;

/// <summary>
/// A class that will take a list of tokens and write out a stream with them.
/// </summary>
public sealed class ContentStreamWriter
{
    private readonly Stream output;

    /// <summary>
    /// Space character.
    /// </summary>
    public static readonly byte[] SPACE = [(byte)' '];

    /// <summary>
    /// Standard line separator.
    /// </summary>
    public static readonly byte[] EOL = [(byte)'\n'];

    /// <summary>
    /// This will create a new content stream writer.
    /// </summary>
    /// <param name="outStream">The stream to write the data to.</param>
    public ContentStreamWriter(Stream outStream)
    {
        output = outStream ?? throw new ArgumentNullException(nameof(outStream));
    }

    /// <summary>
    /// Writes a single operand token.
    /// </summary>
    /// <param name="baseObject">The operand to write to the stream.</param>
    public void WriteToken(COSBase baseObject)
    {
        WriteObject(baseObject);
    }

    /// <summary>
    /// Writes a single operator token.
    /// </summary>
    /// <param name="op">The operator to write to the stream.</param>
    public void WriteToken(Operator op)
    {
        WriteObject(op);
    }

    /// <summary>
    /// Writes a series of tokens followed by a new line.
    /// </summary>
    /// <param name="tokens">The tokens to write to the stream.</param>
    public void WriteTokens(params object[] tokens)
    {
        foreach (object token in tokens)
        {
            WriteObject(token);
        }

        output.Write(EOL);
    }

    /// <summary>
    /// This will write out the list of tokens to the stream.
    /// </summary>
    /// <param name="tokens">The tokens to write to the stream.</param>
    public void WriteTokens(IList<object> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        foreach (object token in tokens)
        {
            WriteObject(token);
        }
    }

    private void WriteObject(object token)
    {
        switch (token)
        {
            case COSBase cosBase:
                WriteObject(cosBase);
                break;
            case Operator op:
                WriteObject(op);
                break;
            default:
                throw new IOException("Error:Unknown type in content stream:" + token);
        }
    }

    private void WriteObject(Operator op)
    {
        if (op.GetName().Equals(OperatorName.BEGIN_INLINE_IMAGE, StringComparison.Ordinal))
        {
            output.Write(Encoding.Latin1.GetBytes(OperatorName.BEGIN_INLINE_IMAGE));
            output.Write(EOL);
            COSDictionary imageParameters = op.GetImageParameters() ?? throw new IOException("Error:Missing inline image parameters");
            foreach (COSName key in imageParameters.KeySet())
            {
                COSBase? value = imageParameters.GetDictionaryObject(key);
                key.WritePDF(output);
                output.Write(SPACE);
                WriteObject(value ?? COSNull.NULL);
                output.Write(EOL);
            }

            output.Write(Encoding.Latin1.GetBytes(OperatorName.BEGIN_INLINE_IMAGE_DATA));
            output.Write(EOL);
            output.Write(op.GetImageData() ?? []);
            output.Write(EOL);
            output.Write(Encoding.Latin1.GetBytes(OperatorName.END_INLINE_IMAGE));
            output.Write(EOL);
        }
        else
        {
            output.Write(Encoding.Latin1.GetBytes(op.GetName()));
            output.Write(EOL);
        }
    }

    private void WriteObject(COSBase value)
    {
        switch (value)
        {
            case COSString cosString:
                COSWriter.WriteString(cosString, output);
                output.Write(SPACE);
                break;
            case COSFloat cosFloat:
                cosFloat.WritePDF(output);
                output.Write(SPACE);
                break;
            case COSInteger cosInteger:
                cosInteger.WritePDF(output);
                output.Write(SPACE);
                break;
            case COSBoolean cosBoolean:
                cosBoolean.WritePDF(output);
                output.Write(SPACE);
                break;
            case COSName cosName:
                cosName.WritePDF(output);
                output.Write(SPACE);
                break;
            case COSArray array:
                output.Write(COSWriter.ARRAY_OPEN);
                for (int i = 0; i < array.Size(); i++)
                {
                    WriteObject(array.Get(i) ?? COSNull.NULL);
                }

                output.Write(COSWriter.ARRAY_CLOSE);
                output.Write(SPACE);
                break;
            case COSDictionary dictionary:
                output.Write(COSWriter.DICT_OPEN);
                foreach (KeyValuePair<COSName, COSBase> entry in dictionary.EntrySet())
                {
                    if (entry.Value is not null)
                    {
                        WriteObject(entry.Key);
                        WriteObject(entry.Value);
                    }
                }

                output.Write(COSWriter.DICT_CLOSE);
                output.Write(SPACE);
                break;
            case COSNull:
                output.Write(Encoding.ASCII.GetBytes("null"));
                output.Write(SPACE);
                break;
            default:
                throw new IOException("Error:Unknown type in content stream:" + value);
        }
    }
}
