# PDFBox 3.0 Runtime Parity Report

Generated (UTC): 2026-06-30T17:06:18Z

## Scope

Issue #590 retargets the runtime parity harness for the `release/3.0` line. The local and CI runs compared PdfBox.Net against Apache PDFBox `origin/3.0` using the checked-in runtime corpus manifest.

- Apache PDFBox ref: `origin/3.0`
- Apache PDFBox commit: `ea68b6feae80e671b3d26565b12eccc79e74d967`
- Corpus manifest: `tools/parity/runtime/corpus-manifest.txt`
- Ratchet baseline: `tools/parity/runtime/ratchet-baseline-3.0.json`
- Local artifact directory: `artifacts/runtime-parity-3.0-local`
- CI run: https://github.com/erikbra/pdfbox-net/actions/runs/28461084821/job/84349319303

## Summary

| Environment | Match | Known | Unexpected | PDFs | Merge pairs |
|---|---:|---:|---:|---:|---:|
| Local macOS/Homebrew JDK | 1027 | 0 | 0 | 161 | 80 |
| GitHub Actions Ubuntu/Temurin 17 | 1027 | 0 | 0 | 161 | 80 |

- Result: both environments produced zero known and zero unexpected rows.
- The ratchet keeps `known` and `unexpected` capped at zero.
- Category ceilings use the maximum observed accepted-equivalence bucket count across local macOS and CI Ubuntu runs, because render classifier buckets can shift between graphics/font environments while preserving behavioral parity.

## Accepted Equivalence Category Ceilings

Categories absent from the baseline have an implicit ceiling of zero.

| Category | Local | CI | Baseline ceiling |
|---|---:|---:|---:|
| `merge-structural-match` | 72 | 72 | 72 |
| `render-foreground-shape-equivalence-match` | 36 | 37 | 37 |
| `render-form-widget-bbox-clipping-equivalence-match` | 1 | 1 | 1 |
| `render-form-widget-raster-equivalence-match` | 3 | 2 | 3 |
| `render-global-resource-text-raster-equivalence-match` | 0 | 4 | 4 |
| `render-glyph-layout-equivalence-match` | 2 | 2 | 2 |
| `render-glyph-raster-equivalence-match` | 4 | 5 | 5 |
| `render-image-mask-shape-equivalence-match` | 0 | 2 | 2 |
| `render-java-optional-jpx-reader-missing-match` | 3 | 3 | 3 |
| `render-jbig2-decoder-raster-equivalence-match` | 1 | 1 | 1 |
| `render-lossy-jpeg-decoder-equivalence-match` | 0 | 1 | 1 |
| `render-pattern-transparency-raster-equivalence-match` | 1 | 1 | 1 |
| `render-rotation-overlay-shape-equivalence-match` | 9 | 9 | 9 |
| `render-sparse-equivalence-match` | 1 | 1 | 1 |
| `render-visual-equivalence-match` | 72 | 64 | 72 |
| `save-structural-match` | 148 | 148 | 148 |
| `text-semantic-math-linewrap-match` | 1 | 1 | 1 |
| `text-semantic-punctuation-spacing-match` | 1 | 1 | 1 |

## Operation Results

The operation success/failure counts matched between Java and .NET in the local run.

| Operation | Java OK | Java Fail | .NET OK | .NET Fail |
|---|---:|---:|---:|---:|
| `load` | 150 | 11 | 150 | 11 |
| `merge` | 72 | 8 | 72 | 8 |
| `render` | 150 | 0 | 150 | 0 |
| `save` | 148 | 2 | 148 | 2 |
| `text` | 150 | 0 | 150 | 0 |

## Corpus Categories

| Corpus category | Match | Known | Unexpected | Total |
|---|---:|---:|---:|---:|
| `annotations` | 4 | 0 | 0 | 4 |
| `encryption` | 20 | 0 | 0 | 20 |
| `filters/images` | 26 | 0 | 0 | 26 |
| `fonts and subsetting` | 26 | 0 | 0 | 26 |
| `forms` | 84 | 0 | 0 | 84 |
| `malformed PDFs` | 28 | 0 | 0 | 28 |
| `merge/split/save` | 41 | 0 | 0 | 41 |
| `rendering` | 43 | 0 | 0 | 43 |
| `text extraction` | 32 | 0 | 0 | 32 |
| `uncategorized` | 387 | 0 | 0 | 387 |

## Validation

Local validation used an Apache `origin/3.0` detached worktree and the Apache 3.0 app jar. The run passed in ratchet mode against `tools/parity/runtime/ratchet-baseline-3.0.json` with `--fail-on-unexpected`.

```bash
python3 tools/parity/runtime/run_runtime_parity.py \
  --manifest tools/parity/runtime/corpus-manifest.txt \
  --pdfbox-root <apache origin/3.0 worktree> \
  --java-classpath <pdfbox-app jar> \
  --java-home <jdk> \
  --out-dir artifacts/runtime-parity-3.0-local \
  --ratchet-baseline tools/parity/runtime/ratchet-baseline-3.0.json \
  --gate-mode ratchet \
  --fail-on-unexpected
```

The first CI run on PR #599 also produced zero known and zero unexpected rows, but failed because the original 3.0 baseline captured only the local macOS accepted-equivalence bucket distribution. The baseline now records the reviewed cross-platform maximum for those accepted buckets.
