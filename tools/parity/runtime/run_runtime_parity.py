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
IGNORED_PROBE_OUTPUT_SAMPLE_LIMIT = 8


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


def warn_ignored_probe_output(runtime: str, lines: list[str]) -> None:
    if not lines:
        return

    counts = Counter(lines)
    unique_count = len(counts)
    sample_count = min(unique_count, IGNORED_PROBE_OUTPUT_SAMPLE_LIMIT)
    print(
        f"warning: ignored {len(lines)} non-JSON {runtime} probe output lines "
        f"(showing {sample_count} of {unique_count} unique):",
        file=sys.stderr,
    )
    for line, count in counts.most_common(IGNORED_PROBE_OUTPUT_SAMPLE_LIMIT):
        print(f"warning:   {count}x {line}", file=sys.stderr)


def parse_jsonl(text: str, runtime: str) -> list[Result]:
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
    warn_ignored_probe_output(runtime, ignored_lines)
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


def known_failure_id(file: str, op: str, category: str, entries: list[dict]) -> str | None:
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
        return str(entry.get("id", "known-failure"))
    return None


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
    )


def collect_merge_structures(
    java_home: Path | None,
    java_cp: str,
    java_rows: list[Result],
    dotnet_rows: list[Result],
    java_out: Path,
    dotnet_out: Path,
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
    if is_low_ink_render_drift(java, dotnet, java_png, dotnet_png):
        return "render-low-ink-equivalence-match"
    if is_sparse_render_drift(java, dotnet, java_png, dotnet_png):
        return "render-sparse-equivalence-match"
    if is_near_blank_threshold_render_drift(java, dotnet, java_png, dotnet_png):
        return "render-near-blank-threshold-equivalence-match"
    if is_render_placeholder(java, dotnet):
        return "render-placeholder"
    return "detail-mismatch"


def is_lossy_jpeg_decoder_drift(file: str, java_png: Path, dotnet_png: Path) -> bool:
    normalized_name = Path(file).name.lower()
    if "jpeg" not in normalized_name and "jpg" not in normalized_name:
        return False
    return render_jpeg_images_equivalent(java_png, dotnet_png)


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


def is_java_optional_jpx_reader_gap(file: str, java: Result, dotnet: Result) -> bool:
    if "JPX" not in Path(file).name:
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
        known_id = None if is_match_category(category) else known_failure_id(file, op, category, known_entries)
        status = "match" if is_match_category(category) else ("known" if known_id else "unexpected")
        counts[status] += 1
        counts[category] += 1
        rows.append(
            {
                "file": file,
                "corpusCategory": corpus_category(file, corpus_entries),
                "op": op,
                "category": category,
                "status": status,
                "knownFailure": known_id,
                "java": None if java is None else result_payload(java),
                "dotnet": None if dotnet is None else result_payload(dotnet),
                "saveStructural": save_structural_payload(file, op, save_structures),
                "mergeStructural": merge_structural_payload(file, op, merge_structures),
            }
        )
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
    return {
        "ok": result.ok,
        "pages": result.pages,
        "ms": result.ms,
        "detail": result.detail,
        "diagnostic": result.diagnostic,
    }


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
    )
    dotnet_rows = parse_jsonl(
        run([*dotnet_args, str(dotnet_out), *[str(pdf) for pdf in pdfs]]).stdout,
        "dotnet",
    )

    for a, b in merge_pairs:
        java_rows.extend(parse_jsonl(run([*java_probe_args(args.java_home, java_cp), "--merge", str(java_out), str(a), str(b)]).stdout, "java"))
        dotnet_rows.extend(parse_jsonl(run([*dotnet_args, "--merge", str(dotnet_out), str(a), str(b)]).stdout, "dotnet"))

    write_jsonl(out_dir / "java-results.jsonl", java_rows)
    write_jsonl(out_dir / "dotnet-results.jsonl", dotnet_rows)
    save_structures = collect_save_structures(args.java_home, java_cp, java_rows, dotnet_rows, java_out, dotnet_out)
    write_jsonl(out_dir / "save-structures.jsonl", save_structures.values())
    merge_structures = collect_merge_structures(args.java_home, java_cp, java_rows, dotnet_rows, java_out, dotnet_out)
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
