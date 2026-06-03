/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for ContentStreamWriter and Operator token writing behavior.
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
using PdfBox.Net.PdfWriter;
using Xunit;

namespace PdfBox.Net.Tests;

public class ContentStreamWriterTest
{
    [Fact]
    public void WriteTokensListWritesOperandsAndOperator()
    {
        using MemoryStream output = new();
        ContentStreamWriter writer = new(output);

        writer.WriteTokens(new List<object> { COSInteger.ONE, COSName.TYPE, Operator.GetOperator(OperatorName.SAVE) });

        Assert.Equal("1 /Type q\n", Encoding.Latin1.GetString(output.ToArray()));
    }

    [Fact]
    public void WriteTokenInlineImageOperatorWritesImageSection()
    {
        using MemoryStream output = new();
        ContentStreamWriter writer = new(output);

        Operator op = Operator.GetOperator(OperatorName.BEGIN_INLINE_IMAGE);
        COSDictionary parameters = new();
        parameters.SetInt(COSName.GetPDFName("W"), 1);
        op.SetImageParameters(parameters);
        op.SetImageData([1, 2, 3]);

        writer.WriteToken(op);

        byte[] expected =
        [
            (byte)'B', (byte)'I', (byte)'\n',
            (byte)'/', (byte)'W', (byte)' ', (byte)'1', (byte)' ', (byte)'\n',
            (byte)'I', (byte)'D', (byte)'\n',
            1, 2, 3, (byte)'\n',
            (byte)'E', (byte)'I', (byte)'\n'
        ];
        Assert.Equal(expected, output.ToArray());
    }

    [Fact]
    public void WriteTokensToByteArrayWritesTokenSequence()
    {
        byte[] bytes = ContentStreamWriter.WriteTokensToByteArray(
        [
            COSInteger.ONE,
            COSName.TYPE,
            Operator.GetOperator(OperatorName.SAVE)
        ]);

        Assert.Equal("1 /Type q\n", Encoding.Latin1.GetString(bytes));
    }

    [Fact]
    public void CosWriterWriteStringUsesHexForEolBytes()
    {
        using MemoryStream output = new();
        COSWriter.WriteString([(byte)'A', (byte)'\n', (byte)'B'], output);
        Assert.Equal("<410A42>", Encoding.ASCII.GetString(output.ToArray()));
    }
}
