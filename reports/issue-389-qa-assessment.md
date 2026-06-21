# Issue 389 QA Assessment

## Production parity

| Target | Upstream-test parity | Evidence |
| --- | --- | --- |
| `Interactive/Form/FieldRemover.cs` | converted | `TestFieldRemover` creates a fixture, removes the requested field, and verifies that the remaining field persists. |
| `Lucene/IndexPDFFiles.cs` | converted | `TestLuceneIntegration` covers index creation and searchable content. |
| `Lucene/LucenePDFDocument.cs` | converted | `TestLuceneIntegration` verifies indexed document conversion. |
| `Printing/Printing.cs` | converted | `PrintingTest` covers configured print-to-file behavior on supported platforms. |
| `Printing/OpaquePDFRenderer.cs` | converted | `PrintingTest` covers renderer integration on supported platforms. |

## Port assessment

The remaining examples-layer `PORT_MODE` headers are mechanical. The completed behavior preserves
the upstream workflows while adapting unavoidable .NET APIs for printing and Lucene.NET.

`TestCreateSignature` retains tests that require external TSA, OCSP, or CRL services. `MergePDFATest`
remains skipped because VeraPDF has no integrated .NET equivalent and `CreatePDFA` cannot yet create
a validated PDF/A-1b fixture.

## Traceability

`traceability-parity-report.json` has in-sync mechanical rows for the promoted production files.
The report tracks production source paths; test targets without existing traceability rows are
intentionally represented through their source-file provenance headers and, where present,
`conversion-records.json`. No production report rows are missing for this issue's completed classes.
