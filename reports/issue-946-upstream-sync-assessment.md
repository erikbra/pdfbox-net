# Issue 946 Upstream Sync Assessment

## Scope

- Apache PDFBox branch: `trunk`
- Previous tracked commit: `fee11b453d66725c2b3a28b6f862a8dc24d33177`
- Assessed through commit: `bf37c60dfa43cb9fb21497b44a667d091d809084`
- Upstream commits reviewed: 3

## Applicability

- `a8875ceeeecac555efc768791e1a9441ff76940d` changes only
  `pdfbox-layout-fop`, which is an explicitly excluded Java-only module. No
  .NET production change is applicable.
- `7a7be017c8da1bd99ddad324bf5dae86eedbb670` adds
  `OperatorNameTest.testUnkownOperator`. It is converted literally as the
  dedicated xUnit test `TestUnkownOperator`, using `"UNKNOWN"` and
  `ArgumentException`, while the existing generalized cached-name assertions
  remain intact.
- `bf37c60dfa43cb9fb21497b44a667d091d809084` fixes PDFBOX-6175. The port now
  passes the document resource cache from `PDResources` through `PDFontFactory`
  and `PDType0Font` into descendant CID fonts, reuses and inserts indirect
  descendant `PDFontDescriptor` instances, caches indirect descendant fonts,
  and evicts both descendants and their descriptors when page resources are
  removed.

## Sync log

| Source path | Target path | Previous sync commit | New sync commit | Conflict type | Result status | Local regions | Sync note |
|---|---|---|---|---|---|---:|---|
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPage.java` | `src/PdfBox.Net/PDModel/PDPage.cs` | `ccd281cfecedcc0ad39709bece5e67b19a54e8db` | `bf37c60dfa43cb9fb21497b44a667d091d809084` | none | in-sync | 0 | Ported Type 0 descendant-font and descendant-descriptor eviction; retained recursive form-resource cleanup and the upstream direct-page-resource boundary. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFont.java` | `src/PdfBox.Net/PDModel/Font/PDCIDFont.cs` | `fee11b453d66725c2b3a28b6f862a8dc24d33177` | `bf37c60dfa43cb9fb21497b44a667d091d809084` | semantic-divergence | in-sync | 0 | The .NET CID font inherits `PDFont`; an override backed by an eagerly resolved readonly descriptor preserves upstream caching semantics without changing the existing hierarchy. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0.java` | `src/PdfBox.Net/PDModel/Font/PDCIDFontType0.cs` | `fee11b453d66725c2b3a28b6f862a8dc24d33177` | `bf37c60dfa43cb9fb21497b44a667d091d809084` | none | in-sync | 0 | Added the cache-aware constructor and retained the existing one-argument compatibility overload. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType2.java` | `src/PdfBox.Net/PDModel/Font/PDCIDFontType2.cs` | `fee11b453d66725c2b3a28b6f862a8dc24d33177` | `bf37c60dfa43cb9fb21497b44a667d091d809084` | semantic-divergence | in-sync | 0 | Added the upstream three-argument cache-aware constructor. The existing optional `TrueTypeFont` constructor remains so a two-argument null call does not become ambiguous with a cache overload. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType2Embedder.java` | `src/PdfBox.Net/PDModel/Font/PDCIDFontType2Embedder.cs` | `fee11b453d66725c2b3a28b6f862a8dc24d33177` | `bf37c60dfa43cb9fb21497b44a667d091d809084` | semantic-divergence | in-sync | 0 | The adapted embedder does not contain upstream `getCIDFont()`; its equivalent embedded construction remains in `PDType0Font` and uses the compatibility constructor with no resource cache. No absent architecture was fabricated. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDFontFactory.java` | `src/PdfBox.Net/PDModel/Font/PDFontFactory.cs` | `fee11b453d66725c2b3a28b6f862a8dc24d33177` | `bf37c60dfa43cb9fb21497b44a667d091d809084` | semantic-divergence | in-sync | 0 | Added cache-aware factory and descendant-factory paths while retaining one-argument public/internal compatibility overloads and the port's existing direct-CID dispatch behavior. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType0Font.java` | `src/PdfBox.Net/PDModel/Font/PDType0Font.cs` | `fee11b453d66725c2b3a28b6f862a8dc24d33177` | `bf37c60dfa43cb9fb21497b44a667d091d809084` | none | in-sync | 0 | Reuses cached indirect descendant fonts and passes the resource cache into new descendants before caching them. |
| `pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDResources.java` | `src/PdfBox.Net/PDModel/Resources/PDResources.cs` | `aba442860ed4f9f99f9e52e78e34bb23570c2390` | `aba442860ed4f9f99f9e52e78e34bb23570c2390` | semantic-divergence | in-sync | 0 | Upstream did not change in this range; the existing upstream cache-aware factory call was added to the adapted .NET resource lookup so PDFBOX-6175 is reachable. |
| `pdfbox/src/test/java/org/apache/pdfbox/pdfwriter/OperatorNameTest.java` | `tests/PdfBox.Net.Tests/ContentStreamWriterTest.cs` | `fee11b453d66725c2b3a28b6f862a8dc24d33177` | `bf37c60dfa43cb9fb21497b44a667d091d809084` | none | in-sync | 0 | Added the new upstream test literally and preserved the broader cached-operator-name test. |

## Upstream-test parity

| Upstream class or behavior | Status | Evidence |
|---|---|---|
| `OperatorNameTest.testUnkownOperator` | converted | `ContentStreamWriterTest.TestUnkownOperator` is a literal JUnit-to-xUnit port. |
| Type 0 descendant descriptor cache reuse | converted | `PDResources_Type0DescendantsReuseCachedFontDescriptor` loads distinct descendants sharing one indirect descriptor and asserts reference reuse plus one descriptor insertion. |
| Type 0 descendant and descriptor eviction | converted | `PDPage_RemovePageResourceFromCacheEvictsType0DescendantAndDescriptor` verifies parent font, descendant CID font, and descriptor eviction. |

No production regression test changed upstream for PDFBOX-6175. The two native
tests above provide deterministic coverage of the newly reachable cache behavior;
no upstream test is deferred or missing.

## Normalization and similarity

- Java overloads and nullable cache parameters were mapped directly except for
  `PDCIDFontType2`: publishing both two-argument reference-type overloads would
  make existing calls with a null literal ambiguous. The three-argument upstream
  constructor carries the cache semantics and the existing constructor remains
  source-compatible.
- The port's adapted `PDCIDFont`/`PDFont` inheritance required an override rather
  than the independent descriptor getter used by Java. Instance identity and
  cache insertion behavior are equivalent.
- `PDCIDFontType2Embedder` remains an explicitly adapted, smaller implementation;
  the changed upstream construction site has no corresponding local method.
- Report-row gaps: none for the changed production files or the new upstream
  test. Conversion, normalization, and traceability records identify their
  current targets and synchronization status.

## Validation

- Targeted cache/operator suite: 15 passed, 0 failed, 0 skipped.
- Release build: succeeded with 0 errors and one pre-existing BouncyCastle
  obsolete-API warning.
- Full Release solution suite after rerun: 1,533 passed, 0 failed, 14 skipped.
- The first full Release run had one unrelated failure in
  `COSPrimitivesTest.TestCOSNameCacheReleasesUnusedDynamicNames`, a
  garbage-collection-sensitive weak-cache test. The isolated rerun passed, and
  the complete solution rerun then passed.
- Canonical upstream inventory: 1,073 mapped / 1,073 in-scope Java production
  files, 0 missing, 0 non-`in-sync` traceability rows; tracked commit advanced
  to `bf37c60dfa43cb9fb21497b44a667d091d809084`.
- API-surface review and ratchet gate: passed with no unreviewed deltas.
- Runtime parity harness compile/help smoke: passed.
