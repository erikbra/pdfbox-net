/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDExtendedGraphicsState.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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
using PdfBox.Net.PDModel.Graphics;

namespace PdfBox.Net.PDModel.Graphics.State;

public class PDExtendedGraphicsState : COSObjectable
{
    private static readonly COSName TypeName = COSName.GetPDFName("Type");
    private static readonly COSName ExtGStateName = COSName.GetPDFName("ExtGState");
    private static readonly COSName LwName = COSName.GetPDFName("LW");
    private static readonly COSName LcName = COSName.GetPDFName("LC");
    private static readonly COSName LjName = COSName.GetPDFName("LJ");
    private static readonly COSName MlName = COSName.GetPDFName("ML");
    private static readonly COSName DName = COSName.GetPDFName("D");
    private static readonly COSName RiName = COSName.GetPDFName("RI");
    private static readonly COSName FlName = COSName.GetPDFName("FL");
    private static readonly COSName SaName = COSName.GetPDFName("SA");
    private static readonly COSName CaName = COSName.GetPDFName("CA");
    private static readonly COSName CaNsName = COSName.GetPDFName("ca");
    private static readonly COSName AisName = COSName.GetPDFName("AIS");
    private static readonly COSName TkName = COSName.GetPDFName("TK");
    private static readonly COSName SmaskName = COSName.GetPDFName("SMask");
    private static readonly COSName BmName = COSName.GetPDFName("BM");

    private readonly COSDictionary _dictionary;

    public PDExtendedGraphicsState()
        : this(new COSDictionary())
    {
        _dictionary.SetItem(TypeName, ExtGStateName);
    }

    public PDExtendedGraphicsState(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSDictionary GetCOSObject() => _dictionary;
    COSBase COSObjectable.GetCOSObject() => _dictionary;

    public float? GetLineWidth() => GetFloatItem(LwName);
    public void SetLineWidth(float? value) => SetFloatItem(LwName, value);
    public int GetLineCapStyle() => _dictionary.GetInt(LcName, 0);
    public void SetLineCapStyle(int value) => _dictionary.SetInt(LcName, value);
    public int GetLineJoinStyle() => _dictionary.GetInt(LjName, 0);
    public void SetLineJoinStyle(int value) => _dictionary.SetInt(LjName, value);
    public float? GetMiterLimit() => GetFloatItem(MlName);
    public void SetMiterLimit(float? value) => SetFloatItem(MlName, value);
    public float? GetFlatnessTolerance() => GetFloatItem(FlName);
    public void SetFlatnessTolerance(float? value) => SetFloatItem(FlName, value);

    public PDLineDashPattern? GetLineDashPattern()
    {
        COSArray? dp = _dictionary.GetCOSArray(DName);
        if (dp is null || dp.Size() != 2)
        {
            return null;
        }

        COSArray? dashArray = dp.GetObject(0) as COSArray;
        COSNumber? phase = dp.GetObject(1) as COSNumber;
        if (dashArray is null || phase is null)
        {
            return null;
        }

        return new PDLineDashPattern(dashArray, phase.IntValue());
    }

    public void SetLineDashPattern(PDLineDashPattern? pattern)
    {
        _dictionary.SetItem(DName, pattern?.GetCOSObject());
    }

    public string? GetRenderingIntent() => _dictionary.GetNameAsString(RiName);
    public void SetRenderingIntent(string? value) => _dictionary.SetName(RiName, value);
    public float? GetStrokingAlphaConstant() => GetFloatItem(CaName);
    public void SetStrokingAlphaConstant(float? value) => SetFloatItem(CaName, value);
    public float? GetNonStrokingAlphaConstant() => GetFloatItem(CaNsName);
    public void SetNonStrokingAlphaConstant(float? value) => SetFloatItem(CaNsName, value);
    public bool GetAutomaticStrokeAdjustment() => _dictionary.GetBoolean(SaName, false);
    public void SetAutomaticStrokeAdjustment(bool value) => _dictionary.SetBoolean(SaName, value);
    public bool GetAlphaSourceFlag() => _dictionary.GetBoolean(AisName, false);
    public void SetAlphaSourceFlag(bool value) => _dictionary.SetBoolean(AisName, value);
    public bool GetTextKnockoutFlag() => _dictionary.GetBoolean(TkName, true);
    public void SetTextKnockoutFlag(bool value) => _dictionary.SetBoolean(TkName, value);

    public BlendMode GetBlendMode() => BlendModeExtensions.FromCos(_dictionary.GetDictionaryObject(BmName));

    public void SetBlendMode(BlendMode mode) => _dictionary.SetItem(BmName, mode.ToCosName());

    public PDSoftMask? GetSoftMask()
    {
        COSBase? mask = _dictionary.GetDictionaryObject(SmaskName);
        return PDSoftMask.Create(mask);
    }

    public void SetSoftMask(PDSoftMask? softMask)
    {
        _dictionary.SetItem(SmaskName, softMask);
    }

    public void CopyIntoGraphicsState(PDGraphicsState graphicsState)
    {
        ArgumentNullException.ThrowIfNull(graphicsState);
        if (_dictionary.ContainsKey(LwName))
        {
            graphicsState.SetLineWidth(GetLineWidth() ?? 1f);
        }

        if (_dictionary.ContainsKey(LcName))
        {
            graphicsState.SetLineCap(GetLineCapStyle());
        }

        if (_dictionary.ContainsKey(LjName))
        {
            graphicsState.SetLineJoin(GetLineJoinStyle());
        }

        if (_dictionary.ContainsKey(MlName))
        {
            graphicsState.SetMiterLimit(GetMiterLimit() ?? 10f);
        }

        if (_dictionary.ContainsKey(DName))
        {
            graphicsState.SetLineDashPattern(GetLineDashPattern() ?? new PDLineDashPattern());
        }

        if (_dictionary.ContainsKey(RiName))
        {
            graphicsState.SetRenderingIntent(GetRenderingIntent() ?? string.Empty);
        }

        if (_dictionary.ContainsKey(FlName))
        {
            graphicsState.SetFlatness(GetFlatnessTolerance() ?? 1f);
        }

        if (_dictionary.ContainsKey(SaName))
        {
            graphicsState.SetStrokeAdjustment(GetAutomaticStrokeAdjustment());
        }

        if (_dictionary.ContainsKey(CaName))
        {
            graphicsState.SetAlphaConstant(GetStrokingAlphaConstant() ?? 1f);
        }

        if (_dictionary.ContainsKey(CaNsName))
        {
            graphicsState.SetNonStrokeAlphaConstant(GetNonStrokingAlphaConstant() ?? 1f);
        }

        if (_dictionary.ContainsKey(AisName))
        {
            graphicsState.SetAlphaSource(GetAlphaSourceFlag());
        }

        if (_dictionary.ContainsKey(TkName))
        {
            graphicsState.GetTextState().SetKnockoutFlag(GetTextKnockoutFlag());
        }

        if (_dictionary.ContainsKey(SmaskName))
        {
            PDSoftMask? softMask = GetSoftMask();
            if (softMask is not null)
            {
                softMask.SetInitialTransformationMatrix(graphicsState.GetCurrentTransformationMatrix());
            }

            graphicsState.SetSoftMask(softMask);
        }

        if (_dictionary.ContainsKey(BmName))
        {
            graphicsState.SetBlendMode(GetBlendMode());
        }
    }

    private float? GetFloatItem(COSName key)
    {
        return _dictionary.GetDictionaryObject(key) is COSNumber number ? number.FloatValue() : null;
    }

    private void SetFloatItem(COSName key, float? value)
    {
        if (value is null)
        {
            _dictionary.RemoveItem(key);
        }
        else
        {
            _dictionary.SetFloat(key, value.Value);
        }
    }
}
