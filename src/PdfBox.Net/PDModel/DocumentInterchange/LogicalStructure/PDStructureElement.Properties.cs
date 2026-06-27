/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDStructureElement.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

public partial class PDStructureElement
{
    public string? ActualText
    {
        get => GetActualText();
        set => SetActualText(value!);
    }

    public string? AlternateDescription
    {
        get => GetAlternateDescription();
        set => SetAlternateDescription(value!);
    }

    public Revisions<PDAttributeObject> Attributes
    {
        get => GetAttributes();
        set => SetAttributes(value);
    }

    public Revisions<string> ClassNames
    {
        get => GetClassNames();
        set => SetClassNames(value);
    }

    public string? ElementIdentifier
    {
        get => GetElementIdentifier();
        set => SetElementIdentifier(value!);
    }

    public string? ExpandedForm
    {
        get => GetExpandedForm();
        set => SetExpandedForm(value!);
    }

    public string? Language
    {
        get => GetLanguage();
        set => SetLanguage(value!);
    }

    public PDPage? Page
    {
        get => GetPage();
        set => SetPage(value!);
    }

    public PDStructureNode? Parent
    {
        get => GetParent();
        set => SetParent(value!);
    }

    public int RevisionNumber
    {
        get => GetRevisionNumber();
        set => SetRevisionNumber(value);
    }

    public string? StructureType
    {
        get => GetStructureType();
        set => SetStructureType(value!);
    }

    public string? Title
    {
        get => GetTitle();
        set => SetTitle(value!);
    }
}
