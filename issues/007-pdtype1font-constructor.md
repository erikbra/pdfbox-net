# Issue 007 — `PDType1Font(PDDocument, Stream)` constructor

## Summary

Implement the `PDType1Font` constructor that accepts a `PDDocument` and a `Stream` so that
examples which embed external Type 1 fonts (`.pfb` / `.afm`) can be ported without stubs.

## Required API surface

- `PDType1Font(PDDocument document, Stream pfbStream)` — loads and embeds a Type 1 font from
  a PFB stream, building the font dictionary and embedding the font program
- Optional: `PDType1Font(PDDocument document, Stream pfbStream, Encoding encoding)` — with
  explicit encoding

## Affected example files

- `PDModel/HelloWorldType1.cs`

## Acceptance criteria

- The constructor is implemented and produces a valid embedded Type 1 font in the PDF.
- `HelloWorldType1` compiles without stubs and produces a PDF with readable Type 1 text.
- Integration test verifies the output PDF exists and the font is embedded.
- Traceability row for the affected source path is `in-sync`.
