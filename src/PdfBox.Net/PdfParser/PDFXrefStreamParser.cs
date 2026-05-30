/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFXrefStreamParser.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PdfParser;

public sealed class PDFXrefStreamParser
{
    private readonly COSStream _stream;

    public PDFXrefStreamParser(COSStream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public PDFXRefStream Parse()
    {
        return new PDFXRefStream(_stream);
    }
}
