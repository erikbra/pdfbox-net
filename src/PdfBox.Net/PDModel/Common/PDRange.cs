/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDRange.java
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

namespace PdfBox.Net.PDModel.Common;

public partial class PDRange : COSObjectable
{
    private readonly COSArray _rangeArray;
    private readonly int _startingIndex;

    public PDRange()
    {
        _rangeArray = new COSArray();
        _rangeArray.Add(COSFloat.ZERO);
        _rangeArray.Add(COSFloat.ONE);
        _startingIndex = 0;
    }

    public PDRange(COSArray range)
        : this(range, 0)
    {
    }

    public PDRange(COSArray range, int index)
    {
        _rangeArray = range ?? throw new ArgumentNullException(nameof(range));
        _startingIndex = index;
    }

    public COSBase GetCOSObject() => _rangeArray;

    public COSArray GetCOSArray() => _rangeArray;

    public float GetMin()
    {
        return _rangeArray.GetObject(_startingIndex * 2) is COSNumber min ? min.FloatValue() : 0f;
    }

    public void SetMin(float min)
    {
        _rangeArray.Set(_startingIndex * 2, new COSFloat(min));
    }

    public float GetMax()
    {
        return _rangeArray.GetObject((_startingIndex * 2) + 1) is COSNumber max ? max.FloatValue() : 0f;
    }

    public void SetMax(float max)
    {
        _rangeArray.Set((_startingIndex * 2) + 1, new COSFloat(max));
    }

    public override string ToString() => $"PDRange{{{GetMin()}, {GetMax()}}}";
}
