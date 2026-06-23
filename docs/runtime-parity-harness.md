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
  --fail-on-unexpected
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

Known divergences are tracked in `tools/parity/runtime/known-failures.json` with owner and reason fields.
Current blank or near-uniform .NET render regressions are listed in `tools/parity/runtime/render-placeholder-fixtures.txt`.
When `--fail-on-unexpected` is set, the harness exits non-zero only for divergences that are not covered by that ledger.

## CI

CI always smoke-checks the harness by compiling the Python runner and printing its help text.
The full parity suite runs when both environment variables are present:

- `PDFBOX_PARITY_MANIFEST`
- `PDFBOX_PARITY_CLASSPATH`

That lets normal CI stay lightweight while allowing scheduled or provisioned parity jobs to print and archive a concise summary from `artifacts/runtime-parity/summary.md`.
