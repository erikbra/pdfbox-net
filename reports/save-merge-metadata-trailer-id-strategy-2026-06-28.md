# Save/Merge Metadata and Trailer-ID Strategy

Issue: #551

## Findings

Apache PDFBox generates a trailer `/ID` array during full save when the trailer ID is missing or incomplete. The generation uses `PDDocument.getDocumentId()` as an optional deterministic seed, otherwise the current time, plus document-info values. Existing complete two-entry trailer IDs are preserved for normal full saves.

The .NET writer already exposed the Java-compatible `PDDocument.GetDocumentId()` and `SetDocumentId(long?)` API, but full save did not consume it. `PDDocument.Save(...)` now mirrors the Java full-save behavior: it creates a two-entry trailer `/ID` only when one is missing or incomplete, preserves complete source IDs, and uses `SetDocumentId(...)` as the deterministic seed when supplied.

The .NET writer does not auto-stamp `/Producer`, `/Creator`, `/CreationDate`, or `/ModDate`. Document-info values are preserved from the source or written only when callers, merge setup, encryption flows, signatures, or parity fixtures set them.

## Deterministic Parity Path

The runtime save probes now set the same deterministic document-id seed, `1719000000000`, before calling `document.save(...)` / `document.Save(...)`. This only affects documents whose trailer ID is missing or incomplete; source PDFs with complete trailer IDs continue to preserve those IDs.

Merge output remains harder to seed because Java `PDFMergerUtility` owns its destination `PDDocument` internally and has no public destination-document-id setter. The analyzer therefore reports trailer-ID differences explicitly and also computes metadata-normalized byte identity by replacing only `/CreationDate`, `/ModDate`, document-info string/name fields, and trailer `/ID` payloads.

## Analyzer Rerun

The updated analyzer was rerun against the same runtime-parity artifact used for #539: PR #550 run `28329163828`.

| Operation | Date rows | Info-field rows | Trailer-ID rows | Rows made byte-identical by metadata/ID normalization |
|---|---:|---:|---:|---:|
| `merge` | 60 | 62 | 72 | 0 |
| `save` | 91 | 93 | 148 | 0 |

This explains the original `metadata/timestamps` count: trailer IDs differed in every non-identical save and merge row, while date/info differences were present in many but not all rows. Normalizing metadata and trailer IDs alone did not make any additional row byte-identical, so metadata is a co-occurring difference rather than the sole byte-identity blocker in this corpus. The remaining blockers are still object numbering, compression/stream choices, dictionary ordering, and xref layout.

## Gate Decision

The default save/merge structural-equivalence gate remains unchanged. Strict byte identity should not be ratcheted until the follow-up writer investigations reduce the non-metadata causes.
