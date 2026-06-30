# PDFBox 3.0 CLI And App Parity

Issue #601 closes the Apache PDFBox 3.0 `pdfbox-app` gap for the
`release/3.0` branch by choosing a .NET global tool facade over the existing
ported tools surface.

## App Entry Point

The branch now includes a packable .NET tool project:

- Project: `apps/PdfBox.Net.Tools.App/PdfBox.Net.Tools.App.csproj`
- Package ID: `PdfBox.Net.Tools`
- Tool command: `pdfbox`
- Dispatcher: `PdfBox.Net.Tools.PDFBox.Run`

The app is intentionally thin. Command behavior lives in the mechanically ported
tools classes so future syncs can keep comparing those classes to the Apache
Java sources.

## Dispatcher Commands

| Apache PDFBox 3.0 command | PdfBox.Net command target | State |
|---|---|---|
| `debug` | explicit unsupported message pointing to the non-packable `pdfdebugger` inspection app and debugger UI adaptation report | Adapted |
| `decrypt` | `Decrypt.Run` | Implemented |
| `encrypt` | `Encrypt.Run` | Implemented |
| `decode` | `WriteDecodedDoc.Run` | Implemented |
| `export:images` | `ExtractImages.Run` | Implemented |
| `export:xmp` | `ExtractXMP.Run` | Implemented |
| `export:text` | `ExtractText.Run` | Implemented |
| `export:fdf` | `ExportFDF.Run` | Implemented |
| `export:xfdf` | `ExportXFDF.Run` | Implemented |
| `import:fdf` | `ImportFDF.Run` | Implemented |
| `import:xfdf` | `ImportXFDF.Run` | Implemented |
| `overlay` | `OverlayPDF.Run` | Implemented |
| `print` | `PrintPDF.Run` | Implemented with registered print backend; otherwise explicit platform/backend error |
| `render` | `PDFToImage.Run` | Implemented with registered rendering backend |
| `merge` | `PDFMerger.Run` | Implemented |
| `split` | `PDFSplit.Run` | Implemented |
| `fromimage` | `ImageToPDF.Run` | Implemented |
| `fromtext` | `TextToPDF.Run` | Implemented |
| `version` | `Version.GetVersion` | Implemented |
| `help` | `PDFBox.Run` usage/help text | Implemented |

Legacy .NET command names such as `texttopdf`, `imagetopdf`, `extracttext`,
`pdftoimage`, `pdfmerger`, `pdfsplit`, `overlaypdf`, `extractxmp`,
`extractimages`, `exportfdf`, `exportxfdf`, `importfdf`, and `importxfdf` remain
available as compatibility aliases.

## Core Options

The dispatcher now covers representative Apache 3.0 option shapes:

| Command | Supported option families |
|---|---|
| `decode` | positional input/output, `-i`, `--input`, `-o`, `--output`, `-password`, `-skipImages` |
| `export:text` | `-i`, `--input`, `-o`, `--output`, `-console`, `-password`, `-encoding`, `-startPage`, `-endPage`, `-html`, `-md`, `-sort`, `-ignoreBeads`, `-addFileName`, `-append` |
| `render` | `-i`, `--input`, `-password`, `-format`, `-prefix`, `-outputPrefix`, `-page`, `-startPage`, `-endPage`, `-dpi`, `-resolution`, `-color`, `-quality`, `-cropbox`, `-time`, `-subsampling` |
| `merge` | repeated or grouped `-i`/`--input`, `-o`/`--output` |
| `split` | `-i`, `--input`, `-password`, `-split`, `-startPage`, `-endPage`, `-outputPrefix` |
| `overlay` | `-i`, `--input`, `-o`, `--output`, `-default`, `-first`, `-last`, `-odd`, `-even`, `-useAllPages`, `-page`, `-position`, `-adjustRotation` |

Other previously implemented command classes keep their existing option support.

## Accepted Adaptations

- `debug` is not implemented inside the core `pdfbox` dispatcher because the
  Java command starts the Swing debugger UI. PdfBox.Net documents the debugger
  desktop UI as an accepted 3.0 adaptation in
  `reports/pdfbox-3.0-debugger-ui-parity.md` and returns a clear unsupported
  message from `pdfbox debug`; the non-packable `pdfdebugger` console app
  remains available for text inspection.
- `render` writes PNG and JPEG through the registered rendering backend. Other
  image formats fail explicitly instead of writing mismatched data.
- `decode -skipImages` is accepted for command-shape compatibility, but the
  current rewrite path delegates to the existing document save path.

## Validation

Issue #601 adds command-level tests for:

- `pdfbox help`
- missing subcommand usage behavior
- `fromtext`
- `export:text`
- `decode`
- `merge`
- `split`
- explicit `debug` adaptation
- explicit unsupported render format handling

Build, full tests, package smoke, and CI runtime parity remain the merge gates.
