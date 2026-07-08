# Conversion Quality Harness

This harness is the shared measuring stick for the PDF-to-HTML and
PDF-to-Markdown package work. It is intentionally converter-independent: a
converter can write files into a result directory, then this script evaluates
those files against a fixture manifest and writes machine-readable and
human-readable reports.

## Local Smoke Run

```bash
python3 tools/conversion_quality/run_conversion_quality.py \
  --manifest tools/conversion_quality/smoke/manifest.json \
  --results-dir tools/conversion_quality/smoke/results \
  --out-dir artifacts/conversion-quality-smoke \
  --known-divergences tools/conversion_quality/smoke/known-divergences.json \
  --ratchet-baseline tools/conversion_quality/smoke/ratchet-baseline.json \
  --fail-on-unexpected \
  --fail-on-regression
```

The command writes:

- `comparison.json`, with per-fixture metrics, failure categories, and ratchet
  status. Each fixture also includes `qualityChecks` entries for DOM,
  text-coverage, and visual categories when applicable.
- `summary.md`, with the same result in a compact table suitable for CI step
  summaries.

CI also writes and uploads `artifacts/conversion-quality-smoke/html-review`.
That directory is a human review bundle for real PDF fixtures: each example
contains the original `source.pdf`, generated `index.html`, CSS/assets,
`summary.md`, diagnostics, and a `compare.html` page that shows the PDF and
generated HTML side by side. Each example also contains `quality/quality-report.md`
and `quality/quality-report.json`, plus page-level PNG artifacts from the
source PDF render, browser-rendered HTML, and a foreground-mask diff. These
quality probe findings are non-gating: `needs-review` means the artifact found
a likely visual or structural conversion issue for humans to inspect, not that
the CI step failed. Download the `conversion-quality-smoke-*` workflow artifact
and open `html-review/index.html` to browse the examples.

The HTML quality probe currently checks:

- browser page dimensions and text-run counts against extracted layout data
- word-boundary loss by comparing browser-visible HTML text with `pdftotext`
  when available, otherwise the PdfBox.Net layout text fallback
- text overlaps with rendered image boxes and large vector boxes
- foreground-mask deltas between a source PDF page render and the browser page
  screenshot, with visual report pages to make mismatches easy to inspect

HTML review examples can set `qualityPages` to cap how many pages the browser
probe renders. Keeping this small makes CI artifacts quick while still giving
us stable samples to improve against.

## Manifest Shape

Each fixture declares a target, output files, expected text, and measurable
expectations. Current gates cover:

- converter crashes or missing declared outputs
- text coverage after normalization
- broken local HTML asset references
- diagnostic counts
- required files
- required substrings in generated outputs
- simple HTML DOM selector counts through `expectations.domSelectors`
- visual check reports through an optional `outputs.visual` JSON file with a
  `checks` array

Known divergences are listed separately and must include an owning issue and
reason. Ratchet baselines cap the number of accepted `failed` and `known`
fixtures, cap failure categories, and can set aggregate metric floors such as
`minimumTextCoverage`.

The first checked-in smoke fixture uses synthetic HTML output. Future converter
issues should add real PDF fixtures and expected result folders as soon as the
HTML and Markdown packages have runnable vertical slices.
