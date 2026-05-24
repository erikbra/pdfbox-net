/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Visitor traversal and serialization-focused xUnit coverage for issue #28
 * (COS visitors and serialization interactions).
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
using PdfBox.Net.PdfWriter;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Covers visitor traversal and serialization interactions across COS types (issue #28).
/// </summary>
public class COSVisitorSerializerTest
{
    // -----------------------------------------------------------------------
    // Visitor dispatch - all COS types route through Accept/VisitFromXxx
    // -----------------------------------------------------------------------

    [Fact]
    public void AllCOSPrimitivesDispatchToCorrectVisitorMethod()
    {
        RecordingVisitor v = new();

        COSBoolean.TRUE.Accept(v);
        COSBoolean.FALSE.Accept(v);
        COSInteger.ONE.Accept(v);
        new COSFloat(1.5f).Accept(v);
        COSNull.NULL.Accept(v);
        COSName.TYPE.Accept(v);
        new COSString("hello").Accept(v);

        Assert.Equal(2, v.BooleanCount);
        Assert.Equal(1, v.IntCount);
        Assert.Equal(1, v.FloatCount);
        Assert.Equal(1, v.NullCount);
        Assert.Equal(1, v.NameCount);
        Assert.Equal(1, v.StringCount);
    }

    [Fact]
    public void ContainerCOSTypesDispatchToCorrectVisitorMethod()
    {
        RecordingVisitor v = new();

        COSArray array = new();
        array.Add(COSInteger.ONE);
        array.Accept(v);

        COSDictionary dict = new();
        dict.SetItem(COSName.TYPE, COSName.GetPDFName("Test"));
        dict.Accept(v);

        new COSStream().Accept(v);
        new COSObject(COSInteger.ONE).Accept(v);

        Assert.Equal(1, v.ArrayCount);
        Assert.Equal(1, v.DictionaryCount);
        Assert.Equal(1, v.StreamCount);
        Assert.Equal(1, v.ObjectCount);
    }

    [Fact]
    public void MixedCOSObjectGraphTraversalReachesAllNodes()
    {
        RecordingVisitor v = new();

        COSArray graph = new();
        graph.Add(COSBoolean.TRUE);
        graph.Add(COSInteger.Get(42));
        graph.Add(new COSFloat(3.14f));
        graph.Add(COSNull.NULL);
        graph.Add(COSName.GetPDFName("Key"));
        graph.Add(new COSString("value"));

        // Visit each element individually (visitor operates on individual nodes)
        foreach (COSBase? item in graph)
        {
            item?.Accept(v);
        }

        Assert.Equal(1, v.BooleanCount);
        Assert.Equal(1, v.IntCount);
        Assert.Equal(1, v.FloatCount);
        Assert.Equal(1, v.NullCount);
        Assert.Equal(1, v.NameCount);
        Assert.Equal(1, v.StringCount);
    }

    [Fact]
    public void COSObjectVisitorDereferencesWrappedPrimitive()
    {
        RecordingVisitor v = new();

        COSObject wrappedInt = new(COSInteger.Get(5));
        COSObject wrappedNull = new((COSBase?)null);
        COSObject wrappedName = new(COSName.TYPE);

        wrappedInt.Accept(v);
        wrappedNull.Accept(v);
        wrappedName.Accept(v);

        Assert.Equal(3, v.ObjectCount);
    }

    // -----------------------------------------------------------------------
    // Serialization via visitor dispatch (COSWriter implements ICOSVisitor)
    // -----------------------------------------------------------------------

    [Fact]
    public void SerializeCOSBooleanViaVisitorDispatch()
    {
        Assert.Equal("true", COSWriter.SerializeToString(COSBoolean.TRUE));
        Assert.Equal("false", COSWriter.SerializeToString(COSBoolean.FALSE));
    }

    [Fact]
    public void SerializeCOSIntegerViaVisitorDispatch()
    {
        Assert.Equal("0", COSWriter.SerializeToString(COSInteger.ZERO));
        Assert.Equal("1", COSWriter.SerializeToString(COSInteger.ONE));
        Assert.Equal("-1", COSWriter.SerializeToString(COSInteger.Get(-1)));
        Assert.Equal("255", COSWriter.SerializeToString(COSInteger.Get(255)));
    }

    [Fact]
    public void SerializeCOSFloatViaVisitorDispatch()
    {
        string result = COSWriter.SerializeToString(new COSFloat(1.5f));
        Assert.Equal("1.5", result);
    }

    [Fact]
    public void SerializeCOSNullViaVisitorDispatch()
    {
        Assert.Equal("null", COSWriter.SerializeToString(COSNull.NULL));
        // null reference also serializes as "null"
        Assert.Equal("null", COSWriter.SerializeToString(null));
    }

    [Fact]
    public void SerializeCOSNameViaVisitorDispatch()
    {
        Assert.Equal("/Type", COSWriter.SerializeToString(COSName.TYPE));
        Assert.Equal("/A#20B", COSWriter.SerializeToString(COSName.GetPDFName("A B")));
    }

    [Fact]
    public void SerializeCOSStringViaVisitorDispatch()
    {
        string result = COSWriter.SerializeToString(new COSString("Hello"));
        Assert.Equal("(Hello)", result);
    }

    [Fact]
    public void SerializeCOSStringForceHexViaVisitorDispatch()
    {
        string result = COSWriter.SerializeToString(new COSString([0x48, 0x69], forceHex: true));
        Assert.StartsWith("<", result);
        Assert.EndsWith(">", result);
    }

    [Fact]
    public void SerializeCOSArrayViaVisitorDispatch()
    {
        COSArray array = new();
        array.Add(COSInteger.ONE);
        array.Add(COSBoolean.TRUE);
        array.Add(COSName.GetPDFName("X"));

        string result = COSWriter.SerializeToString(array);
        Assert.Equal("[1 true /X]", result);
    }

    [Fact]
    public void SerializeEmptyCOSArrayViaVisitorDispatch()
    {
        Assert.Equal("[]", COSWriter.SerializeToString(new COSArray()));
    }

    [Fact]
    public void SerializeCOSDictionaryViaVisitorDispatch()
    {
        COSDictionary dict = new();
        dict.SetItem(COSName.TYPE, COSName.GetPDFName("Test"));

        string result = COSWriter.SerializeToString(dict);
        Assert.StartsWith("<<", result);
        Assert.EndsWith(">>", result);
        Assert.Contains("/Type /Test", result);
    }

    [Fact]
    public void SerializeCOSObjectDereferencesInnerValue()
    {
        COSObject wrapped = new(COSInteger.Get(42));
        Assert.Equal("42", COSWriter.SerializeToString(wrapped));
    }

    [Fact]
    public void SerializeCOSObjectWithNullInnerWritesNull()
    {
        COSObject empty = new((COSBase?)null);
        Assert.Equal("null", COSWriter.SerializeToString(empty));
    }

    [Fact]
    public void SerializeCOSStreamWritesDictionaryPortion()
    {
        COSStream stream = new();
        string result = COSWriter.SerializeToString(stream);
        Assert.StartsWith("<<", result);
        Assert.EndsWith(">>", result);
    }

    // -----------------------------------------------------------------------
    // Deterministic serialization on representative fixtures
    // -----------------------------------------------------------------------

    [Fact]
    public void SerializationIsDeterministicForSameObject()
    {
        COSDictionary dict = new();
        dict.SetItem(COSName.TYPE, COSName.GetPDFName("Catalog"));
        dict.SetInt(COSName.GetPDFName("Count"), 5);
        dict.SetBoolean(COSName.GetPDFName("Flag"), false);

        string first = COSWriter.SerializeToString(dict);
        string second = COSWriter.SerializeToString(dict);
        Assert.Equal(first, second);
    }

    [Fact]
    public void SerializationIsStableForNestedContainers()
    {
        COSArray inner = new();
        inner.Add(COSInteger.ZERO);
        inner.Add(COSInteger.ONE);

        COSDictionary dict = new();
        dict.SetItem(COSName.GetPDFName("Range"), inner);
        dict.SetItem(COSName.TYPE, COSName.GetPDFName("Example"));

        string serialized = COSWriter.SerializeToString(dict);
        Assert.Contains("[0 1]", serialized);
        Assert.Contains("/Type /Example", serialized);
    }

    [Fact]
    public void SerializationRoundtripPreservesAllPrimitiveTypes()
    {
        COSArray array = new();
        array.Add(COSBoolean.TRUE);
        array.Add(COSInteger.Get(-7));
        array.Add(new COSFloat(0.5f));
        array.Add(COSNull.NULL);
        array.Add(COSName.GetPDFName("Hello"));
        array.Add(new COSString("world"));

        string serialized = COSWriter.SerializeToString(array);

        Assert.Contains("true", serialized);
        Assert.Contains("-7", serialized);
        Assert.Contains("0.5", serialized);
        Assert.Contains("null", serialized);
        Assert.Contains("/Hello", serialized);
        Assert.Contains("(world)", serialized);
    }

    [Fact]
    public void SerializeWriterDispatchesViaSamePathAsCOSWriterSerialize()
    {
        COSString value = new("test");

        // Both code paths must produce the same bytes.
        byte[] viaSerialize = COSWriter.Serialize(value);

        using MemoryStream ms = new();
        COSWriter writer = new(ms);
        writer.Write(value);
        ms.Flush();
        byte[] viaDirect = ms.ToArray();

        Assert.Equal(viaSerialize, viaDirect);
    }

    // -----------------------------------------------------------------------
    // Recording visitor helper
    // -----------------------------------------------------------------------

    private sealed class RecordingVisitor : ICOSVisitor
    {
        public int BooleanCount { get; private set; }
        public int IntCount { get; private set; }
        public int FloatCount { get; private set; }
        public int NullCount { get; private set; }
        public int NameCount { get; private set; }
        public int StringCount { get; private set; }
        public int ArrayCount { get; private set; }
        public int DictionaryCount { get; private set; }
        public int StreamCount { get; private set; }
        public int ObjectCount { get; private set; }

        public void VisitFromBoolean(COSBoolean obj) => BooleanCount++;
        public void VisitFromInt(COSInteger obj) => IntCount++;
        public void VisitFromFloat(COSFloat obj) => FloatCount++;
        public void VisitFromNull(COSNull obj) => NullCount++;
        public void VisitFromName(COSName obj) => NameCount++;
        public void VisitFromString(COSString obj) => StringCount++;
        public void VisitFromArray(COSArray obj) => ArrayCount++;
        public void VisitFromDictionary(COSDictionary obj) => DictionaryCount++;
        public void VisitFromStream(COSStream obj) => StreamCount++;
        public void VisitFromObject(COSObject obj) => ObjectCount++;
    }
}
