/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache XmpBox Java source. This compatibility name intentionally
 * lives in the XmpBox.Type namespace; use System.Attribute explicitly for CLR attributes.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/Attribute.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.XmpBox.Type;

public class Attribute : XmpAttribute
{
    public Attribute(string? nsURI, string localName, string value)
        : base(nsURI, localName, value)
    {
    }

    public string GetName() => Name;

    public void SetName(string lname) => Name = lname;

    public string? GetNamespace() => Namespace;

    public void SetNsURI(string? nsURI) => Namespace = nsURI;

    public string GetValue() => Value;

    public void SetValue(string value) => Value = value;

    public override string ToString()
    {
        return $"[attr:{{{Namespace}}}{Name}={Value}]";
    }
}
