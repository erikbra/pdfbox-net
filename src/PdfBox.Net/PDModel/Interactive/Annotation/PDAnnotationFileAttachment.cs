/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationFileAttachment.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAnnotationFileAttachment : PDAnnotationMarkup
{
    public const string SUB_TYPE = "FileAttachment";

    public PDAnnotationFileAttachment()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationFileAttachment(COSDictionary dict)
        : base(dict)
    {
    }

    public PDFileSpecification? GetFile()
    {
        COSBase? baseFile = GetCOSDictionary().GetDictionaryObject(COSName.GetPDFName("FS"));
        return baseFile != null ? PDFileSpecification.CreateFS(baseFile) : null;
    }

    public void SetFile(PDFileSpecification? file)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("FS"), file);
    }
}
