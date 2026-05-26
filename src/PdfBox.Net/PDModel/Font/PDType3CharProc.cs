/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType3CharProc.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.PDModel.Font;

public sealed class PDType3CharProc
{
    private readonly PDType3Font _font;
    private readonly COSStream _charStream;

    public PDType3CharProc(PDType3Font font, COSStream charStream)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _charStream = charStream ?? throw new ArgumentNullException(nameof(charStream));
    }

    public COSStream GetCOSObject() => _charStream;

    public PDType3Font GetFont() => _font;

    public PDStream GetContentStream() => new(_charStream);

    public PDResources? GetResources()
    {
        return _charStream.GetCOSDictionary(COSName.RESOURCES) is COSDictionary resourcesDictionary
            ? new PDResources(resourcesDictionary)
            : _font.GetResources();
    }

    public PDRectangle? GetGlyphBBox()
    {
        if (!TryReadLeadingOperator(out Operator? op, out List<COSBase> arguments) ||
            !string.Equals(op.GetName(), "d1", StringComparison.Ordinal) ||
            arguments.Count < 6 ||
            arguments[2] is not COSNumber x ||
            arguments[3] is not COSNumber y ||
            arguments[4] is not COSNumber maxX ||
            arguments[5] is not COSNumber maxY)
        {
            return null;
        }

        float minX = x.FloatValue();
        float minY = y.FloatValue();
        return new PDRectangle(minX, minY, maxX.FloatValue() - minX, maxY.FloatValue() - minY);
    }

    public float GetWidth()
    {
        if (!TryReadLeadingOperator(out Operator? op, out List<COSBase> arguments))
        {
            throw new IOException("Unexpected end of Type 3 charproc stream.");
        }

        string name = op.GetName();
        if (!string.Equals(name, "d0", StringComparison.Ordinal) && !string.Equals(name, "d1", StringComparison.Ordinal))
        {
            throw new IOException("First operator must be d0 or d1.");
        }

        if (arguments.Count == 0 || arguments[0] is not COSNumber width)
        {
            throw new IOException("Type 3 charproc width operator missing numeric width.");
        }

        return width.FloatValue();
    }

    private bool TryReadLeadingOperator(out Operator? op, out List<COSBase> arguments)
    {
        using Stream stream = _charStream.CreateInputStream();
        IList<object> tokens = PDFStreamParser.Parse(stream);
        arguments = new List<COSBase>();
        foreach (object token in tokens)
        {
            if (token is Operator @operator)
            {
                op = @operator;
                return true;
            }

            if (token is COSBase cosBase)
            {
                arguments.Add(cosBase);
            }
        }

        op = null;
        return false;
    }
}
