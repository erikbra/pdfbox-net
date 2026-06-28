# FontBox CFF/TTF Compatibility Helper Review - 2026-06-28

Issue: #547

## Summary

Reviewed the remaining FontBox CFF/TTF API rows from `reports/api-surface-comparison.json`.
The only low-risk public helper gap closed in this pass is `CFFParser.toString()`, now mapped
to `CFFParser.ToString()` with the same last-parsed-font diagnostic shape as Apache PDFBox.

## Reduced Rows

| Family | Java type | Java member | .NET action |
|---|---|---|---|
| `fontbox:cff` | `org.apache.fontbox.cff.CFFParser` | `toString()` | Added `CFFParser.ToString()` and tests for the initial `CFFParser[null]` state and parsed-font diagnostic state. |

## Reaffirmed Adaptations

| Family | Java type | Members | Decision |
|---|---|---|---|
| `fontbox:cff` | `CharStringCommand` | `getInstance(...)`, `getValue()`, `getType1KeyWord()`, `getType2KeyWord()`, `toString()` | Keep as a C# enum with `CharStringCommandExtensions` for Java helper behavior. Rewriting the enum as a class-style Java enum would churn parser/rendering code and public type identity for little user value. The enum `ToString()` spelling remains the C# enum name; callers needing Java helper behavior use the extension helpers. |
| `fontbox:cff` | `Type1CharString` | protected sequence/interpreter helpers | Keep internal-by-design. These are renderer/interpreter hooks in Java and are not a stable external .NET extension point in the current port. |
| `fontbox:cff` | `Type1CharString` | `getBounds()`, `toString()` | Keep as accepted diagnostics/geometry adaptations. The port exposes the rendered `GeneralPath`; it does not expose Java AWT `Rectangle2D`. |
| `fontbox:ttf` | `GsubData`, `GlyphArraySplitter`, `GsubWorker`, `ScriptFeature` | interface/type-name rows | Keep the established .NET `I*` interface naming. The project has explicitly decided not to add parallel Java-named public interfaces. |
| `fontbox:ttf` | `Language` | `getScriptNames()` | Keep `LanguageExtensions.GetScriptNames()` rather than converting the enum to a class-style enum solely to host an instance method. |
| `fontbox:ttf` | `SubstitutingCmapLookup` | type and members | Keep non-public. Glyph substitution lookup is exposed through `TrueTypeFont` and `GlyphSubstitutionTable`; there is no current public extension use case for this implementation type. |

## Follow-Up

No additional public FontBox CFF/TTF helper wrappers are recommended unless a concrete user
porting scenario needs Java-shaped access to one of the reaffirmed internal or enum/interface
adaptations.
