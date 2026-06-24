#!/usr/bin/env python3
from __future__ import annotations

import argparse
import fnmatch
import json
import os
import re
import subprocess
import sys
import unicodedata
from collections import Counter, defaultdict
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Iterable


ROOT = Path(__file__).resolve().parents[3]
PROBE_PROJECT = ROOT / "tools/parity/runtime/DotnetPdfProbe/DotnetPdfProbe.csproj"
PROBE_ASSEMBLY = ROOT / "tools/parity/runtime/DotnetPdfProbe/bin/Release/net10.0/DotnetPdfProbe.dll"
JAVA_PROBE = ROOT / "tools/parity/runtime/JavaPdfProbe.java"
KNOWN_FAILURES = ROOT / "tools/parity/runtime/known-failures.json"
CORPUS_CATEGORIES = ROOT / "tools/parity/runtime/corpus-categories.json"


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


def read_manifest(path: Path) -> list[Path]:
    pdfs: list[Path] = []
    with path.open("r", encoding="utf-8") as f:
        for raw in f:
            line = raw.strip()
            if not line or line.startswith("#"):
                continue
            pdfs.append(Path(line).expanduser().resolve())
    return pdfs


def read_merge_pairs(path: Path | None, pdfs: list[Path]) -> list[tuple[Path, Path]]:
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
            pairs.append((Path(parts[0]).expanduser().resolve(), Path(parts[1]).expanduser().resolve()))
    return pairs


def parse_jsonl(text: str, runtime: str) -> list[Result]:
    results: list[Result] = []
    for raw in text.splitlines():
        line = raw.strip()
        if not line:
            continue
        if not line.startswith("{"):
            print(f"warning: ignored non-JSON {runtime} probe output: {line}", file=sys.stderr)
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
    return category == "match" or (category.startswith("text-semantic-") and category.endswith("-match"))


def contains_non_ascii(text: str) -> bool:
    return any(ord(ch) > 127 for ch in text)


def text_artifact_name(file: str, runtime: str) -> str:
    return f"{Path(file).stem}-{runtime}-text.txt"


def read_text_artifact(out_dir: Path, file: str, runtime: str) -> str | None:
    path = out_dir / text_artifact_name(file, runtime)
    if not path.exists():
        return None
    return path.read_text(encoding="utf-8")


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


def classify(op: str, java: Result | None, dotnet: Result | None, java_out: Path, dotnet_out: Path) -> str:
    if java is None or dotnet is None:
        return "missing-result"
    if java.ok != dotnet.ok:
        return "status-mismatch"
    if java.pages != dotnet.pages and java.pages >= 0 and dotnet.pages >= 0:
        return "metadata-mismatch"
    if not java.ok:
        return "match" if java.diagnostic == dotnet.diagnostic else "diagnostic-mismatch"
    if op == "render" and is_near_blank_render(dotnet) and not is_near_blank_render(java):
        return "render-placeholder"
    if java.detail != dotnet.detail:
        if op == "text":
            return classify_text_mismatch(java.file, java_out, dotnet_out)
        return "detail-mismatch"
    return "match"


def compare(
    java_rows: list[Result],
    dotnet_rows: list[Result],
    known_entries: list[dict],
    corpus_entries: list[dict],
    java_out: Path,
    dotnet_out: Path,
) -> tuple[list[dict], Counter]:
    java_by_key = {(row.file, row.op): row for row in java_rows}
    dotnet_by_key = {(row.file, row.op): row for row in dotnet_rows}
    keys = sorted(set(java_by_key) | set(dotnet_by_key))
    rows: list[dict] = []
    counts: Counter = Counter()
    for file, op in keys:
        java = java_by_key.get((file, op))
        dotnet = dotnet_by_key.get((file, op))
        category = classify(op, java, dotnet, java_out, dotnet_out)
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
            }
        )
    return rows, counts


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


def short(value: str) -> str:
    return value if len(value) <= 96 else value[:93] + "..."


def main() -> int:
    parser = argparse.ArgumentParser(description="Run Java-vs-.NET PDFBox runtime parity probes and compare structured results.")
    parser.add_argument("--manifest", required=True, type=Path, help="Text file containing one PDF path per line.")
    parser.add_argument("--out-dir", required=True, type=Path, help="Output directory for probe artifacts and reports.")
    parser.add_argument("--java-classpath", required=True, help="Classpath containing Apache PDFBox and dependencies.")
    parser.add_argument("--java-home", type=Path, help="Optional JDK home containing bin/java and bin/javac.")
    parser.add_argument("--merge-pairs", type=Path, help="Optional text file containing '<pdf-a> <pdf-b>' merge pairs.")
    parser.add_argument("--known-failures", default=KNOWN_FAILURES, type=Path, help="Known-failure JSON file.")
    parser.add_argument("--corpus-categories", default=CORPUS_CATEGORIES, type=Path, help="Corpus category JSON file.")
    parser.add_argument("--ratchet-baseline", type=Path, help="Fail when status/category counts exceed this baseline.")
    parser.add_argument("--fail-on-unexpected", action="store_true", help="Exit non-zero when an untracked divergence is found.")
    parser.add_argument("--skip-build", action="store_true", help="Skip dotnet build and javac compile.")
    args = parser.parse_args()

    pdfs = read_manifest(args.manifest)
    merge_pairs = read_merge_pairs(args.merge_pairs, pdfs)
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

    comparison_rows, counts = compare(
        java_rows,
        dotnet_rows,
        load_known_failures(args.known_failures),
        load_corpus_categories(args.corpus_categories),
        java_out,
        dotnet_out,
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
    if args.ratchet_baseline is not None:
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
