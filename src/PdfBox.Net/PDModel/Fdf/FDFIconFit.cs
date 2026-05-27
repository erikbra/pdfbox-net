/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFIconFit.java
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

namespace PdfBox.Net.PDModel.Fdf;

public class FDFIconFit : COSObjectable
{
    public const string SCALE_OPTION_ALWAYS = "A";
    public const string SCALE_OPTION_ONLY_WHEN_ICON_IS_BIGGER = "B";
    public const string SCALE_OPTION_ONLY_WHEN_ICON_IS_SMALLER = "S";
    public const string SCALE_OPTION_NEVER = "N";

    public const string SCALE_TYPE_ANAMORPHIC = "A";
    public const string SCALE_TYPE_PROPORTIONAL = "P";

    private static readonly COSName ScaleOptionName = COSName.GetPDFName("SW");
    private static readonly COSName FractionalSpaceName = COSName.A;
    private static readonly COSName ScaleToFitName = COSName.GetPDFName("FB");

    private readonly COSDictionary _fit;

    public FDFIconFit()
    {
        _fit = new COSDictionary();
    }

    public FDFIconFit(COSDictionary fit)
    {
        _fit = fit ?? throw new ArgumentNullException(nameof(fit));
    }

    public COSBase GetCOSObject()
    {
        return _fit;
    }

    public string GetScaleOption()
    {
        return _fit.GetNameAsString(ScaleOptionName) ?? SCALE_OPTION_ALWAYS;
    }

    public void SetScaleOption(string? option)
    {
        _fit.SetName(ScaleOptionName, option);
    }

    public string GetScaleType()
    {
        return _fit.GetNameAsString(COSName.S) ?? SCALE_TYPE_PROPORTIONAL;
    }

    public void SetScaleType(string? scale)
    {
        _fit.SetName(COSName.S, scale);
    }

    public PDRange GetFractionalSpaceToAllocate()
    {
        COSArray? array = _fit.GetCOSArray(FractionalSpaceName);
        if (array is not null)
        {
            return new PDRange(array);
        }

        PDRange created = new();
        created.SetMin(0.5f);
        created.SetMax(0.5f);
        SetFractionalSpaceToAllocate(created);
        return created;
    }

    public void SetFractionalSpaceToAllocate(PDRange? space)
    {
        _fit.SetItem(FractionalSpaceName, space);
    }

    public bool ShouldScaleToFitAnnotation()
    {
        return _fit.GetBoolean(ScaleToFitName, false);
    }

    public void SetScaleToFitAnnotation(bool value)
    {
        _fit.SetBoolean(ScaleToFitName, value);
    }
}
