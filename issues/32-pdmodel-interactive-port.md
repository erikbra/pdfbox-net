### Title
Port PDModel interactive layer (outlines, annotations, actions, forms)

### Background
The `org.apache.pdfbox.pdmodel.interactive` package is almost entirely absent (~7% ported).
Only two empty stubs exist: `PDOutlineItem` and `PDThreadBead` (in `TextStubs.cs`).
Almost every real-world PDF uses at least one of: bookmarks (document outline), annotations
(links, highlights, comments), or form fields.

### Depends on
- Issue #31 (full PDF loading) — reading interactive features requires loading the doc first
- Issue #35 (PDModel/Common completeness) — PDDestination, PDNameTreeNode required
- COS layer (complete)

### Scope

Port the following Java package areas in dependency order:

#### Phase A — Document outline / bookmarks (~4 files)
- `PDDocumentOutline.java` — root outline dictionary
- `PDOutlineNode.java` — abstract tree node (parent of outline items)
- `PDOutlineItem.java` — real bookmark entry (title, destination, children, color, style)
- `PDOutlineTreeNode.java` (if separate from above)

#### Phase B — Navigation destinations (~3 files)
- `PDDestination.java` — abstract base (depends on PDCommon completeness)
- `PDNamedDestination.java` — name-keyed destination
- `PDPageFitDestination.java`, `PDPageFitHeightDestination.java`, `PDPageXYZDestination.java` etc.

#### Phase C — Actions (~8 files)
- `PDActionGoTo.java` — go-to named/explicit destination
- `PDActionURI.java` — open a URL
- `PDActionLaunch.java` — launch application
- `PDActionJavaScript.java` — execute JavaScript
- `PDActionNamed.java` — named action
- `PDActionRemoteGoTo.java` — link to another document
- `PDAction.java` — abstract base

#### Phase D — Annotations (~15 files, the largest slice)
- `PDAnnotation.java` — abstract annotation base
- `PDAnnotationLink.java` — clickable link annotation
- `PDAnnotationText.java` — text comment/note
- `PDAnnotationHighlight.java`, `PDAnnotationUnderline.java`, `PDAnnotationStrikeOut.java`,
  `PDAnnotationSquiggly.java` — text markup annotations
- `PDAnnotationSquare.java`, `PDAnnotationCircle.java` — shape annotations
- `PDAnnotationFreeText.java`, `PDAnnotationLine.java` — drawing annotations
- `PDAnnotationFileAttachment.java`, `PDAnnotationStamp.java`
- `PDAnnotationWidget.java` — form field widget

#### Phase E — Forms/AcroForm (~5 files, lower priority)
- `PDAcroForm.java`, `PDField.java`, `PDTextField.java`, `PDCheckBox.java`, etc.

### Expected test scope
- Open a PDF with bookmarks; traverse the outline tree and verify titles/page targets.
- Open a PDF with link annotations; verify URI and destination targets.
- Parse annotation rectangles and verify against known fixture values.

### Entry criteria
- Issue #31 (PDF loading) is functional enough to open fixture PDFs.

### Exit criteria
- Outline traversal works on a fixture PDF with bookmarks.
- Link and text annotations can be read from a fixture PDF.
- Provenance headers and conversion records updated.

### Risk register
- Annotation hierarchy has many subtypes; scope should be phased (A→B→C→D→E).
- Form fields depend on additional infrastructure (appearance streams, field values).
- Some annotations reference fonts/resources that require the font layer.

### Definition of done
- `dotnet build` passes.
- Outline and annotation tests pass.
- `reports/conversion-records.json` and `traceability-parity-report.json` updated.
