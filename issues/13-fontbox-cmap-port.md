### Title
Port `fontbox/cmap/**` — CMap parser and data model

### Depends on
- Initial fontbox port (this branch)
- #8 `fontbox/cff/**` (for CID-keyed font integration)

### Background
The `org.apache.fontbox.cmap` package provides a CMap (Character Map) file/stream parser and
its data model. CMaps are used by Type0 (composite) fonts and CIDFonts to map character codes
to CID values or Unicode code points. Without this package, PDF files using CJK or other
multi-byte composite fonts cannot be decoded correctly.

### Scope
Port all 5 files in `fontbox/src/main/java/org/apache/fontbox/cmap/`:

| Java file | Purpose |
|---|---|
| `CMap.java` | CMap data model: codespace ranges, char/range entries, CID mappings |
| `CMapParser.java` | CMap file/stream parser (PostScript-like syntax) |
| `CMapStrings.java` | Embedded CMap string constants (pre-defined CMap names) |
| `CodespaceRange.java` | A codespace range entry (start/end byte sequences and their length) |
| `CIDRange.java` | A CID range mapping: code range → starting CID |

Target paths:
- `src/PdfBox.Net.FontBox/FontBox/CMap/*.cs`

### Exit criteria
- All 5 CMap files are ported with provenance headers.
- `CMapParser` can parse a representative CMap stream fixture.
- Tests verify codespace and CID range lookup for at least one CMap fixture.
- `dotnet test` remains green.
