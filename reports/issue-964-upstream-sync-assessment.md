# Issue 964 upstream synchronization assessment

## Scope

Apache PDFBox `trunk`, `bd21bd44f5dd050fa716c9bb033adcf725b7ebd3` through
`a9ae066a942c122c9dded9bfa1b08dfae4078ba4`: **46 commits, 16 changed paths**.
The snapshot is fixed to the issue's target; later upstream commits belong to the
next watcher batch. No new production Java class is added by this batch.

## Behavior and source similarity

- PDFBOX-6292 / 6293: substitute encoding no longer determines PDF space width;
  foreign Identity-ordered CFF substitutes use Unicode to find CIDFontType2 glyphs.
  The exact Noto CJK subset covers both Type0 and Type2 descendants. Three existing
  port prerequisites are repaired: the parent font must run its own space-width
  algorithm so its ToUnicode/hook are consulted, and explicit PDF `/DW` must take
  precedence over the substitute program's advance. CFF substitutes need their outline
  path and GID checks instead of a nonexistent glyf table; the existing normalizer
  applies UPEM scaling without mutating the cached CFF outline (tested at 2000 UPEM).
  The established TTF advance
  fallback when both `/W` and `/DW` are absent remains an adaptation.
- PDFBOX-6296 / 6300: merger uses the page-label model to flatten child number trees,
  seed page zero, and reset default numbering at document boundaries, including an
  appended document with no labels. PDPageLabels already had the new type guard;
  its constructor XML documentation now follows upstream wording.
- PDFBOX-6299 / 6295: destination validation occurs before the empty-source return;
  language and viewer preferences merge for untagged documents, preserving destination
  nonboolean values and OR-ing the six upstream booleans. Names are re-read after
  merge before removing bogus `/Names/IDTree`. The existing source Threads copy,
  per-instance field counter, and maximum version propagation already satisfy the
  other applicable deltas.
- PDFBOX-6301: page/annotation/form/image structure indexes are offset, with one visited
  set for the append operation so nested/shared/cyclic XObjects update once. The
  necessary existing parent-tree integration is added: retain array/dictionary
  values, merge numeric keys and root K, and retain role/ID entries. The exact
  octopdf fixture is merged twice and saved/reloaded. Generated resources additionally
  cover shared images/forms, cycles, absent NextKey fallback, identity of parent-tree
  Pg/Obj references, and unchanged source objects.
- PDFBOX-6283 / 6294: link destinations are resolved per split document for direct
  destinations and GoTo actions. Cross-output links lose their page; internal links
  point at the imported page. The existing local per-call variables, catch-all cleanup,
  and factored named-destination cloning already satisfy upstream reuse/cleanup/DRY
  changes. New tests cover IO and unchecked failure cleanup, reuse, and start past EOF.
- PDFBOX-6275: number-tree conversion errors name the requested value type and retain
  the cause.

The small font/error changes closely follow upstream control flow and documentation.
Merge/split remain existing adapted classes; new behavior uses their established
cloning and document ownership model. In particular PDFCloneUtility reuses each clone
by identity, so parent-tree cloning already resolves Pg/Obj to the same imported
objects; explicit reference traversal retains the upstream mapping role without
mutating source dictionaries. The port's convenience PDParentTreeNumberTreeNode only
accepts structure-element dictionaries, so the merger deliberately reads the raw tree
with the upstream PDParentTreeValue wrapper to preserve array entries as well.

## Test parity and existing limitations

| Changed production source | Upstream test parity |
|---|---|
| PDFMergerUtility | New label and exact PDFBOX-6301 mapping assertions converted in Issue964MergerTest; generated boundary/identity tests added. Existing general merger fixture-suite gaps remain deferred. |
| Splitter | All four new link/lifecycle/reuse/out-of-range tests converted in Issue964SplitterTest. Existing named-destination tests remain in Issue957CoreTest; basic split/range/rotation coverage remains in MultiPdfExtractionSplittingTest. |
| PDCIDFontType2 | Both exact IdentityCJKSubstituteTest cases converted, with the upstream Noto subset and original glyph IDs. |
| PDFont / PDType0Font | SubstituteSpaceWidthTest converted with a bundled deterministic substitute; explicit parent ToUnicode regression added. |
| PDNumberTreeNode | Existing TestPDNumberTreeNode suite plus new type-specific failure regression. |
| PDPageLabels | Existing PDPageLabelsTest suite and focused default/child-range merge regressions; source type guard was already present. |

The newly added upstream SplitterTest mainly relocates older tests from
PDFMergerUtilityTest. The older full structure-tree/ID-tree, incorrect Pg,
removed/popup/orphan annotation, broken-destination and external tagged fixtures
remain deferred under the pre-existing splitter adaptation. Their upstream changes
in this batch cache the source ID tree, avoid an empty ID tree and refactor existing
traversal; full tagged splitting is not claimed here. The new merger parent-tree
integration proves the new XObject mapping behavior and does not claim the entire
old tagged merger suite is converted. The pre-existing no-merge-modes API and eager
.NET Stream loader remain; Java optimized-mode and StreamCacheCreateFunction changes
have no corresponding mode/cache API to update.

Java JaCoCo 0.8.16 is Java-only development tooling. The Noto OTF is byte-identical to
upstream, with its complete Adobe copyright/SIL OFL 1.1 notice copied beside it
(trailing whitespace normalized). The octopdf SHA-512 matches the upstream Maven
fixture declaration. No new .NET library dependency is introduced.

Report-row gaps: none for the seven touched upstream production classes, their normalizer helper, or the three new font
helper/test sources. The new SplitterTest source is mapped to new and existing tests;
older deferred test cases are explicitly listed above. The per-file sync log accounts
for every one of the 16 changed upstream paths, including both Maven files, the font
license and the binary fixtures. Existing mapping coverage does not assert complete
behavioral or API parity of the library.

## Validation

- Release solution build: passed, 0 errors; 16 existing package/obsolete-API warnings.
- Full .NET solution: **1,963 passed, 0 failed, 21 explicit existing skips**.
- Focused .NET regressions: **20 passed, 0 failed, 0 skipped**, including exact
  save/reload-between-appends structure identities and 2000-UPEM CFF cache/scaling.
- Exact focused upstream Java suite: **44 passed, 0 failed/errors/skipped**.
- API review gate: **0 unreviewed deltas, 0 missing types**; existing reviewed
  member/arity/type-name gaps and the ratchet remain unchanged.
- Runtime gate: **691 distinct comparison rows, all matching, 0 known and 0 unexpected
  divergences**, covering 161 PDFs and 80 merge pairs. The existing harness summary
  reports 1,027 matches because it double-counts exact-match categories; distinct rows
  are the meaningful comparison count. Category caps, tolerances and known-failure
  policies are unchanged. These comparisons include existing structural/semantic/raster
  policies and do not assert universally byte-identical files.
- Inventory: **1,091 mapped production paths, 0 missing**; the tracked revision and
  both CI Java reference pins advance to the tested target.
- Whitespace/provenance checks: passed. Fixture hashes and all command details are
  recorded in `issue-964-validation.json`.

Initial compile/fixture assertion failures and the CFF prerequisite they exposed are
recorded candidly in the validation artifact. The optional old-source sensitivity run
was not executed; no mutation/baseline-failure claim is made. Later review corrected
indirect K children and added the save/load-between-appends regression before the
final focused/full runs. Only XML documentation and provenance/report edits followed
those semantic checks.
