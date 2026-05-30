/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/FDFParser.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 */

using PdfBox.Net.PDModel.Fdf;

namespace PdfBox.Net.PdfParser;

public sealed class FDFParser
{
    private readonly Stream _input;

    public FDFParser(Stream input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
    }

    public FDFDocument Parse()
    {
        return FDFDocument.Load(_input);
    }
}
