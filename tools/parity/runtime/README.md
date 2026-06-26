# Runtime Parity Corpus

This directory contains the Java-vs-.NET runtime parity gate for the tracked
PDF corpus.

## Corpus inputs

- `corpus-manifest.txt` is the checked-in corpus manifest. `repo:` entries are
  resolved relative to this repository. `pdfbox:` entries are resolved relative
  to an Apache PDFBox source checkout passed with `--pdfbox-root` or the
  `PDFBOX_SOURCE_ROOT` environment variable.
- `known-failures.json` is the reviewed ledger for current Java/.NET
  divergences. The issue #441 closeout leaves it empty; any future entry must
  identify an owning issue, root-cause category, expiry condition, and ratchet
  rule.
- `ratchet-baseline.json` stores the maximum accepted known/unexpected and
  category counts for ratchet mode.

The CI job checks out Apache PDFBox at the pinned
`PDFBOX_PARITY_PDFBOX_REF` workflow variable, builds the app jar, runs the
manifest, uploads `java-results.jsonl`, `dotnet-results.jsonl`,
`comparison.json`, structure JSONL files, generated PDFs/images/text, and
`summary.md` as a workflow artifact.

`comparison.json` includes the matched known-failure id, owning issue,
root-cause bucket, parsed render metrics, render pixel-diff statistics, and
Java/.NET artifact paths for each row where artifacts exist. `summary.md`
groups known rows by root cause and lists known render detail rows with the
same metrics and artifact paths so CI/local 51-vs-52 variance can be compared
directly from uploaded artifacts.

The broad render-quality known-failure allowance has been removed. The ratchet
baseline now accepts zero known and zero unexpected rows.

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
Near-blank render rows with less than 0.5% foreground coverage have a separate
low-ink visual-equivalence classifier, guarded by low moderate/large
pixel-difference ratios plus RMS and mean channel-error limits.
Sparse non-near-blank rows have their own visual-equivalence classifier with a
2% foreground cap and low mean, RMS, moderate, and large pixel-difference
limits.
Rows where only one runtime crosses the near-blank metric boundary use a
separate near-blank-threshold visual classifier, capped at 1% foreground
coverage and the same sparse diff limits, before they are treated as render
placeholders.
Non-near-blank rows with less than 10% foreground coverage may also be accepted
as low-mean raster drift when the mean channel error stays below 0.8, RMS stays
below 6, large-difference pixels stay below 0.5%, and moderate-difference
pixels stay below 4%.
The closed image/mask render bucket has a fixture-scoped foreground-shape
equivalence classifier. It accepts only the reviewed #492 image-heavy fixtures
whose Java and .NET foreground masks have the same shape after dilation, with
tight caps on foreground-count delta, miss ratios, RMS, mean, and large
pixel-difference ratio.
The closed pattern/transparency render bucket has a separate fixture-scoped
classifier for the reviewed #493 render fixtures. It is capped by low mean/RMS
channel error, bounded moderate/large pixel-difference ratios, and
foreground-shape limits so it only accepts the reviewed Java2D-vs-Skia raster
drift after the appearance-stream resource lookup fix.
The closed form/widget appearance bucket has a fixture-scoped raster
equivalence classifier for the reviewed AcroForm fixtures. It is capped by
mean/RMS error, bounded moderate/large pixel-difference ratios, and
foreground-shape limits so these rows no longer require a known-failure ledger
entry after the #489 renderer fixes.

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
