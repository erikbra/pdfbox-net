# Issue 009 — `PDSeparation` / `PDDeviceN` color spaces

## Summary

Implement `PDSeparation` and `PDDeviceN` color spaces so that examples which use spot colors and
multi-channel color spaces compile and produce correct output.

## Required API surface

- `PDSeparation` color space:
  - Constructor `PDSeparation(string colorantName, PDColorSpace alternateColorSpace, PDFunction tintTransform)`
  - `ToRGB(float[] value)` — converts separation color to RGB via tint transform
- `PDDeviceN` color space:
  - Constructor with colorant names, alternate color space, and tint transform
  - `ToRGB(float[] value)` — converts DeviceN color to RGB
- Both must be registerable as page color space resources

## Affected example files

- `PDModel/CreateSeparationColorBox.cs`

## Acceptance criteria

- `PDSeparation` and `PDDeviceN` are fully functional color spaces.
- `CreateSeparationColorBox` compiles without stubs and produces a valid PDF with a separation
  color box.
- Integration test verifies the output PDF exists and is loadable.
- Traceability rows for all affected source paths are `in-sync`.
