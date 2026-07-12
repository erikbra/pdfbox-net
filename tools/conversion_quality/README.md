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

CI also writes and uploads `artifacts/conversion-quality-smoke/html-examples`.
That directory is a human review bundle for real PDF fixtures: each example
contains the original `source.pdf`, generated `index.html`, CSS/assets,
`summary.md`, diagnostics, and a `compare.html` page that shows the PDF and
continuous semantic HTML side by side. Each example also contains `quality/quality-report.md`
and `quality/quality-report.json`, plus page-level PNG artifacts from the
source PDF render, browser-rendered continuous semantic HTML, and a foreground-mask diff. These
quality probe findings are non-gating: `needs-review` means the artifact found
a likely visual or structural conversion issue for humans to inspect, not that
the CI step failed. Download the `conversion-quality-smoke-*` workflow artifact
and open `html-examples/index.html` to browse the examples.

## Remote Academic Corpus

The pinned remote corpus covers four freely available academic papers: JMLR's
*Latent Dirichlet Allocation*, ACL Anthology's *BERT*, arXiv's *Adam*, and
arXiv's *U-Net*. Together they exercise long-form text, formulas, dense
two-column layout, tables, figures, diagrams, images, and links. Canonical
source pages, direct HTTPS PDF URLs, categories, and SHA-256 hashes are recorded
in `remote-corpus-manifest.json`.

Run the complete download, verification, conversion, expectation, and review
artifact path locally with one command:

```bash
python3 tools/conversion_quality/run_remote_corpus.py --build
```

PDFs are downloaded atomically with retries and timeouts into the ignored
`artifacts/cache/conversion-quality/remote-pdfs` directory. Every cached or
downloaded file must match its pinned SHA-256 hash before it is used. The script
then materializes a normal HTML review manifest and writes comparisons to
`artifacts/conversion-quality-smoke/remote-html-examples`. CI runs the same
networked path after the Release build and includes that directory beneath the
existing uploaded `conversion-quality-smoke` artifact root.

The remote manifest's expectations deliberately cover stable structural
invariants only: exact page count, normalized required title words, minimum
text runs, and category-specific minimum image, vector-path, and link counts.
Known visual and text-reconstruction shortcomings remain review findings owned
by issues #728 through #733 rather than brittle expected failures.

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
