# API Compatibility Backlog

Generated UTC: 2026-06-28

Issue: #533

## Current State

The API surface review is complete: every reviewable missing, arity-drift, and
type-name/visibility row has a disposition in
`reports/api-surface-dispositions.json`.

The project should not chase 100% Java API identity blindly. The policy is to
add Java-compatible entry points when they help Java PDFBox users port code
mechanically and when the entry point can be represented honestly in .NET.
Reviewed .NET adaptations remain valid where Java exposes implementation
internals, Java platform types, AWT classes, checked-exception-only overloads,
or Java collection shapes that would make the public C# API misleading.

The CI API gate now has two layers:

- `--fail-on-unreviewed` rejects new unreviewed Java API deltas.
- `reports/api-surface-ratchet-baseline.json` rejects increases in reviewed
  gap counts or decreases in matched/arity member coverage.

## Prioritization Rules

1. Prefer high-traffic user entry points over implementation internals.
2. Add overloads as sidecar partials or small compatibility helpers where that
   preserves Java-source comparability.
3. Include behavior tests or reflection/API-shape tests for each compatibility
   family before lowering the ratchet.
4. Do not expose Java platform types literally when the C# replacement type is
   the intended abstraction.
5. Keep parser/writer protected internals internal unless an actual extension
   use case appears.

## Priority Backlog

| Priority | Family | Current signal | Decision | Tracking issue |
|---|---|---:|---|---|
| P0 | API ratchet and backlog | 0 unreviewed rows, reviewed baseline exists | Done in #533. Keep CI enforcing the reviewed baseline. | #533 |
| P1 | XMP schema convenience accessors | 341 reviewed schema missing-member rows | Add Java-compatible schema property/accessor methods where they proxy existing generic XMP storage behavior. | #544 |
| P1 | PDModel tagged PDF and document model entry points | 411 reviewed `pdfbox:pdmodel` missing-member rows | Add sidecar compatibility members for stable constants, enum-like names, and low-risk model accessors. | #545 |
| P1 | High-traffic document APIs | Included in PDModel, multipdf, text, rendering, image/font factories | Add overloads for `Loader`, `PDDocument`, `PDFMergerUtility`, `PDFTextStripper`, image factories, and font loading where behavior already exists. | #546 |
| P2 | FontBox CFF/TTF compatibility helpers | 18 reviewed missing/renamed rows across CFF/TTF | Prefer wrapper/static helper methods only when Java client code commonly calls them. Keep internal interpreter hooks hidden. | #547 |
| P2 | PDF parser/writer extension points | 82 reviewed missing/arity rows across parser/writer | Keep protected parser/writer internals internal by default; only expose compatibility methods if a real extension scenario appears. | #548 |
| P3 | XmpBox type metadata/attribute replacements | 14 missing type rows and 2 renamed attribute replacements | Document as .NET adaptations unless Java annotation-like metadata is needed by consumers. | TBD |

## Accepted Adaptation Buckets

These buckets should remain documented rather than treated as automatic
implementation work:

| Bucket | Reason |
|---|---|
| Java `File`/`InputStream`/`RandomAccessRead` overloads | C# should use `string`, `FileInfo`, `Stream`, `ReadOnlyMemory<byte>`, or existing random-access interfaces instead of mirroring Java platform types literally. |
| AWT geometry/image types | The port uses Java-shaped proxies where they preserve comparability, but backend-specific AWT details should stay behind rendering abstractions. |
| Protected parser/writer internals | These are implementation hooks in Java and do not form a stable .NET extension contract today. |
| Java enum helper artifacts | Java enum/static-initializer parser artifacts should not force public C# API clutter when behavior is already covered. |
| Annotation metadata types | Java annotations are represented by .NET attributes where applicable; exact Java annotation shape is not a useful C# compatibility target. |

## Ratchet Workflow

For each compatibility issue:

1. Add the Java-compatible entry point in the lowest-impact sidecar file.
2. Add behavior tests or API reflection checks proving the member exists and
   delegates to the existing implementation.
3. Regenerate `reports/api-surface-comparison.json` and
   `reports/pdfbox-api-surface-analysis.md`.
4. Lower `reports/api-surface-ratchet-baseline.json` if missing/arity/type-gap
   counts decrease.
5. Keep the disposition ledger in sync: implemented rows should either leave
   the report or be reclassified as `compat-overload-added` only if the
   heuristic still cannot detect the overload.

## Judgment

The reviewed API surface is far enough along to protect with a ratchet, but not
close enough to call Java API parity complete. The next best progress is not a
single broad refactor; it is a sequence of focused, family-level compatibility
PRs with ratchet reductions after each one.
