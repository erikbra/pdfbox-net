/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDJavascriptNameTreeNode.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.PDModel;

public class PDJavascriptNameTreeNode : PDNameTreeNode<PDActionJavaScript>
{
    public PDJavascriptNameTreeNode()
    {
    }

    public PDJavascriptNameTreeNode(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    protected override PDActionJavaScript ConvertCOSToPD(COSBase? baseValue)
    {
        if (baseValue is not COSDictionary dictionary)
        {
            throw new IOException($"Error creating Javascript object, expected a COSDictionary and not {baseValue}");
        }

        PDAction? action = PDActionFactory.CreateAction(dictionary);
        if (action is not PDActionJavaScript javascript)
        {
            throw new IOException("Error creating Javascript object, expected PDActionJavaScript");
        }

        return javascript;
    }

    protected override PDNameTreeNode<PDActionJavaScript> CreateChildNode(COSDictionary dictionary)
    {
        return new PDJavascriptNameTreeNode(dictionary);
    }
}
