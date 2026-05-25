# `.csproj` package/folder structure mapping (Java -> .NET)

This document captures the project split decision using upstream published Maven artifacts and Java package/folder layout.

## Current project mapping (implemented)

| Java Maven artifact | Java package/folder scope (source) | .NET project | .NET source folder owner |
| --- | --- | --- | --- |
| `org.apache.pdfbox:pdfbox-io` | `io/src/main/java/org/apache/pdfbox/io/**` | `src/PdfBox.Net.IO/PdfBox.Net.IO.csproj` | `src/PdfBox.Net.IO/IO/**` |
| `org.apache.pdfbox:fontbox` | `fontbox/src/main/java/org/apache/fontbox/**` | `src/PdfBox.Net.FontBox/PdfBox.Net.FontBox.csproj` | `src/PdfBox.Net.FontBox/FontBox/**` |
| `org.apache.pdfbox:xmpbox` | `xmpbox/src/main/java/org/apache/xmpbox/**` | `src/PdfBox.Net.XmpBox/PdfBox.Net.XmpBox.csproj` | `src/PdfBox.Net.XmpBox/XmpBox/**` |
| `org.apache.pdfbox:pdfbox` | `pdfbox/src/main/java/org/apache/pdfbox/**` (except IO artifact scope) | `src/PdfBox.Net/PdfBox.Net.csproj` | `src/PdfBox.Net/**` |

Dependency direction:

- `PdfBox.Net` -> `PdfBox.Net.IO`
- `PdfBox.Net.FontBox` -> `PdfBox.Net.IO`
- `PdfBox.Net` -> `PdfBox.Net.FontBox`

## Recommended next project mapping (when ported)

| Java Maven artifact | Java package/folder scope (source) | Recommended .NET project |
| --- | --- | --- |
| `org.apache.pdfbox:fontbox` | `fontbox/src/main/java/org/apache/fontbox/**` | `src/PdfBox.Net.FontBox/PdfBox.Net.FontBox.csproj` |
| `org.apache.pdfbox:preflight` | `preflight/src/main/java/org/apache/pdfbox/preflight/**` | `src/PdfBox.Net.Preflight/PdfBox.Net.Preflight.csproj` |
| `org.apache.pdfbox:pdfbox-tools` | `tools/src/main/java/org/apache/pdfbox/tools/**` | `src/PdfBox.Net.Tools/PdfBox.Net.Tools.csproj` (optional tooling layer) |

Expected dependency direction (matching upstream artifacts):

- `PdfBox.Net.FontBox` -> `PdfBox.Net.IO`
- `PdfBox.Net` -> `PdfBox.Net.IO` + `PdfBox.Net.FontBox`
- `PdfBox.Net.Preflight` -> `PdfBox.Net` + `PdfBox.Net.XmpBox`

## Package-level note

`package.html` folders are useful for package inventory and optional future internal sub-splits, but not for one-project-per-package assembly boundaries.
