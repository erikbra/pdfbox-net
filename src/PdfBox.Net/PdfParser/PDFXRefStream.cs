/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFXRefStream.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PdfParser;

public sealed class PDFXRefStream
{
    private readonly COSStream _stream;

    public PDFXRefStream(COSStream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public COSStream GetStream()
    {
        return _stream;
    }
}
