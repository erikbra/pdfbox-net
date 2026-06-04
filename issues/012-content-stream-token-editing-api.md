# Issue 012 — Content stream token-level editing API

## Summary

Implement a token-level content stream editing API so that examples that parse and modify the
raw PDF content stream (e.g., to remove all text) compile and work correctly.

## Required API surface

- `PDFStreamTokenizer` (or equivalent) — reads a content stream as a sequence of operands and
  operators
- `ContentStreamWriter` (or equivalent) — writes a sequence of operands and operators back to
  a content stream
- Ability to read, filter, and write back content stream tokens on a `PDPage`

This maps to the upstream pattern used in `RemoveAllText.java`:
```java
PDFStreamParser parser = new PDFStreamParser(page);
List<Object> tokens = parser.parse();
// ... filter tokens ...
PDStream updatedStream = new PDStream(doc);
ContentStreamWriter tokenWriter = new ContentStreamWriter(updatedStream.createOutputStream());
tokenWriter.writeTokenList(filteredTokens);
page.setContentStreams(Collections.singletonList(updatedStream));
```

## Affected example files

- `Util/RemoveAllText.cs`

## Acceptance criteria

- `RemoveAllText` compiles without stubs.
- When run against a sample PDF, it produces a PDF with all text operators removed while
  other content (images, paths) is preserved.
- Integration test verifies the output PDF exists and contains no text operators.
- Traceability row for the affected source path is `in-sync`.
