/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFOptionElement.java
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

namespace PdfBox.Net.PDModel.Fdf;

public class FDFOptionElement : COSObjectable
{
    private readonly COSArray _option;

    public FDFOptionElement()
    {
        _option = new COSArray();
        _option.Add(new COSString(string.Empty));
        _option.Add(new COSString(string.Empty));
    }

    public FDFOptionElement(COSArray option)
    {
        _option = option ?? throw new ArgumentNullException(nameof(option));
        EnsureRequiredEntries();
    }

    public COSBase GetCOSObject()
    {
        return _option;
    }

    public COSArray GetCOSArray()
    {
        return _option;
    }

    public string GetOption()
    {
        return _option.GetObject(0) is COSString option ? option.GetString() : string.Empty;
    }

    public void SetOption(string option)
    {
        EnsureRequiredEntries();
        _option.Set(0, new COSString(option ?? string.Empty));
    }

    public string GetDefaultAppearanceString()
    {
        return _option.GetObject(1) is COSString appearance ? appearance.GetString() : string.Empty;
    }

    public void SetDefaultAppearanceString(string defaultAppearanceString)
    {
        EnsureRequiredEntries();
        _option.Set(1, new COSString(defaultAppearanceString ?? string.Empty));
    }

    private void EnsureRequiredEntries()
    {
        while (_option.Size() < 2)
        {
            _option.Add(new COSString(string.Empty));
        }
    }
}
