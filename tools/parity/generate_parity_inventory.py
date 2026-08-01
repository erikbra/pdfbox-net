#!/usr/bin/env python3
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import subprocess
import urllib.request
from collections import Counter, defaultdict
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Iterable


SOURCE_PATH_RE = re.compile(r"PDFBOX_SOURCE_PATH:\s*(.+?)\s*$")
REPORTS_DIR = Path("reports")
SYNC_STATE_PATH = REPORTS_DIR / "upstream-sync-state.json"
TRACEABILITY_PATH = REPORTS_DIR / "traceability-parity-report.json"
COVERAGE_STATE_PATH = REPORTS_DIR / "upstream-port-coverage-state.json"
ALL_COVERAGE_PATH = REPORTS_DIR / "all-upstream-coverage.json"
GAP_ANALYSIS_PATH = REPORTS_DIR / "pdfbox-main-gap-analysis.md"
FILE_COMPARISON_PATH = REPORTS_DIR / "upstream-file-comparison.json"


@dataclass(frozen=True)
class UpstreamPath:
    module: str
    path: str
    family: str


def utc_now_iso() -> str:
    return (
        datetime.now(timezone.utc)
        .isoformat(timespec="milliseconds")
        .replace("+00:00", "Z")
    )


def read_json(path: Path):
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def write_json(path: Path, payload: object) -> None:
    with path.open("w", encoding="utf-8") as f:
        json.dump(payload, f, indent=2)
        f.write("\n")


def github_json(url: str, token: str) -> dict:
    headers = {
        "Accept": "application/vnd.github+json",
        "X-GitHub-Api-Version": "2022-11-28",
    }
    if token:
        headers["Authorization"] = "Bearer " + token
    req = urllib.request.Request(url, headers=headers)
    with urllib.request.urlopen(req) as resp:
        return json.load(resp)


def extract_family(upstream_path: str) -> str:
    marker = "/src/main/java/"
    idx = upstream_path.find(marker)
    if idx < 0:
        return "(unknown)"

    pkg = upstream_path[idx + len(marker) :].split("/")
    if len(pkg) < 4:
        return "(root)"

    # Typical paths:
    # org/apache/pdfbox/<family>/...
    # org/apache/fontbox/<family>/...
    # org/apache/xmpbox/<family>/...
    root_pkg = pkg[2]
    if len(pkg) == 4:
        return "(root)"
    if root_pkg in {"pdfbox", "fontbox", "xmpbox"}:
        return pkg[3]
    return root_pkg


def discover_upstream_paths(tree: Iterable[dict]) -> list[UpstreamPath]:
    scoped: list[UpstreamPath] = []
    for item in tree:
        path = item.get("path", "")
        if item.get("type") != "blob":
            continue
        if not path.endswith(".java") or "/src/main/java/" not in path:
            continue
        module = path.split("/", 1)[0]
        scoped.append(UpstreamPath(module=module, path=path, family=extract_family(path)))
    scoped.sort(key=lambda p: p.path)
    return scoped


def discover_upstream_path_names(path_names: Iterable[str]) -> list[UpstreamPath]:
    scoped: list[UpstreamPath] = []
    for path in path_names:
        if not path.endswith(".java") or "/src/main/java/" not in path:
            continue
        module = path.split("/", 1)[0]
        scoped.append(UpstreamPath(module=module, path=path, family=extract_family(path)))
    scoped.sort(key=lambda p: p.path)
    return scoped


def discover_local_upstream_paths(upstream_root: Path) -> list[UpstreamPath]:
    scoped: list[UpstreamPath] = []
    for java_file in upstream_root.rglob("*.java"):
        try:
            relative_path = java_file.relative_to(upstream_root).as_posix()
        except ValueError:
            continue
        if "/src/main/java/" not in relative_path:
            continue
        module = relative_path.split("/", 1)[0]
        scoped.append(
            UpstreamPath(
                module=module,
                path=relative_path,
                family=extract_family(relative_path),
            )
        )
    scoped.sort(key=lambda p: p.path)
    return scoped


def local_git_ref(upstream_root: Path, ref: str) -> str:
    result = subprocess.run(
        ["git", "-C", str(upstream_root), "rev-parse", ref],
        check=True,
        capture_output=True,
        encoding="utf-8",
    )
    return result.stdout.strip()


def discover_local_git_ref_paths(upstream_root: Path, ref: str) -> list[UpstreamPath]:
    result = subprocess.run(
        ["git", "-C", str(upstream_root), "ls-tree", "-r", "--name-only", ref],
        check=True,
        capture_output=True,
        encoding="utf-8",
    )
    return discover_upstream_path_names(result.stdout.splitlines())


def github_ref_name(ref: str) -> str:
    return ref.removeprefix("origin/")


def parse_excluded_modules(values: Iterable[str]) -> list[str]:
    modules: set[str] = set()
    for value in values:
        for item in value.split(","):
            item = item.strip()
            if item:
                modules.add(item)
    return sorted(modules)


def collect_provenance_paths(src_root: Path) -> dict[str, list[str]]:
    result: dict[str, list[str]] = defaultdict(list)
    for cs_file in src_root.rglob("*.cs"):
        try:
            text = cs_file.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            continue
        source_path_value: str | None = None
        for line in text.splitlines():
            m = SOURCE_PATH_RE.search(line)
            if not m:
                continue
            source_path = m.group(1).strip()
            if source_path.startswith("("):
                # Placeholder provenance marker; treat this file as unmapped.
                source_path_value = None
                break
            source_path_value = source_path
            break
        if source_path_value:
            result[source_path_value].append(cs_file.as_posix())
    return result


def collect_traceability_paths(rows: list[dict], upstream_set: set[str]) -> set[str]:
    result: set[str] = set()
    for row in rows:
        source_path = row.get("source_path")
        if isinstance(source_path, str) and source_path in upstream_set:
            result.add(source_path)
    return result


def collect_traceability_targets(rows: list[dict], upstream_set: set[str]) -> dict[str, list[str]]:
    result: dict[str, list[str]] = defaultdict(list)
    for row in rows:
        source_path = row.get("source_path")
        target_path = row.get("target_path")
        if (
            isinstance(source_path, str)
            and source_path in upstream_set
            and isinstance(target_path, str)
            and target_path
        ):
            result[source_path].append(target_path)
    return result


def build_gap_analysis_markdown(
    *,
    generated_at: str,
    tracked_commit: str,
    upstream_head: str,
    module_rows: list[dict],
    totals: dict,
    core_total: int,
    core_mapped: int,
    status_counts: dict,
    file_gap_counts: dict,
    file_gap_module_rows: list[dict],
    upstream_ref: str,
    excluded_modules: list[str],
    excluded_module_counts: dict[str, int],
) -> str:
    lines: list[str] = []
    lines.append("# PDFBox Upstream Java Gap Analysis (All Modules)")
    lines.append("")
    lines.append(f"Datetime (UTC): {generated_at}")
    lines.append(f"Reference upstream Java repository: Apache PDFBox `{upstream_ref}`")
    lines.append(f"Tracked parity baseline commit: `{tracked_commit}`")
    lines.append(f"Latest upstream head scanned: `{upstream_head}`")
    lines.append("")
    lines.append("## Scope and method")
    lines.append("")
    lines.append("- Scanned **all current upstream Java files** under `**/src/main/java/**/*.java`.")
    lines.append("- Counted Java source as mapped using the canonical union of:")
    lines.append("  - `PDFBOX_SOURCE_PATH` matches in `src/**/*.cs`, and")
    lines.append("  - `source_path` rows in `reports/traceability-parity-report.json`.")
    if excluded_modules:
        lines.append("")
        lines.append("Excluded upstream modules:")
        for module in excluded_modules:
            lines.append(f"- `{module}`: {excluded_module_counts.get(module, 0)} Java files")
    lines.append("")
    lines.append("## Summary")
    lines.append("")
    lines.append("| Upstream module | Java files | Mapped C# ports | Missing | % Done |")
    lines.append("|---|---:|---:|---:|---:|")
    for row in module_rows:
        lines.append(
            f"| `{row['module']}` | {row['java_files']} | {row['mapped']} | {row['missing']} | {row['pct']:.1f}% |"
        )
    lines.append(
        f"| **TOTAL** | **{totals['total']}** | **{totals['mapped']}** | **{totals['missing']}** | **{totals['pct']:.1f}%** |"
    )
    lines.append("")
    lines.append(
        f"Library-core subset (`pdfbox` + `fontbox` + `xmpbox` + `io`) coverage: **{core_mapped} / {core_total} = {(100.0 * core_mapped / core_total if core_total else 0.0):.1f}%**."
    )
    lines.append("")
    lines.append("## Traceability status for mapped upstream source rows")
    lines.append("")
    lines.append(
        f"Among **{status_counts.get('total_rows', 0)}** rows with scoped upstream `source_path`:"
    )
    for k in ("in-sync", "partially-in-sync", "partial"):
        lines.append(f"- `{k}`: **{status_counts.get(k, 0)}**")
    lines.append("")
    lines.append("## 100% parity gate")
    lines.append("")
    lines.append(
        "- `mapped == total` and `missing == 0` for the scoped upstream Java inventory."
    )
    lines.append(
        "- No `partial` or `partially-in-sync` rows remain for scoped upstream `source_path` entries."
    )
    lines.append("- Build and tests are green on the parity branch.")
    lines.append("")
    lines.append("## File-by-file report")
    lines.append("")
    lines.append(
        "The generated `reports/upstream-file-comparison.json` contains one row for each scoped upstream Java file, including mapping evidence and metadata-gap classification."
    )
    lines.append("")
    lines.append("| Gap category | Files |")
    lines.append("|---|---:|")
    for key in ("missing-port", "missing-provenance-marker", "missing-traceability-row", "none"):
        lines.append(f"| `{key}` | {file_gap_counts.get(key, 0)} |")
    lines.append("")
    lines.append("| Gap category | Module | Files |")
    lines.append("|---|---|---:|")
    for row in file_gap_module_rows:
        lines.append(f"| `{row['gap_category']}` | `{row['module']}` | {row['files']} |")
    lines.append("")
    return "\n".join(lines) + "\n"


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate canonical PDFBox parity inventory and gap-analysis reports."
    )
    parser.add_argument(
        "--repo-root",
        default=".",
        help="Repository root path (default: current directory).",
    )
    parser.add_argument(
        "--write-sync-state",
        action="store_true",
        help="Update reports/upstream-sync-state.json with canonical scan counters.",
    )
    parser.add_argument(
        "--upstream-root",
        help="Use a local Apache PDFBox checkout instead of the GitHub API.",
    )
    parser.add_argument(
        "--upstream-ref",
        help=(
            "Apache PDFBox branch, tag, or commit to scan. Defaults to "
            "PDFBOX_UPSTREAM_REF, then reports/upstream-sync-state.json."
        ),
    )
    parser.add_argument(
        "--exclude-module",
        action="append",
        default=[],
        help=(
            "Exclude an upstream top-level module from coverage. May be passed "
            "multiple times or as a comma-separated list."
        ),
    )
    parser.add_argument(
        "--update-tracked-commit",
        action="store_true",
        help="When writing sync state, advance tracked_commit to the scanned upstream head.",
    )
    args = parser.parse_args()

    repo_root = Path(args.repo_root).resolve()
    os.chdir(repo_root)

    sync_state = read_json(SYNC_STATE_PATH)
    traceability_rows = read_json(TRACEABILITY_PATH)
    token = os.environ.get("GITHUB_TOKEN", "")

    upstream_repo = sync_state["upstream_repository"]
    configured_exclusions = sync_state.get("excluded_upstream_modules", [])
    if not isinstance(configured_exclusions, list):
        configured_exclusions = []
    env_exclusions = os.environ.get("PDFBOX_EXCLUDED_UPSTREAM_MODULES", "")
    excluded_modules = parse_excluded_modules(
        [
            *[str(value) for value in configured_exclusions],
            env_exclusions,
            *args.exclude_module,
        ]
    )
    upstream_branch = (
        args.upstream_ref
        or os.environ.get("PDFBOX_UPSTREAM_REF")
        or sync_state["upstream_branch"]
    )
    tracked_commit = sync_state["tracked_commit"]

    print("Checking upstream Java repository state...")
    print(f"- Upstream repository: {upstream_repo}")
    print(f"- Upstream ref: {upstream_branch}")
    if excluded_modules:
        print(f"- Excluded upstream modules: {', '.join(excluded_modules)}")
    print(f"- Tracked parity baseline commit: {tracked_commit}")
    if args.upstream_root:
        upstream_root = Path(args.upstream_root).resolve()
        print(f"Scanning local upstream checkout: {upstream_root}")
        if args.upstream_ref or os.environ.get("PDFBOX_UPSTREAM_REF"):
            upstream_head = local_git_ref(upstream_root, upstream_branch)
            upstream_paths = discover_local_git_ref_paths(upstream_root, upstream_branch)
        else:
            upstream_head = local_git_ref(upstream_root, "HEAD")
            upstream_paths = discover_local_upstream_paths(upstream_root)
        print(f"- Latest upstream head commit: {upstream_head}")
        print(f"- Upstream Java files in scope: {len(upstream_paths)}")
    else:
        print("Resolving latest upstream head commit...")
        api_ref = github_ref_name(upstream_branch)
        upstream_head = github_json(
            f"https://api.github.com/repos/{upstream_repo}/commits/{api_ref}",
            token,
        )["sha"]
        print(f"- Latest upstream head commit: {upstream_head}")
        print("Fetching recursive upstream repository tree...")
        tree = github_json(
            f"https://api.github.com/repos/{upstream_repo}/git/trees/{api_ref}?recursive=1",
            token,
        )["tree"]
        upstream_paths = discover_upstream_paths(tree)
        print(f"- Upstream tree entries fetched: {len(tree)}")
    excluded_module_counts = Counter(p.module for p in upstream_paths if p.module in set(excluded_modules))
    excluded_module_count_payload = {
        module: excluded_module_counts.get(module, 0)
        for module in excluded_modules
    }
    if excluded_modules:
        upstream_paths = [p for p in upstream_paths if p.module not in set(excluded_modules)]
        print(f"- Upstream Java files after exclusions: {len(upstream_paths)}")
    upstream_set = {p.path for p in upstream_paths}

    if args.update_tracked_commit:
        tracked_commit = upstream_head

    print(f"- Tracked baseline matches head: {tracked_commit == upstream_head}")

    provenance_targets = collect_provenance_paths(Path("src"))
    mapped_by_provenance = set(provenance_targets)
    mapped_by_traceability = collect_traceability_paths(traceability_rows, upstream_set)
    traceability_targets = collect_traceability_targets(traceability_rows, upstream_set)
    mapped_union = mapped_by_provenance | mapped_by_traceability
    mapped_union_in_scope = mapped_union & upstream_set
    mapped_by_provenance_in_scope = mapped_by_provenance & upstream_set
    mapped_by_traceability_in_scope = mapped_by_traceability & upstream_set
    print("Computing mapped vs missing upstream Java source paths...")
    print(f"- Mapped by provenance (in scope): {len(mapped_by_provenance_in_scope)}")
    print(f"- Mapped by traceability (in scope): {len(mapped_by_traceability_in_scope)}")
    print(f"- Total mapped (union, in scope): {len(mapped_union_in_scope)}")

    missing = sorted(upstream_set - mapped_union_in_scope)
    generated_at = utc_now_iso()
    missing_hash = hashlib.sha256("\n".join(missing).encode("utf-8")).hexdigest()

    module_totals: dict[str, Counter] = defaultdict(Counter)
    family_totals: dict[tuple[str, str], Counter] = defaultdict(Counter)
    for item in upstream_paths:
        mapped = item.path in mapped_union_in_scope
        module_totals[item.module]["java_files"] += 1
        module_totals[item.module]["mapped"] += int(mapped)
        family_totals[(item.module, item.family)]["java_files"] += 1
        family_totals[(item.module, item.family)]["mapped"] += int(mapped)

    modules = []
    for module, c in sorted(module_totals.items()):
        java_files = c["java_files"]
        mapped = c["mapped"]
        missing_count = java_files - mapped
        modules.append(
            {
                "module": module,
                "java_files": java_files,
                "mapped": mapped,
                "missing": missing_count,
                "pct": round((100.0 * mapped / java_files) if java_files else 0.0, 1),
            }
        )

    families = []
    for (module, family), c in sorted(family_totals.items()):
        java_files = c["java_files"]
        mapped = c["mapped"]
        missing_count = java_files - mapped
        families.append(
            {
                "family": f"{module}:{family}",
                "java_files": java_files,
                "mapped": mapped,
                "missing": missing_count,
                "pct": round((100.0 * mapped / java_files) if java_files else 0.0, 1),
            }
        )

    traceability_status_counter = Counter()
    scoped_rows = 0
    for row in traceability_rows:
        source_path = row.get("source_path")
        if not isinstance(source_path, str) or source_path not in upstream_set:
            continue
        scoped_rows += 1
        traceability_status_counter[row.get("status", "(unset)")] += 1

    totals = {
        "total": len(upstream_paths),
        "mapped": len(mapped_union_in_scope),
        "missing": len(missing),
        "pct": round((100.0 * len(mapped_union_in_scope) / len(upstream_paths)) if upstream_paths else 0.0, 1),
    }

    module_by_name = {m["module"]: m for m in modules}
    core_total = sum(module_by_name.get(m, {}).get("java_files", 0) for m in ("pdfbox", "fontbox", "xmpbox", "io"))
    core_mapped = sum(module_by_name.get(m, {}).get("mapped", 0) for m in ("pdfbox", "fontbox", "xmpbox", "io"))

    all_coverage = {
        "generated_at_utc": generated_at,
        "upstream_repository": upstream_repo,
        "upstream_branch": upstream_branch,
        "excluded_upstream_modules": excluded_modules,
        "excluded_upstream_module_counts": excluded_module_count_payload,
        "upstream_head": upstream_head,
        "tracked_parity_baseline_commit": tracked_commit,
        "canonical_mapping_method": "mapped = provenance(PDFBOX_SOURCE_PATH) UNION traceability(source_path)",
        "total": totals["total"],
        "mapped": totals["mapped"],
        "missing": totals["missing"],
        "modules": modules,
        "families": families,
        "traceability_status_counts": {
            "total_rows": scoped_rows,
            "in-sync": traceability_status_counter.get("in-sync", 0),
            "partially-in-sync": traceability_status_counter.get("partially-in-sync", 0),
            "partial": traceability_status_counter.get("partial", 0),
        },
        "top_missing_modules": {
            m["module"]: [p.path for p in upstream_paths if p.module == m["module"] and p.path in set(missing)][:20]
            for m in sorted(modules, key=lambda x: (-x["missing"], x["module"]))
            if m["missing"] > 0
        },
    }

    file_comparison_rows = []
    for item in upstream_paths:
        provenance_target_paths = sorted(provenance_targets.get(item.path, []))
        traceability_target_paths = sorted(traceability_targets.get(item.path, []))
        has_provenance = bool(provenance_target_paths)
        has_traceability = bool(traceability_target_paths)
        mapped = item.path in mapped_union_in_scope
        if not mapped:
            gap_category = "missing-port"
        elif not has_provenance:
            gap_category = "missing-provenance-marker"
        elif not has_traceability:
            gap_category = "missing-traceability-row"
        else:
            gap_category = "none"
        file_comparison_rows.append(
            {
                "source_path": item.path,
                "module": item.module,
                "family": item.family,
                "mapped": mapped,
                "mapping_sources": [
                    source
                    for source, present in (
                        ("provenance", has_provenance),
                        ("traceability", has_traceability),
                    )
                    if present
                ],
                "provenance_target_paths": provenance_target_paths,
                "traceability_target_paths": traceability_target_paths,
                "target_paths": sorted(set(provenance_target_paths) | set(traceability_target_paths)),
                "gap_category": gap_category,
            }
        )

    file_gap_counts = Counter(row["gap_category"] for row in file_comparison_rows)
    file_gap_module_counts = Counter(
        (row["gap_category"], row["module"])
        for row in file_comparison_rows
        if row["gap_category"] != "none"
    )
    file_gap_module_rows = [
        {"gap_category": gap_category, "module": module, "files": files}
        for (gap_category, module), files in sorted(file_gap_module_counts.items())
    ]
    file_comparison = {
        "generated_at_utc": generated_at,
        "upstream_repository": upstream_repo,
        "upstream_branch": upstream_branch,
        "excluded_upstream_modules": excluded_modules,
        "excluded_upstream_module_counts": excluded_module_count_payload,
        "upstream_head": upstream_head,
        "tracked_parity_baseline_commit": tracked_commit,
        "canonical_mapping_method": all_coverage["canonical_mapping_method"],
        "total": totals["total"],
        "mapped": totals["mapped"],
        "missing": totals["missing"],
        "gap_category_counts": dict(sorted(file_gap_counts.items())),
        "rows": file_comparison_rows,
    }

    previous_coverage = read_json(COVERAGE_STATE_PATH) if COVERAGE_STATE_PATH.exists() else {}

    coverage_state = {
        "upstream_repository": upstream_repo,
        "upstream_branch": upstream_branch,
        "excluded_upstream_modules": excluded_modules,
        "excluded_upstream_module_counts": excluded_module_count_payload,
        "upstream_head_commit": upstream_head,
        "tracked_parity_baseline_commit": tracked_commit,
        "last_scan_utc": generated_at,
        "upstream_java_files_total": totals["total"],
        "mapped_java_files_total": totals["mapped"],
        "missing_java_files_total": totals["missing"],
        "mapped_java_files_provenance_total": len(mapped_by_provenance_in_scope),
        "mapped_java_files_traceability_total": len(mapped_by_traceability_in_scope),
        "canonical_mapping_method": all_coverage["canonical_mapping_method"],
        "missing_source_paths_sha256": missing_hash,
        "missing_source_paths_sample": missing[:500],
        "missing_source_paths_sample_truncated": len(missing) > 500,
        "target_100_percent_parity": {
            "mapped_equals_total": totals["mapped"] == totals["total"],
            "missing_equals_zero": totals["missing"] == 0,
            "traceability_non_insync_rows": traceability_status_counter.get("partial", 0)
            + traceability_status_counter.get("partially-in-sync", 0),
            "definition": "100% parity requires mapped==total, missing==0, and no scoped traceability rows with status partial/partially-in-sync.",
        },
    }

    write_json(COVERAGE_STATE_PATH, coverage_state)
    write_json(ALL_COVERAGE_PATH, all_coverage)
    write_json(FILE_COMPARISON_PATH, file_comparison)

    GAP_ANALYSIS_PATH.write_text(
        build_gap_analysis_markdown(
            generated_at=generated_at,
            tracked_commit=tracked_commit,
            upstream_head=upstream_head,
            module_rows=modules,
            totals=totals,
            core_total=core_total,
            core_mapped=core_mapped,
            status_counts=all_coverage["traceability_status_counts"],
            file_gap_counts=file_comparison["gap_category_counts"],
            file_gap_module_rows=file_gap_module_rows,
            upstream_ref=upstream_branch,
            excluded_modules=excluded_modules,
            excluded_module_counts=excluded_module_count_payload,
        ),
        encoding="utf-8",
    )

    if args.write_sync_state:
        sync_state["upstream_branch"] = upstream_branch
        if excluded_modules:
            sync_state["excluded_upstream_modules"] = excluded_modules
            sync_state["excluded_upstream_module_counts"] = excluded_module_count_payload
        elif "excluded_upstream_modules" in sync_state:
            sync_state.pop("excluded_upstream_modules", None)
            sync_state.pop("excluded_upstream_module_counts", None)
        if args.update_tracked_commit:
            sync_state["tracked_commit"] = upstream_head
            sync_state["tracked_commit_updated_utc"] = generated_at
        sync_state["latest_upstream_commit_seen"] = upstream_head
        sync_state["last_port_coverage_scan_utc"] = generated_at
        sync_state["upstream_java_files_total"] = totals["total"]
        sync_state["ported_java_files_total"] = totals["mapped"]
        sync_state["missing_java_files_total"] = totals["missing"]
        sync_state["missing_source_paths_sha256"] = missing_hash
        sync_state["scan_mapping_method"] = all_coverage["canonical_mapping_method"]
        sync_state["parity_target"] = {
            "mapped_java_files_total": totals["total"],
            "missing_java_files_total": 0,
            "traceability_status_required": "in-sync only",
        }
        write_json(SYNC_STATE_PATH, sync_state)

    changed = (
        previous_coverage.get("missing_source_paths_sha256") != coverage_state["missing_source_paths_sha256"]
        or previous_coverage.get("upstream_head_commit") != coverage_state["upstream_head_commit"]
        or previous_coverage.get("mapped_java_files_total") != coverage_state["mapped_java_files_total"]
        or previous_coverage.get("canonical_mapping_method") != coverage_state["canonical_mapping_method"]
    )
    github_output = os.environ.get("GITHUB_OUTPUT")
    if github_output:
        with open(github_output, "a", encoding="utf-8") as f:
            f.write(f"changed={'true' if changed else 'false'}\n")

    print(f"Generated canonical parity inventory at {generated_at}")
    print(f"Mapped: {totals['mapped']} / {totals['total']} | Missing: {totals['missing']}")
    print(f"State changed: {changed}")


if __name__ == "__main__":
    main()
