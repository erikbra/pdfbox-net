/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/XrefParser.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 */

namespace PdfBox.Net.PdfParser;

public sealed class XrefParser
{
    private readonly PDFParser _parser;

    public XrefParser(Stream input)
    {
        _parser = new PDFParser(input);
    }

    public ParsedPDFDocument Parse()
    {
        return _parser.Parse();
    }
}
