/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDCheckBox.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed class PDCheckBox : PDField
{
    public PDCheckBox(PDAcroForm acroForm)
        : base(acroForm)
    {
        dictionary.SetName(COSName.GetPDFName("FT"), "Btn");
    }

    internal PDCheckBox(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    public bool IsChecked()
    {
        return string.Equals(dictionary.GetNameAsString(COSName.V), "Yes", StringComparison.Ordinal);
    }

    public void Check()
    {
        dictionary.SetName(COSName.V, "Yes");
    }

    public void UnCheck()
    {
        dictionary.SetName(COSName.V, "Off");
    }

    public override string? GetValueAsString()
    {
        return dictionary.GetNameAsString(COSName.V);
    }
}
