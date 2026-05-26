/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/TIFFExtension.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: direct
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.Filter;

internal static class TIFFExtension
{
    public const int COMPRESSION_CCITT_T4 = 3;
    public const int COMPRESSION_CCITT_T6 = 4;
    public const int COMPRESSION_LZW = 5;
    public const int COMPRESSION_OLD_JPEG = 6;
    public const int COMPRESSION_JPEG = 7;
    public const int COMPRESSION_DEFLATE = 32946;
    public const int COMPRESSION_ZLIB = 8;

    public const int PHOTOMETRIC_SEPARATED = 5;
    public const int PHOTOMETRIC_YCBCR = 6;
    public const int PHOTOMETRIC_CIELAB = 8;
    public const int PHOTOMETRIC_ICCLAB = 9;
    public const int PHOTOMETRIC_ITULAB = 10;

    public const int PLANARCONFIG_PLANAR = 2;

    public const int PREDICTOR_HORIZONTAL_DIFFERENCING = 2;
    public const int PREDICTOR_HORIZONTAL_FLOATINGPOINT = 3;

    public const int FILL_RIGHT_TO_LEFT = 2;

    public const int SAMPLEFORMAT_INT = 2;
    public const int SAMPLEFORMAT_FP = 3;
    public const int SAMPLEFORMAT_UNDEFINED = 4;

    public const int YCBCR_POSITIONING_CENTERED = 1;
    public const int YCBCR_POSITIONING_COSITED = 2;

    public const int JPEG_PROC_BASELINE = 1;
    public const int JPEG_PROC_LOSSLESS = 14;

    public const int INKSET_CMYK = 1;
    public const int INKSET_NOT_CMYK = 2;

    public const int ORIENTATION_TOPRIGHT = 2;
    public const int ORIENTATION_BOTRIGHT = 3;
    public const int ORIENTATION_BOTLEFT = 4;
    public const int ORIENTATION_LEFTTOP = 5;
    public const int ORIENTATION_RIGHTTOP = 6;
    public const int ORIENTATION_RIGHTBOT = 7;
    public const int ORIENTATION_LEFTBOT = 8;

    public const int GROUP3OPT_2DENCODING = 1;
    public const int GROUP3OPT_UNCOMPRESSED = 2;
    public const int GROUP3OPT_FILLBITS = 4;
    public const int GROUP3OPT_BYTEALIGNED = 8;
    public const int GROUP4OPT_UNCOMPRESSED = 2;
    public const int GROUP4OPT_BYTEALIGNED = 4;
    public const int COMPRESSION_CCITT_MODIFIED_HUFFMAN_RLE = 2;
    public const int FILL_LEFT_TO_RIGHT = 1;
}
