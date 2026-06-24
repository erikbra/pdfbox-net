# Runtime Parity Corpus

This directory contains the Java-vs-.NET runtime parity gate for the tracked
PDF corpus.

## Corpus inputs

- `corpus-manifest.txt` is the checked-in corpus manifest. `repo:` entries are
  resolved relative to this repository. `pdfbox:` entries are resolved relative
  to an Apache PDFBox source checkout passed with `--pdfbox-root` or the
  `PDFBOX_SOURCE_ROOT` environment variable.
- `known-failures.json` is the reviewed ledger for current Java/.NET
  divergences. Each entry must identify an owning issue, root-cause category,
  expiry condition, and ratchet rule.
- `ratchet-baseline.json` stores the maximum accepted known/unexpected and
  category counts for ratchet mode.

The CI job checks out Apache PDFBox at the pinned
`PDFBOX_PARITY_PDFBOX_REF` workflow variable, builds the app jar, runs the
manifest, uploads `java-results.jsonl`, `dotnet-results.jsonl`,
`comparison.json`, structure JSONL files, generated PDFs/images/text, and
`summary.md` as a workflow artifact.

Render rows first compare raw rendered pixel hashes and image metrics. When
those differ, the harness decodes the saved Java/.NET PNGs and accepts only
small visual-equivalence drift: same dimensions, low moderate-difference pixel
ratio, very low large-difference pixel ratio, and low RMS channel error. Save
and merge rows compare Java-observable document structure signatures when byte
hashes differ.

JPX/JPEG 2000 rows where the Java probe renders a blank page because its
optional image reader is unavailable, while .NET renders visible pixels, are
classified separately so they do not count as .NET render-quality gaps.
JPEG-named rows also have a narrow lossy-decoder equivalence classifier for
small Java/.NET decoder and color-management drift.

## Local ratchet run

```bash
PDFBOX_ROOT=/path/to/apache/pdfbox
mvn -B -f "$PDFBOX_ROOT/pom.xml" -pl app -am -DskipTests package

python3 tools/parity/runtime/run_runtime_parity.py \
  --manifest tools/parity/runtime/corpus-manifest.txt \
  --pdfbox-root "$PDFBOX_ROOT" \
  --java-classpath "$PDFBOX_ROOT/app/target/pdfbox-app-4.0.0-SNAPSHOT.jar" \
  --out-dir artifacts/runtime-parity \
  --ratchet-baseline tools/parity/runtime/ratchet-baseline.json \
  --gate-mode ratchet \
  --fail-on-unexpected
```

Ratchet mode fails when an unexpected divergence appears or when known/category
counts exceed `ratchet-baseline.json`.

## Strict zero-known run

```bash
python3 tools/parity/runtime/run_runtime_parity.py \
  --manifest tools/parity/runtime/corpus-manifest.txt \
  --pdfbox-root "$PDFBOX_ROOT" \
  --java-classpath "$PDFBOX_ROOT/app/target/pdfbox-app-4.0.0-SNAPSHOT.jar" \
  --out-dir artifacts/runtime-parity-strict \
  --gate-mode strict
```

Strict mode passes only when both `known` and `unexpected` counts are zero. CI
can be switched to strict mode by setting `PDFBOX_PARITY_GATE_MODE=strict`.
