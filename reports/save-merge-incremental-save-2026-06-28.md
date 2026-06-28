# Save/Merge Byte Identity Measurement

Issue: #555 (follow-up to #539)

Source: GitHub Actions runtime-parity artifact from PR #573 run 28338481346, rerun for #555 incremental-save analysis
Runtime parity output: `artifacts/runtime-parity-issue-555-download/runtime-parity-28338481346-1`
Runtime comparison generated UTC: `2026-06-28T22:59:50Z`
Manifest: `tools/parity/runtime/corpus-manifest.txt`

## Strict Identity Counts

| Operation | Total rows | Byte-identical | Byte-different |
|---|---:|---:|---:|
| `merge` | 72 | 0 | 72 |
| `save` | 150 | 2 | 148 |

## Writer Cause Counts

Rows can have more than one cause label. Cause labels are heuristics over serialized PDF artifacts; they identify writer areas to inspect, not exclusive root causes.

### `merge`

| Cause | Rows |
|---|---:|
| COS object numbering | 72 |
| compression | 72 |
| dictionary ordering | 72 |
| metadata/timestamps | 72 |
| stream filters | 72 |
| xref layout | 72 |
| incremental-save behavior | 9 |

### `save`

| Cause | Rows |
|---|---:|
| COS object numbering | 148 |
| compression | 148 |
| dictionary ordering | 148 |
| metadata/timestamps | 148 |
| stream filters | 148 |
| xref layout | 147 |
| incremental-save behavior | 16 |
| byte-identical | 2 |

## Metadata and Trailer-ID Breakdown

The broad `metadata/timestamps` label is split into serialized document-info date fields, serialized document-info string/name fields, and trailer `/ID` arrays.

### `merge` Metadata Differences

| Difference | Rows |
|---|---:|
| trailer IDs | 72 |
| info fields | 62 |
| dates | 60 |

### `save` Metadata Differences

| Difference | Rows |
|---|---:|
| trailer IDs | 148 |
| info fields | 93 |
| dates | 91 |
| byte-identical | 2 |

### Metadata-Normalized Identity

This replaces only `/CreationDate`, `/ModDate`, document-info text/name fields, and trailer `/ID` payloads with stable placeholders before comparing bytes.

| Operation | Total rows | Original byte-identical | Metadata-normalized byte-identical | Made identical by normalization | Still different after normalization |
|---|---:|---:|---:|---:|---:|
| `merge` | 72 | 0 | 0 | 0 | 72 |
| `save` | 150 | 2 | 2 | 0 | 148 |

## Dictionary Sequence Breakdown

The `dictionary ordering` label compares serialized dictionary key sequences. This breakdown separates pure order permutations from rows where dictionary counts or key sets differ, which usually means earlier writer decisions changed content or object layout.

| Operation | Rows with dictionary label | Rows with only order permutations | Rows with dictionary-count differences | Rows with key-set differences | Order-only pair mismatches | Key-set pair mismatches | Max dictionary-count delta |
|---|---:|---:|---:|---:|---:|---:|---:|
| `merge` | 72 | 0 | 72 | 72 | 0 | 5020 | 3076 |
| `save` | 148 | 0 | 148 | 148 | 0 | 3921 | 3182 |

## Stream and Compression Breakdown

The `stream filters` and `compression` labels are split into serialized `/Filter` token differences, stream marker counts, `/Length` counts and sequences, `/FlateDecode` token counts, and `/ObjStm` token counts. Length-sequence differences can be downstream symptoms of object layout or deflater output, not necessarily independent stream-ownership bugs.

| Operation | Rows with stream-filter label | Rows with compression label | Filter token differences | Stream-count differences | Length-count differences | Length-sequence differences | Flate-count differences | ObjStm-count differences | Max stream delta | Max length-count delta | Max Flate delta | Max ObjStm delta |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `merge` | 72 | 72 | 72 | 72 | 72 | 72 | 72 | 72 | 44 | 44 | 44 | 43 |
| `save` | 148 | 148 | 148 | 148 | 148 | 148 | 148 | 147 | 18 | 18 | 18 | 17 |

## COS Object and XRef Breakdown

The `COS object numbering` and `xref layout` labels are split into object-id sequence/set differences, generation-number sequence differences, `startxref` count/value differences, xref table/stream count differences, `/Prev` count differences, and overall xref style differences. A style difference means one artifact uses an xref stream and/or classic xref table differently from the other.

| Operation | Rows with object-numbering label | Rows with xref-layout label | Object-id sequence differences | Object-id set differences | Object-count differences | Generation sequence differences | Startxref value differences | XRef table-count differences | XRef stream-count differences | XRef style differences | Max object-count delta | Max last-startxref delta | Max xref-stream delta |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `merge` | 72 | 72 | 72 | 72 | 47 | 47 | 72 | 72 | 72 | 72 | 183 | 228391 | 1 |
| `save` | 148 | 147 | 148 | 148 | 146 | 146 | 147 | 148 | 148 | 148 | 3226 | 240939 | 1 |

## Incremental Marker Breakdown

The `incremental-save behavior` label is driven by serialized `%%EOF` and `/Prev` markers. These counts explain whether the strict byte mismatch is marker presence, marker count, or both-side incremental history rather than a missing ability to load, modify, save, and reload the document.

| Operation | Rows with incremental label | Java has markers | .NET has markers | Both have markers | Marker presence differs | EOF-count differences | /Prev-count differences | Max EOF delta | Max /Prev delta |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `merge` | 9 | 0 | 9 | 0 | 9 | 0 | 9 | 0 | 11 |
| `save` | 16 | 0 | 16 | 0 | 16 | 0 | 16 | 0 | 17 |

## Feasibility Assessment

| Cause | Judgment | Follow-up |
|---|---|---|
| metadata/timestamps | Feasible when values are generated by the writer or test harness; preserve source document metadata by default. | Track deterministic writer metadata/trailer-ID normalization separately. |
| dictionary ordering | Feasible to investigate for deterministic output, but risky if it disturbs Java-source comparability or COS update semantics. | Track stable COS dictionary serialization separately. |
| stream filters | Feasible for a small set of writer-controlled streams; should not force Java filter internals onto .NET. | Track stream filter/compression alignment separately. |
| compression | Feasible only where compression level and stream ownership are writer-controlled; byte identity may still vary by deflater implementation. | Track compression alignment with stream filter work. |
| COS object numbering | Lower value and higher coupling risk; object allocation order is an implementation detail unless downstream users require deterministic IDs. | Track as measurement-first investigation. |
| xref layout | Low value to force unless object numbering and serialization are already stable; structural equivalence is usually sufficient. | Track with object numbering/xref investigation. |
| incremental-save behavior | Should be behavior-tested separately because incremental save has user-visible semantics beyond byte identity. | Create a follow-up only if rows show incremental markers. |

## Ratchet Decision

Do not lower `save-structural-match` or `merge-structural-match` in this PR. The strict run is measurement-only; structural equivalence remains the default compatibility contract until targeted writer work converts specific rows to byte identity.

## Examples

### COS object numbering

| Operation | File | Java bytes | .NET bytes | First diff |
|---|---|---:|---:|---:|
| `save` | `4PP-Highlighting.pdf` | 2485 | 2632 | 7 |
| `merge` | `4PP-Highlighting.pdf+AcrobatMerge-DifferentExportValues-WasMaster.pdf` | 37414 | 46924 | 7 |
| `save` | `AcroFormForMerge-DifferentExportValues.pdf` | 18107 | 23084 | 9 |
| `merge` | `AcroFormForMerge-DifferentExportValues.pdf+AcroFormForMerge-DifferentFieldType.pdf` | 31199 | 38918 | 7 |
| `save` | `AcroFormForMerge-DifferentFieldType.pdf` | 17720 | 22615 | 9 |

### compression

| Operation | File | Java bytes | .NET bytes | First diff |
|---|---|---:|---:|---:|
| `save` | `4PP-Highlighting.pdf` | 2485 | 2632 | 7 |
| `merge` | `4PP-Highlighting.pdf+AcrobatMerge-DifferentExportValues-WasMaster.pdf` | 37414 | 46924 | 7 |
| `save` | `AcroFormForMerge-DifferentExportValues.pdf` | 18107 | 23084 | 9 |
| `merge` | `AcroFormForMerge-DifferentExportValues.pdf+AcroFormForMerge-DifferentFieldType.pdf` | 31199 | 38918 | 7 |
| `save` | `AcroFormForMerge-DifferentFieldType.pdf` | 17720 | 22615 | 9 |

### dictionary ordering

| Operation | File | Java bytes | .NET bytes | First diff |
|---|---|---:|---:|---:|
| `save` | `4PP-Highlighting.pdf` | 2485 | 2632 | 7 |
| `merge` | `4PP-Highlighting.pdf+AcrobatMerge-DifferentExportValues-WasMaster.pdf` | 37414 | 46924 | 7 |
| `save` | `AcroFormForMerge-DifferentExportValues.pdf` | 18107 | 23084 | 9 |
| `merge` | `AcroFormForMerge-DifferentExportValues.pdf+AcroFormForMerge-DifferentFieldType.pdf` | 31199 | 38918 | 7 |
| `save` | `AcroFormForMerge-DifferentFieldType.pdf` | 17720 | 22615 | 9 |

### incremental-save behavior

| Operation | File | Java bytes | .NET bytes | First diff |
|---|---|---:|---:|---:|
| `merge` | `4PP-Highlighting.pdf+AcrobatMerge-DifferentExportValues-WasMaster.pdf` | 37414 | 46924 | 7 |
| `save` | `AcroFormsRotation.pdf` | 19376 | 47378 | 9 |
| `save` | `AcrobatMerge-DifferentExportValues-WasMaster.pdf` | 35036 | 47261 | 9 |
| `save` | `AcrobatMerge-DifferentExportValues.pdf` | 34994 | 47229 | 9 |
| `merge` | `AcrobatMerge-DifferentExportValues.pdf+AcrobatMerge-DifferentFieldType-WasMaster.pdf` | 64515 | 79259 | 7 |

### metadata/timestamps

| Operation | File | Java bytes | .NET bytes | First diff |
|---|---|---:|---:|---:|
| `save` | `4PP-Highlighting.pdf` | 2485 | 2632 | 7 |
| `merge` | `4PP-Highlighting.pdf+AcrobatMerge-DifferentExportValues-WasMaster.pdf` | 37414 | 46924 | 7 |
| `save` | `AcroFormForMerge-DifferentExportValues.pdf` | 18107 | 23084 | 9 |
| `merge` | `AcroFormForMerge-DifferentExportValues.pdf+AcroFormForMerge-DifferentFieldType.pdf` | 31199 | 38918 | 7 |
| `save` | `AcroFormForMerge-DifferentFieldType.pdf` | 17720 | 22615 | 9 |

### stream filters

| Operation | File | Java bytes | .NET bytes | First diff |
|---|---|---:|---:|---:|
| `save` | `4PP-Highlighting.pdf` | 2485 | 2632 | 7 |
| `merge` | `4PP-Highlighting.pdf+AcrobatMerge-DifferentExportValues-WasMaster.pdf` | 37414 | 46924 | 7 |
| `save` | `AcroFormForMerge-DifferentExportValues.pdf` | 18107 | 23084 | 9 |
| `merge` | `AcroFormForMerge-DifferentExportValues.pdf+AcroFormForMerge-DifferentFieldType.pdf` | 31199 | 38918 | 7 |
| `save` | `AcroFormForMerge-DifferentFieldType.pdf` | 17720 | 22615 | 9 |

### xref layout

| Operation | File | Java bytes | .NET bytes | First diff |
|---|---|---:|---:|---:|
| `save` | `4PP-Highlighting.pdf` | 2485 | 2632 | 7 |
| `merge` | `4PP-Highlighting.pdf+AcrobatMerge-DifferentExportValues-WasMaster.pdf` | 37414 | 46924 | 7 |
| `save` | `AcroFormForMerge-DifferentExportValues.pdf` | 18107 | 23084 | 9 |
| `merge` | `AcroFormForMerge-DifferentExportValues.pdf+AcroFormForMerge-DifferentFieldType.pdf` | 31199 | 38918 | 7 |
| `save` | `AcroFormForMerge-DifferentFieldType.pdf` | 17720 | 22615 | 9 |

## Byte-Identical Rows

| Operation | File | Bytes |
|---|---|---:|
| `save` | `PDFBOX-2725-878725.pdf` | 0 |
| `save` | `encrypted-owner-restricted.pdf` | 0 |

