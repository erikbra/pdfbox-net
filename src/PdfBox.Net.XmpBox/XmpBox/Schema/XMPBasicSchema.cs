/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/XMPBasicSchema.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.XmpBox.Schema;

public class XMPBasicSchema : XMPSchema
{
    public const string NamespaceUri = "http://ns.adobe.com/xap/1.0/";
    public const string PreferredPrefix = "xmp";

    public XMPBasicSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public XMPBasicSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }
}
