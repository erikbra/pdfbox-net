# Incremental Save Parity Closeout

Issue: #555

Parent measurement: `reports/save-merge-byte-identity-2026-06-28.md`

Updated measurement: `reports/save-merge-incremental-save-2026-06-28.md`

Machine-readable summary: `reports/save-merge-incremental-save-2026-06-28.json`

## Scope

#539 labeled incremental-save behavior on strict save/merge byte-identity rows. Incremental save has user-visible semantics, so #555 validates those semantics directly instead of forcing byte identity for appended revisions.

## Java PDFBox Reference Behavior

Apache PDFBox `PDDocument.saveIncremental(OutputStream)` requires a source-backed document, constructs `COSWriter(output, pdfSource)`, and writes an incremental update. `COSWriter` buffers the new revision, writes `/Prev` from the previous `startxref`, writes either an incremental xref table or xref stream depending on the source document, appends `startxref` and `%%EOF`, then copies the original PDF bytes followed by the incremental bytes.

The port follows the same user-visible contract:

- `PDDocument.SaveIncremental(Stream)` requires `_sourceBytes`.
- The output starts with the exact original source bytes.
- Modified or new indirect objects are appended.
- The incremental trailer sets `/Prev` to the loaded document startxref.
- The final `startxref` points to the appended xref section.
- The resulting PDF reloads with the update visible.

One accepted serialization adaptation remains: PdfBox.Net currently appends a classic xref table for incremental updates, including xref-stream source PDFs, while Java PDFBox writes an xref stream when the source document uses xref streams. This is an xref-layout byte difference, not a behavior gap, because the updated document reloads and preserves the previous revision chain.

## Coverage Added

`PDDocumentIncrementalSaveParityTest` now covers:

- classic-xref input and xref-stream input,
- exact original-byte prefix preservation,
- two `%%EOF` markers and two `startxref` markers after one incremental update,
- `/Prev` pointing to the original `startxref`,
- final `startxref` pointing to the appended xref table,
- reloadability and metadata update visibility,
- full-save behavior proving that regular `Save` rewrites instead of appending incremental markers.

The runtime probes also gained an opt-in `--incremental` mode for Java and .NET. The mode mutates document information metadata, writes an incremental save, stores the output artifact, and emits a marker signature containing output hash, original-prefix preservation, EOF count, `/Prev` count, `startxref` count, and final `startxref` offset.

## Rerun Results

The analyzer was rerun against the GitHub Actions runtime-parity artifact from PR #573 run `28338481346`.

Strict byte-identity counts are unchanged from the parent measurement:

| Operation | Total rows | Byte-identical | Byte-different |
|---|---:|---:|---:|
| `merge` | 72 | 0 | 72 |
| `save` | 150 | 2 | 148 |

Incremental marker breakdown:

| Operation | Rows with incremental label | Java has markers | .NET has markers | Marker presence differs | EOF-count differences | /Prev-count differences |
|---|---:|---:|---:|---:|---:|---:|
| `merge` | 9 | 0 | 9 | 9 | 0 | 9 |
| `save` | 16 | 0 | 16 | 16 | 0 | 16 |

The row count is explained, not reduced. These strict save/merge artifacts are not invoking the focused incremental-save path equally on both sides; they expose marker/layout differences in serialized artifacts. The focused incremental-save tests cover the actual user-visible semantics directly.

## Decision

Do not force byte identity for incremental-save rows in #555. The correct contract for this issue is structural behavior parity: preserve the original revision, append a valid update, maintain a `/Prev` chain, and reload with the change visible. Future byte-tightening should happen under xref-stream/incremental writer work only if the port intentionally adopts Java's source-dependent xref-stream incremental serialization.
