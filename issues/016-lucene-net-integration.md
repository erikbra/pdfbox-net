### Title
Implement Lucene.Net integration for the `Lucene` examples module

### Summary

The two `Lucene/` examples build a full-text search index over PDF files using Apache Lucene. In Java this is a direct dependency on `org.apache.lucene`. The .NET equivalent is `Lucene.Net` (the official .NET port of Apache Lucene, available on NuGet).

### Affected example files (currently stubs)

- `Lucene/IndexPDFFiles.cs` — crawls a directory of PDFs, extracts text with `PDFTextStripper`, and indexes each document in a Lucene index.
- `Lucene/LucenePDFDocument.cs` — helper that wraps a `PDDocument` as a Lucene `Document` with fields for title, author, content, creation date, etc.

### Missing dependency

Add `Lucene.Net` (and `Lucene.Net.Analysis.Common`) as a NuGet reference to the `PdfBox.Net.Examples` project.

Suggested NuGet packages:
- `Lucene.Net` ≥ 4.8.0
- `Lucene.Net.Analysis.Common` ≥ 4.8.0

### Upstream Java reference

`examples/src/main/java/org/apache/pdfbox/examples/lucene/`

### Acceptance criteria

- `Lucene.Net` is referenced from `PdfBox.Net.Examples.csproj`.
- `LucenePDFDocument.cs` is upgraded to `PORT_MODE: mechanical`, mapping Java `Document`/`Field` types to their `Lucene.Net` equivalents.
- `IndexPDFFiles.cs` is upgraded to `PORT_MODE: mechanical`, using `Lucene.Net.Index.IndexWriter` and `Lucene.Net.Analysis.Standard.StandardAnalyzer`.
