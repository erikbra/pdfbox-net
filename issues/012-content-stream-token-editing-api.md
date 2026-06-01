### Title
Implement content stream token-level editing API for operator-level mutation

### Summary

The Java PDFBox exposes a public API for parsing a page content stream into a list of operator/operand tokens (`PDFStreamParser`), mutating that list (e.g., removing all text operators), and writing the modified token sequence back as a new content stream. No equivalent public API exists in the .NET port.

### Required capabilities

1. **Parse a content stream into tokens** — `PDFStreamParser` already partially exists but is not publicly accessible for building editable token lists.
2. **Iterate and filter operators** — access the sequence of `Operator` and `COSBase` operand tokens.
3. **Write a token sequence back to a stream** — convert a mutated `List<object>` (operators + operands) to bytes and set as the page's content stream.

### Affected example files (currently stubs)

- `Util/RemoveAllText.cs`

### Upstream Java reference

`pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFStreamParser.java`
`pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/Operator.java`

### Acceptance criteria

- A public API (e.g., `PDFStreamParser.ParseTokens(Stream)`) exists and returns a `List<object>` of `Operator` / `COSBase` tokens from a content stream.
- A complementary write-back API exists to turn a mutated token list back into a content stream byte sequence.
- `Util/RemoveAllText.cs` is upgraded from `PORT_MODE: adapted` to `PORT_MODE: mechanical`.
