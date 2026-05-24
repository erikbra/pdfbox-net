### Title
Port remaining `fontbox/ttf/**` tables and TrueType/OpenType parsing pipeline

### Depends on
- #6 `fontbox/util` follow-up
- #7 `fontbox/encoding` port
- #8 `fontbox/type1` + `fontbox/cff` port

### Current state
83/83 upstream `fontbox/ttf/**` files ported (100%).

All target slices are now present in `src/PdfBox.Net.FontBox/FontBox/TTF/**`, including
root tables, `Table/Common`, `Table/GSub`, `Model`, `GSub` worker pipeline, collection,
and subsetter components.

### Scope — recommended order

#### Slice 9a — Core table stack (prerequisite for glyph access)
Port these 14 root files first as they are required for character-to-glyph mapping and outline data:
- `CmapLookup.java` + `CmapSubtable.java` + `CmapTable.java` — cmap table (char → glyph)
- `HorizontalHeaderTable.java` — hhea table
- `HorizontalMetricsTable.java` — hmtx table (advance widths)
- `IndexToLocationTable.java` — loca table (glyph offsets)
- `GlyphDescription.java` + `GlyfDescript.java` + `GlyfSimpleDescript.java` + `GlyfCompositeDescript.java` + `GlyfCompositeComp.java` — glyph descriptions
- `GlyphData.java` — glyph outline data
- `GlyphTable.java` — glyf table
- `PostScriptTable.java` — post table (glyph names)

#### Slice 9b — Metrics and extended tables
- `OS2WindowsMetricsTable.java` — OS/2 table (line metrics, Unicode ranges)
- `KerningSubtable.java` + `KerningTable.java` — kern table
- `VerticalHeaderTable.java` + `VerticalMetricsTable.java` + `VerticalOriginTable.java` — vertical layout
- `GlyphRenderer.java` — shape-based glyph renderer
- `CFFTable.java` — CFF table embedded in OTF
- `WGL4Names.java` — Windows Glyph List 4 name table
- `SubstitutingCmapLookup.java` — GSUB-aware cmap lookup
- `GlyphSubstitutionTable.java` — GSUB table (top level)
- `OTLTable.java` + `OpenTypeScript.java` — OpenType layout base types

#### Slice 9c — OpenType layout common + GSUB tables
All 12 `ttf/table/common` files and all 9 `ttf/table/gsub` files (21 total).

#### Slice 9d — GSUB model + worker pipeline (complex script support)
All 5 `ttf/model` files and all 13 `ttf/gsub` worker files (18 total).

#### Slice 9e — Collection and subsetter
- `TrueTypeCollection.java` + `TTCDataStream.java` — .ttc multi-font container
- `TTFSubsetter.java` — font subsetter for PDF embedding
- `RandomAccessReadDataStream.java` + `RandomAccessReadUnbufferedDataStream.java`
- `DigitalSignatureTable.java` — DSIG table

### Exit criteria
- Core glyph-access pipeline (9a) compiles and fixture-driven tests pass. ✅
- Each subsequent slice passes targeted tests before moving on. ✅
- `dotnet test` remains green after each slice. ✅
