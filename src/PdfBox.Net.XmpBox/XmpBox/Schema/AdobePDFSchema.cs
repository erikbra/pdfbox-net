/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/AdobePDFSchema.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.XmpBox.Schema;

public class AdobePDFSchema : XMPSchema
{
    public const string NamespaceUri = "http://ns.adobe.com/pdf/1.3/";
    public const string PreferredPrefix = "pdf";

    public AdobePDFSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public AdobePDFSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }
}
