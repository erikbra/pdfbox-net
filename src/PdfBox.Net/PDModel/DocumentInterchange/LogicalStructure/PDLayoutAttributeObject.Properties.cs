/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDLayoutAttributeObject.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

public partial class PDLayoutAttributeObject
{
    public float BaselineShift
    {
        get => GetBaselineShift();
        set => SetBaselineShift(value);
    }

    public string? BlockAlign
    {
        get => GetBlockAlign();
        set => SetBlockAlign(value!);
    }

    public int ColumnCount
    {
        get => GetColumnCount();
        set => SetColumnCount(value);
    }

    public float EndIndent
    {
        get => GetEndIndent();
        set => SetEndIndent(value);
    }

    public string? InlineAlign
    {
        get => GetInlineAlign();
        set => SetInlineAlign(value!);
    }

    public string? Placement
    {
        get => GetPlacement();
        set => SetPlacement(value!);
    }

    public float SpaceAfter
    {
        get => GetSpaceAfter();
        set => SetSpaceAfter(value);
    }

    public float SpaceBefore
    {
        get => GetSpaceBefore();
        set => SetSpaceBefore(value);
    }

    public float StartIndent
    {
        get => GetStartIndent();
        set => SetStartIndent(value);
    }

    public string? TextAlign
    {
        get => GetTextAlign();
        set => SetTextAlign(value!);
    }

    public float TextIndent
    {
        get => GetTextIndent();
        set => SetTextIndent(value);
    }

    public string? WritingMode
    {
        get => GetWritingMode();
        set => SetWritingMode(value!);
    }
}
