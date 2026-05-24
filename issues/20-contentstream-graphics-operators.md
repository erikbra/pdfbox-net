### Title
Port all missing `contentstream/operator` graphics, color, path, and state processors

### Depends on
- #19 filter implementations (FlateFilter needed to decode real content streams for testing)
- ContentStream execution core (PDFStreamEngine already ported)
- PDModel font/color-space work in progress (stubs acceptable for initial compile)

### Background
The content stream operator layer is ~32% complete. Only text, basic graphics state, marked
content, and DrawObject operators are ported. The full PDF operator set covers path construction,
path painting, color space selection, color setting, inline images, shading fills, Type 3 font
glyph definition, and compatibility sections.

Without these operators:
- PDF pages with any vector graphics, images, or color cannot be fully processed
- The rendering backend (PageDrawer) cannot paint any non-text content
- Text extraction that depends on accurate text-matrix/clipping context is incomplete

All operator names are already enumerated in `OperatorName.cs`; only the processor classes are missing.

### Scope
Port the following operator classes from
`pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/`:

**Color operators** (~12 classes in `color/` subpackage):
- `SetNonStrokingColor.java` — sc
- `SetNonStrokingColorN.java` — scn
- `SetNonStrokingColorSpace.java` — cs
- `SetNonStrokingDeviceCMYKColor.java` — k
- `SetNonStrokingDeviceGrayColor.java` — g
- `SetNonStrokingDeviceRGBColor.java` — rg
- `SetStrokingColor.java` — SC
- `SetStrokingColorN.java` — SCN
- `SetStrokingColorSpace.java` — CS
- `SetStrokingDeviceCMYKColor.java` — K
- `SetStrokingDeviceGrayColor.java` — G
- `SetStrokingDeviceRGBColor.java` — RG

**Path construction operators** (~7 classes in `graphics/` subpackage):
- `LineTo.java` — l
- `MoveTo.java` — m
- `CurveTo.java` — c
- `CurveToReplicateFinalPoint.java` — y
- `CurveToReplicateInitialPoint.java` — v
- `AppendRectangleToPath.java` — re
- `ClosePath.java` — h

**Path painting operators** (~9 classes in `graphics/` subpackage):
- `StrokePath.java` — S
- `CloseAndStrokePath.java` — s
- `FillNonZeroRule.java` — f / F
- `FillEvenOddRule.java` — f*
- `FillNonZeroAndStrokePath.java` — B
- `FillEvenOddAndStrokePath.java` — B*
- `ClipNonZeroRule.java` — W
- `ClipEvenOddRule.java` — W*
- `EndPath.java` — n

**Remaining state operators** (~7 classes in `state/` subpackage):
- `SetLineWidth.java` — w
- `SetLineCap.java` — J
- `SetLineJoin.java` — j
- `SetMiterLimit.java` — M
- `SetLineDashPattern.java` — d
- `SetFlatness.java` — i
- `SetRenderingIntent.java` — ri

**Inline image operators** (~3 classes in `graphics/` subpackage):
- `BeginInlineImage.java` — BI
- `BeginInlineImageData.java` — ID
- `EndInlineImage.java` — EI

**Other operators** (~4 classes):
- `ShadingFill.java` — sh
- `SetType3GlyphWidth.java` — d0
- `SetType3GlyphWidthAndBoundingBox.java` — d1
- `BeginCompatibilitySection.java` — BX
- `EndCompatibilitySection.java` — EX

Also register all newly ported operators in `PDFStreamEngine.cs` dispatch table.

### Expected test scope
- Add operator processor tests in `tests/PdfBox.Net.Tests/OperatorProcessorsTest.cs` (extend existing)
- Test each operator family's state mutations (color, path, clipping stack)
- Add a multi-operator content stream fixture test covering a basic path + text scenario

### Entry criteria
- `dotnet build` and `dotnet test` green
- PDFStreamEngine operator dispatch mechanism is functional (already ported)
- Color-space stub types are available (already in RenderingSupportStubs.cs for compilation)

### Exit criteria
- All ~41 operator processor classes are ported and registered in PDFStreamEngine
- Operator state mutations (color, matrix, path stack, clipping) are covered by tests
- `reports/conversion-records.json` and traceability report updated
- `dotnet build` and `dotnet test` remain green

### Risk register
- Color operators require PDColorSpace implementations to be useful; use stubs/TODOs for color-space
  resolution until #22 lands (operators should compile and register without full color-space logic)
- Inline image operators must integrate with FilterFactory; defer decoding if #19 is not yet merged
- Type 3 operators depend on PDType3Font which is not yet ported (#21); use a TODO stub call
- Clipping stack semantics differ from Java AWT; track as adaptation note for later rendering work

### PR slicing rule
- First PR: all state operators (w, J, j, M, d, i, ri) — low risk, no cross-dependencies
- Second PR: all path construction operators (m, l, c, v, y, re, h, n)
- Third PR: all path painting operators (S, s, f, f*, B, B*, W, W*, n)
- Fourth PR: all color operators (sc, scn, cs, rg, g, k, SC, SCN, CS, RG, G, K)
- Fifth PR: inline image + shading + Type3 + compatibility operators

### Definition of done
- `dotnet build` passes
- All operator processor classes registered in PDFStreamEngine
- Tests validate state mutation for each operator family
- Provenance headers on all ported files
- Conversion and traceability records updated
- Size: ~41 files, estimated 2–4 engineer-days
