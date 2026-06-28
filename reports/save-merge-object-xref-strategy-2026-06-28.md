# Save/Merge Object Numbering and XRef Strategy

Issue: #554

## Findings

Apache PDFBox full save defaults to `CompressParameters.DEFAULT_COMPRESSION`. That sets the COS document to xref-stream mode, raises the document version to at least 1.6 when needed, partitions the COS graph into top-level objects and object-stream candidates, writes compressed object streams, and emits an xref stream.

PdfBox.Net currently writes a classic xref table for full save and merge. The `Save(..., CompressParameters)` overload accepts Java-shaped parameters but delegates to the classic save path, so `DefaultCompression` and `NoCompression` are not distinguished in the writer. The port has partial object-stream helper classes, but they do not yet implement Java's full compression-pool traversal, object-stream index layout, xref-stream entry generation, or trailer/xref-stream serialization.

Merge output follows the same root cause. Java `PDFMergerUtility.mergeDocuments(...)` defaults to `CompressParameters.DEFAULT_COMPRESSION`, while PdfBox.Net creates a destination `PDDocument` and saves it through the classic writer.

Object generation numbers are not the blocker in this corpus: the analyzer found zero rows with different non-zero generation counts. The dominant differences are object-id set/sequence changes caused by object-stream packing and classic-vs-xref-stream layout choices.

## Analyzer Rerun

The byte-identity analyzer was extended with object/xref diagnostics and rerun against the latest post-#553 runtime-parity artifact: PR #572 run `28337846363`.

| Operation | Object-numbering label rows | XRef-layout label rows | Object-id sequence differences | Object-id set differences | Object-count differences | Startxref value differences | XRef table-count differences | XRef stream-count differences | XRef style differences |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `merge` | 72 | 72 | 72 | 72 | 47 | 72 | 72 | 72 | 72 |
| `save` | 148 | 147 | 148 | 148 | 146 | 147 | 148 | 148 | 148 |

The original #539 object/xref labels therefore do not point to a narrow numbering-order bug. They point to a writer-mode difference: Java is producing compressed object streams plus xref streams by default, while PdfBox.Net is producing classic top-level objects plus xref tables.

## Safe Stabilization Decision

No low-risk object/xref stabilization is implemented in this PR. Enabling Java-default object streams and xref streams would be a broad writer feature, not a small byte-identity adjustment. It would require completing the compression pool, object-stream serialization, xref-stream serialization, trailer handling, parser round-trips, encryption interactions, incremental-save exclusions, and merge coverage together.

Changing only object ordering, free-entry ranges, or `startxref` formatting in the classic writer was rejected because it would not address the corpus-wide xref-style deltas and could make the Java-shaped port harder to compare without moving rows toward strict byte identity.

## Gate Decision

Classic-table output remains an accepted structural-equivalence difference for now. Strict byte identity should not be ratcheted on save/merge until a dedicated compressed-writer feature closes the Java default-compression gap end to end.
