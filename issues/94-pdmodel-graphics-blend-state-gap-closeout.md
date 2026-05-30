### Title
Close remaining pdmodel graphics/blend/state mapping gaps (7 files)

### Depends on
- #93 pdmodel resource and content-stream foundation gap closeout

### Background
The final missing `pdfbox` mapping block is the pdmodel graphics set covering font setting, PostScript XObject, blend support, transparency attributes, and rendering enums.

### Scope
- Port remaining missing pdmodel graphics files:
  - `graphics/PDFontSetting`
  - `graphics/PDPostScriptXObject`
  - `graphics/blend/BlendComposite`
  - `graphics/blend/BlendMode`
  - `graphics/form/PDTransparencyGroupAttributes`
  - `graphics/state/RenderingIntent`
  - `graphics/state/RenderingMode`
- Verify compatibility with rendering/contentstream behavior.
- Update traceability/conversion/normalization rows for touched paths.

### Expected test scope
- Targeted graphics/rendering/contentstream tests.

### Exit criteria
- `missing_java_files_total == 0` for `pdfbox` mapping scope.
- Touched traceability rows are `in-sync`.

### Definition of done
- `dotnet build PdfBoxNet.slnx` passes.
- Targeted tests pass.
- Canonical reports are regenerated and checked in.
