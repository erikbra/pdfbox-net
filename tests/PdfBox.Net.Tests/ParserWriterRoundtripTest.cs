/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused parser/writer roundtrip tests for chunk-2 low-level bridge scope.
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

using PdfBox.Net.COS;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using Xunit;

namespace PdfBox.Net.Tests;

public class ParserWriterRoundtripTest
{
    [Fact]
    public void ParseSerializeRoundtripIsDeterministicForSmallFixture()
    {
        const string fixture = "<< /Type /Example /Count 3 /Enabled true /Title (Hi\\)There) /Names [ /A /B /C ] /Hex <4869> /Nothing null >>";

        COSBase parsed = COSParser.Parse(fixture);
        string serialized = COSWriter.SerializeToString(parsed);
        Assert.Equal("<< /Type /Example /Count 3 /Enabled true /Title (Hi\\)There) /Names [/A /B /C] /Hex (Hi) /Nothing null >>", serialized);

        COSBase reparsed = COSParser.Parse(serialized);
        string reserialized = COSWriter.SerializeToString(reparsed);
        Assert.Equal(serialized, reserialized);
    }

    [Fact]
    public void ParserHandlesCommentsAndWhitespaceWithoutChangingObjectMeaning()
    {
        const string fixtureWithComments = @"
            % leading comment
            <<
                /Type /Example
                % dictionary comment
                /Count 3
                /Enabled true
                /Label (ok)
            >>
            % trailing comment
        ";

        COSDictionary parsed = Assert.IsType<COSDictionary>(COSParser.Parse(fixtureWithComments));
        Assert.Equal("Example", parsed.GetNameAsString(COSName.TYPE));
        Assert.Equal(3, parsed.GetInt(COSName.GetPDFName("Count")));
        Assert.True(parsed.GetBoolean(COSName.GetPDFName("Enabled"), false));
        Assert.Equal("ok", parsed.GetString(COSName.GetPDFName("Label")));
    }

    [Fact]
    public void WriterDoesNotCloseProvidedOutputStream()
    {
        var nonClosableStream = new NonClosableMemoryStream();
        var writer = new COSWriter(nonClosableStream);
        writer.Write(new COSString("abc"));

        Assert.False(nonClosableStream.CloseAttempted);
        Assert.True(nonClosableStream.Length > 0);
    }

    private sealed class NonClosableMemoryStream : MemoryStream
    {
        public bool CloseAttempted { get; private set; }

        public override void Close()
        {
            CloseAttempted = true;
            throw new IOException("Stream was closed");
        }
    }
}
