/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/common/TestPDNameTreeNode.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.Tests;

public class TestPDNameTreeNode
{
    private PDNameTreeNode<COSInteger> _node1 = null!;
    private PDNameTreeNode<COSInteger> _node2 = null!;
    private PDNameTreeNode<COSInteger> _node4 = null!;
    private PDNameTreeNode<COSInteger> _node5 = null!;
    private PDNameTreeNode<COSInteger> _node24 = null!;

    public TestPDNameTreeNode()
    {
        SetUp();
    }

    private void SetUp()
    {
        _node5 = new PDIntegerNameTreeNode();
        SortedDictionary<string, COSInteger> names = new(StringComparer.Ordinal)
        {
            ["Actinium"] = COSInteger.Get(89),
            ["Aluminum"] = COSInteger.Get(13),
            ["Americium"] = COSInteger.Get(95),
            ["Antimony"] = COSInteger.Get(51),
            ["Argon"] = COSInteger.Get(18),
            ["Arsenic"] = COSInteger.Get(33),
            ["Astatine"] = COSInteger.Get(85)
        };
        _node5.SetNames(names);

        _node24 = new PDIntegerNameTreeNode();
        names = new SortedDictionary<string, COSInteger>(StringComparer.Ordinal)
        {
            ["Xenon"] = COSInteger.Get(54),
            ["Ytterbium"] = COSInteger.Get(70),
            ["Yttrium"] = COSInteger.Get(39),
            ["Zinc"] = COSInteger.Get(30),
            ["Zirconium"] = COSInteger.Get(40)
        };
        _node24.SetNames(names);

        _node2 = new PDIntegerNameTreeNode();
        IList<PDNameTreeNode<COSInteger>> kids = _node2.GetKids() ?? new COSArrayList<PDNameTreeNode<COSInteger>>();
        kids.Add(_node5);
        _node2.SetKids(kids);

        _node4 = new PDIntegerNameTreeNode();
        kids = _node4.GetKids() ?? new COSArrayList<PDNameTreeNode<COSInteger>>();
        kids.Add(_node24);
        _node4.SetKids(kids);

        _node1 = new PDIntegerNameTreeNode();
        kids = _node1.GetKids() ?? new COSArrayList<PDNameTreeNode<COSInteger>>();
        kids.Add(_node2);
        kids.Add(_node4);
        _node1.SetKids(kids);
    }

    [Fact]
    public void UpperLimit()
    {
        Assert.Equal("Astatine", _node5.GetUpperLimit());
        Assert.Equal("Astatine", _node2.GetUpperLimit());
        Assert.Equal("Zirconium", _node24.GetUpperLimit());
        Assert.Equal("Zirconium", _node4.GetUpperLimit());
        Assert.Null(_node1.GetUpperLimit());
    }

    [Fact]
    public void LowerLimit()
    {
        Assert.Equal("Actinium", _node5.GetLowerLimit());
        Assert.Equal("Actinium", _node2.GetLowerLimit());
        Assert.Equal("Xenon", _node24.GetLowerLimit());
        Assert.Equal("Xenon", _node4.GetLowerLimit());
        Assert.Null(_node1.GetLowerLimit());
    }

    [Fact]
    public void ValueLookup()
    {
        Assert.Equal(COSInteger.Get(18), _node1.GetValue("Argon"));
        Assert.Null(_node1.GetValue("Unknown"));
    }
}
