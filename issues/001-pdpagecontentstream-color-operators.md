# Issue 001 — `PDPageContentStream` color operators

## Summary

Implement the full set of color-setting operators on `PDPageContentStream` so that examples that
draw with specific color spaces compile and run correctly.

## Required API surface

- `SetNonStrokingColor(float gray)` — DeviceGray non-stroking
- `SetStrokingColor(float gray)` — DeviceGray stroking
- `SetNonStrokingColor(float r, float g, float b)` — DeviceRGB non-stroking
- `SetStrokingColor(float r, float g, float b)` — DeviceRGB stroking
- `SetNonStrokingColor(float c, float m, float y, float k)` — DeviceCMYK non-stroking
- `SetStrokingColor(float c, float m, float y, float k)` — DeviceCMYK stroking
- `SetNonStrokingColor(PDColor color)` — generic color-space non-stroking
- `SetStrokingColor(PDColor color)` — generic color-space stroking
- `SetNonStrokingColorSpace(PDColorSpace)` / `SetStrokingColorSpace(PDColorSpace)` — explicit CS operators
- `SetNonStrokingColorWithPattern` / `SetStrokingColorWithPattern` — pattern color operators

## Affected example files

- `PDModel/ShowColorBoxes.cs`
- `PDModel/AddMessageToEachPage.cs`
- `Util/AddWatermarkText.cs`
- `Util/PDFHighlighter.cs`
- `Util/PrintTextColors.cs`

## Acceptance criteria

- All operators listed above are implemented and emit the correct PDF content-stream tokens.
- The five example files compile without stubs and produce valid PDF output.
- Unit tests or integration tests verify that the emitted tokens are correct.
- Traceability rows for all affected source paths are `in-sync`.
