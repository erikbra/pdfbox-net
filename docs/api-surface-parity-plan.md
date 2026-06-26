# Reviewed API Surface Parity Plan

Generated: 2026-06-27

Source reports:

- `reports/pdfbox-api-surface-analysis.md`
- `reports/api-surface-comparison.json`

Tracking issue: https://github.com/erikbra/pdfbox-net/issues/511

## Target

Reach **100% reviewed API surface parity** for the Apache PDFBox library modules:

- `io`
- `fontbox`
- `xmpbox`
- `pdfbox`

This target is intentionally not "blindly implement every Java public member."  Every Java public/protected type and member should be reviewed and placed in exactly one disposition:

| Disposition | Meaning |
|---|---|
| `implemented` | Public .NET API is available with compatible behavior or a directly mapped idiom. |
| `compat-overload-added` | A Java-compatible overload was added to reduce porting friction. |
| `intentional-dotnet-adaptation` | The .NET API intentionally differs, and the replacement is documented. |
| `internal-by-design` | The Java public type/member is intentionally not public in .NET. |
| `not-applicable` | The Java API is platform-specific or not meaningful in .NET. |
| `behavior-covered` | The exact API shape differs, but runtime behavior is covered by parity tests. |

The end state is:

- `unreviewed_missing_members == 0`
- `unreviewed_arity_drift_members == 0`
- `unreviewed_type_name_or_visibility_gaps == 0`
- Future API-surface report runs fail or flag regressions when new unreviewed gaps appear.

## Current State

API comparison generated against Apache PDFBox commit `833ed8f378f00838fd8df8c01bfc4b915b4c350b` and PdfBox.Net commit `655e4d55cf346556e280a4edd22ba43cc23ae740`.

| Metric | Count |
|---|---:|
| Java public/protected types | 581 |
| Matched public .NET types | 579 |
| Same-name public .NET types | 573 |
| Renamed public .NET replacements | 6 |
| Mapped but non-public/replacement-marker types | 2 |
| Missing mapped public .NET types | 0 |
| Java public/protected members | 6305 |
| Matched members | 4651 |
| Arity-drift members | 105 |
| Missing members | 1549 |
| Reflected .NET extra members on matched types | 963 |

Member coverage by name/signature heuristic: **4756 / 6305 = 75.4%**.

The review backlog is roughly:

- 232 type-level reviews.
- 1549 missing-member decisions.
- 105 arity-drift decisions.
- 8 type-name/visibility decisions.

## Execution Rules

1. Work by bounded API family, not by global search-and-replace.
2. For each family, review every missing member and arity-drift row from `reports/api-surface-comparison.json`.
3. Prefer compatibility overloads for common Java client-code entry points when the overload can delegate safely to an existing .NET implementation.
4. Do not expose implementation-only internals just to satisfy the report.
5. For intentional .NET adaptations, document the replacement API and record the disposition.
6. For behavior-sensitive APIs, add focused tests before marking the row `implemented` or `behavior-covered`.
7. Rerun `tools/parity/generate_api_surface_report.py` after each workstream and update report totals.

## Workstreams

| Workstream | Scope | Review types | Missing members | Arity drift | GitHub issue |
|---|---|---:|---:|---:|---|
| API review infrastructure and ratchet | Disposition ledger, generator support, CI/regression policy | all | all | all | [#501](https://github.com/erikbra/pdfbox-net/issues/501) |
| XmpBox metadata API surface | `xmpbox:schema`, `xmpbox:type`, `xmpbox:xml` | 22 | 359 | 5 | [#502](https://github.com/erikbra/pdfbox-net/issues/502) |
| FontBox API surface | `fontbox:afm`, `fontbox:ttf`, `fontbox:cff`, `fontbox:encoding` | 26 | 147 | 9 | [#503](https://github.com/erikbra/pdfbox-net/issues/503) |
| Core parser/writer/COS/filter/io API surface | `pdfbox:pdfparser`, `pdfbox:pdfwriter`, `pdfbox:cos`, `pdfbox:filter`, `io:io` | 21 | 95 | 31 | [#504](https://github.com/erikbra/pdfbox-net/issues/504) |
| Content stream API surface | `pdfbox:contentstream` | 54 | 61 | 1 | [#505](https://github.com/erikbra/pdfbox-net/issues/505) |
| PDModel document/common/FDF/navigation API surface | `pdmodel` root, `common`, `fdf`, document/page navigation and small document-interchange families | 31 | 120 | 10 | [#506](https://github.com/erikbra/pdfbox-net/issues/506) |
| Annotation and form API surface | `pdmodel:interactive/annotation`, `interactive/form`, related interactive small families | 41 | 185 | 21 | [#507](https://github.com/erikbra/pdfbox-net/issues/507) |
| Font API surface | `pdmodel:font` | 21 | 158 | 7 | [#508](https://github.com/erikbra/pdfbox-net/issues/508) |
| Graphics/rendering API surface | `pdmodel:graphics/*`, `pdfbox:rendering`, `pdfbox:text` renderer-facing rows | 22 | 130 | 14 | [#509](https://github.com/erikbra/pdfbox-net/issues/509) |
| Digital signature and encryption API surface | `pdmodel:interactive/digitalsignature`, `pdmodel:encryption` | 9 | 107 | 5 | [#510](https://github.com/erikbra/pdfbox-net/issues/510) |

The totals above are planning counts grouped from the generated report.  The source of truth remains `reports/api-surface-comparison.json`.

## Acceptance Criteria For Each Workstream

- Every scoped `missing` member has a recorded disposition.
- Every scoped `arity-drift` member has a recorded disposition.
- Every scoped renamed/non-public type decision has a recorded disposition.
- Public API additions include XML documentation copied or adapted from JavaDoc where applicable.
- Behavior-sensitive additions include parity tests or fixture-backed regression tests.
- `reports/api-surface-comparison.json` and `reports/pdfbox-api-surface-analysis.md` are regenerated.
- The workstream issue is closed only when unreviewed rows for its scope are zero.

## Recommended Order

1. Build the disposition ledger and ratchet support.
2. Close low-risk metadata/model areas: XmpBox and FontBox.
3. Close core parser/writer/COS/filter/io overloads.
4. Close content stream APIs.
5. Close PDModel document/common/FDF/navigation APIs.
6. Close annotation/form APIs.
7. Close PDModel font APIs.
8. Close graphics/rendering APIs.
9. Close digital signature/encryption APIs.

This order front-loads review infrastructure and lower-risk model APIs before high-risk areas where public API shape is tightly coupled to behavior.
