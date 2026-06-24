/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Compatibility class for Apache PDFBox Java source naming.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/FontMapperImpl.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.PDModel.Font;

public sealed class FontMapperImpl : FontMapper
{
    private readonly DefaultFontProvider _provider = new();

    public string? FindFontFile(string postScriptName)
    {
        return _provider.FindFontFile(postScriptName);
    }
}
