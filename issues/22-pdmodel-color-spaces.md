### Title
Replace `PDModel` color-space stubs with full color-space implementations

### Depends on
- #19 filter implementations (ICCBased profiles may be Flate-compressed)
- #20 contentstream graphics operators (color operators need real PDColorSpace to set state)

### Background
The current `src/PdfBox.Net/PDModel/RenderingSupportStubs.cs` contains empty stub classes for:
- `PDColorSpace` (empty)
- `PDColor` (partial, always returns 0 for ToRGB)
- Color-space-aware patterns and soft masks

Without real color space implementations:
- All color operations in content streams produce placeholder/zero values
- Image extraction cannot convert pixel data to correct RGB/CMYK values
- ICC color profile handling is completely absent
- Device color spaces (RGB, CMYK, Gray) cannot produce correct outputs

### Scope
Port the following from `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/`:

**Abstract base and device color spaces** (highest priority):
- `PDColorSpace.java` — abstract base with factory method and profile handling
- `PDDeviceRGB.java` — device-independent RGB color space
- `PDDeviceGray.java` — device-independent grayscale color space
- `PDDeviceCMYK.java` — device-independent CMYK color space
- `PDColor.java` — real color value (replaces stub)

**Calibrated and ICC color spaces**:
- `PDCalGray.java` — CIE-calibrated gray
- `PDCalRGB.java` — CIE-calibrated RGB
- `PDLab.java` — CIE L\*a\*b\* color space
- `PDICCBased.java` — ICC profile-backed color space (needs ICC profile stream decode)

**Special/indexed color spaces**:
- `PDIndexed.java` — indexed/palettized color space
- `PDSeparation.java` — spot color separation
- `PDDeviceN.java` — multi-component device color space
- `PDPattern.java` — pattern color space (links to pattern resources)

**Color-related support classes**:
- `PDColorSpaceFactory.java` — constructs PDColorSpace from COSDictionary/COSName
- Color conversion helpers (if standalone Java classes exist)

Also update:
- `PDGraphicsState.cs` to hold real `PDColorSpace` instances (currently uses stubs)
- `RenderingSupportStubs.cs`: remove or replace all color-space-related stubs after real
  implementations land

For .NET adaptation:
- Use `System.Windows.Media.ColorContext` or `System.Drawing.Imaging.ColorMatrix` for ICC
  color profile handling where suitable; consider a managed ICC library if needed
- Aim for correct float-array color conversion methods matching Java's `toRGB()` behavior

### Expected test scope
- Add `tests/PdfBox.Net.Tests/ColorSpaceTest.cs` with:
  - PDDeviceRGB, PDDeviceGray, PDDeviceCMYK construction and toRGB conversion tests
  - PDColorSpaceFactory lookup by COSName and COSDictionary
  - PDColor value round-trip tests
  - PDICCBased decode test using a known ICC profile byte fixture

### Entry criteria
- #19 filter implementations landed (to decode ICC profile streams)
- `dotnet build` and `dotnet test` green

### Exit criteria
- All listed color-space classes are ported and replace the corresponding stubs
- `PDColorSpaceFactory` resolves all standard PDF color-space names
- `PDGraphicsState` uses real `PDColorSpace` instances
- `RenderingSupportStubs.cs` has no remaining color-space stubs
- Color conversion tests pass for RGB, Gray, CMYK
- `reports/conversion-records.json` and traceability updated
- `dotnet build` and `dotnet test` remain green

### Risk register
- ICC profile handling (PDICCBased) may require a managed ICC library; stub with
  `NotSupportedException` or use sRGB fallback if no suitable .NET library is found in scope
- PDSeparation and PDDeviceN require alternate-space functions (PDFunction types from
  `pdmodel/common/function/`); may depend on common function port in #23
- Color-space-dependent rendering is not tested until rendering backend is wired (#20+)

### PR slicing rule
- First PR: `PDColorSpace.java` base + `PDDeviceRGB` + `PDDeviceGray` + `PDDeviceCMYK` + `PDColor`
  + `PDColorSpaceFactory` (covers device color spaces — most common case)
- Second PR: `PDCalGray` + `PDCalRGB` + `PDLab` + `PDICCBased`
- Third PR: `PDIndexed` + `PDSeparation` + `PDDeviceN` + `PDPattern`

### Definition of done
- `dotnet build` passes
- Device color space conversion tests pass
- `PDColorSpaceFactory` resolves all standard color-space COSNames
- All color-space stubs in `RenderingSupportStubs.cs` removed/replaced
- Provenance headers on all ported files
- Conversion and traceability records updated
- Size: ~20 files, estimated 3–4 engineer-days
