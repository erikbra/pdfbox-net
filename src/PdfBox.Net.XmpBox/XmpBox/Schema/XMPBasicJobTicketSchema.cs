/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/XMPBasicJobTicketSchema.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.XmpBox.Schema;

public class XMPBasicJobTicketSchema : XMPSchema
{
    public const string NamespaceUri = "http://ns.adobe.com/xap/1.0/bj/";
    public const string PreferredPrefix = "xmpBJ";

    public XMPBasicJobTicketSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public XMPBasicJobTicketSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }
}
