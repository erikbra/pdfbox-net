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

## Notes
- Default `PORT_MODE` should be `mechanical` for one-to-one conversion output.
- Set `PORT_MODE` to `adapted` only when behavior/API is intentionally changed from upstream mechanical parity.

## Required provenance header format
Place this block at the top of every converted C# file (after license header if present):

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
