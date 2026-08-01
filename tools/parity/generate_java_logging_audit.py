#!/usr/bin/env python3
"""Generate the exhaustive Java-to-C# logging migration inventory."""

from __future__ import annotations

import argparse
import csv
import json
import re
import subprocess
from collections import Counter, defaultdict
from dataclasses import dataclass
from difflib import SequenceMatcher
from pathlib import Path
from typing import Iterable


JAVA_LEVELS = ("trace", "debug", "info", "warn", "error")
CSHARP_LEVELS = {
    "trace": "LogTrace",
    "debug": "LogDebug",
    "info": "LogInformation",
    "warn": "LogWarning",
    "error": "LogError",
}
GUARD_LEVELS = {
    "isTraceEnabled": "Trace",
    "isDebugEnabled": "Debug",
    "isWarnEnabled": "Warning",
}
DEFAULT_EXCLUDED_MODULES = {"pdfbox-layout-fop"}


@dataclass(frozen=True)
class Occurrence:
    line: int
    kind: str
    level: str
    text: str


def run(*args: str, cwd: Path | None = None) -> str:
    result = subprocess.run(args, cwd=cwd, check=True, text=True,
                            stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    return result.stdout


def balanced_call(source: str, match: re.Match[str]) -> str:
    index = match.end()
    depth = 1
    quote: str | None = None
    escaped = False
    while index < len(source) and depth:
        char = source[index]
        if quote is not None:
            if escaped:
                escaped = False
            elif char == "\\":
                escaped = True
            elif char == quote:
                quote = None
        else:
            if char in ('"', "'"):
                quote = char
            elif char == "(":
                depth += 1
            elif char == ")":
                depth -= 1
        index += 1
    while index < len(source) and source[index] in " \t":
        index += 1
    if index < len(source) and source[index] == ";":
        index += 1
    return source[match.start():index]


def one_line(text: str) -> str:
    return " ".join(part.strip() for part in text.splitlines() if part.strip())


def line_number(source: str, index: int) -> int:
    return source.count("\n", 0, index) + 1


def java_occurrences(source: str) -> tuple[list[Occurrence], list[Occurrence]]:
    declarations: list[Occurrence] = []
    usages: list[Occurrence] = []

    declaration_pattern = re.compile(
        r"\b(?:private|protected|public)\s+static\s+(?:final\s+)?(?:Logger|Log)\s+"
        r"(?P<name>[A-Za-z_$][A-Za-z0-9_$]*)\b[^;]*;",
        re.DOTALL,
    )
    logger_names: list[str] = []
    for match in declaration_pattern.finditer(source):
        logger_names.append(match.group("name"))
        declarations.append(Occurrence(line_number(source, match.start()), "field", "",
                                       one_line(match.group(0))))

    if not logger_names:
        return declarations, usages

    logger_expression = "(?:" + "|".join(re.escape(name) for name in logger_names) + ")"
    call_pattern = re.compile(rf"\b{logger_expression}\.(trace|debug|info|warn|error)\s*\(")
    for match in call_pattern.finditer(source):
        usages.append(Occurrence(line_number(source, match.start()), "call", match.group(1),
                                 one_line(balanced_call(source, match))))

    guard_pattern = re.compile(
        rf"\b{logger_expression}\.(isTraceEnabled|isDebugEnabled|isWarnEnabled)\s*\(")
    for match in guard_pattern.finditer(source):
        usages.append(Occurrence(line_number(source, match.start()), "guard",
                                 GUARD_LEVELS[match.group(1)].lower(),
                                 one_line(balanced_call(source, match))))

    helper_pattern = re.compile(r"\bIOUtils\.closeAndLogException\s*\(")
    for match in helper_pattern.finditer(source):
        text = balanced_call(source, match)
        if any(re.search(rf"\b{re.escape(name)}\b", text) for name in logger_names):
            usages.append(Occurrence(line_number(source, match.start()), "helper-pass", "",
                                     one_line(text)))

    for match in re.finditer(rf"\bsynchronized\s*\(\s*{logger_expression}\s*\)", source):
        usages.append(Occurrence(line_number(source, match.start()), "lock", "",
                                 one_line(match.group(0))))

    return declarations, sorted(usages, key=lambda row: (row.line, row.kind))


def java_parameter_occurrences(source: str) -> list[Occurrence]:
    rows: list[Occurrence] = []
    for match in re.finditer(r"\b(?:Logger|Log)\s+logger\b", source):
        rows.append(Occurrence(line_number(source, match.start()), "logger-parameter", "",
                               one_line(match.group(0))))
    pattern = re.compile(r"\blogger\.(trace|debug|info|warn|error)\s*\(")
    for match in pattern.finditer(source):
        rows.append(Occurrence(line_number(source, match.start()), "parameter-call",
                               match.group(1), one_line(balanced_call(source, match))))
    return rows


def git_source_paths(upstream_root: Path, upstream_ref: str,
                     excluded_modules: set[str]) -> list[str]:
    paths = run("git", "ls-tree", "-r", "--name-only", upstream_ref,
                cwd=upstream_root).splitlines()
    result = []
    for path in paths:
        parts = path.split("/")
        if not path.endswith(".java") or "/src/main/java/" not in path:
            continue
        if parts[0] in excluded_modules:
            continue
        result.append(path)
    return sorted(result)


def git_source(upstream_root: Path, upstream_ref: str, source_path: str) -> str:
    return run("git", "show", f"{upstream_ref}:{source_path}", cwd=upstream_root)


def add_report_mappings(repo_root: Path, mappings: dict[str, list[str]], report: str) -> None:
    payload = json.loads((repo_root / report).read_text(encoding="utf-8"))
    for row in payload:
        if not isinstance(row, dict):
            continue
        source_path = row.get("source_path")
        target_path = row.get("target_path")
        if isinstance(source_path, str) and isinstance(target_path, str):
            if (repo_root / target_path).is_file() and target_path not in mappings[source_path]:
                mappings[source_path].append(target_path)


def add_logging_overrides(repo_root: Path, mappings: dict[str, list[str]]) -> None:
    payload = json.loads(
        (repo_root / "reports/java-logging-target-overrides.json").read_text(encoding="utf-8"))
    for row in payload["mappings"]:
        source_path = row["source_path"]
        for target_path in row["target_paths"]:
            if (repo_root / target_path).is_file() and target_path not in mappings[source_path]:
                mappings[source_path].append(target_path)


def consolidated_migrations(repo_root: Path) -> dict[tuple[str, int], dict[str, object]]:
    payload = json.loads(
        (repo_root / "reports/java-logging-target-overrides.json").read_text(encoding="utf-8"))
    return {
        (row["source_path"], int(row["source_line"])): row
        for row in payload.get("consolidated_rows", [])
    }


def target_mappings(repo_root: Path) -> dict[str, list[str]]:
    mappings: dict[str, list[str]] = defaultdict(list)
    for path in repo_root.rglob("*.cs"):
        if "bin" in path.parts or "obj" in path.parts:
            continue
        head = path.read_text(encoding="utf-8", errors="replace")[:3000]
        match = re.search(r"PDFBOX_SOURCE_PATH:\s*(\S+)", head)
        if match:
            target = path.relative_to(repo_root).as_posix()
            mappings[match.group(1)].append(target)
    add_report_mappings(repo_root, mappings, "reports/conversion-records.json")
    add_report_mappings(repo_root, mappings, "reports/traceability-parity-report.json")
    add_logging_overrides(repo_root, mappings)
    return mappings


def extract_string_literals(text: str) -> str:
    literals = re.findall(r'(?<!@)"((?:\\.|[^"\\])*)"', text)
    value = " ".join(literals)
    value = re.sub(r"\\[rnt]", " ", value)
    value = re.sub(r"\{[^}]*\}", " ", value)
    return re.sub(r"\s+", " ", value).strip().lower()


def message_score(java_text: str, csharp_text: str) -> float:
    left = extract_string_literals(java_text)
    right = extract_string_literals(csharp_text)
    if not left or not right:
        return 0.0
    left_words = {word for word in re.findall(r"[a-z0-9]+", left) if len(word) > 2}
    right_words = {word for word in re.findall(r"[a-z0-9]+", right) if len(word) > 2}
    union = left_words | right_words
    jaccard = len(left_words & right_words) / len(union) if union else 0.0
    sequence = SequenceMatcher(None, left, right).ratio()
    return max(jaccard, sequence)


def csharp_occurrences(repo_root: Path, targets: Iterable[str]) -> tuple[list[Occurrence], list[Occurrence]]:
    declarations: list[Occurrence] = []
    usages: list[Occurrence] = []
    for target in targets:
        path = repo_root / target
        if not path.is_file():
            continue
        source = path.read_text(encoding="utf-8", errors="replace")
        for match in re.finditer(r"\bILogger(?:<[^;\r\n]+>)?\s+LOG\s*=>", source):
            declarations.append(Occurrence(line_number(source, match.start()), target, "",
                                           one_line(source[match.start():source.find(";", match.end()) + 1])))
        call_pattern = re.compile(r"\bLOG\.(LogTrace|LogDebug|LogInformation|LogWarning|LogError)\s*\(")
        reverse_levels = {value: key for key, value in CSHARP_LEVELS.items()}
        for match in call_pattern.finditer(source):
            usages.append(Occurrence(line_number(source, match.start()), target,
                                     reverse_levels[match.group(1)],
                                     one_line(balanced_call(source, match))))
        guard_pattern = re.compile(
            r"\bLOG\.IsEnabled\s*\(\s*LogLevel\.(Trace|Debug|Warning)\s*\)")
        for match in guard_pattern.finditer(source):
            usages.append(Occurrence(line_number(source, match.start()), target,
                                     match.group(1).lower(), one_line(match.group(0))))
        helper_pattern = re.compile(r"\bIOUtils\.CloseAndLogException\s*\(")
        for match in helper_pattern.finditer(source):
            text = balanced_call(source, match)
            if re.search(r"\bLOG\b", text):
                usages.append(Occurrence(line_number(source, match.start()), target, "helper-pass",
                                         one_line(text)))
        for match in re.finditer(r"\block\s*\(([^)]*)\)", source):
            if "LOG" not in match.group(1):
                usages.append(Occurrence(line_number(source, match.start()), target, "dedicated-lock",
                                         one_line(match.group(0))))
        for match in re.finditer(r"\bILogger\??\s+logger\b", source):
            usages.append(Occurrence(line_number(source, match.start()), target, "logger-parameter",
                                     one_line(match.group(0))))
        parameter_pattern = re.compile(r"\blogger\?*\.Log(Trace|Debug|Information|Warning|Error)\s*\(")
        parameter_levels = {"Trace": "trace", "Debug": "debug", "Information": "info",
                            "Warning": "warn", "Error": "error"}
        for match in parameter_pattern.finditer(source):
            usages.append(Occurrence(line_number(source, match.start()), target,
                                     "parameter-" + parameter_levels[match.group(1)],
                                     one_line(balanced_call(source, match))))
    return declarations, usages


def read_dispositions(path: Path, upstream_ref: str) -> dict[str, dict[str, object]]:
    if not path.exists():
        return {}
    payload = json.loads(path.read_text(encoding="utf-8"))
    if payload.get("upstream_commit") != upstream_ref:
        raise ValueError(
            f"Disposition baseline {payload.get('upstream_commit')!r} does not match "
            f"requested upstream ref {upstream_ref!r}")
    return {row["source_path"]: row for row in payload["files"]}


def match_calls(java: list[Occurrence], csharp: list[Occurrence],
                absent_lines: set[int]) -> dict[int, Occurrence]:
    matches: dict[int, Occurrence] = {}
    available = set(range(len(csharp)))
    candidates: list[tuple[float, int, int]] = []
    for java_index, java_row in enumerate(java):
        if java_row.kind != "call" or java_row.line in absent_lines:
            continue
        for cs_index, cs_row in enumerate(csharp):
            if cs_row.level == java_row.level and ".Log" in cs_row.text:
                candidates.append((message_score(java_row.text, cs_row.text), java_index, cs_index))
    for score, java_index, cs_index in sorted(candidates, reverse=True):
        if score < 0.34 or java_index in matches or cs_index not in available:
            continue
        matches[java_index] = csharp[cs_index]
        available.remove(cs_index)

    # Message templates necessarily change when Java concatenation or Log4j suppliers are
    # translated to MEL named properties. Once the strong text matches are assigned, pair
    # the remaining non-disposed calls by level and source order. A missing C# call still
    # remains unaccounted because there will be no unused occurrence to pair with it.
    for level in JAVA_LEVELS:
        remaining_java = [
            index for index, row in enumerate(java)
            if row.kind == "call" and row.level == level and row.line not in absent_lines
            and index not in matches
        ]
        remaining_csharp = [
            index for index, row in enumerate(csharp)
            if row.level == level and ".Log" in row.text and index in available
        ]
        for java_index, cs_index in zip(remaining_java, remaining_csharp):
            matches[java_index] = csharp[cs_index]
            available.remove(cs_index)
    return matches


def evidence(row: Occurrence | None) -> str:
    if row is None:
        return ""
    return f"{row.kind}:{row.line}: {row.text}"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--upstream-root", type=Path, required=True)
    parser.add_argument("--upstream-ref")
    parser.add_argument("--repo-root", type=Path,
                        default=Path(__file__).resolve().parents[2])
    parser.add_argument("--dispositions", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    upstream_root = args.upstream_root.resolve()
    sync_state = json.loads((repo_root / "reports/upstream-sync-state.json").read_text())
    upstream_ref = args.upstream_ref or sync_state["tracked_commit"]
    excluded_modules = set(sync_state.get("excluded_upstream_modules", []))
    excluded_modules.update(DEFAULT_EXCLUDED_MODULES)
    dispositions_path = args.dispositions or repo_root / "reports/java-logging-absent-regions.json"
    output = args.output or repo_root / "reports/java-logging-audit.csv"
    dispositions = read_dispositions(dispositions_path, upstream_ref)
    mappings = target_mappings(repo_root)
    consolidated = consolidated_migrations(repo_root)

    rows: list[dict[str, object]] = []
    summary = Counter()
    modules = Counter()
    logger_field_names = Counter()
    logger_apis = Counter()
    unaccounted = 0
    used_consolidated: set[tuple[str, int]] = set()
    source_paths = git_source_paths(upstream_root, upstream_ref, excluded_modules)
    for source_path in source_paths:
        source = git_source(upstream_root, upstream_ref, source_path)
        declarations, usages = java_occurrences(source)
        parameters = java_parameter_occurrences(source)
        if not declarations and not parameters:
            continue
        for declaration in declarations:
            match = re.search(
                r"\b(?:Logger|Log)\s+([A-Za-z_$][A-Za-z0-9_$]*)\b", declaration.text)
            if match:
                logger_field_names[match.group(1)] += 1
        if declarations:
            if "org.apache.logging.log4j.Logger" in source:
                logger_apis["Log4j2"] += len(declarations)
            elif "org.apache.commons.logging.Log" in source:
                logger_apis["Apache Commons Logging"] += len(declarations)
            elif "org.slf4j.Logger" in source:
                logger_apis["SLF4J"] += len(declarations)
            elif "java.util.logging.Logger" in source:
                logger_apis["java.util.logging"] += len(declarations)
            else:
                logger_apis["unclassified"] += len(declarations)
        targets = mappings.get(source_path, [])
        cs_declarations, cs_usages = csharp_occurrences(repo_root, targets)
        disposition = dispositions.get(source_path)
        reason = str(disposition.get("reason", "")) if disposition else ""
        raw_absent_lines = list(disposition.get("source_lines", [])) if disposition else []
        absent_lines = set(raw_absent_lines)
        if len(absent_lines) != len(raw_absent_lines):
            raise ValueError(f"Disposition for {source_path} contains duplicate source lines")
        consolidated_lines = {
            line for path, line in consolidated
            if path == source_path
        }
        call_matches = match_calls(usages, cs_usages, absent_lines | consolidated_lines)
        consolidated_notes: dict[int, str] = {}
        for usage_index, usage in enumerate(usages):
            override = consolidated.get((source_path, usage.line))
            if override is None:
                continue
            target_path = str(override["target_path"])
            target_fragment = str(override["target_fragment"])
            if usage.kind != "call":
                raise ValueError(
                    f"Consolidated migration {source_path}:{usage.line} is not a direct call")
            candidates = [row for row in cs_usages
                          if row.kind == target_path and target_fragment in row.text]
            if len(candidates) != 1:
                raise ValueError(
                    f"Consolidated migration {source_path}:{usage.line} expected exactly one "
                    f"target call containing {target_fragment!r}, found {len(candidates)}")
            call_matches[usage_index] = candidates[0]
            consolidated_notes[usage_index] = str(override["reason"])
            used_consolidated.add((source_path, usage.line))
        declaration_absent = int(disposition.get("absent_declarations", 0)) if disposition else 0
        if disposition and not reason:
            raise ValueError(f"Disposition for {source_path} has no reason")
        if disposition and not absent_lines and declaration_absent == 0:
            raise ValueError(f"Disposition for {source_path} does not consume any inventory rows")
        modules[source_path.split("/", 1)[0]] += len([u for u in usages if u.kind == "call"])

        if declaration_absent < 0 or declaration_absent > len(declarations):
            raise ValueError(
                f"Invalid absent_declarations={declaration_absent} for {source_path}; "
                f"the source contains {len(declarations)} logger fields")
        migrated_declarations = min(
            len(cs_declarations), len(declarations) - declaration_absent)
        consumed_absent_declarations = 0
        for index, declaration in enumerate(declarations):
            is_absent = index < declaration_absent
            migrated = not is_absent and index - declaration_absent < migrated_declarations
            status = "migrated" if migrated else ("absent-region" if is_absent and reason else "unaccounted")
            if status == "unaccounted":
                unaccounted += 1
            rows.append({
                "source_path": source_path,
                "source_line": declaration.line,
                "kind": "logger-field",
                "level": "",
                "java_source": declaration.text,
                "csharp_targets": ";".join(targets),
                "status": status,
                "csharp_evidence": evidence(
                    cs_declarations[index - declaration_absent] if migrated else None),
                "notes": "" if migrated else reason,
            })
            summary[f"declaration_{status}"] += 1
            if status == "absent-region":
                consumed_absent_declarations += 1

        if consumed_absent_declarations != declaration_absent:
            raise ValueError(
                f"Disposition for {source_path} requested {declaration_absent} absent "
                f"declarations but consumed {consumed_absent_declarations}")

        helper_cs = [row for row in cs_usages if row.level == "helper-pass"]
        lock_cs = [row for row in cs_usages if row.level == "dedicated-lock"]
        guard_cs = [row for row in cs_usages
                    if row.level in ("trace", "debug", "warning") and
                    "IsEnabled" in row.text]
        helper_index = lock_index = 0
        guard_indices: Counter[str] = Counter()
        consumed_absent_lines: set[int] = set()
        for usage_index, usage in enumerate(usages):
            matched: Occurrence | None = None
            approved_absent = usage.line in absent_lines
            if approved_absent:
                consumed_absent_lines.add(usage.line)
            elif usage.kind == "call":
                matched = call_matches.get(usage_index)
            elif usage.kind == "guard":
                same = [row for row in guard_cs if row.level == usage.level]
                guard_index = guard_indices[usage.level]
                if guard_index < len(same):
                    matched = same[guard_index]
                    guard_indices[usage.level] += 1
            elif usage.kind == "helper-pass" and helper_index < len(helper_cs):
                matched = helper_cs[helper_index]
                helper_index += 1
            elif usage.kind == "lock" and lock_index < len(lock_cs):
                matched = lock_cs[lock_index]
                lock_index += 1

            if matched is not None:
                status = "migrated"
                if usage_index in consolidated_notes:
                    notes = "Consolidated target site: " + consolidated_notes[usage_index]
                elif usage.kind == "lock":
                    notes = "dedicated lock object; dynamic logger is not used as a monitor"
                else:
                    notes = ""
            elif approved_absent:
                status = "absent-region"
                notes = reason
            else:
                status = "unaccounted"
                notes = "No matching C# logging usage and no approved absent-region disposition."
                unaccounted += 1
            rows.append({
                "source_path": source_path,
                "source_line": usage.line,
                "kind": usage.kind,
                "level": usage.level,
                "java_source": usage.text,
                "csharp_targets": ";".join(targets),
                "status": status,
                "csharp_evidence": evidence(matched),
                "notes": notes,
            })
            summary[f"usage_{status}"] += 1
            summary[f"java_{usage.kind}"] += 1
            if usage.kind == "call":
                summary[f"java_level_{usage.level}"] += 1

        for parameter in parameters:
            expected_level = ("logger-parameter" if parameter.kind == "logger-parameter"
                              else "parameter-" + parameter.level)
            matched = next((row for row in cs_usages if row.level == expected_level), None)
            status = "migrated" if matched else "unaccounted"
            if status == "unaccounted":
                unaccounted += 1
            rows.append({
                "source_path": source_path,
                "source_line": parameter.line,
                "kind": parameter.kind,
                "level": parameter.level,
                "java_source": parameter.text,
                "csharp_targets": ";".join(targets),
                "status": status,
                "csharp_evidence": evidence(matched),
                "notes": "",
            })
            summary[f"parameter_{status}"] += 1

        if disposition:
            stale_lines = absent_lines - consumed_absent_lines
            if stale_lines:
                raise ValueError(
                    f"Disposition for {source_path} contains non-logging source lines: "
                    f"{sorted(stale_lines)}")

    inventoried_paths = {str(row["source_path"]) for row in rows}
    stale_paths = set(dispositions) - inventoried_paths
    if stale_paths:
        raise ValueError(
            "Disposition entries do not identify inventoried logger sources: "
            + ", ".join(sorted(stale_paths)))
    stale_consolidated = set(consolidated) - used_consolidated
    if stale_consolidated:
        raise ValueError(
            "Consolidated mappings were not consumed: "
            + ", ".join(f"{path}:{line}" for path, line in sorted(stale_consolidated)))

    output.parent.mkdir(parents=True, exist_ok=True)
    columns = ["source_path", "source_line", "kind", "level", "java_source",
               "csharp_targets", "status", "csharp_evidence", "notes"]
    with output.open("w", encoding="utf-8", newline="") as stream:
        writer = csv.DictWriter(stream, columns, lineterminator="\n")
        writer.writeheader()
        writer.writerows(rows)

    metadata = {
        "upstream_repository": sync_state["upstream_repository"],
        "upstream_commit": upstream_ref,
        "excluded_modules": sorted(excluded_modules),
        "production_java_files_scanned": len(source_paths),
        "inventory_rows": len(rows),
        "logger_fields_by_api": dict(sorted(logger_apis.items())),
        "logger_fields_by_name": dict(sorted(logger_field_names.items())),
        "summary": dict(sorted(summary.items())),
        "direct_calls_by_module": dict(sorted(modules.items())),
        "unaccounted_rows": unaccounted,
    }
    output.with_suffix(".summary.json").write_text(json.dumps(metadata, indent=2) + "\n",
                                                   encoding="utf-8")
    print(json.dumps(metadata, indent=2))
    return 1 if unaccounted else 0


if __name__ == "__main__":
    raise SystemExit(main())
