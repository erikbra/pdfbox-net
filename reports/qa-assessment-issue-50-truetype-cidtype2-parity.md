# QA assessment — issue 50 TrueType + CIDFontType2 parity

## Upstream-test parity

### Production classes touched

| Class / area | Upstream parity | Note |
|---|---|---|
| `TrueTypeFont.GetWidth(string)` | Converted | Fixed UPM scaling (`advance * 1000 / unitsPerEm`) to match Java upstream. |
| `PDTrueTypeFont` | Converted | `ToUnicodeFallback` via TTF unicode cmap; width extraction via UPM-scaled TTF advance. |
| `PDCIDFontType2` | Converted | `CIDToGIDMap` support (Identity name + stream), `CodeToGID(int)`, TTF advance-width fallback in `GetWidth`, `ToUnicodeFallback` via reverse cmap lookup; removed incorrect unicode-cmap `CodeToCID` override. |
| `PDCIDFont` | Converted | `HasExplicitCidWidth` helper; `_defaultWidth` promoted to protected `DefaultWidth`. |
| `PDType0Font` | Converted | `HasGlyph` and `GetNormalizedPath` now use `PDCIDFontType2.CodeToGID` for correct CID→GID mapping. |

### Tests added (`PDTrueTypeFontAndCIDType2Test.cs`, 24 tests)

- `TrueTypeFont_GetWidth_Returns500ForGlyph1_With1000Upm` — baseline scaling at 1000 UPM.
- `TrueTypeFont_GetWidth_ScalesCorrectlyFor2000Upm` — 500 design units at 2000 UPM → 250.
- `TrueTypeFont_GetWidth_ScalesCorrectlyFor2048Upm` — typical CJK UPM scaling.
- `PDTrueTypeFont_GetWidth_FallsBackToTTFAdvanceWidth_Via_UnicodeGlyphName` — TTF path when no explicit Widths.
- `PDTrueTypeFont_GetWidth_PrefersExplicitDictionaryWidths_OverTTFAdvance` — explicit Widths wins.
- `PDTrueTypeFont_ToUnicode_ReturnsMappedCharWhenCmapHasGlyph` — cmap maps U+0041 → "A".
- `PDTrueTypeFont_ToUnicode_ReturnsNullForCodeWithNoGlyph` — code 0x01 → null.
- `PDCIDFontType2_CodeToGID_IdentityMapping_ReturnsCidAsGid` — no CIDToGIDMap.
- `PDCIDFontType2_CodeToGID_ExplicitIdentityName_ReturnsCidAsGid` — `/Identity` name.
- `PDCIDFontType2_CodeToGID_StreamMapping_ReturnsCorrectGid` — stream-based map.
- `PDCIDFontType2_CodeToCID_IsIdentity` — CID = code (base identity).
- `PDCIDFontType2_GetWidth_PrefersExplicitWArrayEntry` — W array takes precedence.
- `PDCIDFontType2_GetWidth_FallsBackToTTFAdvanceWidth_WhenNoWArrayEntry` — TTF fallback.
- `PDCIDFontType2_GetWidth_ScalesTTFAdvanceByUpm` — 2000-UPM scaling in CID context.
- `PDCIDFontType2_GetWidth_WithStreamCIDToGIDMap_UsesCorrectGid` — GID from map used for advance lookup.
- `PDCIDFontType2_ToUnicode_FallsBackToReverseCmapLookup` — CID 1 → GID 1 → "A".
- `PDCIDFontType2_ToUnicode_ReturnsNullForGidWithNoUnicodeMapping` — GID 0 → null.
- `PDType0Font_WithCIDFontType2_GetWidth_UsesDescendantTTFWidths` — Type0 TTF width.
- `PDType0Font_WithCIDFontType2_HasGlyph_UsesCodeToGID` — HasGlyph via correct GID.
- `PDType0Font_WithCIDFontType2_HasGlyph_WithStreamCIDToGIDMap` — HasGlyph via stream map.
- `PDType0Font_WithCIDFontType2_GetNormalizedPath_ReturnsPathViaCodeToGID` — path via GID.
- `PDType0Font_WithCIDFontType2_ExplicitWArrayWidth_TakesPrecedenceOverTTF` — W array wins in Type0.
- `PDFontFactory_CreateFont_DispatchesToPDCIDFontType2_ForCIDFontType2Subtype` — factory dispatch.
- `PDFontFactory_CreateFont_DispatchesToPDTrueTypeFont_WhenFontFile2Present` — embedded TTF loads.

### Deferred

- Vertical metrics paths (`VerticalHeaderTable`, `vmtx`) — data structures are ported; the
  `IsVertical()` check is in place, but full vertical advance-width extraction is not exercised
  with a fixture that has vertical tables. Tracked for future vertical CID work.
- Width fallback via `OS/2` table `xAvgCharWidth` — not implemented; upstream Java also treats
  this as informational only in the basic extraction path.
- `CIDToGIDMap` streams larger than 64 KiB entries — no fixture; the decoding logic is identical
  for all sizes.

## Source-to-port similarity confidence

- **`TrueTypeFont.GetWidth`**: direct one-line fix mirroring Java `if (unitsPerEM != 1000)` branch.
- **`PDCIDFontType2`**: adapted — Java has more complex `codeToGID` wiring and explicit
  `getWidthFromFont` override; C# collapses this into `CodeToGID` + `HasExplicitCidWidth` guard,
  which is semantically equivalent for the baseline extraction scenarios covered.
- **`PDType0Font`**: mechanical fix replacing raw `CodeToCID` GID-pass-through with explicit
  `CodeToGID` call; logic unchanged.
- **Test fixtures** (`FontBoxTestFixtures`): `CreateMinimalTrueTypeWithUpm` is a straight
  parameterisation of the existing builder; all byte layouts are identical to the 1000-UPM fixture.

## Report-row gaps

- Added conversion-records rows for:
  - `TrueTypeFont.java` (update — UPM scaling fix)
  - `PDTrueTypeFont.java` (update — ToUnicode + width parity)
  - `PDCIDFontType2.java` (update — CIDToGIDMap + width fallback)
  - `PDCIDFont.java` (update — helper addition)
  - `PDType0Font.java` (update — HasGlyph/GetNormalizedPath fix)
  - `PDTrueTypeFontAndCIDType2Test.cs` (new test file)
- Updated traceability rows:
  - `fontbox/.../TrueTypeFont.java` → `in-sync` (was `partial`)
  - `pdfbox/.../PDCIDFontType2.java` → `in-sync` (note updated)
  - `pdfbox/.../PDTrueTypeFont.java` → `in-sync` (note updated)
  - Added `PDTrueTypeFontAndCIDType2Test.cs` entry
  - Added `FontBoxTestFixtures.cs` update entry
