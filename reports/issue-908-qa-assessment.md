# Issue 908 QA assessment

## Sync scope

This batch advances the PDFBox 3.0 upstream baseline from
`10950c29006e36cfba48e74d4031784e31562cbf` to
`a1685ce5bccd2397737b056663fcf4697686fea3` (four commits).

## Production and test parity

| Upstream change | Disposition | .NET evidence |
| --- | --- | --- |
| `PDAbstractContentStream` caches one `GsubWorker` per `PDType0Font` (PDFBOX-6220) | adapted, no runtime change | The adapted .NET stream accepts a `COSName`, writes the `Tf` operator directly, and does not construct or retain GSUB workers. Introducing the Java cache without the surrounding active-font and shaping model would create unused state. |
| `PDDocument` also catches `NoClassDefFoundError` while probing optional AWT classes (PDFBOX-6219) | not applicable | The .NET document type has no AWT class loading or equivalent static initialization probe. |
| `TestFontEmbedding` selects the same Bengali font repeatedly to exercise the GSUB cache | deferred | The adapted .NET content-stream path has no GSUB worker cache to exercise. Existing `GsubPipelineTest` coverage continues to verify the independently ported worker factory and Bengali shaping pipeline. |

No Java production or test files were added or deleted in this range. The deferred
upstream test is confined to the explicitly adapted content-stream architecture and
does not leave an unreported test-parity gap.

## Per-file sync log

| Target | Previous sync | New sync | `conflict_type` | `result_status` | `local_region_count` | Note |
| --- | --- | --- | --- | --- | ---: | --- |
| `PDModel/PDAbstractContentStream.cs` | `ccd281cf` | `a1685ce5` | semantic adaptation | in-sync | 0 | Java GSUB cache is not applicable to the named-font writer abstraction. |
| `PDModel/PDDocument.cs` | `733fcc91` | `a1685ce5` | platform runtime | in-sync | 0 | Java AWT class-loading failure handling has no .NET equivalent. |

## Traceability

- Both changed production mappings point to
  `a1685ce5bccd2397737b056663fcf4697686fea3`.
- Conversion and traceability records describe the adapted or non-applicable behavior.
- Canonical parity and upstream-sync reports were regenerated against the exact
  Apache PDFBox 3.0 revision.

## Local verification

- `dotnet restore PdfBoxNet.slnx`: passed.
- `dotnet build PdfBoxNet.slnx --configuration Release --no-restore`: passed with
  five pre-existing nullability warnings in untouched files.
- `dotnet test PdfBoxNet.slnx --configuration Release --no-build --nologo`:
  1,443 passed, 7 explicitly skipped, 0 failed.
- `dotnet pack PdfBoxNet.slnx --configuration Release --no-build
  -p:ContinuousIntegrationBuild=true`: passed for all ten packages.
- API-surface ratchet gate against upstream `3.0`: passed with no unreviewed changes.
- Runtime parity harness syntax and command-line smoke checks: passed; the full
  Java/.NET corpus remains a pull-request CI job.
- Targeted formatter verification for both touched C# files: passed.
- Canonical upstream inventory: 1,071 mapped, 0 missing at
  `a1685ce5bccd2397737b056663fcf4697686fea3`.
