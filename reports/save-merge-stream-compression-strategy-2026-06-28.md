# Save/Merge Stream and Compression Strategy

Issue: #553

## Findings

Apache PDFBox encodes `/FlateDecode` streams with `Deflater` using the exact numeric value from `org.apache.pdfbox.filter.deflatelevel`, clamped to `-1..9`. The .NET port read the same setting but previously mapped it into broad .NET `CompressionLevel` buckets. Writer-created Flate streams now pass the exact numeric level to `ZLibCompressionOptions`.

The Flate encoder lifecycle also now matches Java more closely: the zlib stream is finished/closed before the destination PDF stream is flushed. This avoids an extra flush-before-finish behavior and aligns the port with Java's `DeflaterOutputStream` close-then-flush sequence.

Full-save copied source streams should not be rewritten for byte identity. `PDDocument.Save(...)` preserves existing source stream bytes and filters by writing raw stream data when no encryption rewrite is required. Recompressing those streams would risk changing user-visible semantics and would reduce source-PDF preservation for a cosmetic Java-byte-identity target.

Writer-controlled streams are the safe alignment surface: streams created through `COSStream.CreateOutputStream(...)`, content stream generation, appearance generation, image/font/metadata stream creation, object-stream creation, and similar code paths where PdfBox.Net owns the encoded bytes.

## Analyzer Rerun

The byte-identity analyzer was extended with stream/filter diagnostics and rerun against the latest post-#552 runtime-parity artifact: PR #571 run `28337156660`.

| Operation | Stream-filter label rows | Compression label rows | Filter token differences | Stream-count differences | Length-sequence differences | Flate-count differences | ObjStm-count differences |
|---|---:|---:|---:|---:|---:|---:|---:|
| `merge` | 72 | 72 | 72 | 72 | 72 | 72 | 72 |
| `save` | 148 | 148 | 148 | 148 | 148 | 148 | 147 |

The broad stream/compression labels are not isolated deflater-byte differences. They co-occur with different stream counts, different `/FlateDecode` counts, and, for almost every row, different `/ObjStm` counts. That means the main remaining strict-byte blocker is writer object-stream/xref layout rather than a local Flate implementation choice.

## Gate Decision

Safe Flate alignment has been implemented for writer-created streams. Exact Java byte identity can still be blocked by platform deflater differences even when both runtimes use the same numeric compression level, because the .NET zlib backend and Java's `Deflater` are independent implementations.

The remaining object-stream and xref-layout differences should be handled by #554. The structural-equivalence ratchet remains the compatibility contract until object layout is intentionally stabilized.
