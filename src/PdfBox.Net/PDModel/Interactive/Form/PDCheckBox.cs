/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDCheckBox.java
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

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed class PDCheckBox : PDField
{
    public PDCheckBox(PDAcroForm acroForm)
        : base(acroForm)
    {
        dictionary.SetName(COSName.GetPDFName("FT"), "Btn");
    }

    internal PDCheckBox(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    public bool IsChecked()
    {
        return string.Equals(dictionary.GetNameAsString(COSName.V), "Yes", StringComparison.Ordinal);
    }

    public void Check()
    {
        dictionary.SetName(COSName.V, "Yes");
    }

    public void UnCheck()
    {
        dictionary.SetName(COSName.V, "Off");
    }

    public override string? GetValueAsString()
    {
        return dictionary.GetNameAsString(COSName.V);
    }
}
