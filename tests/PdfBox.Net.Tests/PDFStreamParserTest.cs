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

using System.Runtime.ExceptionServices;
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
    public void ParseTokensClassifiesOperatorsWithoutNumberParsingExceptions()
    {
        using MemoryStream input = new(Encoding.Latin1.GetBytes("q Q cm BT Tf Tj TJ ET Do re f* W* BDC EMC\n"));
        int parsingThreadId = Environment.CurrentManagedThreadId;
        int parsingExceptions = 0;

        void RecordParsingExceptions(object? sender, FirstChanceExceptionEventArgs eventArgs)
        {
            if (Environment.CurrentManagedThreadId == parsingThreadId &&
                eventArgs.Exception is IOException or FormatException)
            {
                parsingExceptions++;
            }
        }

        AppDomain.CurrentDomain.FirstChanceException += RecordParsingExceptions;
        List<object> tokens;
        try
        {
            tokens = PDFStreamParser.ParseTokens(input);
        }
        finally
        {
            AppDomain.CurrentDomain.FirstChanceException -= RecordParsingExceptions;
        }

        Assert.Equal(14, tokens.Count);
        Assert.All(tokens, token => Assert.IsType<Operator>(token));
        Assert.Equal(0, parsingExceptions);
    }

    [Fact]
    public void ParseTokensRetainsNumberBehavior()
    {
        using MemoryStream input = new(Encoding.Latin1.GetBytes("0 42 +7 -8 .5 1.25 -.75 3.\n"));

        List<object> tokens = PDFStreamParser.ParseTokens(input);

        Assert.Equal(8, tokens.Count);
        Assert.Equal(0, Assert.IsType<COSInteger>(tokens[0]).IntValue());
        Assert.Equal(42, Assert.IsType<COSInteger>(tokens[1]).IntValue());
        Assert.Equal(7, Assert.IsType<COSInteger>(tokens[2]).IntValue());
        Assert.Equal(-8, Assert.IsType<COSInteger>(tokens[3]).IntValue());
        Assert.Equal(0.5f, Assert.IsType<COSFloat>(tokens[4]).FloatValue());
        Assert.Equal(1.25f, Assert.IsType<COSFloat>(tokens[5]).FloatValue());
        Assert.Equal(-0.75f, Assert.IsType<COSFloat>(tokens[6]).FloatValue());
        Assert.Equal(3f, Assert.IsType<COSFloat>(tokens[7]).FloatValue());
    }

    [Fact]
    public void ParseTokensRetainsNumericLookingMalformedTokenBehavior()
    {
        using MemoryStream input = new(Encoding.Latin1.GetBytes("- . + --.5 1x +x -x .x 1e\n"));

        List<object> tokens = PDFStreamParser.ParseTokens(input);

        Assert.Equal(9, tokens.Count);
        Assert.Same(COSInteger.ZERO, tokens[0]);
        Assert.Same(COSInteger.ZERO, tokens[1]);
        Assert.Equal("+", Assert.IsType<Operator>(tokens[2]).GetName());
        Assert.Equal(-0.5f, Assert.IsType<COSFloat>(tokens[3]).FloatValue());
        Assert.Collection(tokens.Skip(4),
            token => Assert.Equal("1x", Assert.IsType<Operator>(token).GetName()),
            token => Assert.Equal("+x", Assert.IsType<Operator>(token).GetName()),
            token => Assert.Equal("-x", Assert.IsType<Operator>(token).GetName()),
            token => Assert.Equal(".x", Assert.IsType<Operator>(token).GetName()),
            token => Assert.Equal("1e", Assert.IsType<Operator>(token).GetName()));
    }

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
    public void JavaCompatibilityMethodsDelegateToTokenParser()
    {
        using MemoryStream input = new(Encoding.Latin1.GetBytes("1 q\n"));

        PDFStreamParser parser = new(input);

        Assert.Equal(1, Assert.IsType<COSInteger>(parser.ParseNextToken()).IntValue());
        Assert.Equal(OperatorName.SAVE, Assert.IsType<Operator>(parser.ParseNextToken()).GetName());
        Assert.Null(parser.ParseNextToken());
    }

    [Fact]
    public void ParseInstanceMethodReturnsAllTokens()
    {
        using MemoryStream input = new(Encoding.Latin1.GetBytes("1 /Type q\n"));

        PDFStreamParser parser = new(input);
        List<object> tokens = parser.Parse();

        Assert.Equal(3, tokens.Count);
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
