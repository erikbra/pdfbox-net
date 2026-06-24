# Runtime Parity Harness

The runtime parity harness compares Apache PDFBox Java behavior with PdfBox.Net on the same PDF corpus.
It is the tracked replacement for the ad hoc probes used during `/tmp/pdfbox-gap-scan`.

## Command

```bash
python3 tools/parity/runtime/run_runtime_parity.py \
  --manifest /tmp/pdfbox-gap-scan/manifest.txt \
  --java-classpath "$PDFBOX_JAVA_CLASSPATH" \
  --java-home "$JAVA_HOME" \
  --out-dir artifacts/runtime-parity \
  --merge-pairs /tmp/pdfbox-gap-scan/merge-pairs.txt \
  --ratchet-baseline tools/parity/runtime/ratchet-baseline.json
```

`--merge-pairs` is optional. If omitted, the harness creates adjacent manifest pairs.
`--java-home` is optional when `java` and `javac` are already available on `PATH`.

The Java classpath must contain Apache PDFBox and its runtime dependencies. For a local Apache PDFBox checkout, a Maven-built dependency classpath plus the PDFBox build output is sufficient.

## What It Runs

For each manifest PDF, both probes run:

- load
- text extraction
- save/copy
- first-page render at 36 DPI

The Java probe is launched with `-Djava.awt.headless=true` so rendering works in CI and server environments without an X11 display.

For each merge pair, both probes run a two-document merge.

The probes emit JSONL records containing:

- `file`
- `op`
- `ok`
- `pages`
- `ms`
- `detail`

For successful text extraction, `detail` is `<utf8-byte-count>:<sha256-prefix>`.
Successful text extraction also writes `<stem>-java-text.txt` and `<stem>-dotnet-text.txt` under each runtime output directory so semantic mismatches can be grouped by text-level heuristics.
For successful save/copy and merge operations, `detail` is `<pdf-byte-count>:<sha256-prefix>`.
For successful rendering, `detail` is `<width>x<height>:<png-byte-count>:<sha256-prefix>:<image-metrics>`.
Render image metrics include `nonBg`, `unique`, `dominant`, `transparent`, and `nearBlank`.
For failures, `detail` is `<exception-type>:<message>`.
Failed operations compare by exception-type category; the message remains in `detail` for triage.
Non-JSON diagnostic lines emitted by either runtime are ignored with a warning so library logging cannot corrupt the structured comparison.

## Outputs

The harness writes these files under `--out-dir`:

- `java-results.jsonl`
- `dotnet-results.jsonl`
- `comparison.json`
- `summary.md`
- Java and .NET generated PDFs/images under `java/` and `dotnet/`

`comparison.json` classifies every paired result as:

- `match`
- `status-mismatch`
- `metadata-mismatch`
- `diagnostic-mismatch`
- `detail-mismatch`
- `render-placeholder`
- `missing-result`

Successful text hash mismatches are refined into `text-*` categories such as line ending, trailing whitespace, whitespace collapse, spacing, content loss, encoding/CMap-like, and remaining semantic mismatches.
On the 2026-06-24 expanded runtime corpus, this groups successful text mismatches into 48 `text-encoding-cmap-mismatch` cases and 31 `text-content-loss` cases.

Known divergences are tracked in `tools/parity/runtime/known-failures.json` with owner and reason fields.
Entries can match by operation plus exact `category`, globbed `categoryGlob`, exact `files`, or `fileGlob`.
Corpus domain categories are tracked in `tools/parity/runtime/corpus-categories.json` with exact file names and file globs.
The generated `summary.md` includes a Corpus Categories table with match, known, unexpected, and total counts per domain.
Current blank or near-uniform .NET render regressions are listed in `tools/parity/runtime/render-placeholder-fixtures.txt`.
When `--fail-on-unexpected` is set, the harness exits non-zero only for divergences that are not covered by that ledger.

## Optional Image Decoders

`JPXDecode` is decoded through the existing Magick.NET image dependency. `JBIG2Decode` is wired through an internal decoder integration point that receives the encoded stream, optional `JBIG2Globals`, source-region, subsampling, and bit-depth options. Until a maintained .NET JBIG2 decoder is plugged into that interface, PdfBox.Net fails JBIG2 streams with a clear `IOException` stating that the decoder is not installed, mirroring Java PDFBox behavior when its optional `jbig2-imageio` plugin is unavailable.

`tools/parity/runtime/ratchet-baseline.json` records the maximum accepted known and unexpected divergence counts for the tracked corpus.
When `--ratchet-baseline` is supplied, the harness exits non-zero if any status count or divergence category count exceeds the baseline.
If a new divergence is expected, update `known-failures.json` with an owner and reason, rerun the parity suite, and update `ratchet-baseline.json` in the same change.

## CI

CI always smoke-checks the harness by compiling the Python runner and printing its help text.
The full parity suite runs when both environment variables are present:

- `PDFBOX_PARITY_MANIFEST`
- `PDFBOX_PARITY_CLASSPATH`

That lets normal CI stay lightweight while allowing scheduled or provisioned parity jobs to print and archive a concise summary from `artifacts/runtime-parity/summary.md`.
Provisioned parity CI uses `--ratchet-baseline tools/parity/runtime/ratchet-baseline.json`, so the job fails when the known-failure or unexpected-divergence count grows without an intentional baseline update.
