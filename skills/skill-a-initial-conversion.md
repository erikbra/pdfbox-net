# Skill A - Initial mechanical conversion + provenance header

## Purpose
Create a first-pass C# file from an upstream PDFBox Java file and stamp required provenance metadata.

## Inputs
- Upstream Java file path
- Upstream commit SHA
- Target .NET module/path mapping

## Output
- New C# file with provenance header fields:
  - `PDFBOX_SOURCE_PATH`
  - `PDFBOX_SOURCE_COMMIT`
  - `PORT_MODE` (`mechanical` or `adapted`)
  - `PORT_LAST_SYNC_COMMIT`
- Apache 2.0 license header copied verbatim from upstream file (for mechanically ported files)
- Small separate two-line notice with:
  - copyright for C# port modifications/adaptations
  - statement that AI assistance was used in the conversion
- Documentation-style comments ported from JavaDoc where present

## Notes
- Default `PORT_MODE` should be `mechanical` for one-to-one conversion output.
- Set `PORT_MODE` to `adapted` only when behavior/API is intentionally changed from upstream mechanical parity.
- Keep the Apache license block verbatim and place it before provenance metadata.
- Keep the copyright + AI conversion notice separate from the license text.
- Prefer preserving upstream inline test data setup over refactoring/extracting helpers when doing mechanical test conversions.

## Required provenance header format
Place this block at the top of every converted C# file (after license header and AI conversion note):

```csharp
// PDFBOX_SOURCE_PATH: <upstream relative path>
// PDFBOX_SOURCE_COMMIT: <upstream commit sha>
// PORT_MODE: mechanical|adapted
// PORT_LAST_SYNC_COMMIT: <upstream commit sha>
```

## Required conversion record fields (per file)
- `source_path`
- `target_path`
- `source_commit`
- `port_mode`
- `sync_commit`
- `conversion_notes`
