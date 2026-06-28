# Save/Merge Dictionary Serialization Strategy

Issue: #552

## Findings

Apache PDFBox stores `COSDictionary` entries in a `LinkedHashMap`, so dictionary keys are serialized in insertion order. The .NET port was already enumerating in insertion order in practice because modern `Dictionary<TKey,TValue>` preserves insertion order, but that was an implementation detail rather than an explicit port contract.

`COSDictionary` now uses `OrderedDictionary<COSName, COSBase>` internally. This is a writer-local implementation hardening that mirrors Java `LinkedHashMap` semantics without changing public APIs or sorting keys globally. Updating an existing key keeps its original position, matching the expected insertion-order behavior.

Global key sorting was rejected. Java PDFBox does not sort COS dictionary keys during normal serialization, and sorting would reduce Java-source comparability while still not addressing the corpus differences measured below.

## Analyzer Rerun

The byte-identity analyzer was extended with dictionary sequence diagnostics and rerun against the latest post-#551 runtime-parity artifact: PR #570 run `28336513539`.

| Operation | Dictionary-label rows | Pure order-permutation rows | Rows with dictionary-count differences | Rows with key-set differences | Order-only pair mismatches | Key-set pair mismatches | Max dictionary-count delta |
|---|---:|---:|---:|---:|---:|---:|---:|
| `merge` | 72 | 0 | 72 | 72 | 0 | 5020 | 3076 |
| `save` | 148 | 0 | 148 | 148 | 0 | 3921 | 3182 |

The original #539 dictionary-ordering counts were `merge=72` and `save=148`; the rerun still reports `merge=72` and `save=148`, so the count delta is zero. The new diagnostics explain why: the analyzer is not seeing simple key-order permutations. It is mostly detecting different dictionary populations and different numbers of serialized dictionaries, which are downstream effects of object numbering, stream/compression choices, merge cloning, and xref/trailer layout.

## Gate Decision

Structural equivalence remains the compatibility contract. Dictionary insertion order is now explicit and Java-aligned, but strict byte identity should wait for the object-layout and stream/compression follow-ups.
