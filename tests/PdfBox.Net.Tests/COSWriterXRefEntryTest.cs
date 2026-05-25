/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused xUnit coverage for the C# port of Apache PDFBox COSWriterXRefEntry behavior.
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
using PdfBox.Net.PdfWriter;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for <see cref="COSWriterXRefEntry"/>.
/// </summary>
public class COSWriterXRefEntryTest
{
    [Fact]
    public void ConstructorStoresValues()
    {
        COSInteger obj = COSInteger.ONE;
        COSObjectKey key = new COSObjectKey(5, 0);
        COSWriterXRefEntry entry = new COSWriterXRefEntry(12345L, obj, key);

        Assert.Equal(12345L, entry.Offset);
        Assert.Same(obj, entry.Object);
        Assert.Same(key, entry.Key);
        Assert.False(entry.IsFree);
    }

    [Fact]
    public void NullEntryIsObjectNumber0Gen65535AndFree()
    {
        COSWriterXRefEntry nullEntry = COSWriterXRefEntry.GetNullEntry();

        Assert.Equal(0L, nullEntry.Offset);
        Assert.Equal(0L, nullEntry.Key.GetNumber());
        Assert.Equal(65535, nullEntry.Key.GetGeneration());
        Assert.True(nullEntry.IsFree);
        Assert.Null(nullEntry.Object);
    }

    [Fact]
    public void GetNullEntryReturnsSameInstance()
    {
        Assert.Same(COSWriterXRefEntry.GetNullEntry(), COSWriterXRefEntry.GetNullEntry());
    }

    [Fact]
    public void IsFreeCanBeSetOnRegularEntry()
    {
        COSWriterXRefEntry entry = new COSWriterXRefEntry(0L, null, new COSObjectKey(1, 0));
        Assert.False(entry.IsFree);
        entry.IsFree = true;
        Assert.True(entry.IsFree);
    }

    [Fact]
    public void OffsetCanBeUpdated()
    {
        COSWriterXRefEntry entry = new COSWriterXRefEntry(100L, null, new COSObjectKey(1, 0));
        entry.Offset = 9999L;
        Assert.Equal(9999L, entry.Offset);
    }

    [Fact]
    public void CompareToSortsLowerObjectNumberFirst()
    {
        COSWriterXRefEntry low = new COSWriterXRefEntry(0L, null, new COSObjectKey(1, 0));
        COSWriterXRefEntry high = new COSWriterXRefEntry(0L, null, new COSObjectKey(5, 0));

        Assert.True(low.CompareTo(high) < 0);
        Assert.True(high.CompareTo(low) > 0);
    }

    [Fact]
    public void CompareToEqualObjectNumbers()
    {
        COSWriterXRefEntry a = new COSWriterXRefEntry(10L, null, new COSObjectKey(3, 0));
        COSWriterXRefEntry b = new COSWriterXRefEntry(20L, null, new COSObjectKey(3, 0));

        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareToNullReturnsNegativeOne()
    {
        COSWriterXRefEntry entry = new COSWriterXRefEntry(0L, null, new COSObjectKey(1, 0));
        Assert.Equal(-1, entry.CompareTo(null));
    }

    [Fact]
    public void EntriesCanBeSortedWithLinq()
    {
        var entries = new List<COSWriterXRefEntry>
        {
            new COSWriterXRefEntry(0L, null, new COSObjectKey(10, 0)),
            new COSWriterXRefEntry(0L, null, new COSObjectKey(2, 0)),
            new COSWriterXRefEntry(0L, null, new COSObjectKey(7, 0)),
        };

        entries.Sort();

        Assert.Equal(2L, entries[0].Key.GetNumber());
        Assert.Equal(7L, entries[1].Key.GetNumber());
        Assert.Equal(10L, entries[2].Key.GetNumber());
    }
}
