### Title
Port `pdmodel/common`, `pdmodel/graphics` support classes, and extended graphics state

### Depends on
- #22 PDModel color spaces
- #19 filter implementations
- COS and pdmodel baseline (already complete)

### Background
Several important PDModel layers are either absent or only stubbed:

1. **`pdmodel/common`**: Only `PDRectangle` is ported. The rest (PDStream, PDMetadata, tree nodes,
   PDDestination, function types, etc.) are absent or stubbed.

2. **`pdmodel/graphics`** (XObjects and advanced rendering support): `PDFormXObject`,
   `PDTransparencyGroup`, `PDShading*`, `PDExtendedGraphicsState`, `PDSoftMask`, and
   `PDLineDashPattern` are all stubs with no real logic.

Without these:
- XObject form execution (used by many PDFs for repeated graphics) does not work
- Transparency / soft mask rendering is absent
- Extended graphics states (blending modes, alpha, stroke width, etc.) have no effect
- PDF tree structures (name trees, number trees) cannot be traversed
- Function-based shading and color conversions cannot operate

### Scope

**`pdmodel/common`** (~14 classes):
- `PDStream.java` — COSStream wrapper for pdmodel (encode/decode via filter layer)
- `PDMetadata.java` — XMP metadata stream access
- `PDNameTreeNode.java` — generic B-tree name-keyed node
- `PDNumberTreeNode.java` — generic B-tree number-keyed node
- `PDDestination.java` — abstract navigation destination
- `PDNamedDestination.java` — named destination reference
- `PDPageXYZDestination.java` / `PDPageFitDestination.java` / `PDPageFitRectangleDestination.java`
- `PDFileSpecification.java` — file attachment reference
- `PDRange.java` — numeric range [min, max]
- `PDPageLabels.java` — page label range map
- `PDTextStream.java` — text-type stream (string or stream)
- `function/PDFunction.java` (abstract, replaces stub) + `PDFunctionType0-4` (~5 classes)

**`pdmodel/graphics` real implementations** (~20 classes):
- `PDExtendedGraphicsState.java` — all extended graphics parameters (alpha, blend mode,
  stroke adjustment, etc.)
- `PDSoftMask.java` — soft mask (luminosity, alpha) — replaces stub
- `PDLineDashPattern.java` — dash array + phase — replaces stub
- `PDFormXObject.java` — real form XObject (content stream + resources + BBox)
- `PDTransparencyGroup.java` — transparency group attributes — replaces stub
- `PDImageXObject.java` — image XObject (decoding, ICC profile, color space)
- `PDInlineImage.java` — inline image (from BI/ID/EI operators)
- `shading/PDShading.java` (abstract) + PDShadingType1–7 (~8 classes)
- `PDPropertyList.java` — property list for marked content resources
- `PDOptionalContentGroup.java` — optional content group (Layer) — replaces stub
- `PDOptionalContentProperties.java` — OC properties dictionary — replaces stub

Also:
- Update `RenderingSupportStubs.cs` to remove all replaced stubs

### Expected test scope
- Add `tests/PdfBox.Net.Tests/PDStreamTest.cs` covering encode/decode via FlateFilter
- Add `tests/PdfBox.Net.Tests/PDExtendedGraphicsStateTest.cs` for alpha, blend mode, line join
- Add `tests/PdfBox.Net.Tests/PDImageXObjectTest.cs` for a simple image decode fixture
- Extend `ContentStreamEngineTest.cs` with a form XObject traversal test

### Entry criteria
- #19 filter implementations landed (PDStream decode uses FlateFilter)
- #22 color spaces landed (PDImageXObject uses PDColorSpace)
- `dotnet build` and `dotnet test` green

### Exit criteria
- `PDStream` decodes and encodes real filtered streams
- `PDFormXObject` exposes content stream + resources for processing
- `PDExtendedGraphicsState` populates PDGraphicsState with real values
- `PDShading` hierarchy compiles; execution-time behavior can remain partial
- `RenderingSupportStubs.cs` reduced to only genuinely deferred items
- Function types (PDFunctionType0–4) compile with basic `Eval` logic
- `reports/conversion-records.json` and traceability updated
- `dotnet build` and `dotnet test` remain green

### Risk register
- `PDFunctionType4.java` is a stack-based PostScript calculator; may be complex to port —
  defer to a stub-with-exception if time-constrained
- `PDImageXObject` decoding involves many filter + color-space paths; test only the simplest
  (uncompressed/FlateEncoded DeviceRGB) fixture initially
- Shading types 4–7 are Gouraud/Coons patches — very complex geometry; stub with TODOs
  for execution-time behavior and focus on compilation + constructor correctness

### PR slicing rule
- First PR: `pdmodel/common` tree nodes, `PDStream`, `PDMetadata`, `PDRange`, `PDDestination*`,
  `PDFileSpecification`, `PDPageLabels`, `PDTextStream`
- Second PR: `function/` package (PDFunction abstract + Types 0–4)
- Third PR: `PDExtendedGraphicsState` + `PDSoftMask` + `PDLineDashPattern` + `PDFormXObject` +
  `PDTransparencyGroup`
- Fourth PR: `PDImageXObject` + `PDInlineImage` + shading types 1–3
- Fifth PR: shading types 4–7 + `PDOptionalContent*` real implementations

### Definition of done
- `dotnet build` passes
- PDStream encode/decode roundtrip test passes
- PDExtendedGraphicsState accessor tests pass
- PDFormXObject exposes content stream and resource dictionary
- All stubs replaced or explicitly documented as deferred
- Provenance headers on all ported files
- Conversion and traceability records updated
- Size: ~34 files, estimated 3–5 engineer-days
