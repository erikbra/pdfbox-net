#!/usr/bin/env python3
"""Verify and report reviewed text semantic parity fixtures."""

from __future__ import annotations

import argparse
import difflib
import json
from collections import Counter
from dataclasses import dataclass
from pathlib import Path


TEXT_SEMANTIC_PREFIX = "text-semantic-"
TEXT_SEMANTIC_SUFFIX = "-match"


@dataclass(frozen=True)
class Fixture:
    file: str
    category: str
    issue: int
    decision: str
    root_cause: str
    java_snippet: str
    dotnet_snippet: str

    @property
    def key(self) -> tuple[str, str]:
        return (self.file, self.category)


@dataclass(frozen=True)
class TextSemanticRow:
    file: str
    category: str
    status: str
    java_text: str
    dotnet_text: str
    java_artifact: str
    dotnet_artifact: str
    fixture: Fixture | None
    java_snippet_found: bool
    dotnet_snippet_found: bool

    @property
    def reviewed(self) -> bool:
        return self.fixture is not None and self.java_snippet_found and self.dotnet_snippet_found


def is_text_semantic_category(category: str) -> bool:
    return category.startswith(TEXT_SEMANTIC_PREFIX) and category.endswith(TEXT_SEMANTIC_SUFFIX)


def load_fixtures(path: Path) -> dict[tuple[str, str], Fixture]:
    payload = json.loads(path.read_text(encoding="utf-8"))
    fixtures: dict[tuple[str, str], Fixture] = {}
    for item in payload.get("fixtures", []):
        fixture = Fixture(
            file=str(item["file"]),
            category=str(item["category"]),
            issue=int(item["issue"]),
            decision=str(item["decision"]),
            root_cause=str(item["rootCause"]),
            java_snippet=str(item["javaSnippet"]),
            dotnet_snippet=str(item["dotnetSnippet"]),
        )
        if fixture.key in fixtures:
            raise ValueError(f"duplicate fixture for {fixture.file} {fixture.category}")
        fixtures[fixture.key] = fixture
    return fixtures


def artifact_path(out_dir: Path, row: dict, runtime: str) -> tuple[Path, str] | None:
    artifacts = row.get("artifacts")
    if not isinstance(artifacts, dict):
        return None
    value = artifacts.get(runtime)
    if not isinstance(value, str):
        return None
    return out_dir / value, value


def read_text_artifact(out_dir: Path, row: dict, runtime: str) -> tuple[str, str]:
    resolved = artifact_path(out_dir, row, runtime)
    if resolved is None:
        raise ValueError(f"missing {runtime} artifact for {row.get('file')}")
    path, relative = resolved
    return path.read_text(encoding="utf-8"), relative


def load_rows(out_dir: Path, fixtures: dict[tuple[str, str], Fixture]) -> tuple[dict, list[TextSemanticRow]]:
    comparison_path = out_dir / "comparison.json"
    payload = json.loads(comparison_path.read_text(encoding="utf-8"))
    rows: list[TextSemanticRow] = []
    for row in payload.get("rows", []):
        if not isinstance(row, dict) or row.get("op") != "text":
            continue
        category = str(row.get("category", ""))
        if not is_text_semantic_category(category):
            continue

        file = str(row.get("file", ""))
        fixture = fixtures.get((file, category))
        java_text, java_artifact = read_text_artifact(out_dir, row, "java")
        dotnet_text, dotnet_artifact = read_text_artifact(out_dir, row, "dotnet")
        rows.append(
            TextSemanticRow(
                file=file,
                category=category,
                status=str(row.get("status", "")),
                java_text=java_text,
                dotnet_text=dotnet_text,
                java_artifact=java_artifact,
                dotnet_artifact=dotnet_artifact,
                fixture=fixture,
                java_snippet_found=fixture is not None and fixture.java_snippet in java_text,
                dotnet_snippet_found=fixture is not None and fixture.dotnet_snippet in dotnet_text,
            )
        )
    return payload, rows


def first_diff(java_text: str, dotnet_text: str, context: int = 2) -> list[str]:
    diff = list(
        difflib.unified_diff(
            java_text.splitlines(),
            dotnet_text.splitlines(),
            fromfile="java",
            tofile="dotnet",
            lineterm="",
            n=context,
        )
    )
    if not diff:
        return []

    selected: list[str] = []
    in_first_hunk = False
    for line in diff:
        if line.startswith("---") or line.startswith("+++"):
            selected.append(line)
            continue
        if line.startswith("@@"):
            if in_first_hunk:
                break
            in_first_hunk = True
            selected.append(line)
            continue
        if in_first_hunk:
            selected.append(line)
    return selected


def summary_payload(out_dir: Path, comparison_payload: dict, rows: list[TextSemanticRow], source_label: str | None) -> dict:
    categories = Counter(row.category for row in rows)
    return {
        "schema": 1,
        "source": {
            "label": source_label,
            "outDir": out_dir.as_posix(),
            "comparisonGeneratedAtUtc": comparison_payload.get("summary", {}).get("generatedAtUtc"),
            "manifest": comparison_payload.get("summary", {}).get("manifest"),
        },
        "summary": {
            "totalTextSemanticRows": len(rows),
            "reviewedRows": sum(1 for row in rows if row.reviewed),
            "unreviewedRows": sum(1 for row in rows if not row.reviewed),
            "categories": dict(sorted(categories.items())),
        },
        "rows": [
            {
                "file": row.file,
                "category": row.category,
                "status": row.status,
                "reviewed": row.reviewed,
                "javaSnippetFound": row.java_snippet_found,
                "dotnetSnippetFound": row.dotnet_snippet_found,
                "decision": row.fixture.decision if row.fixture else None,
                "rootCause": row.fixture.root_cause if row.fixture else None,
                "artifacts": {"java": row.java_artifact, "dotnet": row.dotnet_artifact},
            }
            for row in rows
        ],
    }


def markdown_report(payload: dict, rows: list[TextSemanticRow]) -> str:
    source = payload["source"]
    summary = payload["summary"]
    lines = [
        "# Text Semantic Parity Review",
        "",
        "Issue: #540",
        "",
    ]
    if source.get("label"):
        lines.append(f"Source: {source['label']}")
    lines.extend(
        [
            f"Runtime parity output: `{source['outDir']}`",
            f"Runtime comparison generated UTC: `{source.get('comparisonGeneratedAtUtc')}`",
            f"Manifest: `{source.get('manifest')}`",
            f"Fixture ledger: `tools/parity/runtime/text-semantic-fixtures.json`",
            "",
            "## Summary",
            "",
            f"- Text semantic rows: {summary['totalTextSemanticRows']}",
            f"- Reviewed rows: {summary['reviewedRows']}",
            f"- Unreviewed rows: {summary['unreviewedRows']}",
            "",
            "| Category | Rows |",
            "|---|---:|",
        ]
    )
    for category, count in summary["categories"].items():
        lines.append(f"| `{category}` | {count} |")

    lines.extend(
        [
            "",
            "## Ratchet Decision",
            "",
            "Do not lower the two text semantic ratchet ceilings in this PR. The current rows remain accepted semantic equivalence, not exact text matches. The ratchet already fails new text semantic categories because unknown categories default to zero, and it fails additional rows in these categories because both current ceilings are one.",
            "",
            "## Reviewed Fixtures",
            "",
        ]
    )

    for row in rows:
        fixture = row.fixture
        lines.append(f"### `{row.file}`")
        lines.append("")
        lines.append(f"- Category: `{row.category}`")
        lines.append(f"- Reviewed: {'yes' if row.reviewed else 'no'}")
        if fixture is not None:
            lines.append(f"- Decision: {fixture.decision}")
            lines.append(f"- Root cause: {fixture.root_cause}")
        else:
            lines.append("- Decision: unreviewed")
            lines.append("- Root cause: missing from fixture ledger")
        lines.append("")
        lines.append("Java fixture:")
        lines.append("")
        lines.append("```text")
        lines.append(fixture.java_snippet if fixture is not None else "")
        lines.append("```")
        lines.append("")
        lines.append(".NET fixture:")
        lines.append("")
        lines.append("```text")
        lines.append(fixture.dotnet_snippet if fixture is not None else "")
        lines.append("```")
        lines.append("")
        lines.append("First diff:")
        lines.append("")
        lines.append("```diff")
        lines.extend(first_diff(row.java_text, row.dotnet_text))
        lines.append("```")
        lines.append("")

    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Analyze reviewed text semantic parity rows from runtime parity artifacts.")
    parser.add_argument("--out-dir", required=True, type=Path, help="Runtime parity output directory containing comparison.json.")
    parser.add_argument("--fixtures", required=True, type=Path, help="Reviewed text semantic fixture ledger.")
    parser.add_argument("--report", required=True, type=Path, help="Markdown report path to write.")
    parser.add_argument("--json", dest="json_path", type=Path, help="Optional machine-readable JSON report path.")
    parser.add_argument("--source-label", help="Human-readable source label to include in the report, such as a CI run or PR number.")
    parser.add_argument("--fail-on-unreviewed", action="store_true", help="Exit nonzero if any text semantic row lacks a reviewed fixture.")
    args = parser.parse_args()

    fixtures = load_fixtures(args.fixtures)
    comparison_payload, rows = load_rows(args.out_dir.resolve(), fixtures)
    payload = summary_payload(Path(args.out_dir), comparison_payload, rows, args.source_label)

    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(markdown_report(payload, rows), encoding="utf-8")
    if args.json_path is not None:
        args.json_path.parent.mkdir(parents=True, exist_ok=True)
        args.json_path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")

    print(json.dumps(payload["summary"], indent=2))
    if args.fail_on_unreviewed and payload["summary"]["unreviewedRows"]:
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
