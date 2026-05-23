/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused xUnit coverage for XrefTrailerResolver behavior.
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

namespace PdfBox.Net.Tests;

public class XrefTrailerResolverTest
{
    [Fact]
    public void ResolverChainsPrevAndMergesXRefAndTrailer()
    {
        XrefTrailerResolver resolver = new();

        resolver.NextXrefObj(100, XrefTrailerResolver.XRefType.TABLE);
        resolver.SetXRef(new COSObjectKey(1, 0), 10);
        resolver.SetTrailer(new COSDictionary());

        COSDictionary latestTrailer = new();
        latestTrailer.SetLong(COSName.GetPDFName("Prev"), 100);
        latestTrailer.SetString(COSName.GetPDFName("Custom"), "latest");

        resolver.NextXrefObj(200, XrefTrailerResolver.XRefType.STREAM);
        resolver.SetXRef(new COSObjectKey(2, 0), 20);
        resolver.SetTrailer(latestTrailer);

        resolver.SetStartxref(200);

        Assert.Equal(XrefTrailerResolver.XRefType.STREAM, resolver.GetXrefType());
        Dictionary<COSObjectKey, long> table = Assert.IsType<Dictionary<COSObjectKey, long>>(resolver.GetXrefTable());
        Assert.Equal(10, table[new COSObjectKey(1, 0)]);
        Assert.Equal(20, table[new COSObjectKey(2, 0)]);

        COSDictionary trailer = Assert.IsType<COSDictionary>(resolver.GetTrailer());
        Assert.Equal(100, trailer.GetLong(COSName.GetPDFName("Prev")));
        Assert.Equal("latest", trailer.GetString(COSName.GetPDFName("Custom")));
    }

    [Fact]
    public void MissingStartxrefFallsBackToSortedMerge()
    {
        XrefTrailerResolver resolver = new();

        resolver.NextXrefObj(50, XrefTrailerResolver.XRefType.TABLE);
        resolver.SetXRef(new COSObjectKey(4, 0), 100);
        COSDictionary first = new();
        first.SetString(COSName.GetPDFName("Stage"), "first");
        resolver.SetTrailer(first);

        resolver.NextXrefObj(70, XrefTrailerResolver.XRefType.TABLE);
        resolver.SetXRef(new COSObjectKey(4, 0), 200);
        COSDictionary second = new();
        second.SetString(COSName.GetPDFName("Stage"), "second");
        resolver.SetTrailer(second);

        resolver.SetStartxref(-1);

        Dictionary<COSObjectKey, long> table = Assert.IsType<Dictionary<COSObjectKey, long>>(resolver.GetXrefTable());
        Assert.Equal(200, table[new COSObjectKey(4, 0)]);
        COSDictionary trailer = Assert.IsType<COSDictionary>(resolver.GetTrailer());
        Assert.Equal("second", trailer.GetString(COSName.GetPDFName("Stage")));
    }

    [Fact]
    public void GetContainedObjectNumbersUsesNegativeObjectStreamReference()
    {
        XrefTrailerResolver resolver = new();
        resolver.NextXrefObj(10, XrefTrailerResolver.XRefType.TABLE);
        resolver.SetXRef(new COSObjectKey(11, 0), -7);
        resolver.SetXRef(new COSObjectKey(12, 0), -7);
        resolver.SetXRef(new COSObjectKey(13, 0), -8);
        resolver.SetTrailer(new COSDictionary());

        resolver.SetStartxref(10);

        HashSet<long> objectNumbers = Assert.IsType<HashSet<long>>(resolver.GetContainedObjectNumbers(7));
        Assert.Equal([11L, 12L], objectNumbers.OrderBy(x => x));
    }
}
