# Issue 016 — Lucene.NET integration

## Summary

Implement PDF text indexing examples using Lucene.NET so that the Lucene example files compile
and work as a replacement for the upstream Java Apache Lucene integration.

## Background

The upstream Java examples use `org.apache.lucene.*` (Apache Lucene Java). The .NET equivalent
is the `Lucene.Net` NuGet package suite.

## Required API surface

Adapt the following Java Lucene classes to their Lucene.NET equivalents:

| Java (upstream) | .NET (Lucene.Net) |
|---|---|
| `org.apache.lucene.document.Document` | `Lucene.Net.Documents.Document` |
| `org.apache.lucene.document.Field` | `Lucene.Net.Documents.Field` |
| `org.apache.lucene.index.IndexWriter` | `Lucene.Net.Index.IndexWriter` |
| `org.apache.lucene.index.IndexWriterConfig` | `Lucene.Net.Index.IndexWriterConfig` |
| `org.apache.lucene.analysis.standard.StandardAnalyzer` | `Lucene.Net.Analysis.Standard.StandardAnalyzer` |
| `org.apache.lucene.store.Directory` | `Lucene.Net.Store.Directory` |
| `org.apache.lucene.store.FSDirectory` | `Lucene.Net.Store.FSDirectory` |

## Affected example files

- `Lucene/IndexPDFFiles.cs`
- `Lucene/LucenePDFDocument.cs`

## Acceptance criteria

- Both files compile without stubs using Lucene.Net packages.
- A basic integration test indexes a small sample PDF and verifies that it can be searched.
- NuGet references to the required Lucene.Net packages are added to
  `PdfBox.Net.Examples.csproj`.
- Traceability rows for both affected source paths are `in-sync`.

## Dependencies

Add the following NuGet packages to `PdfBox.Net.Examples.csproj`:
- `Lucene.Net` (version compatible with the library's target framework)
- `Lucene.Net.Analysis.Common`
