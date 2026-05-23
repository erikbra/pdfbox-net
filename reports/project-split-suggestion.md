# Suggested split of pdfbox-net into multiple projects/packages

Apache PDFBox is published as multiple Maven artifacts (for example `pdfbox`, `pdfbox-io`, `fontbox`, `xmpbox`, `preflight`, and `pdfbox-tools`), rather than a single binary.

From the converted sources currently in this repository, Java provenance headers show two upstream published artifacts are already represented:

- `io/src/main/java/...` -> upstream artifact: `pdfbox-io`
- `pdfbox/src/main/java/...` -> upstream artifact: `pdfbox`

## Suggested .NET project/NuGet split

### Implemented in this change

- `src/PdfBox.Net.IO/PdfBox.Net.IO.csproj`
  - Maps to Java `pdfbox-io`
  - Contains the converted `IO` classes (included from `src/PdfBox.Net/IO`)
  - Suggested NuGet package: `PdfBox.Net.IO`

- `src/PdfBox.Net/PdfBox.Net.csproj`
  - Maps to Java `pdfbox`
  - Excludes local `IO` sources and references `PdfBox.Net.IO`
  - Suggested NuGet package: `PdfBox.Net`

### Suggested future projects (when those source areas are ported)

- `PdfBox.Net.FontBox` (maps to Java `fontbox`)
- `PdfBox.Net.XmpBox` (maps to Java `xmpbox`)
- `PdfBox.Net.Preflight` (maps to Java `preflight`)
- `PdfBox.Net.Tools` (maps to Java `pdfbox-tools`)

This keeps the .NET packaging aligned with the Java upstream module boundaries while allowing incremental porting.
