/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for PDFStreamParser token parsing behavior.
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
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using Xunit;

namespace PdfBox.Net.Tests;

public class PDFStreamParserTest
{
    [Fact]
    public void ParseTokensParsesOperandsAndOperator()
    {
        using MemoryStream input = new(Encoding.Latin1.GetBytes("1 /Type q\n"));

        PDFStreamParser parser = new(input);
        IList<object> tokens = parser.ParseTokens();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(1, Assert.IsType<COSInteger>(tokens[0]).IntValue());
        Assert.Equal(COSName.TYPE, Assert.IsType<COSName>(tokens[1]));
        Assert.Equal(OperatorName.SAVE, Assert.IsType<Operator>(tokens[2]).GetName());
    }

    [Fact]
    public void ParseTokensStaticReturnsList()
    {
        using MemoryStream input = new(Encoding.Latin1.GetBytes("1 /Type q\n"));

        List<object> tokens = PDFStreamParser.ParseTokens(input);

        Assert.Equal(3, tokens.Count);
        tokens.RemoveAt(0);
        Assert.Equal(2, tokens.Count);
    }

    [Fact]
    public void ParseTokensParsesInlineImageOperator()
    {
        using MemoryStream input = new();
        ContentStreamWriter writer = new(input);

        Operator inlineImage = Operator.GetOperator(OperatorName.BEGIN_INLINE_IMAGE);
        COSDictionary parameters = new();
        parameters.SetInt(COSName.GetPDFName("W"), 1);
        inlineImage.SetImageParameters(parameters);
        inlineImage.SetImageData([1, 2, 3]);

        writer.WriteToken(inlineImage);
        input.Position = 0;

        PDFStreamParser parser = new(input);
        IList<object> tokens = parser.ParseTokens();

        Operator parsed = Assert.IsType<Operator>(Assert.Single(tokens));
        Assert.Equal(OperatorName.BEGIN_INLINE_IMAGE, parsed.GetName());
        Assert.Equal(1, parsed.GetImageParameters()!.GetInt(COSName.GetPDFName("W")));
        Assert.Equal(new byte[] { 1, 2, 3 }, parsed.GetImageData());
    }
}
