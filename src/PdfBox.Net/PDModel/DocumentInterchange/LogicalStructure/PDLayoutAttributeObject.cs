/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDLayoutAttributeObject.java
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

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// A layout attribute object (owner <c>Layout</c>) for tagged PDF structure elements.
/// Provides typed accessors for the standard layout attributes defined in PDF 32000-1:2008
/// Table 343–344.
/// </summary>
public class PDLayoutAttributeObject : PDDefaultAttributeObject
{
    /// <summary>Owner name for layout attributes.</summary>
    public const string Owner = "Layout";

    // ── Placement ──────────────────────────────────────────────────────────────

    /// <summary>Block, Inline, Before, Start, End, or a keyword string.</summary>
    public static readonly string PlacementBlock = "Block";

    /// <summary>Inline placement value.</summary>
    public static readonly string PlacementInline = "Inline";

    private static readonly COSName PlacementName = COSName.GetPDFName("Placement");
    private static readonly COSName WritingModeName = COSName.GetPDFName("WritingMode");
    private static readonly COSName BackgroundColorName = COSName.GetPDFName("BackgroundColor");
    private static readonly COSName BorderColorName = COSName.GetPDFName("BorderColor");
    private static readonly COSName BorderStyleName = COSName.GetPDFName("BorderStyle");
    private static readonly COSName BorderThicknessName = COSName.GetPDFName("BorderThickness");
    private static readonly COSName PaddingName = COSName.GetPDFName("Padding");
    private static readonly COSName ColorName = COSName.GetPDFName("Color");
    private static readonly COSName SpaceBeforeName = COSName.GetPDFName("SpaceBefore");
    private static readonly COSName SpaceAfterName = COSName.GetPDFName("SpaceAfter");
    private static readonly COSName StartIndentName = COSName.GetPDFName("StartIndent");
    private static readonly COSName EndIndentName = COSName.GetPDFName("EndIndent");
    private static readonly COSName TextIndentName = COSName.GetPDFName("TextIndent");
    private static readonly COSName TextAlignName = COSName.GetPDFName("TextAlign");
    private static readonly COSName BBoxName = COSName.GetPDFName("BBox");
    private static readonly COSName WidthName = COSName.GetPDFName("Width");
    private static readonly COSName HeightName = COSName.GetPDFName("Height");
    private static readonly COSName BlockAlignName = COSName.GetPDFName("BlockAlign");
    private static readonly COSName InlineAlignName = COSName.GetPDFName("InlineAlign");
    private static readonly COSName TBorderStyleName = COSName.GetPDFName("TBorderStyle");
    private static readonly COSName TPaddingName = COSName.GetPDFName("TPadding");
    private static readonly COSName BaselineShiftName = COSName.GetPDFName("BaselineShift");
    private static readonly COSName LineHeightName = COSName.GetPDFName("LineHeight");
    private static readonly COSName TextDecorationColorName = COSName.GetPDFName("TextDecorationColor");
    private static readonly COSName TextDecorationThicknessName = COSName.GetPDFName("TextDecorationThickness");
    private static readonly COSName TextDecorationTypeName = COSName.GetPDFName("TextDecorationType");
    private static readonly COSName RubyAlignName = COSName.GetPDFName("RubyAlign");
    private static readonly COSName RubyPositionName = COSName.GetPDFName("RubyPosition");
    private static readonly COSName GlyphOrientationVerticalName = COSName.GetPDFName("GlyphOrientationVertical");
    private static readonly COSName ColumnCountName = COSName.GetPDFName("ColumnCount");
    private static readonly COSName ColumnGapName = COSName.GetPDFName("ColumnGap");
    private static readonly COSName ColumnWidthsName = COSName.GetPDFName("ColumnWidths");

    /// <summary>
    /// Default constructor — creates a new Layout attribute object.
    /// </summary>
    public PDLayoutAttributeObject()
    {
        SetOwner(Owner);
    }

    /// <summary>
    /// Creates a layout attribute object from an existing dictionary.
    /// </summary>
    public PDLayoutAttributeObject(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    // ── Placement ──────────────────────────────────────────────────────────────

    /// <summary>Returns the Placement attribute value, or <see langword="null"/>.</summary>
    public string? GetPlacement() => GetCOSObject().GetNameAsString(PlacementName);

    /// <summary>Sets the Placement attribute.</summary>
    public void SetPlacement(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(PlacementName);
        GetCOSObject().SetName(PlacementName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(PlacementName));
    }

    // ── WritingMode ────────────────────────────────────────────────────────────

    /// <summary>Returns the WritingMode attribute value, or <see langword="null"/>.</summary>
    public string? GetWritingMode() => GetCOSObject().GetNameAsString(WritingModeName);

    /// <summary>Sets the WritingMode attribute.</summary>
    public void SetWritingMode(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(WritingModeName);
        GetCOSObject().SetName(WritingModeName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(WritingModeName));
    }

    // ── TextAlign ─────────────────────────────────────────────────────────────

    /// <summary>Returns the TextAlign attribute value, or <see langword="null"/>.</summary>
    public string? GetTextAlign() => GetCOSObject().GetNameAsString(TextAlignName);

    /// <summary>Sets the TextAlign attribute.</summary>
    public void SetTextAlign(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(TextAlignName);
        GetCOSObject().SetName(TextAlignName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(TextAlignName));
    }

    // ── Width ─────────────────────────────────────────────────────────────────

    /// <summary>Returns the Width attribute value, or <see cref="float.NaN"/>.</summary>
    public float GetWidth()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(WidthName);
        return base_ is COSNumber n ? n.FloatValue() : float.NaN;
    }

    /// <summary>Returns the Width attribute as a string (may be <c>Auto</c>), or <see langword="null"/>.</summary>
    public string? GetWidthAsName() => GetCOSObject().GetNameAsString(WidthName);

    /// <summary>Sets the Width attribute to a numeric value.</summary>
    public void SetWidth(float value)
    {
        COSBase? old = GetCOSObject().GetItem(WidthName);
        GetCOSObject().SetFloat(WidthName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(WidthName));
    }

    /// <summary>Sets the Width attribute to a keyword such as <c>Auto</c>.</summary>
    public void SetWidth(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(WidthName);
        GetCOSObject().SetName(WidthName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(WidthName));
    }

    // ── Height ────────────────────────────────────────────────────────────────

    /// <summary>Returns the Height attribute value, or <see cref="float.NaN"/>.</summary>
    public float GetHeight()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(HeightName);
        return base_ is COSNumber n ? n.FloatValue() : float.NaN;
    }

    /// <summary>Returns the Height attribute as a string (may be <c>Auto</c>), or <see langword="null"/>.</summary>
    public string? GetHeightAsName() => GetCOSObject().GetNameAsString(HeightName);

    /// <summary>Sets the Height attribute to a numeric value.</summary>
    public void SetHeight(float value)
    {
        COSBase? old = GetCOSObject().GetItem(HeightName);
        GetCOSObject().SetFloat(HeightName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(HeightName));
    }

    /// <summary>Sets the Height attribute to a keyword such as <c>Auto</c>.</summary>
    public void SetHeight(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(HeightName);
        GetCOSObject().SetName(HeightName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(HeightName));
    }

    // ── SpaceBefore / SpaceAfter ──────────────────────────────────────────────

    /// <summary>Returns the SpaceBefore attribute value.</summary>
    public float GetSpaceBefore()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(SpaceBeforeName);
        return base_ is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>Sets the SpaceBefore attribute.</summary>
    public void SetSpaceBefore(float value)
    {
        COSBase? old = GetCOSObject().GetItem(SpaceBeforeName);
        GetCOSObject().SetFloat(SpaceBeforeName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(SpaceBeforeName));
    }

    /// <summary>Returns the SpaceAfter attribute value.</summary>
    public float GetSpaceAfter()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(SpaceAfterName);
        return base_ is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>Sets the SpaceAfter attribute.</summary>
    public void SetSpaceAfter(float value)
    {
        COSBase? old = GetCOSObject().GetItem(SpaceAfterName);
        GetCOSObject().SetFloat(SpaceAfterName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(SpaceAfterName));
    }

    // ── StartIndent / EndIndent ───────────────────────────────────────────────

    /// <summary>Returns the StartIndent attribute value.</summary>
    public float GetStartIndent()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(StartIndentName);
        return base_ is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>Sets the StartIndent attribute.</summary>
    public void SetStartIndent(float value)
    {
        COSBase? old = GetCOSObject().GetItem(StartIndentName);
        GetCOSObject().SetFloat(StartIndentName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(StartIndentName));
    }

    /// <summary>Returns the EndIndent attribute value.</summary>
    public float GetEndIndent()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(EndIndentName);
        return base_ is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>Sets the EndIndent attribute.</summary>
    public void SetEndIndent(float value)
    {
        COSBase? old = GetCOSObject().GetItem(EndIndentName);
        GetCOSObject().SetFloat(EndIndentName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(EndIndentName));
    }

    // ── TextIndent ────────────────────────────────────────────────────────────

    /// <summary>Returns the TextIndent attribute value.</summary>
    public float GetTextIndent()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(TextIndentName);
        return base_ is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>Sets the TextIndent attribute.</summary>
    public void SetTextIndent(float value)
    {
        COSBase? old = GetCOSObject().GetItem(TextIndentName);
        GetCOSObject().SetFloat(TextIndentName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(TextIndentName));
    }

    // ── BaselineShift ─────────────────────────────────────────────────────────

    /// <summary>Returns the BaselineShift attribute value.</summary>
    public float GetBaselineShift()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(BaselineShiftName);
        return base_ is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>Sets the BaselineShift attribute.</summary>
    public void SetBaselineShift(float value)
    {
        COSBase? old = GetCOSObject().GetItem(BaselineShiftName);
        GetCOSObject().SetFloat(BaselineShiftName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(BaselineShiftName));
    }

    // ── LineHeight ────────────────────────────────────────────────────────────

    /// <summary>Returns the LineHeight attribute value, or <see cref="float.NaN"/>.</summary>
    public float GetLineHeight()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(LineHeightName);
        return base_ is COSNumber n ? n.FloatValue() : float.NaN;
    }

    /// <summary>Returns the LineHeight attribute as a name string (e.g. <c>Normal</c> or <c>Auto</c>), or <see langword="null"/>.</summary>
    public string? GetLineHeightAsName() => GetCOSObject().GetNameAsString(LineHeightName);

    /// <summary>Sets the LineHeight attribute to a numeric value.</summary>
    public void SetLineHeight(float value)
    {
        COSBase? old = GetCOSObject().GetItem(LineHeightName);
        GetCOSObject().SetFloat(LineHeightName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(LineHeightName));
    }

    /// <summary>Sets the LineHeight attribute to a keyword such as <c>Normal</c> or <c>Auto</c>.</summary>
    public void SetLineHeight(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(LineHeightName);
        GetCOSObject().SetName(LineHeightName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(LineHeightName));
    }

    // ── BlockAlign / InlineAlign ──────────────────────────────────────────────

    /// <summary>Returns the BlockAlign attribute value, or <see langword="null"/>.</summary>
    public string? GetBlockAlign() => GetCOSObject().GetNameAsString(BlockAlignName);

    /// <summary>Sets the BlockAlign attribute.</summary>
    public void SetBlockAlign(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(BlockAlignName);
        GetCOSObject().SetName(BlockAlignName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(BlockAlignName));
    }

    /// <summary>Returns the InlineAlign attribute value, or <see langword="null"/>.</summary>
    public string? GetInlineAlign() => GetCOSObject().GetNameAsString(InlineAlignName);

    /// <summary>Sets the InlineAlign attribute.</summary>
    public void SetInlineAlign(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(InlineAlignName);
        GetCOSObject().SetName(InlineAlignName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(InlineAlignName));
    }

    // ── ColumnCount / ColumnGap ───────────────────────────────────────────────

    /// <summary>Returns the ColumnCount attribute value.</summary>
    public int GetColumnCount()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(ColumnCountName);
        return base_ is COSNumber n ? n.IntValue() : 1;
    }

    /// <summary>Sets the ColumnCount attribute.</summary>
    public void SetColumnCount(int value)
    {
        COSBase? old = GetCOSObject().GetItem(ColumnCountName);
        GetCOSObject().SetInt(ColumnCountName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(ColumnCountName));
    }
}
