/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox IdentityFilter.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/IdentityFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class IdentityFilter : Filter
{
    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        input.CopyTo(output);
        output.Flush();
        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        input.CopyTo(output);
        output.Flush();
    }
}
