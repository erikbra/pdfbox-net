/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDPushButton.java
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

public class PDPushButton : PDButton
{
    public PDPushButton(PDAcroForm acroForm)
        : base(acroForm)
    {
        dictionary.SetFlag(COSName.GetPDFName("FF"), FlagPushButton, true);
    }

    internal PDPushButton(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    public override List<string> GetExportValues()
    {
        return [];
    }

    public override void SetExportValues(IList<string>? values)
    {
        if (values != null && values.Count > 0)
        {
            throw new ArgumentException("A push button shall not use the Opt entry.", nameof(values));
        }
    }

    public override string GetValue()
    {
        return string.Empty;
    }

    public override string GetDefaultValue()
    {
        return string.Empty;
    }

    public override ISet<string> GetOnValues()
    {
        return new HashSet<string>();
    }
}
