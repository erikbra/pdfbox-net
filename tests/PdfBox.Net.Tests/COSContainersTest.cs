/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for COS containers/value types and stream-adjacent behavior.
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
using Xunit;

namespace PdfBox.Net.Tests;

public class COSContainersTest
{
    [Fact]
    public void TestCOSNameAndCOSStringBasics()
    {
        COSName escaped = COSName.GetPDFName("A B");
        COSString unicode = new("héllo 🌍");

        using MemoryStream nameOutput = new();
        escaped.WritePDF(nameOutput);
        Assert.Equal("/A#20B", Encoding.Latin1.GetString(nameOutput.ToArray()));

        using MemoryStream stringOutput = new();
        unicode.WritePDF(stringOutput);
        Assert.StartsWith("(", Encoding.Latin1.GetString(stringOutput.ToArray()));

        COSString parsed = COSString.ParseHex("48656c6c6f");
        Assert.Equal("Hello", parsed.GetASCII());
        Assert.Equal("48656C6C6F", parsed.ToHexString());
    }

    [Fact]
    public void TestCOSArrayAndDictionaryAccessPatterns()
    {
        COSArray array = new();
        array.Add(COSInteger.ONE);
        array.Add(new COSString("two"));
        array.Add(COSName.GetPDFName("Three"));

        Assert.Equal(3, array.Size());
        Assert.Equal(1, array.GetInt(0));
        Assert.Equal("two", array.GetString(1));
        Assert.Equal("Three", array.GetName(2));

        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.TYPE, COSName.GetPDFName("Example"));
        dictionary.SetItem(COSName.GetPDFName("Values"), array);
        dictionary.SetBoolean(COSName.GetPDFName("Flag"), true);
        dictionary.SetLong(COSName.GetPDFName("Long"), 1234L);

        Assert.True(dictionary.ContainsKey(COSName.TYPE));
        Assert.Equal("Example", dictionary.GetNameAsString(COSName.TYPE));
        Assert.True(dictionary.GetBoolean(COSName.GetPDFName("Flag"), false));
        Assert.Equal(1234L, dictionary.GetLong(COSName.GetPDFName("Long")));
        Assert.Same(array, dictionary.GetCOSArray(COSName.GetPDFName("Values")));
        Assert.Same(COSName.TYPE, dictionary.GetKeyForValue(COSName.GetPDFName("Example")));

        using MemoryStream dictOutput = new();
        dictionary.WritePDF(dictOutput);
        string serialized = Encoding.Latin1.GetString(dictOutput.ToArray());
        Assert.Contains("<<", serialized);
        Assert.Contains("/Type /Example", serialized);
        Assert.Contains("/Flag true", serialized);
    }

    [Fact]
    public void TestCOSStreamRoundTripAndLength()
    {
        COSStream stream = new();
        using (Stream output = stream.CreateOutputStream())
        {
            byte[] payload = Encoding.UTF8.GetBytes("stream-data");
            output.Write(payload, 0, payload.Length);
        }

        Assert.Equal(11L, stream.GetLength());
        Assert.True(stream.HasData());

        using Stream input = stream.CreateInputStream();
        using MemoryStream capture = new();
        input.CopyTo(capture);
        Assert.Equal("stream-data", Encoding.UTF8.GetString(capture.ToArray()));
        Assert.Equal("stream-data", stream.ToTextString());

        stream.Close();
        Assert.Throws<IOException>(() => stream.CreateInputStream());
    }

    [Fact]
    public void TestExpandedVisitorCoverage()
    {
        RecordingVisitor visitor = new();

        new COSArray().Accept(visitor);
        new COSDictionary().Accept(visitor);
        COSName.TYPE.Accept(visitor);
        new COSObject(COSInteger.ONE).Accept(visitor);
        new COSStream().Accept(visitor);
        new COSString("visitor").Accept(visitor);

        Assert.Equal(1, visitor.ArrayVisited);
        Assert.Equal(1, visitor.DictionaryVisited);
        Assert.Equal(1, visitor.NameVisited);
        Assert.Equal(1, visitor.ObjectVisited);
        Assert.Equal(1, visitor.StreamVisited);
        Assert.Equal(1, visitor.StringVisited);
    }

    private sealed class RecordingVisitor : ICOSVisitor
    {
        public int ArrayVisited { get; private set; }
        public int DictionaryVisited { get; private set; }
        public int NameVisited { get; private set; }
        public int ObjectVisited { get; private set; }
        public int StreamVisited { get; private set; }
        public int StringVisited { get; private set; }

        public void VisitFromArray(COSArray obj) => ArrayVisited++;
        public void VisitFromBoolean(COSBoolean obj) { }
        public void VisitFromDictionary(COSDictionary obj) => DictionaryVisited++;
        public void VisitFromFloat(COSFloat obj) { }
        public void VisitFromInt(COSInteger obj) { }
        public void VisitFromName(COSName obj) => NameVisited++;
        public void VisitFromNull(COSNull obj) { }
        public void VisitFromObject(COSObject obj) => ObjectVisited++;
        public void VisitFromStream(COSStream obj) => StreamVisited++;
        public void VisitFromString(COSString obj) => StringVisited++;
    }
}
