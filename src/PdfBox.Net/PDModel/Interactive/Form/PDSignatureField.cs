/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDSignatureField.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.PDModel.Interactive.Form;

public partial class PDSignatureField : PDTerminalField
{
    public PDSignatureField(PDAcroForm acroForm)
        : base(acroForm)
    {
        dictionary.SetItem(COSName.GetPDFName("FT"), COSName.GetPDFName("Sig"));
        PDAnnotationWidget firstWidget = GetWidgets()[0];
        firstWidget.SetLocked(true);
        firstWidget.SetPrinted(true);
        SetPartialName(GeneratePartialName());
    }

    internal PDSignatureField(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    private string GeneratePartialName()
    {
        const string fieldName = "Signature";
        HashSet<string?> nameSet = [];
        foreach (PDField field in acroForm.GetFieldTree())
        {
            nameSet.Add(field.GetPartialName());
        }

        int i = 1;
        while (nameSet.Contains(fieldName + i))
        {
            ++i;
        }

        return fieldName + i;
    }

    public PDSignature? GetSignature() => GetValue();

    public void SetValue(PDSignature value)
    {
        dictionary.SetItem(COSName.V, value);
        ApplyChange();
    }

    public void SetValue(string value)
    {
        throw new NotSupportedException("Signature fields don't support setting the value as String - use setValue(PDSignature value) instead");
    }

    public void SetDefaultValue(PDSignature value)
    {
        dictionary.SetItem(COSName.GetPDFName("DV"), value);
    }

    public PDSignature? GetValue()
    {
        COSDictionary? value = dictionary.GetCOSDictionary(COSName.V);
        return value != null ? new PDSignature(value) : null;
    }

    public PDSignature? GetDefaultValue()
    {
        COSDictionary? value = dictionary.GetCOSDictionary(COSName.GetPDFName("DV"));
        return value != null ? new PDSignature(value) : null;
    }

    public override string? GetValueAsString()
    {
        return GetValue()?.ToString() ?? string.Empty;
    }

    public PDSeedValue? GetSeedValue()
    {
        COSDictionary? dict = dictionary.GetCOSDictionary(COSName.GetPDFName("SV"));
        return dict != null ? new PDSeedValue(dict) : null;
    }

    public void SetSeedValue(PDSeedValue? sv)
    {
        if (sv != null)
        {
            dictionary.SetItem(COSName.GetPDFName("SV"), sv);
        }
    }

    protected override void ConstructAppearances()
    {
        PDAnnotationWidget? widget = GetWidgets().FirstOrDefault();
        if (widget == null)
        {
            return;
        }

        PDRectangle? rectangle = widget.GetRectangle();
        if (rectangle == null ||
            (float.Equals(rectangle.GetHeight(), 0) && float.Equals(rectangle.GetWidth(), 0)) ||
            widget.IsNoView() || widget.IsHidden())
        {
            return;
        }

        // Signature appearance generation is intentionally not performed here.
    }
}
