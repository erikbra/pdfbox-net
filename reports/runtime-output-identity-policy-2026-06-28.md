# Runtime Output Identity Policy

Generated UTC: 2026-06-28

Issue: #532

## Decision Summary

PdfBox.Net should target Java PDFBox behavioral parity by default, not exact
byte-for-byte or pixel-for-pixel identity for every output artifact.

The runtime parity gate remains strict about unreviewed behavior: `known` and
`unexpected` rows must stay at zero. The ratchet may continue to accept
reviewed equivalence categories when the comparison proves the user-observable
PDF behavior is equivalent and the remaining difference is caused by legitimate
runtime, backend, serialization, or platform variation.

Exact identity is still valuable, but it should be treated as a targeted
hardening goal for selected domains, not as the baseline compatibility promise.

## Terms

| Term | Meaning | Default compatibility status |
|---|---|---|
| Byte identity | Java and .NET artifacts have identical bytes or hashes. | Preferred when practical, but not required for generated PDFs. |
| Structural equivalence | Generated PDFs differ in bytes but expose the same Java-observable document structure signature. | Accepted for save/merge parity. |
| Semantic text equivalence | Extracted text differs only by reviewed whitespace, wrapping, or punctuation spacing normalization. | Accepted only for explicitly ratcheted categories. |
| Visual equivalence | Rendered rasters differ but pass bounded pixel-diff, foreground-shape, glyph-layout, or fixture-scoped classifiers. | Accepted only for reviewed classifier buckets. |
| Optional-runtime difference | Java lacks an optional decoder/provider and cannot render what .NET can render. | Accepted when classified separately from .NET render defects. |
| Known failure | Reviewed behavior gap with owner metadata in `known-failures.json`. | Not accepted in the current gate; count must remain zero. |
| Unexpected divergence | Any unclassified behavior difference. | Not accepted; count must remain zero. |

## Current Ratchet Review

The checked-in baseline is `tools/parity/runtime/ratchet-baseline.json`.
At the time of this review it allows zero known and zero unexpected rows, while
allowing these reviewed non-identical match categories:

| Category | Baseline | Decision | Rationale |
|---|---:|---|---|
| `save-structural-match` | 148 | Accepted adaptation; track strict mode separately. | COS object numbers, xref layout, dictionary ordering, compression, timestamps, and writer implementation choices can differ while the resulting PDF structure remains equivalent. |
| `merge-structural-match` | 72 | Accepted adaptation; track strict mode separately. | Same rationale as save output; merged documents should be structurally equivalent rather than byte-cloned from Java internals. |
| `text-semantic-math-linewrap-match` | 1 | Ratchet toward zero when touched. | This is close to user-visible text extraction behavior and should not grow. A focused fixture can decide whether Java-compatible line wrapping is worth tightening. |
| `text-semantic-punctuation-spacing-match` | 1 | Ratchet toward zero when touched. | Punctuation spacing can affect downstream text consumers. Keep the current allowance but treat new rows as regressions. |
| `render-visual-equivalence-match` | 68 | Accepted adaptation with strict bounds. | Java2D and SkiaSharp rasterization differ in antialiasing/color-management details. The classifier requires same dimensions and low bounded pixel drift. |
| `render-lossy-jpeg-decoder-equivalence-match` | 2 | Accepted adaptation. | JPEG decoding and color conversion are lossy and backend-dependent; exact pixels are not a stable cross-runtime promise. |
| `render-foreground-shape-equivalence-match` | 48 | Accepted adaptation with root-cause follow-up. | Foreground shape matching proves the same visible marks exist while tolerating backend raster drift. Broad counts should not grow. |
| `render-image-mask-shape-equivalence-match` | 4 | Accepted adaptation; tighten with image work. | Fixture-scoped mask/stencil equivalence is acceptable until image decoding/mask rendering hardening reduces it. |
| `render-pattern-transparency-raster-equivalence-match` | 6 | Accepted adaptation; tighten with renderer work. | Transparency and blend behavior is visually bounded but backend raster output is not pixel-identical. |
| `render-form-widget-raster-equivalence-match` | 5 | Accepted adaptation; tighten with forms work. | Form appearances can be behaviorally equivalent while text raster placement differs slightly by backend. |
| `render-glyph-layout-equivalence-match` | 10 | Accepted adaptation with strong semantic proof. | The classifier checks glyph rows for matching page/index/unicode/codes/font/embedded flags and bounded glyph geometry. |
| `render-low-ink-equivalence-match` | 1 | Accepted adaptation. | Near-blank documents are sensitive to tiny antialiasing differences; bounded low-ink drift is acceptable. |
| `render-sparse-equivalence-match` | 9 | Accepted adaptation. | Sparse documents amplify small raster differences; bounds keep this from hiding visible defects. |
| `render-near-blank-threshold-equivalence-match` | 1 | Accepted adaptation. | One runtime crossing the near-blank threshold is acceptable only under sparse-drift limits. |
| `render-low-mean-raster-drift-equivalence-match` | 7 | Accepted adaptation. | Low average channel error with bounded large/moderate pixel differences is acceptable backend drift. |
| `render-java-optional-jpx-reader-missing-match` | 3 | Accepted optional-runtime difference. | Java PDFBox depends on an optional JPEG 2000 reader; a blank Java render with a visible .NET render is not a .NET feature gap. |

## Policy

1. The default CI parity gate should remain ratchet mode with `known == 0` and
   `unexpected == 0`.
2. New equivalence categories must not be added casually. They must document:
   owner issue, classifier rule, artifact evidence, and ratchet ceiling.
3. Existing equivalence ceilings may be lowered whenever implementation work
   reduces the count. They should not be raised without a report update.
4. Save and merge byte identity are not default goals. Structural equivalence is
   the compatibility contract for generated PDFs.
5. Render pixel identity is not a default goal across Java2D and .NET graphics
   backends. Visual and semantic classifiers are acceptable when bounded and
   artifact-backed.
6. Text extraction semantic equivalence should be stricter than rendering. The
   two existing text semantic categories are accepted for the current corpus,
   but any new text semantic rows should fail the ratchet until reviewed.
7. A strict identity run may exist for measurement and hardening, but failures
   in strict identity mode should not automatically block releases unless a
   specific downstream contract requires it.

## Follow-Up Issues

The review splits exact-identity hardening into targeted tasks:

| Area | Follow-up issue | Goal |
|---|---|---|
| Save/merge byte identity | #539 | Add a strict measurement report for PDF writer byte drift and identify which writer differences are feasible to tighten. Follow-up implementation work is split into #551, #552, #553, #554, and #555. |
| Text semantic drift | #540 | Reduce or eliminate the current text semantic allowances with targeted fixtures. |
| Render visual equivalence buckets | #541 | Add per-bucket reports and lower raster-equivalence ceilings where renderer fixes make rows byte/pixel-identical. |
| Optional JPX/JPEG 2000 rendering | #542 | Decide whether to keep the optional Java-provider classification or add an optional Java CI provider for strict raster comparison. |

## Practical Judgment

Requiring 100% byte and pixel identity with Java PDFBox would make the .NET port
less useful for little compatibility gain. It would couple PdfBox.Net to Java
writer internals, Java2D rasterization, optional Java image plugins, and
platform-specific antialiasing behavior. The better target is:

- exact identity where it is cheap and stable,
- structural identity for generated PDFs,
- semantic identity for text,
- bounded visual identity for rendering,
- zero unreviewed divergences.

This is the compatibility stance that should guide future parity work unless a
specific downstream user asks for a stricter identity mode.
