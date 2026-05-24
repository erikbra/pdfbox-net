/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/DCTFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: placeholder
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class DCTFilter : Filter
{
    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        throw new NotSupportedException("DCTDecode is not yet implemented in PdfBox.Net.");
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        throw new NotSupportedException("DCTEncode is not implemented; use JPEGFactory-equivalent APIs once available.");
    }
}
