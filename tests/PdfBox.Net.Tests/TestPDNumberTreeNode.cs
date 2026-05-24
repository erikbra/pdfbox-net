/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/common/TestPDNumberTreeNode.java
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

public class TestPDNumberTreeNode
{
    private sealed class PDTest : COSObjectable
    {
        private readonly int _value;

        public PDTest(int value) => _value = value;
        public PDTest(COSInteger cosInt) => _value = cosInt.IntValue();
        public COSInteger GetCOSObject() => COSInteger.Get(_value);
        COSBase COSObjectable.GetCOSObject() => GetCOSObject();
        public override bool Equals(object? obj) => obj is PDTest other && _value == other._value;
        public override int GetHashCode() => _value;
    }

    private readonly PDNumberTreeNode _node1;
    private readonly PDNumberTreeNode _node5;
    private readonly PDNumberTreeNode _node24;
    private readonly PDNumberTreeNode _node2;
    private readonly PDNumberTreeNode _node4;

    public TestPDNumberTreeNode()
    {
        _node5 = new PDNumberTreeNode(typeof(PDTest));
        _node5.SetNumbers(new SortedDictionary<int, COSObjectable?>
        {
            [1] = new PDTest(89), [2] = new PDTest(13), [3] = new PDTest(95), [4] = new PDTest(51), [5] = new PDTest(18), [6] = new PDTest(33), [7] = new PDTest(85)
        });

        _node24 = new PDNumberTreeNode(typeof(PDTest));
        _node24.SetNumbers(new SortedDictionary<int, COSObjectable?>
        {
            [8] = new PDTest(54), [9] = new PDTest(70), [10] = new PDTest(39), [11] = new PDTest(30), [12] = new PDTest(40)
        });

        _node2 = new PDNumberTreeNode(typeof(PDTest));
        IList<PDNumberTreeNode> kids = _node2.GetKids() ?? new COSArrayList<PDNumberTreeNode>();
        kids.Add(_node5);
        _node2.SetKids(kids);

        _node4 = new PDNumberTreeNode(typeof(PDTest));
        kids = _node4.GetKids() ?? new COSArrayList<PDNumberTreeNode>();
        kids.Add(_node24);
        _node4.SetKids(kids);

        _node1 = new PDNumberTreeNode(typeof(PDTest));
        kids = _node1.GetKids() ?? new COSArrayList<PDNumberTreeNode>();
        kids.Add(_node2);
        kids.Add(_node4);
        _node1.SetKids(kids);
    }

    [Fact]
    public void ValueLookup()
    {
        Assert.Equal(new PDTest(51), _node5.GetValue(4));
        Assert.Equal(new PDTest(70), _node1.GetValue(9));
        _node1.SetKids(null);
        _node1.SetNumbers(null);
        Assert.Null(_node1.GetValue(0));
    }

    [Fact]
    public void Limits()
    {
        Assert.Equal(7, _node5.GetUpperLimit());
        Assert.Equal(1, _node5.GetLowerLimit());
        Assert.Equal(12, _node1.GetUpperLimit());
        Assert.Equal(1, _node1.GetLowerLimit());
    }
}
