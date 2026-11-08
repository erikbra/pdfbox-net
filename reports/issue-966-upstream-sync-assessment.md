# Issue 966 upstream synchronization assessment

## Scope and behavior

Apache PDFBox `trunk`, `a9ae066a942c122c9dded9bfa1b08dfae4078ba4` through
`5b8f299279455a437488cf47ea67139a85a3b75f`: **27 commits, 33 changed paths**.
The exact issue snapshot is fixed. No production Java files or binary fixtures were
added or deleted. The per-file log accounts for every changed upstream path.

- PDFBOX-6304: nonpositive predictor row lengths now throw `IOException`, including
  `/Columns 0`. The prior .NET column clamp silently accepted that value. Existing
  checked row arithmetic and row-size limits remain.
- PDFBOX-6303 / 6301: marked content retains its containing page/form StructParents
  key. Forms and transparency groups restore the enclosing value; annotation hooks
  reset to -1, and Type3 hooks use -1 then restore the enclosing value, as upstream.
  Required port prerequisites add nested form execution, the upstream depth limit of
  50, and narrow child-stream hooks that preserve graphics/resources/text state.
  Rendering peers call the direct base form hook to avoid virtual redispatch recursion.
  The exact octopdf original and twice-merged fixture now validate nested
  `(StructParent, MCID)` parent-tree entries, page versus form kids, MCR `/Stm`,
  owning page identity, parent-tree size, and every nested form key.
- PDFBOX-6170: public `PDDocument.AddFontForSubsetting` rejects unsupported fonts,
  deduplicates registration and runs subsetting before a full save. Required
  `PDFont` virtual hooks expose the existing unsupported/full-font default, while
  the page writer registers capable fonts and collects Unicode code points.
- XMP construction methods use upstream's corrected `Instantiate` spelling, with
  callers/tests updated and the protected `XMPSchema.InstantiateSimple` forwarding
  helper added. Adapted schema setters/parser already use typed XML-backed helpers.
- Text word separators compare the singleton by identity. Other changes remove
  Java checked exceptions, simplify Java local syntax, or refactor CID width-cache
  lookup without changing behavior; their C# equivalents were already applicable.
  Byte Buddy/enforcer updates are Java-only development tooling.

Small semantic changes and documentation closely follow upstream. The stream engine,
font lifecycle, parser and XMP schema remain existing adapted implementations. There
is no new external .NET dependency, no changed gate threshold and no new skip.

## Test parity and limits

| Changed production scope | Test parity |
|---|---|
| Predictor | New upstream nonpositive-row regression converted through public FlateFilter; PNG and zero-bit boundaries added. |
| PDMarkedContent / PDFMarkedContentExtractor / COSName | New upstream merger validation converted with exact octopdf; generated nested/group/reuse/cycle/failure and annotation/Type3 hook regressions added. |
| PDDocument / PDAbstractContentStream | Native rejected-font/dedup/save lifecycle and page-writer Unicode tests; no new upstream regression was supplied for the new method. |
| TypeMapping / XMPSchema / AbstractStructuredType / ResourceRefType | Native XmpTypeTest exercises all four corrected construction names; existing schema/structured-type/XML round-trip suites retained. |
| PDFAIdentificationSchema / PhotoshopSchema / XMPBasicSchema / XMPMediaManagementSchema / XMPRightsManagementSchema / DomXmpParser | Existing native schema/parser tests retained; upstream changes only rename internal helpers absent in these XML-backed adaptations. |
| PDFTextStripper / PDFTextStripperByArea | Existing text extraction/area and runtime suites retained; new singleton comparison preserves the intended separator behavior. |
| COSParser / XrefParser / PDEmbeddedFile / FDFAnnotation / PDCIDFont / PDFontFactory / PDType0Font / PDNamedDestination | Existing parser/model/font/destination suites retained; Java checked-exception and cache refactoring deltas require no new behavior test. |

Pre-existing limits remain explicit: concrete PDType0Font always embeds the full font
and still ignores its `embedSubset` option. The new registration contract is verified
with a subset-capable font double; actual Type0 subset output is not claimed. Writers
without document ownership still require explicit registration. Automatic annotation
appearance and Type3 glyph traversal in the extraction engine remain pre-existing
limitations; the new container hooks are tested directly. The old full Java merger
fixture suite, reflection-driven XMP SchemaTester and derived-type test matrix remain
deferred; this batch converts their changed behavior/API coverage, not the entire
historical test architecture. No broad completion claim replaces these limits.

Report-row gaps in this batch: the previously absent extractor production mapping and
new test-source associations are now recorded. The inventory retains **1,091 mapped
production paths, 0 missing**. Older unrelated metadata gaps remain visible in the
canonical gap report (208 sources without traceability rows, one without provenance).
Mapping coverage alone is not complete behavior or API parity.

## Validation

- Release solution build: **passed, 0 errors, 16 existing package/obsolete warnings**.
- Full .NET solution: **1,973 passed, 0 failed, 21 existing explicit skips**.
- Final focused issue966 regressions: **10 passed, 0 failed/skipped**; the final
  test-only tightening adds concrete failure type and all nested form-key checks.
- Upstream Java at the exact target: **879 passed, 0 failures/errors/skips**:
  PredictorTest (3), XmpBox suite (875), and the exact merger mapping test (1).
- API gate: **0 unreviewed deltas, 0 missing types**, 843 reviewed missing members,
  43 reviewed arity gaps and 8 reviewed type/visibility gaps; ratchet/dispositions
  unchanged. The existing XMP public method spelling changes follow upstream.
- Inventory and whitespace checks passed. Both CI Java reference pins advance to
  the exact snapshot; watcher configuration continues following `trunk`.
- Full Java-vs-.NET runtime parity is delegated to PR CI; it was not rerun locally.

Initial compile normalization and four test-setup failures were corrected before the
passing runs. The filter test initially lacked `/Filter`, so no DecodeParms were
applied; the failure-path form initially used an operand error intentionally swallowed
by the engine, so it now uses an invalid filter and expects `IOException`.
