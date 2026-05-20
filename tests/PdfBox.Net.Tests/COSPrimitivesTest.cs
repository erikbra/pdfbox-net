/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache PDFBox COS primitives.
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

using System.IO;
using System.Text;
using PdfBox.Net.COS;
using Xunit;

namespace PdfBox.Net.Tests;

public class COSPrimitivesTest
{
    [Fact]
    public void TestCOSObjectKeyBehavior()
    {
        COSObjectKey key = new(12, 3, 7);
        COSObjectKey equal = new(12, 3);
        COSObjectKey other = new(12, 4);

        Assert.Equal(COSObjectKey.ComputeInternalHash(12, 3), key.GetInternalHash());
        Assert.Equal(12, key.GetNumber());
        Assert.Equal(3, key.GetGeneration());
        Assert.Equal(7, key.GetStreamIndex());
        Assert.Equal("12 3 R", key.ToString());
        Assert.Equal(key, equal);
        Assert.NotEqual(key, other);
        Assert.True(key.CompareTo(other) < 0);
    }

    [Fact]
    public void TestCOSObjectKeyRejectsNegativeValues()
    {
        Assert.Throws<ArgumentException>(() => _ = new COSObjectKey(-1, 0));
        Assert.Throws<ArgumentException>(() => _ = new COSObjectKey(0, -1));
    }

    [Fact]
    public void TestCOSBaseDirectAndKey()
    {
        TestCOSBaseItem item = new();
        COSObjectKey key = new(2, 0);

        Assert.Same(item, item.GetCOSObject());
        Assert.False(item.IsDirect());
        item.SetDirect(true);
        Assert.True(item.IsDirect());

        Assert.Null(item.GetKey());
        item.SetKey(key);
        Assert.Same(key, item.GetKey());
    }

    [Fact]
    public void TestCOSBooleanBehavior()
    {
        Assert.Same(COSBoolean.TRUE, COSBoolean.GetBoolean(true));
        Assert.Same(COSBoolean.FALSE, COSBoolean.GetBoolean(false));
        Assert.True(COSBoolean.TRUE.GetValue());
        Assert.False(COSBoolean.FALSE.GetValueAsObject());
        Assert.Equal("true", COSBoolean.TRUE.ToString());
        Assert.Equal("false", COSBoolean.FALSE.ToString());

        using MemoryStream ms = new();
        COSBoolean.TRUE.WritePDF(ms);
        Assert.Equal("true", Encoding.Latin1.GetString(ms.ToArray()));
    }

    [Fact]
    public void TestCOSNullBehavior()
    {
        Assert.Equal("COSNull{}", COSNull.NULL.ToString());

        using MemoryStream ms = new();
        COSNull.NULL.WritePDF(ms);
        Assert.Equal("null", Encoding.Latin1.GetString(ms.ToArray()));
    }

    [Fact]
    public void TestCOSIntegerBehavior()
    {
        COSInteger five = COSInteger.Get(5);
        COSInteger fiveAgain = COSInteger.Get(5);
        COSInteger large = COSInteger.Get(5000);

        Assert.Same(five, fiveAgain);
        Assert.NotSame(large, COSInteger.Get(5000));
        Assert.True(five.IsValid());
        Assert.Equal(5f, five.FloatValue());
        Assert.Equal(5, five.IntValue());
        Assert.Equal(5L, five.LongValue());
        Assert.Equal("COSInt{5}", five.ToString());

        using MemoryStream ms = new();
        five.WritePDF(ms);
        Assert.Equal("5", Encoding.Latin1.GetString(ms.ToArray()));
    }

    [Fact]
    public void TestCOSFloatBehavior()
    {
        COSFloat value = new("1.25");
        COSFloat malformed = new("0.00-35095424");
        COSFloat verySmall = new(float.Epsilon);

        Assert.Equal(1.25f, value.FloatValue());
        Assert.Equal(1, value.IntValue());
        Assert.Equal(1L, value.LongValue());
        Assert.Equal("COSFloat{1.25}", value.ToString());
        Assert.Equal(-0.0035095424f, malformed.FloatValue());
        Assert.Equal(float.Epsilon, verySmall.FloatValue());

        using MemoryStream ms = new();
        value.WritePDF(ms);
        Assert.Equal("1.25", Encoding.Latin1.GetString(ms.ToArray()));
    }

    [Fact]
    public void TestCOSNumberFactoryBehavior()
    {
        Assert.Same(COSInteger.ZERO, COSNumber.Get("-"));
        Assert.IsType<COSInteger>(COSNumber.Get("7"));
        Assert.IsType<COSFloat>(COSNumber.Get("1.0"));

        COSNumber maxOutOfRange = COSNumber.Get("9999999999999999999999999");
        COSNumber minOutOfRange = COSNumber.Get("-9999999999999999999999999");

        Assert.False(((COSInteger)maxOutOfRange).IsValid());
        Assert.False(((COSInteger)minOutOfRange).IsValid());
        Assert.Throws<IOException>(() => COSNumber.Get("abc"));
    }

    [Fact]
    public void TestVisitorDispatch()
    {
        RecordingVisitor visitor = new();

        COSBoolean.TRUE.Accept(visitor);
        new COSFloat(1f).Accept(visitor);
        COSInteger.ONE.Accept(visitor);
        COSNull.NULL.Accept(visitor);

        Assert.Equal(1, visitor.BooleanVisited);
        Assert.Equal(1, visitor.FloatVisited);
        Assert.Equal(1, visitor.IntVisited);
        Assert.Equal(1, visitor.NullVisited);
    }

    private sealed class TestCOSBaseItem : COSBase
    {
        public override void Accept(ICOSVisitor visitor)
        {
            COSNull.NULL.Accept(visitor);
        }
    }

    private sealed class RecordingVisitor : ICOSVisitor
    {
        public void VisitFromArray(COSArray obj)
        {
        }

        public int BooleanVisited { get; private set; }
        public void VisitFromDictionary(COSDictionary obj)
        {
        }

        public int FloatVisited { get; private set; }
        public int IntVisited { get; private set; }
        public void VisitFromName(COSName obj)
        {
        }

        public int NullVisited { get; private set; }
        public void VisitFromObject(COSObject obj)
        {
        }

        public void VisitFromStream(COSStream obj)
        {
        }

        public void VisitFromString(COSString obj)
        {
        }

        public void VisitFromBoolean(COSBoolean obj)
        {
            BooleanVisited++;
        }

        public void VisitFromFloat(COSFloat obj)
        {
            FloatVisited++;
        }

        public void VisitFromInt(COSInteger obj)
        {
            IntVisited++;
        }

        public void VisitFromNull(COSNull obj)
        {
            NullVisited++;
        }
    }
}
