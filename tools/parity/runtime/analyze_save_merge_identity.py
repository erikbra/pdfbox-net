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
XREF_TABLE_RE = re.compile(rb"(?m)^xref\s*$")
XREF_STREAM_RE = re.compile(rb"/Type\s*/XRef\b")
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
    stream_count_delta: int
    length_count_delta: int
    flate_count_delta: int
    objstm_count_delta: int
    filter_tokens_differ: bool
    length_sequence_differ: bool
    object_count_delta: int
    object_id_sequence_differ: bool
    object_id_set_differ: bool
    generation_sequence_differ: bool
    nonzero_generation_count_delta: int
    startxref_count_delta: int
    startxref_values_differ: bool
    last_startxref_delta: int | None
    java_eof_count: int
    dotnet_eof_count: int
    java_prev_count: int
    dotnet_prev_count: int
    java_has_incremental_markers: bool
    dotnet_has_incremental_markers: bool
    xref_table_count_delta: int
    xref_stream_count_delta: int
    prev_count_delta: int
    xref_style_differ: bool
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


def stream_compression_diagnostics(java_data: bytes, dotnet_data: bytes) -> tuple[int, int, int, int, bool, bool]:
    java_signature = compression_signature(java_data)
    dotnet_signature = compression_signature(dotnet_data)
    java_lengths = java_signature["lengths"]
    dotnet_lengths = dotnet_signature["lengths"]
    if not isinstance(java_lengths, tuple) or not isinstance(dotnet_lengths, tuple):
        raise TypeError("compression_signature lengths must be tuples")

    return (
        abs(int(java_signature["stream_count"]) - int(dotnet_signature["stream_count"])),
        abs(len(java_lengths) - len(dotnet_lengths)),
        abs(int(java_signature["flate_count"]) - int(dotnet_signature["flate_count"])),
        abs(int(java_signature["objstm_count"]) - int(dotnet_signature["objstm_count"])),
        filter_tokens(java_data) != filter_tokens(dotnet_data),
        java_lengths != dotnet_lengths,
    )


def object_xref_diagnostics(
    java_data: bytes,
    dotnet_data: bytes,
) -> tuple[int, bool, bool, bool, int, int, bool, int | None, int, int, int, bool]:
    java_ids = object_ids(java_data)
    dotnet_ids = object_ids(dotnet_data)
    java_startxrefs = startxref_offsets(java_data)
    dotnet_startxrefs = startxref_offsets(dotnet_data)
    java_xref_table_count = len(XREF_TABLE_RE.findall(java_data))
    dotnet_xref_table_count = len(XREF_TABLE_RE.findall(dotnet_data))
    java_xref_stream_count = len(XREF_STREAM_RE.findall(java_data))
    dotnet_xref_stream_count = len(XREF_STREAM_RE.findall(dotnet_data))
    java_has_xref_stream = java_xref_stream_count > 0
    dotnet_has_xref_stream = dotnet_xref_stream_count > 0
    java_has_xref_table = java_xref_table_count > 0
    dotnet_has_xref_table = dotnet_xref_table_count > 0
    last_startxref_delta = (
        abs(java_startxrefs[-1] - dotnet_startxrefs[-1])
        if java_startxrefs and dotnet_startxrefs
        else None
    )

    return (
        abs(len(java_ids) - len(dotnet_ids)),
        java_ids != dotnet_ids,
        Counter(java_ids) != Counter(dotnet_ids),
        tuple(generation for _, generation in java_ids) != tuple(generation for _, generation in dotnet_ids),
        abs(sum(1 for _, generation in java_ids if generation != 0) - sum(1 for _, generation in dotnet_ids if generation != 0)),
        abs(len(java_startxrefs) - len(dotnet_startxrefs)),
        java_startxrefs != dotnet_startxrefs,
        last_startxref_delta,
        abs(java_xref_table_count - dotnet_xref_table_count),
        abs(java_xref_stream_count - dotnet_xref_stream_count),
        abs(java_data.count(b"/Prev") - dotnet_data.count(b"/Prev")),
        (java_has_xref_stream, java_has_xref_table) != (dotnet_has_xref_stream, dotnet_has_xref_table),
    )


def has_incremental_markers(data: bytes) -> bool:
    return data.count(b"%%EOF") > 1 or b"/Prev" in data


def incremental_marker_signature(data: bytes) -> tuple[int, int, bool]:
    eof_count = data.count(b"%%EOF")
    prev_count = data.count(b"/Prev")
    return eof_count, prev_count, eof_count > 1 or prev_count > 0


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
    (
        stream_count_delta,
        length_count_delta,
        flate_count_delta,
        objstm_count_delta,
        filter_tokens_differ,
        length_sequence_differ,
    ) = stream_compression_diagnostics(java_data, dotnet_data)
    (
        object_count_delta,
        object_id_sequence_differ,
        object_id_set_differ,
        generation_sequence_differ,
        nonzero_generation_count_delta,
        startxref_count_delta,
        startxref_values_differ,
        last_startxref_delta,
        xref_table_count_delta,
        xref_stream_count_delta,
        prev_count_delta,
        xref_style_differ,
    ) = object_xref_diagnostics(java_data, dotnet_data)
    java_eof_count, java_prev_count, java_has_incremental_markers = incremental_marker_signature(java_data)
    dotnet_eof_count, dotnet_prev_count, dotnet_has_incremental_markers = incremental_marker_signature(dotnet_data)
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
        stream_count_delta=stream_count_delta,
        length_count_delta=length_count_delta,
        flate_count_delta=flate_count_delta,
        objstm_count_delta=objstm_count_delta,
        filter_tokens_differ=filter_tokens_differ,
        length_sequence_differ=length_sequence_differ,
        object_count_delta=object_count_delta,
        object_id_sequence_differ=object_id_sequence_differ,
        object_id_set_differ=object_id_set_differ,
        generation_sequence_differ=generation_sequence_differ,
        nonzero_generation_count_delta=nonzero_generation_count_delta,
        startxref_count_delta=startxref_count_delta,
        startxref_values_differ=startxref_values_differ,
        last_startxref_delta=last_startxref_delta,
        java_eof_count=java_eof_count,
        dotnet_eof_count=dotnet_eof_count,
        java_prev_count=java_prev_count,
        dotnet_prev_count=dotnet_prev_count,
        java_has_incremental_markers=java_has_incremental_markers,
        dotnet_has_incremental_markers=dotnet_has_incremental_markers,
        xref_table_count_delta=xref_table_count_delta,
        xref_stream_count_delta=xref_stream_count_delta,
        prev_count_delta=prev_count_delta,
        xref_style_differ=xref_style_differ,
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


def stream_compression_comparison_counts(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    counts: dict[str, dict[str, int]] = {}
    for op in sorted(SAVE_MERGE_OPS):
        rows = [row for row in analyses if row.op == op]
        stream_rows = [row for row in rows if "stream filters" in row.causes or "compression" in row.causes]
        counts[op] = {
            "rowsWithStreamFilterCause": sum(1 for row in rows if "stream filters" in row.causes),
            "rowsWithCompressionCause": sum(1 for row in rows if "compression" in row.causes),
            "rowsWithFilterTokenDifference": sum(1 for row in stream_rows if row.filter_tokens_differ),
            "rowsWithStreamCountDifference": sum(1 for row in stream_rows if row.stream_count_delta > 0),
            "rowsWithLengthCountDifference": sum(1 for row in stream_rows if row.length_count_delta > 0),
            "rowsWithLengthSequenceDifference": sum(1 for row in stream_rows if row.length_sequence_differ),
            "rowsWithFlateCountDifference": sum(1 for row in stream_rows if row.flate_count_delta > 0),
            "rowsWithObjStmCountDifference": sum(1 for row in stream_rows if row.objstm_count_delta > 0),
            "maxStreamCountDelta": max((row.stream_count_delta for row in stream_rows), default=0),
            "maxLengthCountDelta": max((row.length_count_delta for row in stream_rows), default=0),
            "maxFlateCountDelta": max((row.flate_count_delta for row in stream_rows), default=0),
            "maxObjStmCountDelta": max((row.objstm_count_delta for row in stream_rows), default=0),
        }
    return counts


def object_xref_comparison_counts(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    counts: dict[str, dict[str, int]] = {}
    for op in sorted(SAVE_MERGE_OPS):
        rows = [row for row in analyses if row.op == op]
        object_xref_rows = [
            row for row in rows if "COS object numbering" in row.causes or "xref layout" in row.causes
        ]
        startxref_deltas = [
            row.last_startxref_delta for row in object_xref_rows if row.last_startxref_delta is not None
        ]
        counts[op] = {
            "rowsWithObjectNumberingCause": sum(1 for row in rows if "COS object numbering" in row.causes),
            "rowsWithXrefLayoutCause": sum(1 for row in rows if "xref layout" in row.causes),
            "rowsWithObjectIdSequenceDifference": sum(1 for row in object_xref_rows if row.object_id_sequence_differ),
            "rowsWithObjectIdSetDifference": sum(1 for row in object_xref_rows if row.object_id_set_differ),
            "rowsWithObjectCountDifference": sum(1 for row in object_xref_rows if row.object_count_delta > 0),
            "rowsWithGenerationSequenceDifference": sum(1 for row in object_xref_rows if row.generation_sequence_differ),
            "rowsWithNonzeroGenerationCountDifference": sum(
                1 for row in object_xref_rows if row.nonzero_generation_count_delta > 0
            ),
            "rowsWithStartxrefCountDifference": sum(1 for row in object_xref_rows if row.startxref_count_delta > 0),
            "rowsWithStartxrefValueDifference": sum(1 for row in object_xref_rows if row.startxref_values_differ),
            "rowsWithXrefTableCountDifference": sum(1 for row in object_xref_rows if row.xref_table_count_delta > 0),
            "rowsWithXrefStreamCountDifference": sum(1 for row in object_xref_rows if row.xref_stream_count_delta > 0),
            "rowsWithPrevCountDifference": sum(1 for row in object_xref_rows if row.prev_count_delta > 0),
            "rowsWithXrefStyleDifference": sum(1 for row in object_xref_rows if row.xref_style_differ),
            "maxObjectCountDelta": max((row.object_count_delta for row in object_xref_rows), default=0),
            "maxLastStartxrefDelta": max(startxref_deltas, default=0),
            "maxXrefStreamCountDelta": max((row.xref_stream_count_delta for row in object_xref_rows), default=0),
        }
    return counts


def incremental_marker_comparison_counts(analyses: Iterable[PairAnalysis]) -> dict[str, dict[str, int]]:
    counts: dict[str, dict[str, int]] = {}
    for op in sorted(SAVE_MERGE_OPS):
        rows = [row for row in analyses if row.op == op]
        incremental_rows = [row for row in rows if "incremental-save behavior" in row.causes]
        counts[op] = {
            "rowsWithIncrementalSaveCause": len(incremental_rows),
            "rowsWhereJavaHasMarkers": sum(1 for row in incremental_rows if row.java_has_incremental_markers),
            "rowsWhereDotnetHasMarkers": sum(1 for row in incremental_rows if row.dotnet_has_incremental_markers),
            "rowsWhereBothHaveMarkers": sum(
                1 for row in incremental_rows if row.java_has_incremental_markers and row.dotnet_has_incremental_markers
            ),
            "rowsWhereMarkerPresenceDiffers": sum(
                1
                for row in incremental_rows
                if row.java_has_incremental_markers != row.dotnet_has_incremental_markers
            ),
            "rowsWithEofCountDifference": sum(
                1 for row in incremental_rows if row.java_eof_count != row.dotnet_eof_count
            ),
            "rowsWithPrevCountDifference": sum(
                1 for row in incremental_rows if row.java_prev_count != row.dotnet_prev_count
            ),
            "maxEofCountDelta": max(
                (abs(row.java_eof_count - row.dotnet_eof_count) for row in incremental_rows),
                default=0,
            ),
            "maxPrevCountDelta": max(
                (abs(row.java_prev_count - row.dotnet_prev_count) for row in incremental_rows),
                default=0,
            ),
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


def json_payload(
    out_dir: Path,
    comparison_payload: dict,
    analyses: list[PairAnalysis],
    source_label: str | None,
    issue: str,
) -> dict:
    return {
        "schema": 1,
        "source": {
            "issue": issue,
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
            "streamCompressionComparison": stream_compression_comparison_counts(analyses),
            "objectXrefComparison": object_xref_comparison_counts(analyses),
            "incrementalMarkerComparison": incremental_marker_comparison_counts(analyses),
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
                "streamCountDelta": row.stream_count_delta,
                "lengthCountDelta": row.length_count_delta,
                "flateCountDelta": row.flate_count_delta,
                "objStmCountDelta": row.objstm_count_delta,
                "filterTokensDiffer": row.filter_tokens_differ,
                "lengthSequenceDiffer": row.length_sequence_differ,
                "objectCountDelta": row.object_count_delta,
                "objectIdSequenceDiffer": row.object_id_sequence_differ,
                "objectIdSetDiffer": row.object_id_set_differ,
                "generationSequenceDiffer": row.generation_sequence_differ,
                "nonzeroGenerationCountDelta": row.nonzero_generation_count_delta,
                "startxrefCountDelta": row.startxref_count_delta,
                "startxrefValuesDiffer": row.startxref_values_differ,
                "lastStartxrefDelta": row.last_startxref_delta,
                "javaEofCount": row.java_eof_count,
                "dotnetEofCount": row.dotnet_eof_count,
                "javaPrevCount": row.java_prev_count,
                "dotnetPrevCount": row.dotnet_prev_count,
                "javaHasIncrementalMarkers": row.java_has_incremental_markers,
                "dotnetHasIncrementalMarkers": row.dotnet_has_incremental_markers,
                "xrefTableCountDelta": row.xref_table_count_delta,
                "xrefStreamCountDelta": row.xref_stream_count_delta,
                "prevCountDelta": row.prev_count_delta,
                "xrefStyleDiffer": row.xref_style_differ,
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
        f"Issue: {source.get('issue')}",
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
            "## Stream and Compression Breakdown",
            "",
            "The `stream filters` and `compression` labels are split into serialized `/Filter` token differences, stream marker counts, `/Length` counts and sequences, `/FlateDecode` token counts, and `/ObjStm` token counts. Length-sequence differences can be downstream symptoms of object layout or deflater output, not necessarily independent stream-ownership bugs.",
            "",
            "| Operation | Rows with stream-filter label | Rows with compression label | Filter token differences | Stream-count differences | Length-count differences | Length-sequence differences | Flate-count differences | ObjStm-count differences | Max stream delta | Max length-count delta | Max Flate delta | Max ObjStm delta |",
            "|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|",
        ]
    )
    for op, counts in summary["streamCompressionComparison"].items():
        lines.append(
            f"| `{op}` | {counts['rowsWithStreamFilterCause']} | {counts['rowsWithCompressionCause']} | "
            f"{counts['rowsWithFilterTokenDifference']} | {counts['rowsWithStreamCountDifference']} | "
            f"{counts['rowsWithLengthCountDifference']} | {counts['rowsWithLengthSequenceDifference']} | "
            f"{counts['rowsWithFlateCountDifference']} | {counts['rowsWithObjStmCountDifference']} | "
            f"{counts['maxStreamCountDelta']} | {counts['maxLengthCountDelta']} | "
            f"{counts['maxFlateCountDelta']} | {counts['maxObjStmCountDelta']} |"
        )
    lines.append("")

    lines.extend(
        [
            "## COS Object and XRef Breakdown",
            "",
            "The `COS object numbering` and `xref layout` labels are split into object-id sequence/set differences, generation-number sequence differences, `startxref` count/value differences, xref table/stream count differences, `/Prev` count differences, and overall xref style differences. A style difference means one artifact uses an xref stream and/or classic xref table differently from the other.",
            "",
            "| Operation | Rows with object-numbering label | Rows with xref-layout label | Object-id sequence differences | Object-id set differences | Object-count differences | Generation sequence differences | Startxref value differences | XRef table-count differences | XRef stream-count differences | XRef style differences | Max object-count delta | Max last-startxref delta | Max xref-stream delta |",
            "|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|",
        ]
    )
    for op, counts in summary["objectXrefComparison"].items():
        lines.append(
            f"| `{op}` | {counts['rowsWithObjectNumberingCause']} | {counts['rowsWithXrefLayoutCause']} | "
            f"{counts['rowsWithObjectIdSequenceDifference']} | {counts['rowsWithObjectIdSetDifference']} | "
            f"{counts['rowsWithObjectCountDifference']} | {counts['rowsWithGenerationSequenceDifference']} | "
            f"{counts['rowsWithStartxrefValueDifference']} | {counts['rowsWithXrefTableCountDifference']} | "
            f"{counts['rowsWithXrefStreamCountDifference']} | {counts['rowsWithXrefStyleDifference']} | "
            f"{counts['maxObjectCountDelta']} | {counts['maxLastStartxrefDelta']} | "
            f"{counts['maxXrefStreamCountDelta']} |"
        )
    lines.append("")

    lines.extend(
        [
            "## Incremental Marker Breakdown",
            "",
            "The `incremental-save behavior` label is driven by serialized `%%EOF` and `/Prev` markers. These counts explain whether the strict byte mismatch is marker presence, marker count, or both-side incremental history rather than a missing ability to load, modify, save, and reload the document.",
            "",
            "| Operation | Rows with incremental label | Java has markers | .NET has markers | Both have markers | Marker presence differs | EOF-count differences | /Prev-count differences | Max EOF delta | Max /Prev delta |",
            "|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|",
        ]
    )
    for op, counts in summary["incrementalMarkerComparison"].items():
        lines.append(
            f"| `{op}` | {counts['rowsWithIncrementalSaveCause']} | {counts['rowsWhereJavaHasMarkers']} | "
            f"{counts['rowsWhereDotnetHasMarkers']} | {counts['rowsWhereBothHaveMarkers']} | "
            f"{counts['rowsWhereMarkerPresenceDiffers']} | {counts['rowsWithEofCountDifference']} | "
            f"{counts['rowsWithPrevCountDifference']} | {counts['maxEofCountDelta']} | {counts['maxPrevCountDelta']} |"
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
    parser.add_argument("--issue", default="#539", help="Issue label to include in the Markdown and JSON reports.")
    args = parser.parse_args()

    out_dir = args.out_dir.resolve()
    comparison_payload, analyses = load_analyses(out_dir)
    payload = json_payload(Path(args.out_dir), comparison_payload, analyses, args.source_label, args.issue)

    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(markdown_report(payload, analyses), encoding="utf-8")
    if args.json_path is not None:
        args.json_path.parent.mkdir(parents=True, exist_ok=True)
        args.json_path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")

    print(json.dumps(payload["summary"], indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
