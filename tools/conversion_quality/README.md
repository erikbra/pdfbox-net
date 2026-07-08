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
