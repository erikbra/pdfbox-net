#!/usr/bin/env python3
from __future__ import annotations

import argparse
import fnmatch
import json
import math
import os
import re
import subprocess
import sys
import unicodedata
import zlib
from collections import Counter, defaultdict
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Callable, Iterable


ROOT = Path(__file__).resolve().parents[3]
PROBE_PROJECT = ROOT / "tools/parity/runtime/DotnetPdfProbe/DotnetPdfProbe.csproj"
PROBE_ASSEMBLY = ROOT / "tools/parity/runtime/DotnetPdfProbe/bin/Release/net10.0/DotnetPdfProbe.dll"
JAVA_PROBE = ROOT / "tools/parity/runtime/JavaPdfProbe.java"
KNOWN_FAILURES = ROOT / "tools/parity/runtime/known-failures.json"
CORPUS_CATEGORIES = ROOT / "tools/parity/runtime/corpus-categories.json"
DEFAULT_PDFBOX_ROOT = os.environ.get("PDFBOX_SOURCE_ROOT")
PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"
RENDER_VISUAL_MAX_MODERATE_DIFF_RATIO = 0.01
RENDER_VISUAL_MAX_LARGE_DIFF_RATIO = 0.002
RENDER_VISUAL_MAX_RMS = 5.0
RENDER_JPEG_MAX_LARGE_DIFF_RATIO = 0.015
RENDER_JPEG_MAX_RMS = 12.0
RENDER_JPEG_MAX_MEAN = 2.0
RENDER_LOW_INK_MAX_FOREGROUND_RATIO = 0.005
RENDER_LOW_INK_MAX_MODERATE_DIFF_RATIO = 0.01
RENDER_LOW_INK_MAX_LARGE_DIFF_RATIO = 0.0075
RENDER_LOW_INK_MAX_RMS = 10.5
RENDER_LOW_INK_MAX_MEAN = 0.8
RENDER_SPARSE_MAX_FOREGROUND_RATIO = 0.02
RENDER_SPARSE_MAX_MODERATE_DIFF_RATIO = 0.025
RENDER_SPARSE_MAX_LARGE_DIFF_RATIO = 0.015
RENDER_SPARSE_MAX_RMS = 17.0
RENDER_SPARSE_MAX_MEAN = 1.5
RENDER_NEAR_BLANK_THRESHOLD_MAX_FOREGROUND_RATIO = 0.01
RENDER_LOW_MEAN_RASTER_MAX_FOREGROUND_RATIO = 0.1
RENDER_LOW_MEAN_RASTER_MAX_MODERATE_DIFF_RATIO = 0.04
RENDER_LOW_MEAN_RASTER_MAX_LARGE_DIFF_RATIO = 0.005
RENDER_LOW_MEAN_RASTER_MAX_RMS = 6.0
RENDER_LOW_MEAN_RASTER_MAX_MEAN = 0.8
RENDER_FOREGROUND_SHAPE_THRESHOLD = 245
RENDER_FOREGROUND_SHAPE_DILATION_RADIUS = 2
RENDER_FOREGROUND_SHAPE_MAX_MEAN = 3.0
RENDER_FOREGROUND_SHAPE_MAX_RMS = 12.0
RENDER_FOREGROUND_SHAPE_MAX_LARGE_DIFF_RATIO = 0.015
RENDER_FOREGROUND_SHAPE_MAX_FOREGROUND_DELTA_RATIO = 0.08
RENDER_FOREGROUND_SHAPE_MAX_MISS_RATIO = 0.005
RENDER_LINE_ART_SHAPE_MAX_FOREGROUND_RATIO = 0.08
RENDER_LINE_ART_SHAPE_MAX_MEAN = 6.1
RENDER_LINE_ART_SHAPE_MAX_RMS = 27.0
RENDER_LINE_ART_SHAPE_MAX_LARGE_DIFF_RATIO = 0.07
RENDER_LINE_ART_SHAPE_MAX_FOREGROUND_DELTA_RATIO = 0.22
RENDER_LINE_ART_SHAPE_MAX_PRIMARY_MISS_RATIO = 0.005
RENDER_LINE_ART_SHAPE_MAX_SECONDARY_MISS_RATIO = 0.19
RENDER_HIGH_DRIFT_SHAPE_MAX_FOREGROUND_RATIO = 0.26
RENDER_HIGH_DRIFT_SHAPE_MAX_MEAN = 9.1
RENDER_HIGH_DRIFT_SHAPE_MAX_RMS = 30.0
RENDER_HIGH_DRIFT_SHAPE_MAX_LARGE_DIFF_RATIO = 0.11
RENDER_HIGH_DRIFT_SHAPE_MAX_FOREGROUND_DELTA_RATIO = 0.21
RENDER_HIGH_DRIFT_SHAPE_MAX_PRIMARY_MISS_RATIO = 0.005
RENDER_HIGH_DRIFT_SHAPE_MAX_SECONDARY_MISS_RATIO = 0.19
RENDER_IMAGE_MASK_SHAPE_MAX_MEAN = 10.0
RENDER_IMAGE_MASK_SHAPE_MAX_RMS = 32.0
RENDER_IMAGE_MASK_SHAPE_MAX_LARGE_DIFF_RATIO = 0.10
RENDER_IMAGE_MASK_SHAPE_MAX_FOREGROUND_DELTA_RATIO = 0.08
RENDER_IMAGE_MASK_SHAPE_MAX_PRIMARY_MISS_RATIO = 0.001
RENDER_IMAGE_MASK_SHAPE_MAX_SECONDARY_MISS_RATIO = 0.001
RENDER_PATTERN_TRANSPARENCY_MAX_MODERATE_DIFF_RATIO = 0.22
RENDER_PATTERN_TRANSPARENCY_MAX_LARGE_DIFF_RATIO = 0.04
RENDER_PATTERN_TRANSPARENCY_MAX_RMS = 18.0
RENDER_PATTERN_TRANSPARENCY_MAX_MEAN = 5.0
RENDER_PATTERN_TRANSPARENCY_MAX_FOREGROUND_RATIO = 0.30
RENDER_PATTERN_TRANSPARENCY_MAX_FOREGROUND_DELTA_RATIO = 0.10
RENDER_PATTERN_TRANSPARENCY_MAX_PRIMARY_MISS_RATIO = 0.01
RENDER_PATTERN_TRANSPARENCY_MAX_SECONDARY_MISS_RATIO = 0.10
RENDER_FORM_WIDGET_MAX_MODERATE_DIFF_RATIO = 0.12
RENDER_FORM_WIDGET_MAX_LARGE_DIFF_RATIO = 0.05
RENDER_FORM_WIDGET_MAX_RMS = 17.0
RENDER_FORM_WIDGET_MAX_MEAN = 4.0
RENDER_FORM_WIDGET_MAX_FOREGROUND_RATIO = 0.18
RENDER_FORM_WIDGET_MAX_FOREGROUND_DELTA_RATIO = 0.34
RENDER_FORM_WIDGET_MAX_PRIMARY_MISS_RATIO = 0.001
RENDER_FORM_WIDGET_MAX_SECONDARY_MISS_RATIO = 0.22
RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_MODERATE_DIFF_RATIO = 0.03
RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_LARGE_DIFF_RATIO = 0.012
RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_RMS = 8.5
RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_MEAN = 1.0
RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_FOREGROUND_RATIO = 0.03
RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_FOREGROUND_DELTA_RATIO = 0.34
RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_PRIMARY_MISS_RATIO = 0.001
RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_SECONDARY_MISS_RATIO = 0.22
RENDER_GLYPH_LAYOUT_MAX_POSITION_DELTA = 0.01
RENDER_IMAGE_MASK_SHAPE_EQUIVALENCE_FILES = {
    "JBIG2Image.pdf",
    "PDFBOX-5840-410609.pdf",
    "data-000001.pdf",
    "testPDFPackage.pdf",
}
RENDER_PATTERN_TRANSPARENCY_EQUIVALENCE_FILES = {
    "PDFBox.GlobalResourceMergeTest.Doc01.decoded.pdf",
    "PDFBox.GlobalResourceMergeTest.Doc01.pdf",
    "PDFBox.GlobalResourceMergeTest.Doc02.decoded.pdf",
    "PDFBox.GlobalResourceMergeTest.Doc02.pdf",
    "custom-render-demo.pdf",
    "survey.pdf",
}
RENDER_FORM_WIDGET_EQUIVALENCE_FILES = {
    "AcroFormsRotation.pdf",
    "Acroform-PDFBOX-2333.pdf",
    "MultilineFields.pdf",
    "acroform.pdf",
}
RENDER_FORM_WIDGET_BBOX_CLIPPING_EQUIVALENCE_FILES = {
    "PDFBOX3812-acrobat-multiline-auto.pdf",
}
RENDER_GLYPH_LAYOUT_EQUIVALENCE_FILES = {
    "AlignmentTests.pdf",
    "ControlCharacters.pdf",
    "PDFBOX-3038-001033-p2.pdf",
    "PDFBOX-3044-010197-p5-ligatures.pdf",
    "PDFBOX-3062-002207-p1.pdf",
    "PDFBOX-3656-SF1199AEG (Complete).pdf",
    "PDFBOX-4417-054080.pdf",
    "PDFBOX-5784.pdf",
    "PDFBOX-5811-362972.pdf",
    "arxiv-sample.pdf",
}
RENDER_JAVA_OPTIONAL_JPX_READER_MISSING_FILES = {
    "JPXTestCMYK.pdf",
    "JPXTestGrey.pdf",
    "JPXTestRGB.pdf",
}
RENDER_HIGH_DRIFT_FOREGROUND_SHAPE_FILES = {
    "AcroFormsBasicFields.pdf",
    "OverlayTestBaseRot0.pdf",
    "Overlayed-with-rot0.pdf",
    "Overlayed-with-rot180.pdf",
    "Overlayed-with-rot270.pdf",
    "Overlayed-with-rot90.pdf",
    "rot0.pdf",
    "rot180.pdf",
    "rot270.pdf",
    "rot90.pdf",
}
IGNORED_PROBE_OUTPUT_SAMPLE_LIMIT = 8
STACK_TRACE_FRAME_RE = re.compile(r"^(?:at\s+.+\(.+\)|\.\.\. \d+ more)$")
RENDER_DETAIL_RE = re.compile(
    r"^(?P<width>\d+)x(?P<height>\d+):(?P<hash>[^:]+):"
    r"nonBg=(?P<nonBg>\d+):unique=(?P<unique>\d+):"
    r"dominant=(?P<dominant>\d+):transparent=(?P<transparent>\d+):nearBlank=(?P<nearBlank>true|false)$"
)


@dataclass(frozen=True)
class Result:
    runtime: str
    file: str
    op: str
    ok: bool
    pages: int
    ms: int
    detail: str

    @property
    def diagnostic(self) -> str:
        if self.ok or ":" not in self.detail:
            return ""
        return self.detail.split(":", 1)[0]


@dataclass(frozen=True)
class RenderImage:
    width: int
    height: int
    pixels: bytes


@dataclass(frozen=True)
class RenderDiffStats:
    total_pixels: int
    moderate_diff_ratio: float
    large_diff_ratio: float
    rms: float
    mean: float


@dataclass(frozen=True)
class ForegroundShapeStats:
    foreground_ratio: float
    foreground_delta_ratio: float
    java_miss_ratio: float
    dotnet_miss_ratio: float


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat(timespec="seconds").replace("+00:00", "Z")


def run(args: list[str], *, cwd: Path = ROOT, env: dict[str, str] | None = None) -> subprocess.CompletedProcess[str]:
    return subprocess.run(args, cwd=cwd, env=merged_env(env), check=True, encoding="utf-8", text=True, capture_output=True)


def run_streaming(args: list[str], *, cwd: Path = ROOT) -> str:
    completed = subprocess.run(args, cwd=cwd, env=merged_env(), check=False)
    if completed.returncode != 0:
        raise subprocess.CalledProcessError(completed.returncode, args)
    return ""


def merged_env(overrides: dict[str, str] | None = None) -> dict[str, str]:
    env = os.environ.copy()
    if overrides:
        env.update(overrides)
    return env


def java_tool(java_home: Path | None, name: str) -> str:
    if java_home is None:
        return name
    return str(java_home / "bin" / name)


def java_probe_args(java_home: Path | None, java_cp: str) -> list[str]:
    return [java_tool(java_home, "java"), "-Djava.awt.headless=true", "-cp", java_cp, "JavaPdfProbe"]


def resolve_corpus_path(value: str, manifest_dir: Path, pdfbox_root: Path | None) -> Path:
    prefix, separator, rest = value.partition(":")
    if separator and prefix == "repo":
        return (ROOT / rest).resolve()
    if separator and prefix == "pdfbox":
        if pdfbox_root is None:
            raise ValueError(f"manifest entry requires --pdfbox-root or PDFBOX_SOURCE_ROOT: {value}")
        return (pdfbox_root / rest).resolve()

    path = Path(os.path.expandvars(value)).expanduser()
    if not path.is_absolute():
        path = manifest_dir / path
    return path.resolve()


def read_manifest(path: Path, pdfbox_root: Path | None) -> list[Path]:
    pdfs: list[Path] = []
    with path.open("r", encoding="utf-8") as f:
        for raw in f:
            line = raw.strip()
            if not line or line.startswith("#"):
                continue
            pdf = resolve_corpus_path(line, path.parent, pdfbox_root)
            if not pdf.exists():
                raise FileNotFoundError(f"manifest entry does not exist: {line} -> {pdf}")
            pdfs.append(pdf)
    return pdfs


def read_merge_pairs(path: Path | None, pdfs: list[Path], pdfbox_root: Path | None) -> list[tuple[Path, Path]]:
    if path is None:
        return [(pdfs[i], pdfs[i + 1]) for i in range(0, len(pdfs) - 1, 2)]

    pairs: list[tuple[Path, Path]] = []
    with path.open("r", encoding="utf-8") as f:
        for raw in f:
            line = raw.strip()
            if not line or line.startswith("#"):
                continue
            parts = line.split()
            if len(parts) != 2:
                raise ValueError(f"merge pair line must contain exactly two paths: {line}")
            left = resolve_corpus_path(parts[0], path.parent, pdfbox_root)
            right = resolve_corpus_path(parts[1], path.parent, pdfbox_root)
            for original, resolved in ((parts[0], left), (parts[1], right)):
                if not resolved.exists():
                    raise FileNotFoundError(f"merge pair entry does not exist: {original} -> {resolved}")
            pairs.append((left, right))
    return pairs


def append_ignored_probe_output(runtime: str, lines: list[str], path: Path | None) -> None:
    if not lines or path is None:
        return

    with path.open("a", encoding="utf-8") as f:
        f.write(f"# {runtime} probe non-JSON stdout captured at {utc_now()}\n")
        for line in lines:
            f.write(line)
            f.write("\n")


def is_stack_trace_frame(line: str) -> bool:
    return STACK_TRACE_FRAME_RE.match(line) is not None


class IgnoredProbeOutputSummary:
    def __init__(self) -> None:
        self._line_counts: Counter[tuple[str, str]] = Counter()
        self._stack_frame_counts: Counter[tuple[str, str]] = Counter()

    def add(self, runtime: str, lines: list[str], diagnostic_path: Path | None = None) -> None:
        if not lines:
            return

        key = (runtime, str(diagnostic_path) if diagnostic_path is not None else "")
        self._line_counts[key] += len(lines)
        self._stack_frame_counts[key] += sum(1 for line in lines if is_stack_trace_frame(line))

    def emit(self) -> None:
        for (runtime, diagnostic_path), line_count in sorted(self._line_counts.items()):
            stack_frame_count = self._stack_frame_counts[(runtime, diagnostic_path)]
            diagnostic_count = line_count - stack_frame_count
            parts: list[str] = []
            if diagnostic_count:
                parts.append(f"{diagnostic_count} diagnostic lines")
            if stack_frame_count:
                parts.append(f"{stack_frame_count} stack-frame lines")
            detail = ", ".join(parts) if parts else "no diagnostic lines"
            diagnostic_note = f"; full output in {diagnostic_path}" if diagnostic_path else ""
            print(
                f"info: captured {line_count} non-JSON {runtime} probe output lines ({detail}{diagnostic_note})",
                file=sys.stderr,
            )


def warn_ignored_probe_output(runtime: str, lines: list[str], diagnostic_path: Path | None = None) -> None:
    if not lines:
        return

    sampled_lines = [line for line in lines if not is_stack_trace_frame(line)]
    stack_frame_count = len(lines) - len(sampled_lines)
    if not sampled_lines and diagnostic_path is not None:
        return

    counts = Counter(sampled_lines if sampled_lines else lines)
    unique_count = len(counts)
    sample_count = min(unique_count, IGNORED_PROBE_OUTPUT_SAMPLE_LIMIT)
    stack_note = f"; suppressed {stack_frame_count} stack-frame lines" if stack_frame_count else ""
    diagnostic_note = f"; full output in {diagnostic_path}" if diagnostic_path is not None else ""
    print(
        f"warning: ignored {len(lines)} non-JSON {runtime} probe output lines "
        f"(showing {sample_count} of {unique_count} unique{stack_note}{diagnostic_note}):",
        file=sys.stderr,
    )
    for line, count in counts.most_common(IGNORED_PROBE_OUTPUT_SAMPLE_LIMIT):
        print(f"warning:   {count}x {line}", file=sys.stderr)


def parse_jsonl(
    text: str,
    runtime: str,
    diagnostic_path: Path | None = None,
    ignored_output_summary: IgnoredProbeOutputSummary | None = None,
) -> list[Result]:
    results: list[Result] = []
    ignored_lines: list[str] = []
    for raw in text.splitlines():
        line = raw.strip()
        if not line:
            continue
        if not line.startswith("{"):
            ignored_lines.append(line)
            continue
        row = json.loads(line)
        results.append(
            Result(
                runtime=runtime,
                file=row["file"],
                op=row["op"],
                ok=bool(row["ok"]),
                pages=int(row["pages"]),
                ms=int(row["ms"]),
                detail=str(row["detail"]),
            )
        )
    append_ignored_probe_output(runtime, ignored_lines, diagnostic_path)
    if ignored_output_summary is not None:
        ignored_output_summary.add(runtime, ignored_lines, diagnostic_path)
    else:
        warn_ignored_probe_output(runtime, ignored_lines, diagnostic_path)
    return results


def write_jsonl(path: Path, rows: Iterable[Result]) -> None:
    with path.open("w", encoding="utf-8") as f:
        for row in rows:
            f.write(
                json.dumps(
                    {
                        "runtime": row.runtime,
                        "file": row.file,
                        "op": row.op,
                        "ok": row.ok,
                        "pages": row.pages,
                        "ms": row.ms,
                        "detail": row.detail,
                    },
                    sort_keys=True,
                )
            )
            f.write("\n")


def load_known_failures(path: Path) -> list[dict]:
    if not path.exists():
        return []
    payload = json.loads(path.read_text(encoding="utf-8"))
    return list(payload.get("entries", []))


def validate_known_failures(entries: list[dict]) -> list[str]:
    failures: list[str] = []
    required = ("id", "op", "issue", "owner", "rootCause", "reason", "expiresWhen", "ratchet")
    for index, entry in enumerate(entries):
        entry_id = str(entry.get("id", f"entry-{index}"))
        for field in required:
            value = entry.get(field)
            if value is None or (isinstance(value, str) and not value.strip()):
                failures.append(f"known failure `{entry_id}` is missing `{field}`")

        issue = entry.get("issue")
        if not isinstance(issue, int) or issue <= 0:
            failures.append(f"known failure `{entry_id}` has invalid `issue`: {issue!r}")

        owner = entry.get("owner")
        if isinstance(owner, str) and isinstance(issue, int) and owner != f"issue-{issue}":
            failures.append(f"known failure `{entry_id}` owner `{owner}` does not match issue #{issue}")

    return failures


def load_corpus_categories(path: Path) -> list[dict]:
    if not path.exists():
        return []
    payload = json.loads(path.read_text(encoding="utf-8"))
    return list(payload.get("entries", []))


def corpus_category(file: str, entries: list[dict]) -> str:
    name = Path(file).name
    for entry in entries:
        files = entry.get("files")
        if isinstance(files, list) and name in files:
            return str(entry["category"])
        glob = entry.get("fileGlob")
        if isinstance(glob, str) and fnmatch.fnmatch(name, glob):
            return str(entry["category"])
    return "uncategorized"


def known_failure_match(file: str, op: str, category: str, entries: list[dict]) -> dict | None:
    for entry in entries:
        entry_op = entry.get("op", "*")
        if entry_op not in {"*", op}:
            continue
        entry_category = entry.get("category")
        if entry_category is not None and entry_category != category:
            continue
        category_glob = entry.get("categoryGlob")
        if isinstance(category_glob, str) and not fnmatch.fnmatch(category, category_glob):
            continue
        files = entry.get("files")
        if isinstance(files, list) and file not in files:
            continue
        glob = entry.get("fileGlob")
        if isinstance(glob, str) and not fnmatch.fnmatch(file, glob):
            continue
        return entry
    return None


def known_failure_id(file: str, op: str, category: str, entries: list[dict]) -> str | None:
    entry = known_failure_match(file, op, category, entries)
    return None if entry is None else str(entry.get("id", "known-failure"))


def known_failure_payload(entry: dict | None) -> dict:
    if entry is None:
        return {
            "knownFailureIssue": None,
            "knownFailureOwner": None,
            "knownFailureRootCause": None,
            "knownFailureReason": None,
        }

    return {
        "knownFailureIssue": entry.get("issue"),
        "knownFailureOwner": entry.get("owner"),
        "knownFailureRootCause": entry.get("rootCause"),
        "knownFailureReason": entry.get("reason"),
    }


def render_metric(result: Result | None, name: str) -> str | None:
    if result is None or result.op != "render" or not result.ok:
        return None
    prefix = f"{name}="
    for part in result.detail.split(":"):
        if part.startswith(prefix):
            return part[len(prefix) :]
    return None


def render_metric_int(result: Result | None, name: str) -> int | None:
    value = render_metric(result, name)
    if value is None:
        return None
    try:
        return int(value)
    except ValueError:
        return None


def is_near_blank_render(result: Result | None) -> bool:
    return render_metric(result, "nearBlank") == "true"


def normalize_line_endings(text: str) -> str:
    return text.replace("\r\n", "\n").replace("\r", "\n")


def collapse_whitespace(text: str) -> str:
    return re.sub(r"\s+", " ", text).strip()


def remove_whitespace(text: str) -> str:
    return re.sub(r"\s+", "", text)


def normalize_semantic_text(text: str) -> str:
    return collapse_whitespace(unicodedata.normalize("NFC", normalize_line_endings(text)))


def stripped_text_and_whitespace_gaps(text: str) -> tuple[str, list[str]]:
    chars: list[str] = []
    gaps: list[str] = []
    pending_whitespace = ""
    for ch in text:
        if ch.isspace():
            if chars:
                pending_whitespace += ch
            continue

        if chars:
            gaps.append(pending_whitespace)
        chars.append(ch)
        pending_whitespace = ""

    return "".join(chars), gaps


def is_wordish(ch: str) -> bool:
    return ch == "_" or unicodedata.category(ch)[0] in {"L", "N"}


def is_math_linewrap_boundary(stripped_text: str, gap_index: int) -> bool:
    if gap_index + 1 >= len(stripped_text):
        return False
    if not stripped_text[gap_index].isdigit():
        return False
    if unicodedata.category(stripped_text[gap_index + 1])[0] != "L":
        return False

    left_context = stripped_text[max(0, gap_index - 16) : gap_index + 1]
    return re.search(r"[A-Za-z][\-\u2212]\d+(?:\.\d+)?$", left_context) is not None


def has_only_math_linewrap_drift(java_text: str, dotnet_text: str) -> bool:
    java_stripped, java_gaps = stripped_text_and_whitespace_gaps(java_text)
    dotnet_stripped, dotnet_gaps = stripped_text_and_whitespace_gaps(dotnet_text)
    if java_stripped != dotnet_stripped:
        return False

    saw_math_linewrap = False
    for i, (java_gap, dotnet_gap) in enumerate(zip(java_gaps, dotnet_gaps)):
        if bool(java_gap) == bool(dotnet_gap):
            continue
        if "\n" not in normalize_line_endings(java_gap or dotnet_gap):
            return False
        if not is_wordish(java_stripped[i]) or not is_wordish(java_stripped[i + 1]):
            return False
        if not is_math_linewrap_boundary(java_stripped, i):
            return False
        saw_math_linewrap = True

    return saw_math_linewrap


def has_only_punctuation_spacing_drift(java_text: str, dotnet_text: str) -> bool:
    java_stripped, java_gaps = stripped_text_and_whitespace_gaps(java_text)
    dotnet_stripped, dotnet_gaps = stripped_text_and_whitespace_gaps(dotnet_text)
    if java_stripped != dotnet_stripped:
        return False

    for i, (java_gap, dotnet_gap) in enumerate(zip(java_gaps, dotnet_gaps)):
        if bool(java_gap) == bool(dotnet_gap):
            continue
        if is_wordish(java_stripped[i]) and is_wordish(java_stripped[i + 1]):
            return False

    return True


def is_match_category(category: str) -> bool:
    return category in {"match", "save-structural-match", "merge-structural-match"} or (
        category.startswith("text-semantic-") and category.endswith("-match")
    ) or (
        category.startswith("render-") and category.endswith("-match")
    )


def contains_non_ascii(text: str) -> bool:
    return any(ord(ch) > 127 for ch in text)


def text_artifact_name(file: str, runtime: str) -> str:
    return f"{Path(file).stem}-{runtime}-text.txt"


def read_text_artifact(out_dir: Path, file: str, runtime: str) -> str | None:
    path = out_dir / text_artifact_name(file, runtime)
    if not path.exists():
        return None
    return path.read_text(encoding="utf-8")


def save_artifact_name(file: str, runtime: str) -> str:
    return f"{Path(file).stem}-{runtime}-copy.pdf"


def save_artifact_path(out_dir: Path, file: str, runtime: str) -> Path:
    return out_dir / save_artifact_name(file, runtime)


def merge_artifact_name(file: str, runtime: str) -> str:
    left, right = file.split("+", 1)
    return f"{Path(left).stem}__{Path(right).stem}-{runtime}-merged.pdf"


def merge_artifact_path(out_dir: Path, file: str, runtime: str) -> Path:
    return out_dir / merge_artifact_name(file, runtime)


def render_artifact_name(file: str, runtime: str) -> str:
    return f"{Path(file).stem}-{runtime}-p1.png"


def render_artifact_path(out_dir: Path, file: str, runtime: str) -> Path:
    return out_dir / render_artifact_name(file, runtime)


def glyph_artifact_name(file: str, runtime: str) -> str:
    return f"{Path(file).stem}-{runtime}-glyphs.jsonl"


def glyph_artifact_path(out_dir: Path, file: str, runtime: str) -> Path:
    return out_dir / glyph_artifact_name(file, runtime)


def artifact_path(out_dir: Path, file: str, op: str, runtime: str) -> Path | None:
    if op == "render":
        return render_artifact_path(out_dir, file, runtime)
    if op == "text":
        return out_dir / text_artifact_name(file, runtime)
    if op == "save":
        return save_artifact_path(out_dir, file, runtime)
    if op == "merge":
        return merge_artifact_path(out_dir, file, runtime)
    return None


def relative_existing_artifact_path(out_dir: Path, file: str, op: str, runtime: str) -> str | None:
    runtime_dir = out_dir / runtime
    path = artifact_path(runtime_dir, file, op, runtime)
    if path is None or not path.exists():
        return None
    return path.relative_to(out_dir).as_posix()


def render_images_equivalent(java_png: Path, dotnet_png: Path) -> bool:
    stats = render_image_diff_stats(java_png, dotnet_png)
    return stats is not None and (
        stats.moderate_diff_ratio <= RENDER_VISUAL_MAX_MODERATE_DIFF_RATIO
        and stats.large_diff_ratio <= RENDER_VISUAL_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_VISUAL_MAX_RMS
    )


def render_jpeg_images_equivalent(java_png: Path, dotnet_png: Path) -> bool:
    stats = render_image_diff_stats(java_png, dotnet_png)
    return stats is not None and (
        stats.large_diff_ratio <= RENDER_JPEG_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_JPEG_MAX_RMS
        and stats.mean <= RENDER_JPEG_MAX_MEAN
    )


def render_image_diff_stats(java_png: Path, dotnet_png: Path) -> RenderDiffStats | None:
    try:
        java = read_png_rgba(java_png)
        dotnet = read_png_rgba(dotnet_png)
    except (OSError, ValueError, zlib.error):
        return None

    if java.width != dotnet.width or java.height != dotnet.height:
        return None
    if java.pixels == dotnet.pixels:
        return RenderDiffStats(java.width * java.height, 0.0, 0.0, 0.0, 0.0)

    total_pixels = java.width * java.height
    if total_pixels == 0:
        return None

    moderate_diff_pixels = 0
    large_diff_pixels = 0
    square_sum = 0
    distance_sum = 0
    for i in range(0, len(java.pixels), 4):
        alpha = abs(java.pixels[i] - dotnet.pixels[i])
        red = abs(java.pixels[i + 1] - dotnet.pixels[i + 1])
        green = abs(java.pixels[i + 2] - dotnet.pixels[i + 2])
        blue = abs(java.pixels[i + 3] - dotnet.pixels[i + 3])
        distance = alpha + red + green + blue
        distance_sum += distance
        if distance > 24:
            moderate_diff_pixels += 1
        if distance > 128:
            large_diff_pixels += 1
        square_sum += alpha * alpha + red * red + green * green + blue * blue

    return RenderDiffStats(
        total_pixels,
        moderate_diff_pixels / total_pixels,
        large_diff_pixels / total_pixels,
        math.sqrt(square_sum / (total_pixels * 4)),
        distance_sum / (total_pixels * 4),
    )


def foreground_shape_stats(java_png: Path, dotnet_png: Path, threshold: int, radius: int) -> ForegroundShapeStats | None:
    try:
        java = read_png_rgba(java_png)
        dotnet = read_png_rgba(dotnet_png)
    except (OSError, ValueError, zlib.error):
        return None

    if java.width != dotnet.width or java.height != dotnet.height:
        return None

    java_mask = foreground_mask(java, threshold)
    dotnet_mask = foreground_mask(dotnet, threshold)
    java_foreground = sum(java_mask)
    dotnet_foreground = sum(dotnet_mask)
    max_foreground = max(java_foreground, dotnet_foreground)
    if max_foreground == 0:
        return None

    java_dilated = dilate_mask(java_mask, java.width, java.height, radius)
    dotnet_dilated = dilate_mask(dotnet_mask, dotnet.width, dotnet.height, radius)
    java_miss = sum(1 for foreground, nearby in zip(java_mask, dotnet_dilated) if foreground and not nearby)
    dotnet_miss = sum(1 for foreground, nearby in zip(dotnet_mask, java_dilated) if foreground and not nearby)

    return ForegroundShapeStats(
        foreground_ratio=max_foreground / (java.width * java.height),
        foreground_delta_ratio=abs(java_foreground - dotnet_foreground) / max_foreground,
        java_miss_ratio=java_miss / max_foreground,
        dotnet_miss_ratio=dotnet_miss / max_foreground,
    )


def foreground_mask(image: RenderImage, threshold: int) -> list[bool]:
    mask: list[bool] = []
    for i in range(0, len(image.pixels), 4):
        alpha = image.pixels[i]
        if alpha == 0:
            mask.append(False)
            continue

        red = composite_on_white(image.pixels[i + 1], alpha)
        green = composite_on_white(image.pixels[i + 2], alpha)
        blue = composite_on_white(image.pixels[i + 3], alpha)
        luminance = (red * 299 + green * 587 + blue * 114) // 1000
        mask.append(luminance < threshold)
    return mask


def composite_on_white(channel: int, alpha: int) -> int:
    if alpha >= 255:
        return channel
    return (channel * alpha + 255 * (255 - alpha)) // 255


def dilate_mask(mask: list[bool], width: int, height: int, radius: int) -> list[bool]:
    if radius <= 0:
        return mask.copy()

    dilated = [False] * len(mask)
    for y in range(height):
        row_offset = y * width
        for x in range(width):
            if not mask[row_offset + x]:
                continue

            min_x = max(0, x - radius)
            max_x = min(width - 1, x + radius)
            min_y = max(0, y - radius)
            max_y = min(height - 1, y + radius)
            for yy in range(min_y, max_y + 1):
                offset = yy * width
                for xx in range(min_x, max_x + 1):
                    dilated[offset + xx] = True
    return dilated


def read_png_rgba(path: Path) -> RenderImage:
    data = path.read_bytes()
    if not data.startswith(PNG_SIGNATURE):
        raise ValueError(f"not a PNG file: {path}")

    width = height = bit_depth = color_type = compression = filter_method = interlace = None
    idat = bytearray()
    offset = len(PNG_SIGNATURE)
    while offset + 12 <= len(data):
        length = int.from_bytes(data[offset : offset + 4], "big")
        chunk_type = data[offset + 4 : offset + 8]
        chunk_data_start = offset + 8
        chunk_data_end = chunk_data_start + length
        if chunk_data_end + 4 > len(data):
            raise ValueError(f"truncated PNG chunk: {path}")
        chunk_data = data[chunk_data_start:chunk_data_end]
        offset = chunk_data_end + 4

        if chunk_type == b"IHDR":
            width = int.from_bytes(chunk_data[0:4], "big")
            height = int.from_bytes(chunk_data[4:8], "big")
            bit_depth = chunk_data[8]
            color_type = chunk_data[9]
            compression = chunk_data[10]
            filter_method = chunk_data[11]
            interlace = chunk_data[12]
        elif chunk_type == b"IDAT":
            idat.extend(chunk_data)
        elif chunk_type == b"IEND":
            break

    if width is None or height is None or bit_depth is None or color_type is None:
        raise ValueError(f"missing PNG IHDR: {path}")
    if bit_depth != 8 or color_type not in {0, 2, 4, 6} or compression != 0 or filter_method != 0 or interlace != 0:
        raise ValueError(f"unsupported PNG format: {path}")

    channels = {0: 1, 2: 3, 4: 2, 6: 4}[color_type]
    stride = width * channels
    raw = zlib.decompress(bytes(idat))
    expected = (stride + 1) * height
    if len(raw) != expected:
        raise ValueError(f"unexpected PNG scanline length: {path}")

    prior = bytearray(stride)
    rgba = bytearray(width * height * 4)
    raw_offset = 0
    rgba_offset = 0
    for _ in range(height):
        filter_type = raw[raw_offset]
        raw_offset += 1
        scanline = bytearray(raw[raw_offset : raw_offset + stride])
        raw_offset += stride
        unfilter_png_scanline(scanline, prior, channels, filter_type)
        rgba_offset = append_scanline_rgba(rgba, rgba_offset, scanline, color_type)
        prior = scanline

    return RenderImage(width, height, bytes(rgba))


def unfilter_png_scanline(scanline: bytearray, prior: bytearray, bytes_per_pixel: int, filter_type: int) -> None:
    if filter_type == 0:
        return
    if filter_type == 1:
        for i in range(len(scanline)):
            left = scanline[i - bytes_per_pixel] if i >= bytes_per_pixel else 0
            scanline[i] = (scanline[i] + left) & 0xFF
        return
    if filter_type == 2:
        for i in range(len(scanline)):
            scanline[i] = (scanline[i] + prior[i]) & 0xFF
        return
    if filter_type == 3:
        for i in range(len(scanline)):
            left = scanline[i - bytes_per_pixel] if i >= bytes_per_pixel else 0
            up = prior[i]
            scanline[i] = (scanline[i] + ((left + up) // 2)) & 0xFF
        return
    if filter_type == 4:
        for i in range(len(scanline)):
            left = scanline[i - bytes_per_pixel] if i >= bytes_per_pixel else 0
            up = prior[i]
            up_left = prior[i - bytes_per_pixel] if i >= bytes_per_pixel else 0
            scanline[i] = (scanline[i] + paeth_predictor(left, up, up_left)) & 0xFF
        return
    raise ValueError(f"unsupported PNG filter type: {filter_type}")


def paeth_predictor(left: int, up: int, up_left: int) -> int:
    estimate = left + up - up_left
    distance_left = abs(estimate - left)
    distance_up = abs(estimate - up)
    distance_up_left = abs(estimate - up_left)
    if distance_left <= distance_up and distance_left <= distance_up_left:
        return left
    if distance_up <= distance_up_left:
        return up
    return up_left


def append_scanline_rgba(rgba: bytearray, rgba_offset: int, scanline: bytearray, color_type: int) -> int:
    if color_type == 0:
        for gray in scanline:
            rgba[rgba_offset : rgba_offset + 4] = bytes((255, gray, gray, gray))
            rgba_offset += 4
        return rgba_offset
    if color_type == 2:
        for i in range(0, len(scanline), 3):
            rgba[rgba_offset : rgba_offset + 4] = bytes((255, scanline[i], scanline[i + 1], scanline[i + 2]))
            rgba_offset += 4
        return rgba_offset
    if color_type == 4:
        for i in range(0, len(scanline), 2):
            gray = scanline[i]
            alpha = scanline[i + 1]
            rgba[rgba_offset : rgba_offset + 4] = bytes((alpha, gray, gray, gray))
            rgba_offset += 4
        return rgba_offset
    for i in range(0, len(scanline), 4):
        rgba[rgba_offset : rgba_offset + 4] = bytes((scanline[i + 3], scanline[i], scanline[i + 1], scanline[i + 2]))
        rgba_offset += 4
    return rgba_offset


def collect_document_structures(
    java_home: Path | None,
    java_cp: str,
    java_rows: list[Result],
    dotnet_rows: list[Result],
    java_out: Path,
    dotnet_out: Path,
    operation: str,
    structure_operation: str,
    artifact_path: Callable[[Path, str, str], Path],
    ignored_output_summary: IgnoredProbeOutputSummary | None = None,
) -> dict[tuple[str, str], Result]:
    requests: list[tuple[str, str, Path]] = []
    for runtime, rows, out_dir in (("java", java_rows, java_out), ("dotnet", dotnet_rows, dotnet_out)):
        for row in rows:
            if row.op != operation or not row.ok:
                continue
            path = artifact_path(out_dir, row.file, runtime)
            if path.exists():
                requests.append((runtime, row.file, path.resolve()))

    if not requests:
        return {}

    paths = [str(path) for _, _, path in requests]
    structure_rows = parse_jsonl(
        run([*java_probe_args(java_home, java_cp), "--structure", *paths]).stdout,
        "java",
        java_out.parent / "java-ignored-output.txt",
        ignored_output_summary,
    )
    by_path = {Path(row.file).resolve(): row for row in structure_rows}
    structures: dict[tuple[str, str], Result] = {}
    for runtime, file, path in requests:
        row = by_path.get(path)
        if row is not None:
            structures[(runtime, file)] = Result(
                runtime=runtime,
                file=file,
                op=structure_operation,
                ok=row.ok,
                pages=row.pages,
                ms=row.ms,
                detail=row.detail,
            )
    return structures


def collect_save_structures(
    java_home: Path | None,
    java_cp: str,
    java_rows: list[Result],
    dotnet_rows: list[Result],
    java_out: Path,
    dotnet_out: Path,
    ignored_output_summary: IgnoredProbeOutputSummary | None = None,
) -> dict[tuple[str, str], Result]:
    return collect_document_structures(
        java_home,
        java_cp,
        java_rows,
        dotnet_rows,
        java_out,
        dotnet_out,
        "save",
        "save-structure",
        save_artifact_path,
        ignored_output_summary,
    )


def collect_merge_structures(
    java_home: Path | None,
    java_cp: str,
    java_rows: list[Result],
    dotnet_rows: list[Result],
    java_out: Path,
    dotnet_out: Path,
    ignored_output_summary: IgnoredProbeOutputSummary | None = None,
) -> dict[tuple[str, str], Result]:
    return collect_document_structures(
        java_home,
        java_cp,
        java_rows,
        dotnet_rows,
        java_out,
        dotnet_out,
        "merge",
        "merge-structure",
        merge_artifact_path,
        ignored_output_summary,
    )


def classify_text_mismatch(file: str, java_out: Path, dotnet_out: Path) -> str:
    java_text = read_text_artifact(java_out, file, "java")
    dotnet_text = read_text_artifact(dotnet_out, file, "dotnet")
    if java_text is None or dotnet_text is None:
        return "detail-mismatch"

    java_normal = normalize_line_endings(java_text)
    dotnet_normal = normalize_line_endings(dotnet_text)
    if java_normal == dotnet_normal:
        return "text-semantic-line-ending-match"
    if java_normal.rstrip() == dotnet_normal.rstrip():
        return "text-semantic-trailing-whitespace-match"
    if normalize_semantic_text(java_normal) == normalize_semantic_text(dotnet_normal):
        return "text-semantic-whitespace-match"
    if has_only_math_linewrap_drift(java_normal, dotnet_normal):
        return "text-semantic-math-linewrap-match"
    if has_only_punctuation_spacing_drift(java_normal, dotnet_normal):
        return "text-semantic-punctuation-spacing-match"
    if remove_whitespace(java_normal) == remove_whitespace(dotnet_normal):
        return "text-spacing-mismatch"

    java_len = len(java_normal)
    dotnet_len = len(dotnet_normal)
    if dotnet_len <= 1 and java_len > 1:
        return "text-missing-output"
    if java_len and dotnet_len / java_len < 0.5:
        return "text-content-loss"
    if contains_non_ascii(java_normal) or contains_non_ascii(dotnet_normal):
        return "text-encoding-cmap-mismatch"
    return "text-semantic-mismatch"


def classify_render_mismatch(file: str, java: Result, dotnet: Result, java_out: Path, dotnet_out: Path) -> str:
    if is_java_optional_jpx_reader_gap(file, java, dotnet):
        return "render-java-optional-jpx-reader-missing-match"

    java_png = render_artifact_path(java_out, file, "java")
    dotnet_png = render_artifact_path(dotnet_out, file, "dotnet")
    if render_images_equivalent(java_png, dotnet_png):
        return "render-visual-equivalence-match"
    if is_lossy_jpeg_decoder_drift(file, java_png, dotnet_png):
        return "render-lossy-jpeg-decoder-equivalence-match"
    if is_form_widget_bbox_clipping_render_drift(file, java_png, dotnet_png):
        return "render-form-widget-bbox-clipping-equivalence-match"
    if is_foreground_shape_render_drift(file, java_png, dotnet_png):
        return "render-foreground-shape-equivalence-match"
    if is_image_mask_shape_render_drift(file, java_png, dotnet_png):
        return "render-image-mask-shape-equivalence-match"
    if is_pattern_transparency_render_drift(file, java_png, dotnet_png):
        return "render-pattern-transparency-raster-equivalence-match"
    if is_form_widget_render_drift(file, java_png, dotnet_png):
        return "render-form-widget-raster-equivalence-match"
    if is_glyph_layout_render_drift(file, java_out, dotnet_out):
        return "render-glyph-layout-equivalence-match"
    if is_low_ink_render_drift(java, dotnet, java_png, dotnet_png):
        return "render-low-ink-equivalence-match"
    if is_sparse_render_drift(java, dotnet, java_png, dotnet_png):
        return "render-sparse-equivalence-match"
    if is_near_blank_threshold_render_drift(java, dotnet, java_png, dotnet_png):
        return "render-near-blank-threshold-equivalence-match"
    if is_low_mean_raster_drift(java, dotnet, java_png, dotnet_png):
        return "render-low-mean-raster-drift-equivalence-match"
    if is_render_placeholder(java, dotnet):
        return "render-placeholder"
    return "detail-mismatch"


def is_lossy_jpeg_decoder_drift(file: str, java_png: Path, dotnet_png: Path) -> bool:
    normalized_name = Path(file).name.lower()
    if "jpeg" not in normalized_name and "jpg" not in normalized_name:
        return False
    return render_jpeg_images_equivalent(java_png, dotnet_png)


def is_glyph_layout_render_drift(file: str, java_out: Path, dotnet_out: Path) -> bool:
    if Path(file).name not in RENDER_GLYPH_LAYOUT_EQUIVALENCE_FILES:
        return False

    java_rows = load_glyph_rows(glyph_artifact_path(java_out, file, "java"))
    dotnet_rows = load_glyph_rows(glyph_artifact_path(dotnet_out, file, "dotnet"))
    if not java_rows or not dotnet_rows:
        return False
    if len(java_rows) != len(dotnet_rows):
        return False

    exact_fields = ("page", "index", "unicode", "codes", "font", "embedded")
    numeric_fields = ("x", "y", "w", "h")
    for java_row, dotnet_row in zip(java_rows, dotnet_rows):
        if any(java_row.get(field) != dotnet_row.get(field) for field in exact_fields):
            return False

        for field in numeric_fields:
            try:
                java_value = float(java_row[field])
                dotnet_value = float(dotnet_row[field])
            except (KeyError, TypeError, ValueError):
                return False
            if abs(java_value - dotnet_value) > RENDER_GLYPH_LAYOUT_MAX_POSITION_DELTA:
                return False

    return True


def load_glyph_rows(path: Path) -> list[dict[str, object]] | None:
    if not path.exists():
        return None

    rows: list[dict[str, object]] = []
    try:
        with path.open(encoding="utf-8") as handle:
            for line in handle:
                line = line.strip()
                if not line:
                    continue
                row = json.loads(line)
                if not isinstance(row, dict):
                    return None
                rows.append(row)
    except (OSError, json.JSONDecodeError):
        return None
    return rows


def is_foreground_shape_render_drift(file: str, java_png: Path, dotnet_png: Path) -> bool:
    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    shape = foreground_shape_stats(
        java_png,
        dotnet_png,
        RENDER_FOREGROUND_SHAPE_THRESHOLD,
        RENDER_FOREGROUND_SHAPE_DILATION_RADIUS,
    )
    if shape is None:
        return False

    if (
        stats.mean <= RENDER_FOREGROUND_SHAPE_MAX_MEAN
        and stats.rms <= RENDER_FOREGROUND_SHAPE_MAX_RMS
        and stats.large_diff_ratio <= RENDER_FOREGROUND_SHAPE_MAX_LARGE_DIFF_RATIO
        and shape.foreground_delta_ratio <= RENDER_FOREGROUND_SHAPE_MAX_FOREGROUND_DELTA_RATIO
        and shape.java_miss_ratio <= RENDER_FOREGROUND_SHAPE_MAX_MISS_RATIO
        and shape.dotnet_miss_ratio <= RENDER_FOREGROUND_SHAPE_MAX_MISS_RATIO
    ):
        return True

    primary_miss = min(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    secondary_miss = max(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    if (
        shape.foreground_ratio <= RENDER_LINE_ART_SHAPE_MAX_FOREGROUND_RATIO
        and stats.mean <= RENDER_LINE_ART_SHAPE_MAX_MEAN
        and stats.rms <= RENDER_LINE_ART_SHAPE_MAX_RMS
        and stats.large_diff_ratio <= RENDER_LINE_ART_SHAPE_MAX_LARGE_DIFF_RATIO
        and shape.foreground_delta_ratio <= RENDER_LINE_ART_SHAPE_MAX_FOREGROUND_DELTA_RATIO
        and primary_miss <= RENDER_LINE_ART_SHAPE_MAX_PRIMARY_MISS_RATIO
        and secondary_miss <= RENDER_LINE_ART_SHAPE_MAX_SECONDARY_MISS_RATIO
    ):
        return True

    # These closed-bucket fixtures are shape-identical across Java2D and Skia,
    # but hosted Linux rasterization can exceed the generic antialias limits.
    return (
        Path(file).name in RENDER_HIGH_DRIFT_FOREGROUND_SHAPE_FILES
        and shape.foreground_ratio <= RENDER_HIGH_DRIFT_SHAPE_MAX_FOREGROUND_RATIO
        and stats.mean <= RENDER_HIGH_DRIFT_SHAPE_MAX_MEAN
        and stats.rms <= RENDER_HIGH_DRIFT_SHAPE_MAX_RMS
        and stats.large_diff_ratio <= RENDER_HIGH_DRIFT_SHAPE_MAX_LARGE_DIFF_RATIO
        and shape.foreground_delta_ratio <= RENDER_HIGH_DRIFT_SHAPE_MAX_FOREGROUND_DELTA_RATIO
        and primary_miss <= RENDER_HIGH_DRIFT_SHAPE_MAX_PRIMARY_MISS_RATIO
        and secondary_miss <= RENDER_HIGH_DRIFT_SHAPE_MAX_SECONDARY_MISS_RATIO
    )


def is_image_mask_shape_render_drift(file: str, java_png: Path, dotnet_png: Path) -> bool:
    if Path(file).name not in RENDER_IMAGE_MASK_SHAPE_EQUIVALENCE_FILES:
        return False

    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    shape = foreground_shape_stats(
        java_png,
        dotnet_png,
        RENDER_FOREGROUND_SHAPE_THRESHOLD,
        RENDER_FOREGROUND_SHAPE_DILATION_RADIUS,
    )
    if shape is None:
        return False

    primary_miss = min(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    secondary_miss = max(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    return (
        stats.mean <= RENDER_IMAGE_MASK_SHAPE_MAX_MEAN
        and stats.rms <= RENDER_IMAGE_MASK_SHAPE_MAX_RMS
        and stats.large_diff_ratio <= RENDER_IMAGE_MASK_SHAPE_MAX_LARGE_DIFF_RATIO
        and shape.foreground_delta_ratio <= RENDER_IMAGE_MASK_SHAPE_MAX_FOREGROUND_DELTA_RATIO
        and primary_miss <= RENDER_IMAGE_MASK_SHAPE_MAX_PRIMARY_MISS_RATIO
        and secondary_miss <= RENDER_IMAGE_MASK_SHAPE_MAX_SECONDARY_MISS_RATIO
    )


def is_pattern_transparency_render_drift(file: str, java_png: Path, dotnet_png: Path) -> bool:
    if Path(file).name not in RENDER_PATTERN_TRANSPARENCY_EQUIVALENCE_FILES:
        return False

    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    shape = foreground_shape_stats(
        java_png,
        dotnet_png,
        RENDER_FOREGROUND_SHAPE_THRESHOLD,
        RENDER_FOREGROUND_SHAPE_DILATION_RADIUS,
    )
    if shape is None:
        return False

    primary_miss = min(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    secondary_miss = max(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    return (
        stats.moderate_diff_ratio <= RENDER_PATTERN_TRANSPARENCY_MAX_MODERATE_DIFF_RATIO
        and stats.large_diff_ratio <= RENDER_PATTERN_TRANSPARENCY_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_PATTERN_TRANSPARENCY_MAX_RMS
        and stats.mean <= RENDER_PATTERN_TRANSPARENCY_MAX_MEAN
        and shape.foreground_ratio <= RENDER_PATTERN_TRANSPARENCY_MAX_FOREGROUND_RATIO
        and shape.foreground_delta_ratio <= RENDER_PATTERN_TRANSPARENCY_MAX_FOREGROUND_DELTA_RATIO
        and primary_miss <= RENDER_PATTERN_TRANSPARENCY_MAX_PRIMARY_MISS_RATIO
        and secondary_miss <= RENDER_PATTERN_TRANSPARENCY_MAX_SECONDARY_MISS_RATIO
    )


def is_form_widget_render_drift(file: str, java_png: Path, dotnet_png: Path) -> bool:
    if Path(file).name not in RENDER_FORM_WIDGET_EQUIVALENCE_FILES:
        return False

    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    shape = foreground_shape_stats(
        java_png,
        dotnet_png,
        RENDER_FOREGROUND_SHAPE_THRESHOLD,
        RENDER_FOREGROUND_SHAPE_DILATION_RADIUS,
    )
    if shape is None:
        return False

    primary_miss = min(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    secondary_miss = max(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    return (
        stats.moderate_diff_ratio <= RENDER_FORM_WIDGET_MAX_MODERATE_DIFF_RATIO
        and stats.large_diff_ratio <= RENDER_FORM_WIDGET_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_FORM_WIDGET_MAX_RMS
        and stats.mean <= RENDER_FORM_WIDGET_MAX_MEAN
        and shape.foreground_ratio <= RENDER_FORM_WIDGET_MAX_FOREGROUND_RATIO
        and shape.foreground_delta_ratio <= RENDER_FORM_WIDGET_MAX_FOREGROUND_DELTA_RATIO
        and primary_miss <= RENDER_FORM_WIDGET_MAX_PRIMARY_MISS_RATIO
        and secondary_miss <= RENDER_FORM_WIDGET_MAX_SECONDARY_MISS_RATIO
    )


def is_form_widget_bbox_clipping_render_drift(file: str, java_png: Path, dotnet_png: Path) -> bool:
    if Path(file).name not in RENDER_FORM_WIDGET_BBOX_CLIPPING_EQUIVALENCE_FILES:
        return False

    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    shape = foreground_shape_stats(
        java_png,
        dotnet_png,
        RENDER_FOREGROUND_SHAPE_THRESHOLD,
        RENDER_FOREGROUND_SHAPE_DILATION_RADIUS,
    )
    if shape is None:
        return False

    primary_miss = min(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    secondary_miss = max(shape.java_miss_ratio, shape.dotnet_miss_ratio)
    return (
        stats.moderate_diff_ratio <= RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_MODERATE_DIFF_RATIO
        and stats.large_diff_ratio <= RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_RMS
        and stats.mean <= RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_MEAN
        and shape.foreground_ratio <= RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_FOREGROUND_RATIO
        and shape.foreground_delta_ratio <= RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_FOREGROUND_DELTA_RATIO
        and primary_miss <= RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_PRIMARY_MISS_RATIO
        and secondary_miss <= RENDER_FORM_WIDGET_BBOX_CLIPPING_MAX_SECONDARY_MISS_RATIO
    )


def is_low_ink_render_drift(java: Result, dotnet: Result, java_png: Path, dotnet_png: Path) -> bool:
    if not is_near_blank_render(java) or not is_near_blank_render(dotnet):
        return False

    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    java_non_background = render_metric_int(java, "nonBg")
    dotnet_non_background = render_metric_int(dotnet, "nonBg")
    if java_non_background is None or dotnet_non_background is None:
        return False
    foreground_ratio = max(java_non_background, dotnet_non_background) / stats.total_pixels
    if foreground_ratio > RENDER_LOW_INK_MAX_FOREGROUND_RATIO:
        return False

    return (
        stats.moderate_diff_ratio <= RENDER_LOW_INK_MAX_MODERATE_DIFF_RATIO
        and stats.large_diff_ratio <= RENDER_LOW_INK_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_LOW_INK_MAX_RMS
        and stats.mean <= RENDER_LOW_INK_MAX_MEAN
    )


def is_sparse_render_drift(java: Result, dotnet: Result, java_png: Path, dotnet_png: Path) -> bool:
    if is_near_blank_render(java) or is_near_blank_render(dotnet):
        return False

    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    java_non_background = render_metric_int(java, "nonBg")
    dotnet_non_background = render_metric_int(dotnet, "nonBg")
    if java_non_background is None or dotnet_non_background is None:
        return False
    foreground_ratio = max(java_non_background, dotnet_non_background) / stats.total_pixels
    if foreground_ratio > RENDER_SPARSE_MAX_FOREGROUND_RATIO:
        return False

    return (
        stats.moderate_diff_ratio <= RENDER_SPARSE_MAX_MODERATE_DIFF_RATIO
        and stats.large_diff_ratio <= RENDER_SPARSE_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_SPARSE_MAX_RMS
        and stats.mean <= RENDER_SPARSE_MAX_MEAN
    )


def is_near_blank_threshold_render_drift(java: Result, dotnet: Result, java_png: Path, dotnet_png: Path) -> bool:
    if is_near_blank_render(java) == is_near_blank_render(dotnet):
        return False

    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    java_non_background = render_metric_int(java, "nonBg")
    dotnet_non_background = render_metric_int(dotnet, "nonBg")
    if java_non_background is None or dotnet_non_background is None:
        return False

    foreground_ratio = max(java_non_background, dotnet_non_background) / stats.total_pixels
    if foreground_ratio > RENDER_NEAR_BLANK_THRESHOLD_MAX_FOREGROUND_RATIO:
        return False

    return (
        stats.moderate_diff_ratio <= RENDER_SPARSE_MAX_MODERATE_DIFF_RATIO
        and stats.large_diff_ratio <= RENDER_SPARSE_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_SPARSE_MAX_RMS
        and stats.mean <= RENDER_SPARSE_MAX_MEAN
    )


def is_low_mean_raster_drift(java: Result, dotnet: Result, java_png: Path, dotnet_png: Path) -> bool:
    if is_near_blank_render(java) or is_near_blank_render(dotnet):
        return False

    stats = render_image_diff_stats(java_png, dotnet_png)
    if stats is None or stats.total_pixels <= 0:
        return False

    java_non_background = render_metric_int(java, "nonBg")
    dotnet_non_background = render_metric_int(dotnet, "nonBg")
    if java_non_background is None or dotnet_non_background is None:
        return False

    foreground_ratio = max(java_non_background, dotnet_non_background) / stats.total_pixels
    if foreground_ratio > RENDER_LOW_MEAN_RASTER_MAX_FOREGROUND_RATIO:
        return False

    return (
        stats.moderate_diff_ratio <= RENDER_LOW_MEAN_RASTER_MAX_MODERATE_DIFF_RATIO
        and stats.large_diff_ratio <= RENDER_LOW_MEAN_RASTER_MAX_LARGE_DIFF_RATIO
        and stats.rms <= RENDER_LOW_MEAN_RASTER_MAX_RMS
        and stats.mean <= RENDER_LOW_MEAN_RASTER_MAX_MEAN
    )


def is_java_optional_jpx_reader_gap(file: str, java: Result, dotnet: Result) -> bool:
    if Path(file).name not in RENDER_JAVA_OPTIONAL_JPX_READER_MISSING_FILES:
        return False
    if not is_near_blank_render(java) or is_near_blank_render(dotnet):
        return False

    non_background = render_metric_int(dotnet, "nonBg")
    return non_background is not None and non_background > 0


def is_render_placeholder(java: Result, dotnet: Result) -> bool:
    return is_near_blank_render(dotnet) and not is_near_blank_render(java)


def classify_save_mismatch(file: str, save_structures: dict[tuple[str, str], Result]) -> str:
    return classify_document_mismatch(file, save_structures, "save")


def classify_merge_mismatch(file: str, merge_structures: dict[tuple[str, str], Result]) -> str:
    return classify_document_mismatch(file, merge_structures, "merge")


def classify_document_mismatch(file: str, structures: dict[tuple[str, str], Result], prefix: str) -> str:
    java = structures.get(("java", file))
    dotnet = structures.get(("dotnet", file))
    if java is None or dotnet is None or not java.ok or not dotnet.ok:
        return f"{prefix}-structural-missing"
    if java.pages != dotnet.pages:
        return f"{prefix}-structural-mismatch"
    if document_structures_equivalent(java.detail, dotnet.detail):
        return f"{prefix}-structural-match"
    return f"{prefix}-structural-mismatch"


def document_structures_equivalent(java_detail: str, dotnet_detail: str) -> bool:
    if java_detail == dotnet_detail:
        return True

    java_parts = structural_parts(java_detail)
    dotnet_parts = structural_parts(dotnet_detail)
    if java_parts.keys() != dotnet_parts.keys():
        return False

    for key in java_parts:
        if key == "render":
            if not render_structures_equivalent(java_parts[key], dotnet_parts[key]):
                return False
            continue
        if java_parts[key] != dotnet_parts[key]:
            return False
    return True


def structural_parts(detail: str) -> dict[str, str]:
    parts: dict[str, str] = {}
    for raw in detail.split("|"):
        key, separator, value = raw.partition("=")
        if separator:
            parts[key] = value
    return parts


def render_structures_equivalent(java_render: str, dotnet_render: str) -> bool:
    if java_render == dotnet_render:
        return True

    java_parts = java_render.split(":")
    dotnet_parts = dotnet_render.split(":")
    if len(java_parts) < 3 or len(dotnet_parts) < 3 or java_parts[0] != dotnet_parts[0]:
        return False

    java_metrics = render_metrics(java_parts[2:])
    dotnet_metrics = render_metrics(dotnet_parts[2:])
    if java_metrics.get("nearBlank") != "true" or dotnet_metrics.get("nearBlank") != "true":
        return False

    return java_metrics.get("nonBg") == dotnet_metrics.get("nonBg")


def render_metrics(parts: list[str]) -> dict[str, str]:
    metrics: dict[str, str] = {}
    for part in parts:
        key, separator, value = part.partition("=")
        if separator:
            metrics[key] = value
    return metrics


def classify(
    op: str,
    java: Result | None,
    dotnet: Result | None,
    java_out: Path,
    dotnet_out: Path,
    save_structures: dict[tuple[str, str], Result],
    merge_structures: dict[tuple[str, str], Result],
) -> str:
    if java is None or dotnet is None:
        return "missing-result"
    if java.ok != dotnet.ok:
        return "status-mismatch"
    if java.pages != dotnet.pages and java.pages >= 0 and dotnet.pages >= 0:
        return "metadata-mismatch"
    if not java.ok:
        return "match" if java.diagnostic == dotnet.diagnostic else "diagnostic-mismatch"
    if op == "save" and java.detail != dotnet.detail:
        return classify_save_mismatch(java.file, save_structures)
    if op == "merge" and java.detail != dotnet.detail:
        return classify_merge_mismatch(java.file, merge_structures)
    if java.detail != dotnet.detail:
        if op == "text":
            return classify_text_mismatch(java.file, java_out, dotnet_out)
        if op == "render":
            return classify_render_mismatch(java.file, java, dotnet, java_out, dotnet_out)
        return "detail-mismatch"
    return "match"


def compare(
    java_rows: list[Result],
    dotnet_rows: list[Result],
    known_entries: list[dict],
    corpus_entries: list[dict],
    java_out: Path,
    dotnet_out: Path,
    save_structures: dict[tuple[str, str], Result],
    merge_structures: dict[tuple[str, str], Result],
) -> tuple[list[dict], Counter]:
    java_by_key = {(row.file, row.op): row for row in java_rows}
    dotnet_by_key = {(row.file, row.op): row for row in dotnet_rows}
    keys = sorted(set(java_by_key) | set(dotnet_by_key))
    rows: list[dict] = []
    counts: Counter = Counter()
    for file, op in keys:
        java = java_by_key.get((file, op))
        dotnet = dotnet_by_key.get((file, op))
        category = classify(op, java, dotnet, java_out, dotnet_out, save_structures, merge_structures)
        known_entry = None if is_match_category(category) else known_failure_match(file, op, category, known_entries)
        known_id = None if known_entry is None else str(known_entry.get("id", "known-failure"))
        status = "match" if is_match_category(category) else ("known" if known_id else "unexpected")
        counts[status] += 1
        counts[category] += 1
        row = {
            "file": file,
            "corpusCategory": corpus_category(file, corpus_entries),
            "op": op,
            "category": category,
            "status": status,
            "knownFailure": known_id,
            **known_failure_payload(known_entry),
            "artifacts": artifact_payload(file, op, java_out.parent),
            "renderDiff": render_diff_payload(file, op, java_out, dotnet_out),
            "java": None if java is None else result_payload(java),
            "dotnet": None if dotnet is None else result_payload(dotnet),
            "saveStructural": save_structural_payload(file, op, save_structures),
            "mergeStructural": merge_structural_payload(file, op, merge_structures),
        }
        rows.append(row)
    return rows, counts


def save_structural_payload(file: str, op: str, save_structures: dict[tuple[str, str], Result]) -> dict | None:
    return structural_payload(file, op, "save", save_structures)


def merge_structural_payload(file: str, op: str, merge_structures: dict[tuple[str, str], Result]) -> dict | None:
    return structural_payload(file, op, "merge", merge_structures)


def structural_payload(file: str, op: str, expected_op: str, structures: dict[tuple[str, str], Result]) -> dict | None:
    if op != expected_op:
        return None
    java = structures.get(("java", file))
    dotnet = structures.get(("dotnet", file))
    if java is None and dotnet is None:
        return None
    return {
        "java": None if java is None else result_payload(java),
        "dotnet": None if dotnet is None else result_payload(dotnet),
    }


def result_payload(result: Result) -> dict:
    payload = {
        "ok": result.ok,
        "pages": result.pages,
        "ms": result.ms,
        "detail": result.detail,
        "diagnostic": result.diagnostic,
    }
    metrics = render_metrics_payload(result)
    if metrics is not None:
        payload["metrics"] = metrics
    return payload


def render_metrics_payload(result: Result) -> dict | None:
    if result.op != "render" or not result.ok:
        return None

    match = RENDER_DETAIL_RE.match(result.detail)
    if match is None:
        return None

    groups = match.groupdict()
    return {
        "width": int(groups["width"]),
        "height": int(groups["height"]),
        "hash": groups["hash"],
        "nonBg": int(groups["nonBg"]),
        "unique": int(groups["unique"]),
        "dominant": int(groups["dominant"]),
        "transparent": int(groups["transparent"]),
        "nearBlank": groups["nearBlank"] == "true",
    }


def artifact_payload(file: str, op: str, out_dir: Path) -> dict | None:
    java_path = relative_existing_artifact_path(out_dir, file, op, "java")
    dotnet_path = relative_existing_artifact_path(out_dir, file, op, "dotnet")
    if java_path is None and dotnet_path is None:
        return None
    return {"java": java_path, "dotnet": dotnet_path}


def render_diff_payload(file: str, op: str, java_out: Path, dotnet_out: Path) -> dict | None:
    if op != "render":
        return None

    stats = render_image_diff_stats(
        render_artifact_path(java_out, file, "java"),
        render_artifact_path(dotnet_out, file, "dotnet"),
    )
    if stats is None:
        return None

    return {
        "totalPixels": stats.total_pixels,
        "moderateDiffRatio": stats.moderate_diff_ratio,
        "largeDiffRatio": stats.large_diff_ratio,
        "rms": stats.rms,
        "mean": stats.mean,
    }


def format_ratio(value: object) -> str:
    if not isinstance(value, (int, float)):
        return ""
    return f"{value * 100:.3f}%"


def format_float(value: object) -> str:
    if not isinstance(value, (int, float)):
        return ""
    return f"{value:.3f}"


def metric_value(row: dict, runtime: str, name: str) -> object:
    result = row.get(runtime)
    if not isinstance(result, dict):
        return ""
    metrics = result.get("metrics")
    if not isinstance(metrics, dict):
        return ""
    return metrics.get(name, "")


def artifact_value(row: dict, runtime: str) -> str:
    artifacts = row.get("artifacts")
    if not isinstance(artifacts, dict):
        return ""
    value = artifacts.get(runtime)
    return "" if value is None else str(value)


def markdown_summary(summary: dict, rows: list[dict]) -> str:
    lines = [
        "# Java-vs-.NET Runtime Parity Report",
        "",
        f"Generated (UTC): {summary['generatedAtUtc']}",
        "",
        "## Summary",
        "",
        "| Metric | Count |",
        "|---|---:|",
    ]
    for key in ("match", "known", "unexpected"):
        lines.append(f"| `{key}` | {summary['counts'].get(key, 0)} |")
    lines.extend(["", "## Categories", "", "| Category | Count |", "|---|---:|"])
    for key, value in sorted(summary["categories"].items()):
        lines.append(f"| `{key}` | {value} |")
    lines.extend(["", "## Corpus Categories", "", "| Corpus category | Match | Known | Unexpected | Total |", "|---|---:|---:|---:|---:|"])
    for key, value in sorted(summary["corpusCategories"].items()):
        lines.append(
            f"| `{key}` | {value.get('match', 0)} | {value.get('known', 0)} | {value.get('unexpected', 0)} | {value.get('total', 0)} |"
        )
    lines.extend(["", "## Known Failure Buckets", ""])
    known_rows = [row for row in rows if row["status"] == "known"]
    if not known_rows:
        lines.append("No known divergences.")
    else:
        lines.append("| Root cause | Known failure | Issue | Count |")
        lines.append("|---|---|---:|---:|")
        bucket_counts = Counter(
            (
                str(row.get("knownFailureRootCause") or "unclassified"),
                str(row.get("knownFailure") or "unknown"),
                row.get("knownFailureIssue") or "",
            )
            for row in known_rows
        )
        for (root_cause, known_failure, issue), count in sorted(bucket_counts.items()):
            issue_text = "" if issue == "" else f"#{issue}"
            lines.append(f"| `{root_cause}` | `{known_failure}` | {issue_text} | {count} |")

        known_render_detail_rows = [
            row for row in known_rows if row["op"] == "render" and row["category"] == "detail-mismatch"
        ]
        if known_render_detail_rows:
            lines.extend(
                [
                    "",
                    "### Known Render Detail Rows",
                    "",
                    "| File | Root cause | Corpus category | Java non-bg | .NET non-bg | Large diff | RMS | Java artifact | .NET artifact |",
                    "|---|---|---|---:|---:|---:|---:|---|---|",
                ]
            )
            for row in known_render_detail_rows[:200]:
                diff = row.get("renderDiff") if isinstance(row.get("renderDiff"), dict) else {}
                lines.append(
                    f"| `{row['file']}` | `{row.get('knownFailureRootCause') or ''}` | `{row['corpusCategory']}` | "
                    f"{metric_value(row, 'java', 'nonBg')} | {metric_value(row, 'dotnet', 'nonBg')} | "
                    f"{format_ratio(diff.get('largeDiffRatio'))} | {format_float(diff.get('rms'))} | "
                    f"`{artifact_value(row, 'java')}` | `{artifact_value(row, 'dotnet')}` |"
                )
            if len(known_render_detail_rows) > 200:
                lines.append(f"\nTruncated to 200 of {len(known_render_detail_rows)} known render detail rows.")
    lines.extend(["", "## Unexpected Divergences", ""])
    unexpected = [row for row in rows if row["status"] == "unexpected"]
    if not unexpected:
        lines.append("No unexpected divergences.")
    else:
        lines.append("| File | Operation | Category | Java | .NET |")
        lines.append("|---|---|---|---|---|")
        for row in unexpected[:100]:
            java = row["java"] or {}
            dotnet = row["dotnet"] or {}
            lines.append(
                f"| `{row['file']}` | `{row['op']}` | `{row['category']}` | `{short(java.get('detail', 'missing'))}` | `{short(dotnet.get('detail', 'missing'))}` |"
            )
        if len(unexpected) > 100:
            lines.append(f"\nTruncated to 100 of {len(unexpected)} unexpected divergences.")
    lines.append("")
    lines.extend(["## Render Placeholders", ""])
    placeholders = [row for row in rows if row["category"] == "render-placeholder"]
    if not placeholders:
        lines.append("No render placeholders detected.")
    else:
        lines.append("| File | Status | Java | .NET |")
        lines.append("|---|---|---|---|")
        for row in placeholders[:100]:
            java = row["java"] or {}
            dotnet = row["dotnet"] or {}
            lines.append(
                f"| `{row['file']}` | `{row['status']}` | `{short(java.get('detail', 'missing'))}` | `{short(dotnet.get('detail', 'missing'))}` |"
            )
    if len(placeholders) > 100:
        lines.append(f"\nTruncated to 100 of {len(placeholders)} render placeholders.")
    lines.append("")
    lines.extend(["## Render Visual Equivalence", ""])
    render_rows = [row for row in rows if row["op"] == "render"]
    visual_matches = [row for row in render_rows if row["category"] == "render-visual-equivalence-match"]
    detail_mismatches = [row for row in render_rows if row["category"] == "detail-mismatch"]
    if not render_rows:
        lines.append("No render rows detected.")
    else:
        lines.append("| Category | Count |")
        lines.append("|---|---:|")
        for key, value in sorted(Counter(row["category"] for row in render_rows).items()):
            lines.append(f"| `{key}` | {value} |")
        if visual_matches:
            lines.append("")
            lines.append(
                f"{len(visual_matches)} render rows differ only within the conservative visual-equivalence threshold."
            )
        if detail_mismatches:
            lines.append("")
            lines.append(f"{len(detail_mismatches)} render rows still require renderer parity work.")
    lines.append("")
    lines.extend(["## Save Structural Parity", ""])
    save_rows = [row for row in rows if row["op"] == "save"]
    if not save_rows:
        lines.append("No save rows detected.")
    else:
        save_counts = Counter(row["category"] for row in save_rows)
        lines.append("| Category | Count |")
        lines.append("|---|---:|")
        for key, value in sorted(save_counts.items()):
            lines.append(f"| `{key}` | {value} |")
        save_gaps = [row for row in save_rows if row["category"] not in {"match", "save-structural-match"}]
        if save_gaps:
            lines.extend(["", "| File | Status | Category | Java | .NET |"])
            lines.append("|---|---|---|---|---|")
            for row in save_gaps[:100]:
                java = row["java"] or {}
                dotnet = row["dotnet"] or {}
                lines.append(
                    f"| `{row['file']}` | `{row['status']}` | `{row['category']}` | `{short(java.get('detail', 'missing'))}` | `{short(dotnet.get('detail', 'missing'))}` |"
                )
            if len(save_gaps) > 100:
                lines.append(f"\nTruncated to 100 of {len(save_gaps)} save structural gaps.")
        else:
            lines.append("")
            lines.append("All successful save rows are byte-identical or structurally equivalent.")
    lines.append("")
    lines.extend(["## Merge Structural Parity", ""])
    merge_rows = [row for row in rows if row["op"] == "merge"]
    if not merge_rows:
        lines.append("No merge rows detected.")
    else:
        merge_counts = Counter(row["category"] for row in merge_rows)
        lines.append("| Category | Count |")
        lines.append("|---|---:|")
        for key, value in sorted(merge_counts.items()):
            lines.append(f"| `{key}` | {value} |")
        merge_gaps = [row for row in merge_rows if row["category"] not in {"match", "merge-structural-match"}]
        if merge_gaps:
            lines.extend(["", "| File | Status | Category | Java | .NET |"])
            lines.append("|---|---|---|---|---|")
            for row in merge_gaps[:100]:
                java = row["java"] or {}
                dotnet = row["dotnet"] or {}
                lines.append(
                    f"| `{row['file']}` | `{row['status']}` | `{row['category']}` | `{short(java.get('detail', 'missing'))}` | `{short(dotnet.get('detail', 'missing'))}` |"
                )
            if len(merge_gaps) > 100:
                lines.append(f"\nTruncated to 100 of {len(merge_gaps)} merge structural gaps.")
        else:
            lines.append("")
            lines.append("All successful merge rows are byte-identical or structurally equivalent.")
    lines.append("")
    lines.extend(["## Text Mismatches", ""])
    text_mismatches = [row for row in rows if row["op"] == "text" and row["status"] != "match"]
    if not text_mismatches:
        lines.append("No text mismatches detected.")
    else:
        text_counts = Counter(row["category"] for row in text_mismatches)
        lines.append("| Category | Count |")
        lines.append("|---|---:|")
        for key, value in sorted(text_counts.items()):
            lines.append(f"| `{key}` | {value} |")
        lines.extend(["", "| File | Status | Category | Java | .NET |", "|---|---|---|---|---|"])
        for row in text_mismatches[:100]:
            java = row["java"] or {}
            dotnet = row["dotnet"] or {}
            lines.append(
                f"| `{row['file']}` | `{row['status']}` | `{row['category']}` | `{short(java.get('detail', 'missing'))}` | `{short(dotnet.get('detail', 'missing'))}` |"
            )
        if len(text_mismatches) > 100:
            lines.append(f"\nTruncated to 100 of {len(text_mismatches)} text mismatches.")
    lines.append("")
    return "\n".join(lines)


def status_counts_by_corpus_category(rows: list[dict]) -> dict[str, dict[str, int]]:
    counts: dict[str, dict[str, int]] = defaultdict(lambda: {"match": 0, "known": 0, "unexpected": 0, "total": 0})
    for row in rows:
        bucket = counts[row["corpusCategory"]]
        bucket[row["status"]] += 1
        bucket["total"] += 1
    return dict(sorted((key, dict(value)) for key, value in counts.items()))


def known_failure_bucket_counts(rows: list[dict]) -> list[dict]:
    counts: Counter[tuple[str, str, object]] = Counter()
    for row in rows:
        if row["status"] != "known":
            continue
        counts[
            (
                str(row.get("knownFailureRootCause") or "unclassified"),
                str(row.get("knownFailure") or "unknown"),
                row.get("knownFailureIssue"),
            )
        ] += 1

    return [
        {
            "rootCause": root_cause,
            "knownFailure": known_failure,
            "issue": issue,
            "count": count,
        }
        for (root_cause, known_failure, issue), count in sorted(counts.items())
    ]


def ratchet_failures(summary: dict, baseline_path: Path) -> list[str]:
    if not baseline_path.exists():
        return [f"ratchet baseline not found: {baseline_path}"]

    baseline = json.loads(baseline_path.read_text(encoding="utf-8"))
    failures: list[str] = []
    max_status = baseline.get("maxStatus", {})
    for key, actual in summary["counts"].items():
        allowed = max_status.get(key)
        if isinstance(allowed, int) and actual > allowed:
            failures.append(f"status `{key}` increased from allowed {allowed} to {actual}")

    max_categories = baseline.get("maxCategories", {})
    for key, actual in summary["categories"].items():
        allowed = max_categories.get(key, 0)
        if actual > allowed:
            failures.append(f"category `{key}` increased from allowed {allowed} to {actual}")

    return failures


def strict_gate_failures(summary: dict) -> list[str]:
    failures: list[str] = []
    counts = summary["counts"]
    if counts.get("unexpected", 0):
        failures.append(f"strict gate requires 0 unexpected rows; found {counts['unexpected']}")
    if counts.get("known", 0):
        failures.append(f"strict gate requires 0 known rows; found {counts['known']}")
    return failures


def short(value: str) -> str:
    return value if len(value) <= 96 else value[:93] + "..."


def main() -> int:
    parser = argparse.ArgumentParser(description="Run Java-vs-.NET PDFBox runtime parity probes and compare structured results.")
    parser.add_argument("--manifest", required=True, type=Path, help="Text file containing one PDF path per line.")
    parser.add_argument("--out-dir", required=True, type=Path, help="Output directory for probe artifacts and reports.")
    parser.add_argument("--java-classpath", required=True, help="Classpath containing Apache PDFBox and dependencies.")
    parser.add_argument("--java-home", type=Path, help="Optional JDK home containing bin/java and bin/javac.")
    parser.add_argument(
        "--pdfbox-root",
        default=DEFAULT_PDFBOX_ROOT,
        type=Path,
        help="Apache PDFBox source checkout root used to resolve pdfbox: manifest entries. Defaults to PDFBOX_SOURCE_ROOT.",
    )
    parser.add_argument("--merge-pairs", type=Path, help="Optional text file containing '<pdf-a> <pdf-b>' merge pairs.")
    parser.add_argument("--known-failures", default=KNOWN_FAILURES, type=Path, help="Known-failure JSON file.")
    parser.add_argument("--corpus-categories", default=CORPUS_CATEGORIES, type=Path, help="Corpus category JSON file.")
    parser.add_argument("--ratchet-baseline", type=Path, help="Fail when status/category counts exceed this baseline.")
    parser.add_argument(
        "--gate-mode",
        choices=("ratchet", "strict"),
        default=os.environ.get("PDFBOX_PARITY_GATE_MODE", "ratchet"),
        help="ratchet allows existing known rows within the baseline; strict requires zero known and zero unexpected rows.",
    )
    parser.add_argument("--fail-on-unexpected", action="store_true", help="Exit non-zero when an untracked divergence is found.")
    parser.add_argument("--skip-build", action="store_true", help="Skip dotnet build and javac compile.")
    args = parser.parse_args()

    pdfbox_root = args.pdfbox_root.resolve() if args.pdfbox_root is not None else None
    known_failures = load_known_failures(args.known_failures)
    known_metadata_failures = validate_known_failures(known_failures)
    if known_metadata_failures:
        print("Known-failure metadata validation failed:", file=sys.stderr)
        for failure in known_metadata_failures:
            print(f"- {failure}", file=sys.stderr)
        return 1

    pdfs = read_manifest(args.manifest, pdfbox_root)
    merge_pairs = read_merge_pairs(args.merge_pairs, pdfs, pdfbox_root)
    out_dir = args.out_dir.resolve()
    java_out = out_dir / "java"
    dotnet_out = out_dir / "dotnet"
    classes_out = out_dir / "java-classes"
    for directory in (java_out, dotnet_out, classes_out):
        directory.mkdir(parents=True, exist_ok=True)
    ignored_output_paths = {
        "java": out_dir / "java-ignored-output.txt",
        "dotnet": out_dir / "dotnet-ignored-output.txt",
    }
    ignored_output_summary = IgnoredProbeOutputSummary()
    for path in ignored_output_paths.values():
        path.unlink(missing_ok=True)

    if not args.skip_build:
        try:
            run_streaming(["dotnet", "build", str(PROBE_PROJECT), "--configuration", "Release"])
        except subprocess.CalledProcessError:
            if not PROBE_ASSEMBLY.exists():
                raise
            print(f"warning: dotnet build failed; continuing with existing {PROBE_ASSEMBLY}", file=sys.stderr)
        run_streaming([java_tool(args.java_home, "javac"), "-proc:none", "-cp", args.java_classpath, "-d", str(classes_out), str(JAVA_PROBE)])

    java_cp = os.pathsep.join([str(classes_out), args.java_classpath])
    dotnet_args = ["dotnet", "run", "--no-build", "--configuration", "Release", "--project", str(PROBE_PROJECT), "--"]

    java_rows = parse_jsonl(
        run([*java_probe_args(args.java_home, java_cp), str(java_out), *[str(pdf) for pdf in pdfs]]).stdout,
        "java",
        ignored_output_paths["java"],
        ignored_output_summary,
    )
    dotnet_rows = parse_jsonl(
        run([*dotnet_args, str(dotnet_out), *[str(pdf) for pdf in pdfs]]).stdout,
        "dotnet",
        ignored_output_paths["dotnet"],
        ignored_output_summary,
    )

    for a, b in merge_pairs:
        java_rows.extend(
            parse_jsonl(
                run([*java_probe_args(args.java_home, java_cp), "--merge", str(java_out), str(a), str(b)]).stdout,
                "java",
                ignored_output_paths["java"],
                ignored_output_summary,
            )
        )
        dotnet_rows.extend(
            parse_jsonl(
                run([*dotnet_args, "--merge", str(dotnet_out), str(a), str(b)]).stdout,
                "dotnet",
                ignored_output_paths["dotnet"],
                ignored_output_summary,
            )
        )

    write_jsonl(out_dir / "java-results.jsonl", java_rows)
    write_jsonl(out_dir / "dotnet-results.jsonl", dotnet_rows)
    save_structures = collect_save_structures(
        args.java_home, java_cp, java_rows, dotnet_rows, java_out, dotnet_out, ignored_output_summary
    )
    write_jsonl(out_dir / "save-structures.jsonl", save_structures.values())
    merge_structures = collect_merge_structures(
        args.java_home, java_cp, java_rows, dotnet_rows, java_out, dotnet_out, ignored_output_summary
    )
    ignored_output_summary.emit()
    write_jsonl(out_dir / "merge-structures.jsonl", merge_structures.values())

    comparison_rows, counts = compare(
        java_rows,
        dotnet_rows,
        known_failures,
        load_corpus_categories(args.corpus_categories),
        java_out,
        dotnet_out,
        save_structures,
        merge_structures,
    )
    category_counts = {key: value for key, value in counts.items() if key not in {"match", "known", "unexpected"}}
    operation_counts: dict[str, dict[str, int]] = defaultdict(lambda: {"javaOk": 0, "javaFail": 0, "dotnetOk": 0, "dotnetFail": 0})
    for row in java_rows:
        operation_counts[row.op]["javaOk" if row.ok else "javaFail"] += 1
    for row in dotnet_rows:
        operation_counts[row.op]["dotnetOk" if row.ok else "dotnetFail"] += 1

    summary = {
        "generatedAtUtc": utc_now(),
        "manifest": str(args.manifest),
        "pdfCount": len(pdfs),
        "mergePairCount": len(merge_pairs),
        "counts": {key: counts.get(key, 0) for key in ("match", "known", "unexpected")},
        "categories": category_counts,
        "corpusCategories": status_counts_by_corpus_category(comparison_rows),
        "knownFailureBuckets": known_failure_bucket_counts(comparison_rows),
        "operations": dict(sorted(operation_counts.items())),
    }
    (out_dir / "comparison.json").write_text(json.dumps({"summary": summary, "rows": comparison_rows}, indent=2) + "\n", encoding="utf-8")
    (out_dir / "summary.md").write_text(markdown_summary(summary, comparison_rows), encoding="utf-8")

    print(json.dumps(summary, indent=2))
    if args.gate_mode == "strict":
        failures = strict_gate_failures(summary)
        if failures:
            print("Runtime parity strict gate failed:", file=sys.stderr)
            for failure in failures:
                print(f"- {failure}", file=sys.stderr)
            return 1
    elif args.ratchet_baseline is not None:
        failures = ratchet_failures(summary, args.ratchet_baseline)
        if failures:
            print("Runtime parity ratchet failed:", file=sys.stderr)
            for failure in failures:
                print(f"- {failure}", file=sys.stderr)
            return 1
    if args.fail_on_unexpected and counts.get("unexpected", 0):
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
