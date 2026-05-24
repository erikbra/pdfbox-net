/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDField.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Form;

public abstract class PDField : COSObjectable
{
    protected readonly PDAcroForm acroForm;
    protected readonly COSDictionary dictionary;

    protected PDField(PDAcroForm acroForm)
    {
        this.acroForm = acroForm ?? throw new ArgumentNullException(nameof(acroForm));
        dictionary = new COSDictionary();
    }

    protected PDField(PDAcroForm acroForm, COSDictionary dictionary)
    {
        this.acroForm = acroForm ?? throw new ArgumentNullException(nameof(acroForm));
        this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public static PDField FromDictionary(PDAcroForm acroForm, COSDictionary dictionary)
    {
        string? fieldType = dictionary.GetNameAsString(COSName.GetPDFName("FT"));
        return fieldType switch
        {
            "Tx" => new PDTextField(acroForm, dictionary),
            "Btn" => new PDCheckBox(acroForm, dictionary),
            _ => new PDUnknownField(acroForm, dictionary)
        };
    }

    public COSBase GetCOSObject()
    {
        return dictionary;
    }

    public string? GetPartialName()
    {
        return dictionary.GetString(COSName.T);
    }

    public void SetPartialName(string? name)
    {
        dictionary.SetString(COSName.T, name);
    }

    public string? GetFullyQualifiedName()
    {
        string? partial = GetPartialName();
        COSDictionary? parent = dictionary.GetCOSDictionary(COSName.PARENT);
        if (parent == null)
        {
            return partial;
        }

        string? parentName = parent.GetString(COSName.T);
        if (string.IsNullOrEmpty(parentName))
        {
            return partial;
        }

        return string.IsNullOrEmpty(partial) ? parentName : $"{parentName}.{partial}";
    }

    public string? GetFieldType()
    {
        return dictionary.GetNameAsString(COSName.GetPDFName("FT"));
    }

    public abstract string? GetValueAsString();
}
