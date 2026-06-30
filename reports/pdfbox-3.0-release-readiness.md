# PDFBox 3.0 Release Readiness

Issue #593 defines the final acceptance gate for the `release/3.0` branch.

## Scope

- Branch: `release/3.0`
- Apache upstream ref: `origin/3.0`
- Apache upstream commit: `ea68b6feae80e671b3d26565b12eccc79e74d967`
- Excluded modules: `preflight`, `preflight-app`

Preflight is Apache PDFBox's PDF/A validation module. It is intentionally
excluded from the initial PdfBox.Net 3.0 line because it was removed from Apache
trunk/4.0 and has a different product shape than the core PDFBox libraries.
PdfBox.Net can add PDF/A validation later as a separate package or milestone.

## Gate Result

Result: **core Apache PDFBox 3.0 parity is ready for preview package publishing,
excluding Preflight/PDF-A validation**.

This is not a claim that every Apache PDFBox 3.0 product surface is complete.
The remaining known work is tracked as explicit follow-up product/runtime
surface issues.

| Gate | Result | Evidence |
|---|---|---|
| Non-Preflight source coverage | Pass | `reports/pdfbox-3.0-source-coverage.md`: 1071 / 1071 scoped production Java files mapped, 0 missing. |
| Main coverage scan | Pass | `reports/pdfbox-main-gap-analysis.md`: scoped totals 1071 / 1071, library-core subset 856 / 856, no partial traceability rows. |
| API review | Pass | `reports/pdfbox-3.0-api-review.md`: 0 unreviewed public/protected API deltas. |
| Runtime parity | Pass | `reports/pdfbox-3.0-runtime-parity.md`: 1027 matches, 0 known gaps, 0 unexpected gaps on local macOS and GitHub Actions Ubuntu. |
| CI/package metadata | Pass | `reports/pdfbox-3.0-ci-package-metadata.md`: branch-specific API/runtime gates and default `3.0.8-preview` package metadata. |
| Tools/examples/app/debugger review | Pass with deferrals | `reports/pdfbox-3.0-tools-examples-debugger-review.md`: source stems mapped, `pdfbox` CLI/global-tool parity closed by #601, and remaining product gaps tracked as #602 and #603. |
| README/docs support level | Pass | `README.md` now describes 3.0 core parity, Preflight exclusion, preview package default, and deferred product gaps. |

## Issue State

The planned 3.0 setup and parity issues are complete:

| Issue | State | Result |
|---|---|---|
| #585 | Closed | Established the 3.0 branch plan and Preflight exclusion policy. |
| #586 | Closed | Retargeted upstream comparison tooling to Apache `origin/3.0`. |
| #587 | Closed | Generated the 3.0 source coverage report excluding Preflight. |
| #588 | Closed | Reconciled the missing non-Preflight 3.0 source files. |
| #589 | Closed | Reviewed changed common APIs against Apache `origin/3.0`. |
| #590 | Closed | Retargeted the runtime parity corpus and Java probes to 3.0. |
| #591 | Closed | Aligned CI and package metadata for `release/3.0`. |
| #592 | Closed | Reviewed tools, examples, app, debugger, and other non-core module parity. |

Issue #601 closes the `pdfbox-app`/CLI product gap by adding the `PdfBox.Net.Tools`
global-tool package and wiring Apache PDFBox 3.0 command names through
`PDFBox.Run`.

The remaining open issues are explicit deferrals, not unreviewed core parity
blockers:

| Issue | Deferred area | Release impact |
|---|---|---|
| #602 | Java Swing debugger UI parity decision and implementation scope. | Blocks claiming debugger application parity; does not block core library preview packages. |
| #603 | Deterministic examples coverage for PDF/A and advanced signature flows. | Blocks closing the examples edge-case coverage ledger; does not add an unreviewed core library source/API/runtime gap. |

## Publishing Decision

The branch should keep its default package version as `3.0.8-preview`.

It is reasonable to publish preview NuGet packages for the core libraries and
optional backend packages from this branch, with release notes that state:

- Apache PDFBox 3.0 core modules are the target.
- Preflight/PDF-A validation is excluded.
- The runtime corpus is green with zero known and zero unexpected gaps.
- API parity is 100% reviewed, not 100% one-for-one Java member identity.
- Debugger UI parity and examples edge coverage remain tracked as #602 and #603.

Stable 3.0 packages should wait until either #602 and #603 are closed or the
project owner explicitly accepts those areas as non-goals for the stable 3.0
package line.

## Accepted Adaptations And Behavior-Covered Differences

The branch has no unreviewed non-Preflight API gaps, but it still has reviewed
differences from Apache PDFBox 3.0:

- Runtime render/save/merge/text output is behaviorally equivalent under the
  corpus ratchet, not byte-for-byte or pixel-identical in every accepted
  category.
- Java collection, stream, file, resource-cache, and protected parser extension
  shapes are sometimes exposed through .NET-native APIs or factory paths.
- XmpBox JavaBean helper expansion remains reviewed backlog where XML
  round-trip behavior and constants are present.
- Java AWT/ImageIO behavior is represented through Java-shaped proxy types and
  optional rendering backends, with SkiaSharp as the complete backend today.
- The Java Swing debugger UI is not yet claimed as equivalent; #602 owns the
  product decision.
- Preflight/PDF-A validation is excluded from this branch's core parity claim.

These adaptations are documented in the API review, runtime parity report,
graphics/backend policy reports, and the tools/examples/debugger review.

## Final Judgment

`release/3.0` satisfies the repository's definition of done for **core Apache
PDFBox 3.0 parity excluding Preflight**. The branch is ready for preview
package publication and continued issue-by-issue hardening toward a stable 3.0
line.
