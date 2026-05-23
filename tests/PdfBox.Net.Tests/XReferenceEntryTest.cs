/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused xUnit coverage for parser xref entry primitives.
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
using PdfBox.Net.PdfParser.Xref;

namespace PdfBox.Net.Tests;

public class XReferenceEntryTest
{
    [Fact]
    public void FreeEntryHasExpectedColumns()
    {
        FreeXReference entry = new(new COSObjectKey(7, 3), 9);

        Assert.Equal(XReferenceType.FREE, entry.GetXReferenceType());
        Assert.Equal(0, entry.GetFirstColumnValue());
        Assert.Equal(9, entry.GetSecondColumnValue());
        Assert.Equal(3, entry.GetThirdColumnValue());
    }

    [Fact]
    public void NormalEntryDetectsObjectStreamParent()
    {
        COSStream stream = new();
        stream.SetItem(COSName.TYPE, COSName.GetPDFName("ObjStm"));

        NormalXReference entry = new(123, new COSObjectKey(11, 0), stream);

        Assert.True(entry.IsObjectStream());
        Assert.Equal(1, entry.GetFirstColumnValue());
        Assert.Equal(123, entry.GetSecondColumnValue());
        Assert.Equal(0, entry.GetThirdColumnValue());
    }

    [Fact]
    public void ObjectStreamEntryHasExpectedColumns()
    {
        COSInteger obj = COSInteger.ONE;
        ObjectStreamXReference entry = new(5, new COSObjectKey(21, 0), obj, new COSObjectKey(99, 0));

        Assert.Equal(XReferenceType.OBJECT_STREAM_ENTRY, entry.GetXReferenceType());
        Assert.Equal(2, entry.GetFirstColumnValue());
        Assert.Equal(99, entry.GetSecondColumnValue());
        Assert.Equal(5, entry.GetThirdColumnValue());
        Assert.Same(obj, entry.GetObject());
    }

    [Fact]
    public void EntriesSortByReferencedKey()
    {
        XReferenceEntry left = new FreeXReference(new COSObjectKey(5, 0), 0);
        XReferenceEntry right = new FreeXReference(new COSObjectKey(8, 0), 0);

        Assert.True(left.CompareTo(right) < 0);
        Assert.True(right.CompareTo(left) > 0);
    }
}
