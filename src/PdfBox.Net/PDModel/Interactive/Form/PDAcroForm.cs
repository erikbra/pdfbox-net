/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDAcroForm.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed class PDAcroForm : COSObjectable
{
    private readonly PDDocument _document;
    private readonly COSDictionary _dictionary;

    public PDAcroForm(PDDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _dictionary = new COSDictionary();
    }

    public PDAcroForm(PDDocument document, COSDictionary dictionary)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject()
    {
        return _dictionary;
    }

    public IList<PDField> GetFields()
    {
        COSArray? array = _dictionary.GetCOSArray(COSName.GetPDFName("Fields"));
        if (array == null)
        {
            return new COSArrayList<PDField>(_dictionary, COSName.GetPDFName("Fields"));
        }

        List<PDField> fields = new(array.Size());
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary fieldDict)
            {
                fields.Add(PDField.FromDictionary(this, fieldDict));
            }
        }
        return new COSArrayList<PDField>(fields, array);
    }

    public void SetFields(IList<PDField>? fields)
    {
        _dictionary.SetItem(COSName.GetPDFName("Fields"), COSArrayList<object>.ConverterToCOSArray(fields?.Cast<object>().ToList()));
    }

    internal PDDocument GetDocument()
    {
        return _document;
    }
}
