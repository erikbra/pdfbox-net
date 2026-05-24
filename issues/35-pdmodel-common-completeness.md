### Title
Complete PDModel/Common layer (PDNameTreeNode, PDNumberTreeNode, PDDestination, etc.)

### Background
`org.apache.pdfbox.pdmodel.common` is only ~20% ported: `PDRectangle`, `PDStream`, and
`PDMetadata` are complete. The remaining ~12 classes are needed for document navigation,
embedded files, page labels, and function-based color spaces.

### Depends on
- COS layer (complete)
- Issue #31 (PDF loading) — tree nodes require resolving objects from a loaded document

### Scope

Port the following Java files:

#### Tree structures (~2 files, needed by interactive layer)
- `PDNameTreeNode.java` — name-keyed B-tree node used for named destinations, embedded
  files, JavaScript actions. Required by issue #32 (interactive layer).
- `PDNumberTreeNode.java` — number-keyed B-tree node used for page labels.

#### Navigation / file references (~3 files)
- `PDDestination.java` — abstract destination base (used by links/bookmarks)
- `PDFileSpecification.java` — file attachment reference (embedded files)
- `PDTextStream.java` — text-type stream wrapper

#### Page organization (~2 files)
- `PDPageLabels.java` — maps page index ranges to label formats (e.g., "i", "ii", "1", "A-1")
- `PDRange.java` — numeric range descriptor (used by function and color space types)

#### Function subtypes (~4 files, needed by color space / shading)
- `PDFunctionType0.java` — Type 0 Sampled function (lookup table)
- `PDFunctionType2.java` — Type 2 Exponential interpolation
- `PDFunctionType3.java` — Type 3 Stitching (combine multiple functions)
- `PDFunctionType4.java` — Type 4 PostScript calculator

### Expected test scope
- Create a PDF with named destinations and verify PDNameTreeNode lookup.
- Parse a PDF with page labels and verify correct label strings.
- Evaluate a Type 2 function with known input/output pairs.

### Entry criteria
- `dotnet build` passes.
- Issue #31 (PDF loading) functional.

### Exit criteria
- PDNameTreeNode and PDNumberTreeNode can be constructed from a loaded document.
- PDPageLabels returns correct label strings.
- PDFunction subtypes produce correct output for test input vectors.
- Provenance headers and conversion records updated.

### Risk register
- Tree node traversal requires resolving COSObject references from the live document.
- PDF function specification (especially Type 4 PostScript) is complex; Type 4 may be
  deferred to a follow-up.

### Definition of done
- `dotnet build` passes.
- Targeted unit tests pass.
- `reports/conversion-records.json` updated.
