#!/usr/bin/env python3
"""Measure strict save/merge byte identity from a runtime parity output directory."""

from __future__ import annotations

import argparse
import hashlib
import json
import re
from collections import Counter, defaultdict
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


SAVE_MERGE_OPS = {"save", "merge"}
CAUSE_ORDER = [
    "incremental-save behavior",
    "metadata/timestamps",
    "stream filters",
    "compression",
    "COS object numbering",
    "xref layout",
    "dictionary ordering",
    "other writer serialization",
]

OBJECT_RE = re.compile(rb"(?m)(\d+)\s+(\d+)\s+obj\b")
STARTXREF_RE = re.compile(rb"startxref\s+(\d+)")
DICT_RE = re.compile(rb"<<(.*?)>>", re.DOTALL)
NAME_RE = re.compile(rb"/([A-Za-z0-9_.+-]+)")
DATE_RE = re.compile(rb"/(?:CreationDate|ModDate)\s*(?:\((?:\\.|[^\\)])*\)|<[0-9A-Fa-f]+>)")
INFO_RE = re.compile(
    rb"/(?:Producer|Creator|Author|Title|Subject|Keywords)\s*(?:\((?:\\.|[^\\)])*\)|<[0-9A-Fa-f]+>|/[A-Za-z0-9_.+-]+)"
)
ID_RE = re.compile(rb"/ID\s*\[(.*?)\]", re.DOTALL)
FILTER_RE = re.compile(rb"/Filter\s*(?:/([A-Za-z0-9_.+-]+)|\[(.*?)\])", re.DOTALL)
LENGTH_RE = re.compile(rb"/Length\s+(\d+)\b")
STREAM_RE = re.compile(rb"(?:^|\r?\n)stream\r?\n")
METADATA_NORMALIZATION_PATTERNS = (
    (DATE_RE, b"/PdfBoxNetNormalizedDate (D:00000000000000+00'00')"),
    (INFO_RE, b"/PdfBoxNetNormalizedInfo (pdfbox-net-normalized-info)"),
    (ID_RE, b"/ID [<pdfbox-net-normalized-id> <pdfbox-net-normalized-id>]"),
)


@dataclass(frozen=True)
class PairAnalysis:
    file: str
    op: str
    category: str
    byte_identical: bool
    java_size: int
    dotnet_size: int
    java_sha256: str
    dotnet_sha256: str
    first_diff_offset: int | None
    causes: tuple[str, ...]
    metadata_differences: tuple[str, ...]
    metadata_normalized_byte_identical: bool
    metadata_normalized_first_diff_offset: int | None
    dictionary_count_delta: int
    dictionary_order_only_mismatches: int
    dictionary_key_set_mismatches: int
    java_artifact: str
    dotnet_artifact: str

    @property
    def primary_cause(self) -> str:
        for cause in CAUSE_ORDER:
            if cause in self.causes:
                return cause
        return "byte-identical" if self.byte_identical else "other writer serialization"


def sha256_hex(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def first_diff_offset(left: bytes, right: bytes) -> int | None:
    limit = min(len(left), len(right))
    for i in range(limit):
        if left[i] != right[i]:
            return i
    return None if len(left) == len(right) else limit


def object_ids(data: bytes) -> list[tuple[int, int]]:
    return [(int(obj), int(gen)) for obj, gen in OBJECT_RE.findall(data)]


def startxref_offsets(data: bytes) -> list[int]:
    return [int(value) for value in STARTXREF_RE.findall(data)]


def dictionary_key_sequences(data: bytes) -> list[tuple[str, ...]]:
    sequences: list[tuple[str, ...]] = []
    for body in DICT_RE.findall(data):
        names = [name.decode("latin1", "replace") for name in NAME_RE.findall(body)]
        if names:
            sequences.append(tuple(names))
    return sequences


def dictionary_sequence_diagnostics(java_data: bytes, dotnet_data: bytes) -> tuple[int, int, int]:
    java_sequences = dictionary_key_sequences(java_data)
    dotnet_sequences = dictionary_key_sequences(dotnet_data)
    dictionary_count_delta = abs(len(java_sequences) - len(dotnet_sequences))
    order_only_mismatches = 0
    key_set_mismatches = 0

    for java_sequence, dotnet_sequence in zip(java_sequences, dotnet_sequences):
        if java_sequence == dotnet_sequence:
            continue
        if Counter(java_sequence) == Counter(dotnet_sequence):
            order_only_mismatches += 1
        else:
            key_set_mismatches += 1

    return dictionary_count_delta, order_only_mismatches, key_set_mismatches


def pattern_tokens(data: bytes, pattern: re.Pattern[bytes]) -> set[bytes]:
    return {match.group(0) for match in pattern.finditer(data)}


def metadata_tokens(data: bytes) -> set[bytes]:
    tokens: set[bytes] = set()
    for pattern in (DATE_RE, INFO_RE, ID_RE):
        tokens.update(pattern_tokens(data, pattern))
    return tokens


def metadata_differences(java_data: bytes, dotnet_data: bytes) -> tuple[str, ...]:
    differences: list[str] = []
    if pattern_tokens(java_data, DATE_RE) != pattern_tokens(dotnet_data, DATE_RE):
        differences.append("dates")
    if pattern_tokens(java_data, INFO_RE) != pattern_tokens(dotnet_data, INFO_RE):
        differences.append("info fields")
    if pattern_tokens(java_data, ID_RE) != pattern_tokens(dotnet_data, ID_RE):
        differences.append("trailer IDs")
    return tuple(differences)


def normalize_metadata_tokens(data: bytes) -> bytes:
    normalized = data
    for pattern, replacement in METADATA_NORMALIZATION_PATTERNS:
        normalized = pattern.sub(replacement, normalized)
    return normalized


def filter_tokens(data: bytes) -> Counter:
    tokens: Counter = Counter()
    for single, array in FILTER_RE.findall(data):
        if single:
            tokens[single.decode("latin1", "replace")] += 1
        if array:
            for name in NAME_RE.findall(array):
                tokens[name.decode("latin1", "replace")] += 1
    return tokens


def compression_signature(data: bytes) -> dict[str, object]:
    return {
        "stream_count": len(STREAM_RE.findall(data)),
        "lengths": tuple(int(value) for value in LENGTH_RE.findall(data)),
        "flate_count": data.count(b"/FlateDecode"),
        "objstm_count": data.count(b"/ObjStm"),
    }


def has_incremental_markers(data: bytes) -> bool:
    return data.count(b"%%EOF") > 1 or b"/Prev" in data


def causes_for(java_data: bytes, dotnet_data: bytes) -> tuple[str, ...]:
    if java_data == dotnet_data:
        return ()

    causes: list[str] = []
    if has_incremental_markers(java_data) != has_incremental_markers(dotnet_data):
        causes.append("incremental-save behavior")
    elif has_incremental_markers(java_data) and has_incremental_markers(dotnet_data):
        causes.append("incremental-save behavior")

    if metadata_tokens(java_data) != metadata_tokens(dotnet_data):
        causes.append("metadata/timestamps")

    if filter_tokens(java_data) != filter_tokens(dotnet_data):
        causes.append("stream filters")

    if compression_signature(java_data) != compression_signature(dotnet_data):
        causes.append("compression")

    if object_ids(java_data) != object_ids(dotnet_data):
        causes.append("COS object numbering")

    if startxref_offsets(java_data) != startxref_offsets(dotnet_data):
        causes.append("xref layout")

    if dictionary_key_sequences(java_data) != dictionary_key_sequences(dotnet_data):
        causes.append("dictionary ordering")

    if not causes:
        causes.append("other writer serialization")
    return tuple(causes)


def artifact_path(out_dir: Path, row: dict, runtime: str) -> Path | None:
    artifacts = row.get("artifacts")
    if not isinstance(artifacts, dict):
        return None
    value = artifacts.get(runtime)
    if not isinstance(value, str):
        return None
    return out_dir / value


def analyze_row(out_dir: Path, row: dict) -> PairAnalysis | None:
    op = str(row.get("op", ""))
    if op not in SAVE_MERGE_OPS:
        return None
    java_path = artifact_path(out_dir, row, "java")
    dotnet_path = artifact_path(out_dir, row, "dotnet")
    if java_path is None or dotnet_path is None or not java_path.exists() or not dotnet_path.exists():
        return None

    java_data = java_path.read_bytes()
    dotnet_data = dotnet_path.read_bytes()
    byte_identical = java_data == dotnet_data
    java_metadata_normalized = normalize_metadata_tokens(java_data)
    dotnet_metadata_normalized = normalize_metadata_tokens(dotnet_data)
    dictionary_count_delta, dictionary_order_only_mismatches, dictionary_key_set_mismatches = dictionary_sequence_diagnostics(
        java_data, dotnet_data
    )
    return PairAnalysis(
        file=str(row.get("file", "")),
        op=op,
        category=str(row.get("category", "")),
        byte_identical=byte_identical,
        java_size=len(java_data),
        dotnet_size=len(dotnet_data),
        java_sha256=sha256_hex(java_data),
        dotnet_sha256=sha256_hex(dotnet_data),
        first_diff_offset=first_diff_offset(java_data, dotnet_data),
        causes=causes_for(java_data, dotnet_data),
        metadata_differences=metadata_differences(java_data, dotnet_data),
        metadata_normalized_byte_identical=java_metadata_normalized == dotnet_metadata_normalized,
        metadata_normalized_first_diff_offset=first_diff_offset(java_metadata_normalized, dotnet_metadata_normalized),
        dictionary_count_delta=dictionary_count_delta,
        dictionary_order_only_mismatches=dictionary_order_only_mismatches,
        dictionary_key_set_mismatches=dictionary_key_set_mismatches,
        java_artifact=java_path.relative_to(out_dir).as_posix(),
        dotnet_artifact=dotnet_path.relative_to(out_dir).as_posix(),
    )


def load_analyses(out_dir: Path) -> tuple[dict, list[PairAnalysis]]:
    comparison_path = out_dir / "comparison.json"
    payload = json.loads(comparison_path.read_text(encoding="utf-8"))
    rows = payload.get("rows", [])
    if not isinstance(rows, list):
        raise ValueError("comparison.json rows must be a list")
    analyses = [analysis for row in rows if isinstance(row, dict) for analysis in [analyze_row(out_dir, row)] if analysis is not None]
    return payload, analyses


def count_by_op(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    counts: dict[str, dict[str, int]] = {}
    for op in sorted(SAVE_MERGE_OPS):
        rows = [row for row in analyses if row.op == op]
        counts[op] = {
            "total": len(rows),
            "byteIdentical": sum(1 for row in rows if row.byte_identical),
            "byteDifferent": sum(1 for row in rows if not row.byte_identical),
        }
    return counts


def cause_counts(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    by_op: dict[str, Counter] = defaultdict(Counter)
    for row in analyses:
        if row.byte_identical:
            by_op[row.op]["byte-identical"] += 1
            continue
        for cause in row.causes:
            by_op[row.op][cause] += 1
    return {op: dict(counter) for op, counter in sorted(by_op.items())}


def metadata_difference_counts(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    by_op: dict[str, Counter] = defaultdict(Counter)
    for row in analyses:
        if row.byte_identical:
            by_op[row.op]["byte-identical"] += 1
            continue
        if not row.metadata_differences:
            by_op[row.op]["none"] += 1
            continue
        for difference in row.metadata_differences:
            by_op[row.op][difference] += 1
    return {op: dict(counter) for op, counter in sorted(by_op.items())}


def metadata_normalization_counts(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    counts: dict[str, dict[str, int]] = {}
    for op in sorted(SAVE_MERGE_OPS):
        rows = [row for row in analyses if row.op == op]
        normalized_identical = sum(1 for row in rows if row.metadata_normalized_byte_identical)
        counts[op] = {
            "total": len(rows),
            "byteIdentical": sum(1 for row in rows if row.byte_identical),
            "metadataNormalizedByteIdentical": normalized_identical,
            "madeIdenticalByMetadataNormalization": sum(
                1 for row in rows if not row.byte_identical and row.metadata_normalized_byte_identical
            ),
            "stillDifferentAfterMetadataNormalization": len(rows) - normalized_identical,
        }
    return counts


def dictionary_comparison_counts(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    counts: dict[str, dict[str, int]] = {}
    for op in sorted(SAVE_MERGE_OPS):
        rows = [row for row in analyses if row.op == op]
        dictionary_rows = [row for row in rows if "dictionary ordering" in row.causes]
        counts[op] = {
            "rowsWithDictionaryOrderingCause": len(dictionary_rows),
            "rowsWithOnlyOrderPermutation": sum(
                1
                for row in dictionary_rows
                if row.dictionary_count_delta == 0
                and row.dictionary_key_set_mismatches == 0
                and row.dictionary_order_only_mismatches > 0
            ),
            "rowsWithDictionaryCountDifference": sum(1 for row in dictionary_rows if row.dictionary_count_delta > 0),
            "rowsWithDictionaryKeySetDifference": sum(1 for row in dictionary_rows if row.dictionary_key_set_mismatches > 0),
            "orderOnlyDictionaryPairMismatches": sum(row.dictionary_order_only_mismatches for row in dictionary_rows),
            "keySetDictionaryPairMismatches": sum(row.dictionary_key_set_mismatches for row in dictionary_rows),
            "maxDictionaryCountDelta": max((row.dictionary_count_delta for row in dictionary_rows), default=0),
        }
    return counts


def primary_cause_counts(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    by_op: dict[str, Counter] = defaultdict(Counter)
    for row in analyses:
        by_op[row.op][row.primary_cause] += 1
    return {op: dict(counter) for op, counter in sorted(by_op.items())}


def examples_by_cause(analyses: Iterable[PairAnalysis], limit: int = 5) -> dict[str, list[PairAnalysis]]:
    examples: dict[str, list[PairAnalysis]] = defaultdict(list)
    for row in analyses:
        if row.byte_identical:
            continue
        for cause in row.causes:
            if len(examples[cause]) < limit:
                examples[cause].append(row)
    return dict(sorted(examples.items()))


def json_payload(out_dir: Path, comparison_payload: dict, analyses: list[PairAnalysis], source_label: str | None) -> dict:
    return {
        "schema": 1,
        "source": {
            "label": source_label,
            "outDir": out_dir.as_posix(),
            "comparisonGeneratedAtUtc": comparison_payload.get("summary", {}).get("generatedAtUtc"),
            "manifest": comparison_payload.get("summary", {}).get("manifest"),
            "pdfCount": comparison_payload.get("summary", {}).get("pdfCount"),
            "mergePairCount": comparison_payload.get("summary", {}).get("mergePairCount"),
        },
        "summary": {
            "byOperation": count_by_op(analyses),
            "causeCounts": cause_counts(analyses),
            "metadataDifferenceCounts": metadata_difference_counts(analyses),
            "metadataNormalization": metadata_normalization_counts(analyses),
            "dictionaryComparison": dictionary_comparison_counts(analyses),
            "primaryCauseCounts": primary_cause_counts(analyses),
        },
        "rows": [
            {
                "file": row.file,
                "op": row.op,
                "category": row.category,
                "byteIdentical": row.byte_identical,
                "javaSize": row.java_size,
                "dotnetSize": row.dotnet_size,
                "javaSha256": row.java_sha256,
                "dotnetSha256": row.dotnet_sha256,
                "firstDiffOffset": row.first_diff_offset,
                "causes": list(row.causes),
                "metadataDifferences": list(row.metadata_differences),
                "metadataNormalizedByteIdentical": row.metadata_normalized_byte_identical,
                "metadataNormalizedFirstDiffOffset": row.metadata_normalized_first_diff_offset,
                "dictionaryCountDelta": row.dictionary_count_delta,
                "dictionaryOrderOnlyMismatches": row.dictionary_order_only_mismatches,
                "dictionaryKeySetMismatches": row.dictionary_key_set_mismatches,
                "primaryCause": row.primary_cause,
                "artifacts": {"java": row.java_artifact, "dotnet": row.dotnet_artifact},
            }
            for row in analyses
        ],
    }


def markdown_report(payload: dict, analyses: list[PairAnalysis]) -> str:
    source = payload["source"]
    summary = payload["summary"]
    lines = [
        "# Save/Merge Byte Identity Measurement",
        "",
        "Issue: #539",
        "",
    ]
    if source.get("label"):
        lines.append(f"Source: {source['label']}")
    lines.extend(
        [
            f"Runtime parity output: `{source['outDir']}`",
            f"Runtime comparison generated UTC: `{source.get('comparisonGeneratedAtUtc')}`",
            f"Manifest: `{source.get('manifest')}`",
            "",
            "## Strict Identity Counts",
            "",
            "| Operation | Total rows | Byte-identical | Byte-different |",
            "|---|---:|---:|---:|",
        ]
    )
    for op, counts in summary["byOperation"].items():
        lines.append(f"| `{op}` | {counts['total']} | {counts['byteIdentical']} | {counts['byteDifferent']} |")

    lines.extend(
        [
            "",
            "## Writer Cause Counts",
            "",
            "Rows can have more than one cause label. Cause labels are heuristics over serialized PDF artifacts; they identify writer areas to inspect, not exclusive root causes.",
            "",
        ]
    )
    for op, counts in summary["causeCounts"].items():
        lines.append(f"### `{op}`")
        lines.append("")
        lines.append("| Cause | Rows |")
        lines.append("|---|---:|")
        for cause, count in sorted(counts.items(), key=lambda item: (-item[1], item[0])):
            lines.append(f"| {cause} | {count} |")
        lines.append("")

    lines.extend(
        [
            "## Metadata and Trailer-ID Breakdown",
            "",
            "The broad `metadata/timestamps` label is split into serialized document-info date fields, serialized document-info string/name fields, and trailer `/ID` arrays.",
            "",
        ]
    )
    for op, counts in summary["metadataDifferenceCounts"].items():
        lines.append(f"### `{op}` Metadata Differences")
        lines.append("")
        lines.append("| Difference | Rows |")
        lines.append("|---|---:|")
        for difference, count in sorted(counts.items(), key=lambda item: (-item[1], item[0])):
            lines.append(f"| {difference} | {count} |")
        lines.append("")

    lines.extend(
        [
            "### Metadata-Normalized Identity",
            "",
            "This replaces only `/CreationDate`, `/ModDate`, document-info text/name fields, and trailer `/ID` payloads with stable placeholders before comparing bytes.",
            "",
            "| Operation | Total rows | Original byte-identical | Metadata-normalized byte-identical | Made identical by normalization | Still different after normalization |",
            "|---|---:|---:|---:|---:|---:|",
        ]
    )
    for op, counts in summary["metadataNormalization"].items():
        lines.append(
            f"| `{op}` | {counts['total']} | {counts['byteIdentical']} | "
            f"{counts['metadataNormalizedByteIdentical']} | {counts['madeIdenticalByMetadataNormalization']} | "
            f"{counts['stillDifferentAfterMetadataNormalization']} |"
        )
    lines.append("")

    lines.extend(
        [
            "## Dictionary Sequence Breakdown",
            "",
            "The `dictionary ordering` label compares serialized dictionary key sequences. This breakdown separates pure order permutations from rows where dictionary counts or key sets differ, which usually means earlier writer decisions changed content or object layout.",
            "",
            "| Operation | Rows with dictionary label | Rows with only order permutations | Rows with dictionary-count differences | Rows with key-set differences | Order-only pair mismatches | Key-set pair mismatches | Max dictionary-count delta |",
            "|---|---:|---:|---:|---:|---:|---:|---:|",
        ]
    )
    for op, counts in summary["dictionaryComparison"].items():
        lines.append(
            f"| `{op}` | {counts['rowsWithDictionaryOrderingCause']} | "
            f"{counts['rowsWithOnlyOrderPermutation']} | {counts['rowsWithDictionaryCountDifference']} | "
            f"{counts['rowsWithDictionaryKeySetDifference']} | {counts['orderOnlyDictionaryPairMismatches']} | "
            f"{counts['keySetDictionaryPairMismatches']} | {counts['maxDictionaryCountDelta']} |"
        )
    lines.append("")

    lines.extend(
        [
            "## Feasibility Assessment",
            "",
            "| Cause | Judgment | Follow-up |",
            "|---|---|---|",
            "| metadata/timestamps | Feasible when values are generated by the writer or test harness; preserve source document metadata by default. | Track deterministic writer metadata/trailer-ID normalization separately. |",
            "| dictionary ordering | Feasible to investigate for deterministic output, but risky if it disturbs Java-source comparability or COS update semantics. | Track stable COS dictionary serialization separately. |",
            "| stream filters | Feasible for a small set of writer-controlled streams; should not force Java filter internals onto .NET. | Track stream filter/compression alignment separately. |",
            "| compression | Feasible only where compression level and stream ownership are writer-controlled; byte identity may still vary by deflater implementation. | Track compression alignment with stream filter work. |",
            "| COS object numbering | Lower value and higher coupling risk; object allocation order is an implementation detail unless downstream users require deterministic IDs. | Track as measurement-first investigation. |",
            "| xref layout | Low value to force unless object numbering and serialization are already stable; structural equivalence is usually sufficient. | Track with object numbering/xref investigation. |",
            "| incremental-save behavior | Should be behavior-tested separately because incremental save has user-visible semantics beyond byte identity. | Create a follow-up only if rows show incremental markers. |",
            "",
            "## Ratchet Decision",
            "",
            "Do not lower `save-structural-match` or `merge-structural-match` in this PR. The strict run is measurement-only; structural equivalence remains the default compatibility contract until targeted writer work converts specific rows to byte identity.",
            "",
            "## Examples",
            "",
        ]
    )
    examples = examples_by_cause(analyses)
    for cause, rows in examples.items():
        lines.append(f"### {cause}")
        lines.append("")
        lines.append("| Operation | File | Java bytes | .NET bytes | First diff |")
        lines.append("|---|---|---:|---:|---:|")
        for row in rows:
            diff = "" if row.first_diff_offset is None else str(row.first_diff_offset)
            lines.append(f"| `{row.op}` | `{row.file}` | {row.java_size} | {row.dotnet_size} | {diff} |")
        lines.append("")

    byte_identical = [row for row in analyses if row.byte_identical]
    if byte_identical:
        lines.append("## Byte-Identical Rows")
        lines.append("")
        lines.append("| Operation | File | Bytes |")
        lines.append("|---|---|---:|")
        for row in byte_identical[:25]:
            lines.append(f"| `{row.op}` | `{row.file}` | {row.java_size} |")
        if len(byte_identical) > 25:
            lines.append(f"| ... | {len(byte_identical) - 25} additional rows omitted | |")
        lines.append("")

    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Analyze strict save/merge byte identity from runtime parity artifacts.")
    parser.add_argument("--out-dir", required=True, type=Path, help="Runtime parity output directory containing comparison.json.")
    parser.add_argument("--report", required=True, type=Path, help="Markdown report path to write.")
    parser.add_argument("--json", dest="json_path", type=Path, help="Optional machine-readable JSON report path.")
    parser.add_argument("--source-label", help="Human-readable source label to include in the report, such as a CI run or PR number.")
    args = parser.parse_args()

    out_dir = args.out_dir.resolve()
    comparison_payload, analyses = load_analyses(out_dir)
    payload = json_payload(Path(args.out_dir), comparison_payload, analyses, args.source_label)

    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(markdown_report(payload, analyses), encoding="utf-8")
    if args.json_path is not None:
        args.json_path.parent.mkdir(parents=True, exist_ok=True)
        args.json_path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")

    print(json.dumps(payload["summary"], indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
