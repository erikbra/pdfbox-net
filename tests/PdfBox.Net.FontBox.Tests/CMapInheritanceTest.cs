/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/cmap/TestCMapParser.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using PdfBox.Net.FontBox.CMap;
using PdfCMap = PdfBox.Net.FontBox.CMap.CMap;
using PdfBox.Net.FontBox.Tests.TTF.Hinting;

namespace PdfBox.Net.FontBox.Tests;

public class CMapInheritanceTest : HintingTestSupport
{
    [Fact]
    public void TestUseCmapOwnMappingsWin()
    {
        PdfCMap parent = new CMapParser().ParsePredefined("ETen-B5-H");
        Equal(13681, parent.ToCID(0x41, 1), "ETen-B5-H maps 0x41 to the full width form");

        PdfCMap cMap = new CMapParser().ParsePredefined("ETenms-B5-H");
        Equal(34, cMap.ToCID(0x41, 1), "ETenms-B5-H overrides 0x41 to the proportional form");
        Equal(1, cMap.ToCID(0x20, 1), "ETenms-B5-H overrides 0x20 to the proportional form");

        // codes the PdfCMap does not redefine still come from the one it uses
        Equal(parent.ToCID(new byte[] { (byte) 0xA1, 0x40 }),
                cMap.ToCID(new byte[] { (byte) 0xA1, 0x40 }),
                "an inherited code is unaffected");

        // the byte[] overload repeats the lookup order of the int one, so check the override there too
        Equal(13681, parent.ToCID(new byte[] { 0x41 }));
        Equal(34, cMap.ToCID(new byte[] { 0x41 }),
                "the byte[] overload has to prefer the CMap's own mapping as well");

        // UniJIS-UCS2-HW-H likewise overrides its parent's proportional latin with the half width forms
        PdfCMap halfWidth = new CMapParser().ParsePredefined("UniJIS-UCS2-HW-H");
        Equal(34, new CMapParser().ParsePredefined("UniJIS-UCS2-H").ToCID(0x41, 2),
                "UniJIS-UCS2-H maps 0x0041 to the proportional form");
        Equal(264, halfWidth.ToCID(0x41, 2),
                "UniJIS-UCS2-HW-H overrides 0x0041 to the half width form");
    }

    /// <summary>
    /// The override has to survive a chain of usecmap: ETenms-B5-V uses ETenms-B5-H, which in turn
    /// uses ETen-B5-H. A code that only the middle PdfCMap redefines has to keep that redefinition.
    /// </summary>
    [Fact]
    public void TestUseCmapChainKeepsNearestMapping()
    {
        PdfCMap cMap = new CMapParser().ParsePredefined("ETenms-B5-V");

        Equal(1, cMap.WMode, "ETenms-B5-V is vertical");
        Equal(34, cMap.ToCID(0x41, 1),
                "ETenms-B5-V inherits the proportional override from ETenms-B5-H, not ETen-B5-H");
    }

    /// <summary>
    /// Both kinds of mapping a PdfCMap declares have to beat the ranges it inherits. ETenms-B5-V
    /// declares six cidchars and twelve cidranges for the punctuation whose vertical form differs,
    /// on top of the horizontal forms it inherits from ETenms-B5-H and ETen-B5-H.
    ///
    /// The cidchars were already resolved correctly before this was fixed, a cidchar being consulted
    /// ahead of any range either way, so they are here as a guard rather than as a second
    /// reproducer.
    /// </summary>
    [Fact]
    public void TestUseCmapOwnMappingsBeatInheritedRanges()
    {
        PdfCMap horizontal = new CMapParser().ParsePredefined("ETenms-B5-H");
        PdfCMap vertical = new CMapParser().ParsePredefined("ETenms-B5-V");

        // the horizontal forms come from an inherited range in both CMaps
        Equal(110, horizontal.ToCID(0xA14B, 2));
        Equal(111, horizontal.ToCID(0xA14C, 2));
        Equal(121, horizontal.ToCID(0xA156, 2));

        // ETenms-B5-V's own cidchars replace them with the vertical forms
        Equal(13646, vertical.ToCID(0xA14B, 2), "own cidchar has to beat the inherited range");
        Equal(109, vertical.ToCID(0xA14C, 2), "own cidchar has to beat the inherited range");
        Equal(312, vertical.ToCID(0xA156, 2), "own cidchar has to beat the inherited range");

        // and its own cidranges likewise, two usecmap levels down
        Equal(128, horizontal.ToCID(0xA15D, 2));
        Equal(130, vertical.ToCID(0xA15D, 2), "own cidrange has to beat the inherited range");
    }

    /// <summary>
    /// Identity-V is the one predefined PdfCMap that declares no cid mappings at all, it only uses
    /// Identity-H. Every lookup it answers is therefore an inherited one, which also makes it the
    /// case that proves hasCIDMappings has to account for what a PdfCMap inherited.
    /// </summary>
    [Fact]
    public void TestUseCmapOnlyInheritedMappings()
    {
        PdfCMap cMap = new CMapParser().ParsePredefined("Identity-V");

        Equal(1, cMap.WMode, "Identity-V is vertical");
        Assert.True(cMap.HasCIDMappings(), "Identity-V has cid mappings, all of them inherited");

        Equal(65, cMap.ToCID(new byte[] { 0, 65 }), "Identity-V CID 65");
        Equal(12345, cMap.ToCID(new byte[] { 0x30, 0x39 }), "Identity-V CID 12345");
        Equal(0xFFFF, cMap.ToCID(new byte[] { (byte) 0xFF, (byte) 0xFF }),
                "Identity-V CID 0xFFFF");
        Equal(12345, cMap.ToCID(0x3039, 2), "Identity-V CID 12345");
    }

    /// <summary>
    /// A PdfCMap holds on to the CMaps it uses rather than copying their mappings, so it must never
    /// write into one: adding a mapping to the importing PdfCMap must not reach back into the used one.
    /// </summary>
    [Fact]
    public void TestUseCmapDoesNotShareMappingsWithTheUsedCMap()
    {
        PdfCMap used = new PdfCMap();
        used.AddCIDMapping(new byte[] { 0x41 }, 100);
        used.AddCIDRange(new byte[] { 0x50 }, new byte[] { 0x5F }, 200);

        PdfCMap cMap = new PdfCMap();
        cMap.UseCmap(used);
        Equal(100, cMap.ToCID(0x41, 1), "the mapping is inherited");
        Equal(205, cMap.ToCID(0x55, 1), "the range is inherited");

        cMap.AddCIDMapping(new byte[] { 0x41 }, 300);
        cMap.AddCIDRange(new byte[] { 0x50 }, new byte[] { 0x5F }, 400);

        Equal(300, cMap.ToCID(0x41, 1), "the CMap's own mapping wins");
        Equal(405, cMap.ToCID(0x55, 1), "the CMap's own range wins");
        Equal(100, used.ToCID(0x41, 1), "the used PdfCMap must not have been modified");
        Equal(205, used.ToCID(0x55, 1), "the used PdfCMap must not have been modified");
    }

    /// <summary>
    /// Everything a PdfCMap declares outranks everything it inherits, so a cidrange of its own has to
    /// beat an inherited cidchar too, not just an inherited cidrange. No predefined PdfCMap pairs the
    /// two that way round, hence the hand built pair here.
    /// </summary>
    [Fact]
    public void TestUseCmapOwnRangeBeatsInheritedChar()
    {
        PdfCMap used = new PdfCMap();
        used.AddCIDMapping(new byte[] { 0x41 }, 100);

        PdfCMap cMap = new PdfCMap();
        cMap.UseCmap(used);
        cMap.AddCIDRange(new byte[] { 0x40 }, new byte[] { 0x4F }, 200);

        Equal(201, cMap.ToCID(0x41, 1), "the CMap's own range has to beat the inherited char");
        Equal(200, cMap.ToCID(0x40, 1), "a code the used PdfCMap says nothing about");
        Equal(100, used.ToCID(0x41, 1), "the used PdfCMap must not have been modified");
    }

    /// <summary>
    /// A usecmap chain is resolved nearest first: a PdfCMap is asked for its own mappings, and only if
    /// it has none for the code does it pass the question on to the PdfCMap it uses. So a range in the
    /// nearer PdfCMap outranks a cidchar in the one behind it, even though a cidchar outranks a range
    /// within a single CMap.
    /// </summary>
    [Fact]
    public void TestUseCmapNearerCMapWins()
    {
        PdfCMap far = new PdfCMap();
        far.AddCIDMapping(new byte[] { 0x41 }, 100);

        PdfCMap near = new PdfCMap();
        near.UseCmap(far);
        near.AddCIDRange(new byte[] { 0x40 }, new byte[] { 0x4F }, 200);

        PdfCMap cMap = new PdfCMap();
        cMap.UseCmap(near);

        Equal(201, cMap.ToCID(0x41, 1), "the nearer CMap's range has to beat the farther "
                + "CMap's cidchar");
        Equal(100, far.ToCID(0x41, 1), "a used PdfCMap answers for itself unchanged");
    }

    /// <summary>
    /// "PdfCMap files can be nested to five levels", so a redefinition has to survive that depth, and
    /// each level has to be able to redefine what the level below it declared.
    /// </summary>
    [Fact]
    public void TestUseCmapNestedToFiveLevels()
    {
        PdfCMap cMap = new PdfCMap();
        cMap.AddCIDMapping(new byte[] { 0x01 }, 10);
        for (int level = 2; level <= 5; level++)
        {
            PdfCMap nested = new PdfCMap();
            nested.UseCmap(cMap);
            // redefine the code the level below just defined, and add one of its own
            nested.AddCIDMapping(new byte[] { (byte) (level - 1) }, 10 * level);
            nested.AddCIDMapping(new byte[] { (byte) level }, 10 * level);
            cMap = nested;
        }

        Assert.True(cMap.HasCIDMappings());
        // every code but the last was redefined one level up, the last one wasn't
        Equal(20, cMap.ToCID(0x01, 1));
        Equal(30, cMap.ToCID(0x02, 1));
        Equal(40, cMap.ToCID(0x03, 1));
        Equal(50, cMap.ToCID(0x04, 1));
        Equal(50, cMap.ToCID(0x05, 1));
    }

    /// <summary>
    /// A PdfCMap with no cid mappings of its own answers with the ones of the PdfCMap it uses, five levels
    /// down if need be. Identity-V is the predefined case of this, testUseCmapOnlyInheritedMappings
    /// covers that one.
    /// </summary>
    [Fact]
    public void TestUseCmapPassesThroughEmptyLevels()
    {
        PdfCMap cMap = new PdfCMap();
        cMap.AddCIDMapping(new byte[] { 0x41 }, 100);
        for (int level = 2; level <= 5; level++)
        {
            PdfCMap nested = new PdfCMap();
            nested.UseCmap(cMap);
            cMap = nested;
        }

        Assert.True(cMap.HasCIDMappings(), "the mappings are five levels down but they are there");
        Equal(100, cMap.ToCID(0x41, 1));
        Equal(0, cMap.ToCID(0x42, 1), "a code no level in the chain maps");
    }

    /// <summary>
    /// The specification gives a PdfCMap one usecmap, but nothing here has to break if a file carries
    /// more than one. Every used PdfCMap is kept, and they are asked in the order they were declared.
    /// </summary>
    [Fact]
    public void TestUseCmapSeveralUsedCMaps()
    {
        PdfCMap first = new PdfCMap();
        first.AddCIDMapping(new byte[] { 0x41 }, 100);
        first.AddCIDMapping(new byte[] { 0x42 }, 101);

        PdfCMap second = new PdfCMap();
        second.AddCIDMapping(new byte[] { 0x42 }, 200);
        second.AddCIDMapping(new byte[] { 0x43 }, 201);

        PdfCMap cMap = new PdfCMap();
        cMap.UseCmap(first);
        cMap.UseCmap(second);
        cMap.AddCIDMapping(new byte[] { 0x41 }, 300);

        Equal(300, cMap.ToCID(0x41, 1), "the CMap's own mapping beats both");
        Equal(101, cMap.ToCID(0x42, 1), "a code both used CMaps map comes from the first");
        Equal(201, cMap.ToCID(0x43, 1), "a code only the second maps still resolves");
        Equal(0, cMap.ToCID(0x44, 1), "a code none of them maps");
    }

    /// <summary>
    /// CID 0 is the .notdef glyph, and a PdfCMap may map a code to it deliberately. That is a mapping,
    /// not the absence of one, so it has to outrank whatever the PdfCMap it uses says about the code.
    /// </summary>
    [Fact]
    public void TestUseCmapOwnMappingToCidZeroIsNotAFallthrough()
    {
        PdfCMap used = new PdfCMap();
        used.AddCIDRange(new byte[] { 0x00 }, new byte[] { (byte) 0xFF }, 500);

        PdfCMap cMap = new PdfCMap();
        cMap.UseCmap(used);
        cMap.AddCIDRange(new byte[] { 0x41 }, new byte[] { 0x41 }, 0);

        Equal(0, cMap.ToCID(0x41, 1), "the CMap's own .notdef has to win");
        Equal(0, cMap.ToCID(new byte[] { 0x41 }), "the byte[] overload as well");
        Equal(566, cMap.ToCID(0x42, 1), "a code it doesn't redefine still comes from the parent");
        Equal(565, used.ToCID(0x41, 1), "the used PdfCMap answers for itself unchanged");
    }

    /// <summary>
    /// The length guessing overload probes the code lengths shortest first. A code mapped to CID 0 at
    /// the shortest length is mapped, so the probing stops there rather than running on to a longer
    /// length that happens to map the same value to something else.
    /// </summary>
    [Fact]
    public void TestToCidZeroAtShortestLengthStopsTheLengthProbing()
    {
        PdfCMap cMap = new PdfCMap();
        cMap.AddCIDMapping(new byte[] { 0x41 }, 0);
        cMap.AddCIDMapping(new byte[] { 0x00, 0x41 }, 700);

        Equal(0, cMap.ToCID(0x41, 1), "the one byte code maps to .notdef");
        Equal(700, cMap.ToCID(0x41, 2), "the two byte code maps to 700");
        Equal(0, cMap.ToCID(0x41), "the shortest length maps the code, so that is the answer");
    }
}
