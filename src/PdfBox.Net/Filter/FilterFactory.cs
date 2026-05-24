/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox FilterFactory.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/FilterFactory.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class FilterFactory
{
    public static FilterFactory Instance { get; } = new();

    private readonly IReadOnlyDictionary<COSName, Filter> _filters;

    private FilterFactory()
    {
        Filter flate = new FlateFilter();
        Filter dct = new DCTFilter();
        Filter ccittFax = new CCITTFaxDecodeFilter();
        Filter lzw = new LZWFilter();
        Filter asciiHex = new ASCIIHexFilter();
        Filter ascii85 = new ASCII85Filter();
        Filter runLength = new RunLengthDecodeFilter();
        Filter crypt = new CryptFilter();
        Filter jpx = new JPXFilter();
        Filter jbig2 = new JBIG2Filter();

        _filters = new Dictionary<COSName, Filter>
        {
            [COSName.FLATE_DECODE] = flate,
            [COSName.FLATE_DECODE_ABBREVIATION] = flate,
            [COSName.DCT_DECODE] = dct,
            [COSName.DCT_DECODE_ABBREVIATION] = dct,
            [COSName.CCITTFAX_DECODE] = ccittFax,
            [COSName.CCITTFAX_DECODE_ABBREVIATION] = ccittFax,
            [COSName.LZW_DECODE] = lzw,
            [COSName.LZW_DECODE_ABBREVIATION] = lzw,
            [COSName.ASCII_HEX_DECODE] = asciiHex,
            [COSName.ASCII_HEX_DECODE_ABBREVIATION] = asciiHex,
            [COSName.ASCII85_DECODE] = ascii85,
            [COSName.ASCII85_DECODE_ABBREVIATION] = ascii85,
            [COSName.RUN_LENGTH_DECODE] = runLength,
            [COSName.RUN_LENGTH_DECODE_ABBREVIATION] = runLength,
            [COSName.CRYPT] = crypt,
            [COSName.JPX_DECODE] = jpx,
            [COSName.JBIG2_DECODE] = jbig2
        };
    }

    public Filter GetFilter(string filterName)
    {
        return GetFilter(COSName.GetPDFName(filterName));
    }

    public Filter GetFilter(COSName filterName)
    {
        return _filters.TryGetValue(filterName, out Filter? filter)
            ? filter
            : throw new IOException($"Invalid filter: {filterName}");
    }

    internal IEnumerable<Filter> GetAllFilters()
    {
        return _filters.Values;
    }
}
