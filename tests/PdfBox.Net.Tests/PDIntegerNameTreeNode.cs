/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/common/PDIntegerNameTreeNode.java
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

internal sealed class PDIntegerNameTreeNode : PDNameTreeNode<COSInteger>
{
    public PDIntegerNameTreeNode()
    {
    }

    public PDIntegerNameTreeNode(COSDictionary dic)
        : base(dic)
    {
    }

    protected override COSInteger ConvertCOSToPD(COSBase? baseValue)
    {
        if (baseValue is not null && baseValue is not COSInteger)
        {
            throw new IOException($"integer expected here, but got {baseValue}");
        }

        return (COSInteger)baseValue!;
    }

    protected override PDNameTreeNode<COSInteger> CreateChildNode(COSDictionary dic) => new PDIntegerNameTreeNode(dic);
}
