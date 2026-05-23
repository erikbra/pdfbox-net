# Suggested split of pdfbox-net into projects/packages (package.html assessment)

This assessment revisits the split using the provided `package.html` paths.

## 1) What `package.html` indicates in PDFBox

- `package.html` describes a **Java package namespace**, not a published Maven artifact by itself.
- The provided list includes many duplicates across:
  - `src/main/java` (source of truth for production)
  - `target/classes` (build output duplicate)
  - `src/test/java` and `target/test-classes` (test-only packages)
- Therefore, creating one `.csproj` for every listed `package.html` folder would over-split the codebase and include duplicates/test-output paths that should not define production assemblies.

## 2) Does one `.csproj` per `package.html` folder make sense?

**Short answer: generally no.**

For current ported code, package-level projects like:
- `org/apache/pdfbox/contentstream/operator`
- `org/apache/pdfbox/pdmodel/common`
- `org/apache/pdfbox/pdmodel/interactive/annotation`

are tightly coupled with sibling/root packages (`contentstream`, `cos`, `pdmodel`, `pdfparser`, etc). Splitting each package into its own assembly would create many small projects with heavy cross-references and high maintenance cost, with little packaging benefit.

## 3) What does make sense

Use `package.html` as a **boundary discovery tool**, but keep assembly boundaries at **published artifact level first**:

- Java `pdfbox-io` -> .NET `PdfBox.Net.IO`
- Java `pdfbox` -> .NET `PdfBox.Net`
- Java `fontbox` -> future `.NET` `PdfBox.Net.FontBox`
- Java `xmpbox` -> future `.NET` `PdfBox.Net.XmpBox`
- Java `preflight` -> future `.NET` `PdfBox.Net.Preflight`
- Java `pdfbox-tools` -> future `.NET` `PdfBox.Net.Tools`

Inside `pdfbox`, package groups from `package.html` can later guide optional sub-projecting (only when dependencies are stable enough), for example:
- COS/parser/writer cluster (`cos`, `pdfparser`, `pdfwriter`, `filter`)
- model cluster (`pdmodel/**`)
- content/text/rendering cluster (`contentstream/**`, `text`, `rendering`)

## 4) Physical layout alignment adjustment made

To better align with module boundaries and avoid linked-file indirection:

- IO sources were moved from:
  - `src/PdfBox.Net/IO/*`
- to:
  - `src/PdfBox.Net.IO/IO/*`

This keeps the source tree aligned with the dedicated `PdfBox.Net.IO` project while preserving namespaces (`PdfBox.Net.IO`) and behavior.

## 5) Current practical recommendation

1. Keep project boundaries at Maven artifact level (implemented for `pdfbox` + `pdfbox-io`).
2. Use `package.html` inventory to plan future **internal** split candidates, not an automatic one-project-per-package conversion.
3. Exclude `target/**` and `src/test/**` package paths from production project-boundary decisions.
