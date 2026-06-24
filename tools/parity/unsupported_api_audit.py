#!/usr/bin/env python3
"""Audit semantic unsupported API paths in the product source tree."""

from __future__ import annotations

import argparse
import fnmatch
import json
import re
import sys
from pathlib import Path


FORBIDDEN_MARKERS = (
    ("NotImplementedException", re.compile(r"\bNotImplementedException\b")),
    ("TODO", re.compile(r"\bTODO\b")),
    ("not implemented", re.compile(r"not\s+implemented", re.IGNORECASE)),
)


def rel(path: Path, root: Path) -> str:
    return path.resolve().relative_to(root.resolve()).as_posix()


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def statement_from(lines: list[str], index: int) -> str:
    parts: list[str] = []
    for line in lines[index:]:
        parts.append(line.strip())
        if ";" in line:
            break
    return " ".join(parts)


def matches_rule(rule: dict, file_name: str, statement: str) -> bool:
    file_value = rule.get("file")
    if file_value is not None and file_value != file_name:
        return False

    glob_value = rule.get("fileGlob")
    if glob_value is not None and not fnmatch.fnmatch(file_name, glob_value):
        return False

    contains = rule.get("statementContains")
    if contains is not None and contains not in statement:
        return False

    return True


def find_forbidden_markers(source_root: Path, repo_root: Path) -> list[str]:
    failures: list[str] = []
    for path in sorted(source_root.rglob("*.cs")):
        text = path.read_text(encoding="utf-8", errors="ignore")
        for name, pattern in FORBIDDEN_MARKERS:
            for match in pattern.finditer(text):
                line_number = text.count("\n", 0, match.start()) + 1
                failures.append(f"{rel(path, repo_root)}:{line_number}: forbidden marker `{name}`")
    return failures


def find_unsupported_statements(source_root: Path, repo_root: Path) -> list[tuple[str, int, str]]:
    statements: list[tuple[str, int, str]] = []
    for path in sorted(source_root.rglob("*.cs")):
        lines = path.read_text(encoding="utf-8", errors="ignore").splitlines()
        file_name = rel(path, repo_root)
        for index, line in enumerate(lines):
            if "new NotSupportedException" in line:
                statements.append((file_name, index + 1, statement_from(lines, index)))
    return statements


def audit(repo_root: Path, classifications_path: Path) -> list[str]:
    source_root = repo_root / "src" / "PdfBox.Net"
    classifications = load_json(classifications_path)
    mechanical = classifications.get("mechanicalUnsupported", [])
    semantic = classifications.get("semanticUnsupported", [])

    failures = find_forbidden_markers(source_root, repo_root)
    matched_semantic_ids: set[str] = set()

    for file_name, line_number, statement in find_unsupported_statements(source_root, repo_root):
        if any(matches_rule(rule, file_name, statement) for rule in mechanical):
            continue

        matches = [rule for rule in semantic if matches_rule(rule, file_name, statement)]
        if not matches:
            failures.append(f"{file_name}:{line_number}: unclassified semantic NotSupportedException: {statement}")
            continue

        for match in matches:
            if "issue" not in match and not match.get("rationale"):
                failures.append(f"{file_name}:{line_number}: classification `{match.get('id')}` lacks issue or rationale")
            matched_semantic_ids.add(match.get("id", ""))

    stale = sorted(
        entry.get("id", "")
        for entry in semantic
        if entry.get("id", "") not in matched_semantic_ids
    )
    for entry_id in stale:
        failures.append(f"classification `{entry_id}` did not match any current NotSupportedException")

    return failures


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo-root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument(
        "--classifications",
        type=Path,
        default=Path(__file__).with_name("unsupported-api-classifications.json"),
    )
    args = parser.parse_args()

    failures = audit(args.repo_root, args.classifications)
    if failures:
        print("Unsupported API audit failed:", file=sys.stderr)
        for failure in failures:
            print(f"- {failure}", file=sys.stderr)
        return 1

    print("Unsupported API audit passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
