### Title
Port `fontbox/afm/**` — Adobe Font Metrics parser and data model

### Depends on
- Initial fontbox port (this branch)

### Background
The `org.apache.fontbox.afm` package provides an AFM (Adobe Font Metrics) file parser and its
corresponding data model. AFM files describe per-character metrics (advance widths, bounding
boxes, ligatures, kern pairs) for Type1 fonts. This data is needed when PDFBox loads embedded
Type1 fonts from PDF files.

### Scope
Port all 8 files in `fontbox/src/main/java/org/apache/fontbox/afm/`:

| Java file | Purpose |
|---|---|
| `AFMParser.java` | AFM file format parser (reads `.afm` text format) |
| `FontMetrics.java` | Top-level AFM font metrics model |
| `CharMetric.java` | Per-character metrics: width, bounding box, ligatures, kern pairs |
| `KernPair.java` | Kerning pair entry (first/second glyph name + delta width/height) |
| `Ligature.java` | Ligature substitution entry (successor glyph + ligature glyph name) |
| `Composite.java` | Composite character entry |
| `CompositePart.java` | Part of a composite character (component name + dx/dy offsets) |
| `TrackKern.java` | Track kerning entry (degree, min/max pt size, min/max kern) |

Target paths:
- `src/PdfBox.Net.FontBox/FontBox/AFM/*.cs`

### Exit criteria
- All 8 AFM files are ported with provenance headers.
- `AFMParser` can parse a representative `.afm` file fixture.
- Tests verify round-trip metrics for at least one representative font.
- `dotnet test` remains green.
