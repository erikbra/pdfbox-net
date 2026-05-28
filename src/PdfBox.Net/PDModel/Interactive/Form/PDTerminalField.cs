/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDTerminalField.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Interactive.Form;

public abstract class PDTerminalField : PDField
{
    protected PDTerminalField(PDAcroForm acroForm)
        : base(acroForm)
    {
    }

    protected PDTerminalField(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    public List<PDAnnotationWidget> GetWidgets()
    {
        List<PDAnnotationWidget> widgets = [];
        COSArray? kids = dictionary.GetCOSArray(COSName.KIDS);
        if (kids == null)
        {
            widgets.Add(new PDAnnotationWidget(dictionary));
        }
        else if (!kids.IsEmpty())
        {
            for (int i = 0; i < kids.Size(); i++)
            {
                if (kids.GetObject(i) is COSDictionary kid)
                {
                    widgets.Add(new PDAnnotationWidget(kid));
                }
            }
        }

        return widgets;
    }

    public void SetWidgets(List<PDAnnotationWidget> children)
    {
        COSArray kidsArray = new(children);
        dictionary.SetItem(COSName.KIDS, kidsArray);
        foreach (PDAnnotationWidget widget in children)
        {
            if (widget.GetCOSObject() is COSDictionary dictionary)
            {
                dictionary.SetItem(COSName.PARENT, GetCOSObject());
            }
        }
    }

    protected virtual void ApplyChange()
    {
        ConstructAppearances();
    }

    internal override void RefreshAppearance()
    {
        ApplyChange();
    }

    protected abstract void ConstructAppearances();
}
