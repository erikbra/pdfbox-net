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
