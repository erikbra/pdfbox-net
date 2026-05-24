/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox CryptFilter.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/CryptFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class CryptFilter : Filter
{
    private static readonly IdentityFilter Identity = new();

    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        COSName? encryptionName = parameters.GetCOSName(COSName.NAME);
        if (encryptionName is null || encryptionName.Equals(COSName.IDENTITY))
        {
            return Identity.Decode(input, output, parameters, index, options);
        }

        throw new NotSupportedException($"Unsupported crypt filter {encryptionName.GetName()}.");
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        COSName? encryptionName = parameters.GetCOSName(COSName.NAME);
        if (encryptionName is null || encryptionName.Equals(COSName.IDENTITY))
        {
            Identity.Encode(input, output, parameters, index);
            return;
        }

        throw new NotSupportedException($"Unsupported crypt filter {encryptionName.GetName()}.");
    }
}
