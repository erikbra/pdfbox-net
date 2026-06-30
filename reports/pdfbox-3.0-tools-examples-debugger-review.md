# PDFBox 3.0 Tools, Examples, App, And Debugger Review

Issue #592 reviews the non-core Apache PDFBox 3.0 modules outside Preflight
after the `release/3.0` branch reached full source-file coverage for scoped
production Java files.

## Scope

Apache upstream ref: `origin/3.0`
(`ea68b6feae80e671b3d26565b12eccc79e74d967`)

Excluded modules:

- `preflight`
- `preflight-app`

Preflight remains outside the initial 3.0 branch because it was removed from
Apache trunk/4.0 and should be treated as a separate package or milestone if it
is needed later.

## Module Review

| Module | Apache 3.0 production Java files | 3.0 branch state | Follow-up |
|---|---:|---|---|
| `io` | 18 | All source paths are mapped. Runtime and API gates cover the shared IO surface. | None from this review. |
| `fontbox` | 143 | All source paths are mapped. Existing FontBox tests and the API surface gate cover the production surface. | None from this review. |
| `xmpbox` | 74 | All source paths are mapped. XMP parsing/serialization remains part of the normal test and parity corpus path. | None from this review. |
| `benchmark` | 4 | All Apache benchmark source stems are present, with additional .NET BenchmarkDotNet wrappers. The benchmark project is intentionally non-packable. | None from this review. |
| `examples` | 94 | All Apache example source stems are present. The .NET branch has deterministic tests for PDF/A-shaped creation/merge, detached signing, visible signing, forms, rendering, text, image, Lucene, Ant, and utility examples. Remaining skipped flows are accepted external-service or external-validator adaptations documented by #603. | None from this review. |
| `tools` | 26 | All Apache tool source stems are present. Issue #601 adds the `pdfbox` global-tool facade and wires the Apache 3.0 command names through `PDFBox.Run`, including core options for decode, text export, render, merge, split, and overlay. | None from this review. |
| `debugger` | 91 | All Apache debugger source stems are present. The .NET debugger library exposes useful COS/tree/text/font inspection models, including backend-backed Type 3 glyph previews. The non-packable `pdfdebugger` app is a console inspector, and the Java Swing desktop UI is documented as an accepted 3.0 product adaptation. | None from this review. |
| `app` | 0 | Apache `app` is a Maven packaging module that builds the `pdfbox-app` command-line bundle with `org.apache.pdfbox.tools.PDFBox` as the main class. Issue #601 adds `PdfBox.Net.Tools`, a packable .NET global tool with command name `pdfbox`; the separate debugger app remains non-packable. | None from this review. |

The source-stem comparison for `examples`, `tools`, `debugger`, and `benchmark`
found no missing Apache 3.0 production stems. Extra .NET stems are either
properties/helpers introduced by the port, BenchmarkDotNet wrappers, backend
registration helpers, or examples from the trunk/future line already present in
the branch.

## Small Fixes In This Review

`PdfBox.Net.Tools.PDFBox` now dispatches the Apache PDFBox 3.0 command aliases
for implemented tools:

| Apache 3.0 command | .NET implementation |
|---|---|
| `decode` | `WriteDecodedDoc.Run` |
| `export:fdf` | `ExportFDF.Run` |
| `export:xfdf` | `ExportXFDF.Run` |
| `export:images` | `ExtractImages.Run` |
| `export:xmp` | `ExtractXMP.Run` |
| `export:text` | `ExtractText.Run` |
| `fromimage` | `ImageToPDF.Run` |
| `import:fdf` | `ImportFDF.Run` |
| `import:xfdf` | `ImportXFDF.Run` |
| `merge` | `PDFMerger.Run` |
| `overlay` | `OverlayPDF.Run` |
| `print` | `PrintPDF.Run` |
| `render` | `PDFToImage.Run` |
| `split` | `PDFSplit.Run` |
| `fromtext` | `TextToPDF.Run` |

The older .NET command aliases remain available for compatibility. A regression
test covers the new `fromtext` dispatcher path end to end by creating a PDF and
extracting the generated text. Issue #601 adds broader dispatcher tests for
`export:text`, `decode`, `merge`, `split`, help, and explicit unsupported
command handling.

## Judgment

The 3.0 branch has source-file coverage for the reviewed non-Preflight modules.
After issue #601, the `pdfbox` app/CLI surface is present as a .NET global tool
facade over the implemented commands. After issue #602, the debugger UI product
shape is resolved as an accepted adaptation with covered non-GUI inspection
models. Issue #603 closes the examples edge coverage ledger by adding local
PDF/A/signature coverage where practical and documenting the remaining
external-service adaptations.
