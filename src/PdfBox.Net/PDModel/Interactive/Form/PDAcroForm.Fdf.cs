/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDAcroForm.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.PDModel.Fdf;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed partial class PDAcroForm
{
    public void ImportFDF(FDFDocument fdfDocument)
    {
        ArgumentNullException.ThrowIfNull(fdfDocument);

        foreach (FDFField fdfField in fdfDocument.GetCatalog().GetFDF().GetFields() ?? [])
        {
            ImportFDFField(fdfField, null);
        }
    }

    public FDFDocument ExportFDF()
    {
        FDFDocument fdfDocument = new();
        List<FDFField> fdfFields = [];
        foreach (PDField field in GetFieldTree())
        {
            string? name = field.GetFullyQualifiedName();
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            FDFField fdfField = new();
            fdfField.SetPartialFieldName(name);
            fdfField.SetValue(ExportFieldValue(field));
            fdfFields.Add(fdfField);
        }

        fdfDocument.GetCatalog().GetFDF().SetFields(fdfFields);
        return fdfDocument;
    }

    public PDField? GetField(string fullyQualifiedName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullyQualifiedName);
        foreach (PDField field in GetFieldTree())
        {
            if (string.Equals(field.GetFullyQualifiedName(), fullyQualifiedName, StringComparison.Ordinal))
            {
                return field;
            }
        }

        return null;
    }

    private void ImportFDFField(FDFField fdfField, string? parentName)
    {
        string? partialName = fdfField.GetPartialFieldName();
        string? qualifiedName = string.IsNullOrEmpty(parentName)
            ? partialName
            : string.IsNullOrEmpty(partialName) ? parentName : $"{parentName}.{partialName}";

        if (!string.IsNullOrEmpty(qualifiedName))
        {
            PDField? field = GetField(qualifiedName);
            if (field is not null)
            {
                ApplyFDFValue(field, fdfField.GetValue());
            }
        }

        foreach (FDFField child in fdfField.GetKids() ?? [])
        {
            ImportFDFField(child, qualifiedName);
        }
    }

    private static object? ExportFieldValue(PDField field)
    {
        return field switch
        {
            PDChoice choice => choice.GetValue(),
            PDTextField textField => textField.GetValue(),
            PDButton button => button.GetValue(),
            PDNonTerminalField nonTerminal => nonTerminal.GetValue(),
            _ => field.GetValueAsString()
        };
    }

    private static void ApplyFDFValue(PDField field, object? value)
    {
        switch (field)
        {
            case PDTextField textField:
                textField.SetValue(value as string ?? value?.ToString());
                break;
            case PDChoice choice when value is IList<string> values:
                choice.SetValue(values);
                break;
            case PDChoice choice:
                choice.SetValue(value as string ?? value?.ToString());
                break;
            case PDButton button when value is not null:
                button.SetValue(value.ToString() ?? string.Empty);
                break;
            case PDNonTerminalField nonTerminal:
                nonTerminal.SetValue(value as string ?? value?.ToString());
                break;
        }
    }
}
