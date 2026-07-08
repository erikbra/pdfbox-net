#!/usr/bin/env python3
from __future__ import annotations

import argparse
import html.parser
import json
import os
import re
import shlex
import subprocess
import sys
import unicodedata
from collections import Counter
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any
from urllib.parse import unquote, urlparse


ROOT = Path(__file__).resolve().parents[2]
TOKEN_RE = re.compile(r"\w+|[^\w\s]", re.UNICODE)
CSS_URL_RE = re.compile(r"url\(\s*(['\"]?)(?P<url>[^)'\"\s]+)\1\s*\)", re.IGNORECASE)
LOCAL_REFERENCE_ATTRIBUTES = {"href", "src", "poster", "data"}
LOCAL_REFERENCE_SCHEMES = {"", "file"}
STATUS_ORDER = ("passed", "known", "failed")


@dataclass(frozen=True)
class CommandFailure:
    exit_code: int
    stdout: str
    stderr: str


class HtmlTextExtractor(html.parser.HTMLParser):
    def __init__(self) -> None:
        super().__init__(convert_charrefs=True)
        self._skip_depth = 0
        self.parts: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if tag.lower() in {"script", "style"}:
            self._skip_depth += 1

    def handle_endtag(self, tag: str) -> None:
        if tag.lower() in {"script", "style"} and self._skip_depth:
            self._skip_depth -= 1

    def handle_data(self, data: str) -> None:
        if not self._skip_depth and data.strip():
            self.parts.append(data)

    def text(self) -> str:
        return " ".join(self.parts)


class HtmlReferenceExtractor(html.parser.HTMLParser):
    def __init__(self) -> None:
        super().__init__(convert_charrefs=True)
        self.references: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        attr_map = {name.lower(): value for name, value in attrs if value}
        for name, value in attr_map.items():
            if name in LOCAL_REFERENCE_ATTRIBUTES:
                self.references.append(value)
            elif name == "srcset":
                self.references.extend(parse_srcset(value))
            elif name == "style":
                self.references.extend(parse_css_urls(value))


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat(timespec="seconds").replace("+00:00", "Z")


def load_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as handle:
        data = json.load(handle)
    if not isinstance(data, dict):
        raise ValueError(f"{path} must contain a JSON object")
    return data


def write_json(path: Path, data: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(data, handle, indent=2, sort_keys=True)
        handle.write("\n")


def write_text(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8", newline="\n")


def normalize_text(value: str) -> str:
    value = unicodedata.normalize("NFKC", value)
    return " ".join(value.split())


def token_counts(value: str) -> Counter[str]:
    normalized = normalize_text(value).casefold()
    return Counter(TOKEN_RE.findall(normalized))


def text_coverage(expected: str, actual: str) -> float:
    expected_counts = token_counts(expected)
    if not expected_counts:
        return 1.0

    actual_counts = token_counts(actual)
    matched = sum(min(count, actual_counts[token]) for token, count in expected_counts.items())
    return matched / sum(expected_counts.values())


def extract_html_text(path: Path) -> str:
    extractor = HtmlTextExtractor()
    extractor.feed(path.read_text(encoding="utf-8", errors="replace"))
    extractor.close()
    return extractor.text()


def extract_markdown_text(path: Path) -> str:
    text = path.read_text(encoding="utf-8", errors="replace")
    text = re.sub(r"```.*?```", " ", text, flags=re.DOTALL)
    text = re.sub(r"!\[([^\]]*)\]\([^)]+\)", r"\1", text)
    text = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", text)
    text = re.sub(r"^[#>*+\-\d.\s]+", " ", text, flags=re.MULTILINE)
    return re.sub(r"[_`~|]", " ", text)


def extract_output_text(path: Path, target: str) -> str:
    suffix = path.suffix.casefold()
    if target == "html" or suffix in {".html", ".htm"}:
        return extract_html_text(path)
    if target == "markdown" or suffix in {".md", ".markdown"}:
        return extract_markdown_text(path)
    return path.read_text(encoding="utf-8", errors="replace")


def parse_srcset(value: str) -> list[str]:
    references: list[str] = []
    for candidate in value.split(","):
        candidate = candidate.strip()
        if not candidate:
            continue
        references.append(candidate.split()[0])
    return references


def parse_css_urls(value: str) -> list[str]:
    return [match.group("url") for match in CSS_URL_RE.finditer(value)]


def is_local_reference(reference: str) -> bool:
    parsed = urlparse(reference)
    if parsed.scheme.casefold() not in LOCAL_REFERENCE_SCHEMES:
        return False
    if parsed.netloc:
        return False
    return bool(parsed.path) and not parsed.path.startswith("#")


def resolve_reference(reference: str, *, base_file: Path, results_dir: Path) -> Path:
    parsed = urlparse(reference)
    path = unquote(parsed.path)
    if Path(path).is_absolute():
        return results_dir / path.lstrip("/")
    return base_file.parent / path


def collect_html_references(path: Path, results_dir: Path) -> list[tuple[str, Path]]:
    parser = HtmlReferenceExtractor()
    parser.feed(path.read_text(encoding="utf-8", errors="replace"))
    parser.close()

    references = [(reference, path) for reference in parser.references]
    for reference in parser.references:
        if not is_local_reference(reference):
            continue
        css_path = resolve_reference(reference, base_file=path, results_dir=results_dir)
        if css_path.suffix.casefold() == ".css" and css_path.exists():
            references.extend(
                (css_reference, css_path)
                for css_reference in parse_css_urls(css_path.read_text(encoding="utf-8", errors="replace"))
            )

    return references


def find_broken_local_references(path: Path, results_dir: Path) -> list[dict[str, str]]:
    broken: list[dict[str, str]] = []
    for reference, source_file in collect_html_references(path, results_dir):
        if not is_local_reference(reference):
            continue
        resolved = resolve_reference(reference, base_file=source_file, results_dir=results_dir)
        if not resolved.exists():
            broken.append(
                {
                    "reference": reference,
                    "resolvedPath": str(resolved),
                    "source": str(source_file),
                }
            )
    return broken


def load_diagnostics(path: Path | None) -> list[Any]:
    if path is None or not path.exists():
        return []
    data = json.loads(path.read_text(encoding="utf-8"))
    if isinstance(data, list):
        return data
    if isinstance(data, dict) and isinstance(data.get("diagnostics"), list):
        return data["diagnostics"]
    raise ValueError(f"{path} must contain a diagnostics array or an object with a diagnostics array")


def primary_output_key(fixture: dict[str, Any]) -> str | None:
    outputs = fixture.get("outputs", {})
    if not isinstance(outputs, dict) or not outputs:
        return None

    target = fixture.get("target")
    preferred = [target, "html", "markdown", "text"]
    for key in preferred:
        if key and key in outputs:
            return key
    return next(iter(outputs))


def output_path(fixture: dict[str, Any], key: str, results_dir: Path) -> Path | None:
    outputs = fixture.get("outputs", {})
    value = outputs.get(key) if isinstance(outputs, dict) else None
    if not isinstance(value, str):
        return None
    return results_dir / value


def required_substring_file(
    entry: str | dict[str, Any],
    fixture: dict[str, Any],
    results_dir: Path,
) -> tuple[Path | None, str]:
    if isinstance(entry, str):
        key = primary_output_key(fixture)
        return (output_path(fixture, key, results_dir) if key else None, entry)

    text = entry.get("text") or entry.get("value")
    if not isinstance(text, str):
        raise ValueError(f"requiredSubstrings entry for fixture {fixture.get('id')} is missing text")

    if isinstance(entry.get("output"), str):
        return output_path(fixture, entry["output"], results_dir), text
    if isinstance(entry.get("path"), str):
        return results_dir / entry["path"], text

    key = primary_output_key(fixture)
    return (output_path(fixture, key, results_dir) if key else None, text)


def load_known_divergences(path: Path | None) -> dict[tuple[str, str], dict[str, Any]]:
    if path is None:
        return {}
    data = load_json(path)
    divergences = data.get("divergences", [])
    if not isinstance(divergences, list):
        raise ValueError(f"{path} divergences must be an array")

    known: dict[tuple[str, str], dict[str, Any]] = {}
    for entry in divergences:
        if not isinstance(entry, dict):
            raise ValueError(f"{path} divergence entries must be objects")
        fixture = entry.get("fixture")
        category = entry.get("category")
        if not isinstance(fixture, str) or not isinstance(category, str):
            raise ValueError(f"{path} divergence entries need fixture and category")
        known[(fixture, category)] = entry
    return known


def evaluate_fixture(
    fixture: dict[str, Any],
    *,
    results_dir: Path,
    known: dict[tuple[str, str], dict[str, Any]],
    command_failure: CommandFailure | None = None,
) -> dict[str, Any]:
    fixture_id = fixture["id"]
    target = fixture.get("target", "unknown")
    expectations = fixture.get("expectations", {})
    failures: list[dict[str, Any]] = []
    metrics: dict[str, Any] = {
        "textCoverage": None,
        "brokenLocalReferences": 0,
        "diagnostics": 0,
        "missingRequiredFiles": 0,
    }

    if command_failure is not None:
        failures.append(
            {
                "category": "crash",
                "message": f"Converter command exited with code {command_failure.exit_code}",
                "exitCode": command_failure.exit_code,
                "stdout": command_failure.stdout[-4000:],
                "stderr": command_failure.stderr[-4000:],
            }
        )

    outputs = fixture.get("outputs", {})
    if isinstance(outputs, dict) and command_failure is None:
        for key, relative_path in outputs.items():
            if key == "diagnostics" or not isinstance(relative_path, str):
                continue
            if not (results_dir / relative_path).exists():
                failures.append(
                    {
                        "category": "crash",
                        "message": f"Expected output '{key}' was not produced",
                        "path": relative_path,
                    }
                )

    required_files = expectations.get("requiredFiles", [])
    if not isinstance(required_files, list):
        raise ValueError(f"Fixture {fixture_id} requiredFiles must be an array")
    for relative_path in required_files:
        if not isinstance(relative_path, str):
            raise ValueError(f"Fixture {fixture_id} requiredFiles entries must be strings")
        if not (results_dir / relative_path).exists():
            metrics["missingRequiredFiles"] += 1
            failures.append(
                {
                    "category": "required-files",
                    "message": "Required file is missing",
                    "path": relative_path,
                }
            )

    primary_key = primary_output_key(fixture)
    primary_path = output_path(fixture, primary_key, results_dir) if primary_key else None
    if primary_path is not None and primary_path.exists():
        expected_text = fixture.get("expectedText", "")
        if not isinstance(expected_text, str):
            raise ValueError(f"Fixture {fixture_id} expectedText must be a string")

        actual_text = extract_output_text(primary_path, str(target))
        coverage = text_coverage(expected_text, actual_text)
        metrics["textCoverage"] = round(coverage, 6)

        min_text_coverage = float(expectations.get("minTextCoverage", 0.0))
        if coverage + 1e-12 < min_text_coverage:
            failures.append(
                {
                    "category": "text-coverage",
                    "message": f"Text coverage {coverage:.3f} is below required {min_text_coverage:.3f}",
                    "expected": min_text_coverage,
                    "actual": round(coverage, 6),
                }
            )

        if str(target) == "html" or primary_path.suffix.casefold() in {".html", ".htm"}:
            broken = find_broken_local_references(primary_path, results_dir)
            metrics["brokenLocalReferences"] = len(broken)
            max_broken = int(expectations.get("maxBrokenLocalReferences", 0))
            if broken and len(broken) > max_broken:
                failures.append(
                    {
                        "category": "broken-assets",
                        "message": f"{len(broken)} local HTML references are broken",
                        "expected": max_broken,
                        "actual": len(broken),
                        "references": broken,
                    }
                )

        required_substrings = expectations.get("requiredSubstrings", [])
        if not isinstance(required_substrings, list):
            raise ValueError(f"Fixture {fixture_id} requiredSubstrings must be an array")
        for entry in required_substrings:
            path, text = required_substring_file(entry, fixture, results_dir)
            if path is None or not path.exists():
                failures.append(
                    {
                        "category": "required-substrings",
                        "message": "Required substring file is missing",
                        "text": text,
                    }
                )
                continue
            if text not in path.read_text(encoding="utf-8", errors="replace"):
                failures.append(
                    {
                        "category": "required-substrings",
                        "message": "Required substring was not found",
                        "path": str(path),
                        "text": text,
                    }
                )

    diagnostics_path = output_path(fixture, "diagnostics", results_dir)
    diagnostics = load_diagnostics(diagnostics_path)
    metrics["diagnostics"] = len(diagnostics)
    max_diagnostics = int(expectations.get("maxDiagnostics", 0))
    if len(diagnostics) > max_diagnostics:
        failures.append(
            {
                "category": "diagnostics",
                "message": f"{len(diagnostics)} diagnostics exceed allowed {max_diagnostics}",
                "expected": max_diagnostics,
                "actual": len(diagnostics),
            }
        )

    for failure in failures:
        known_entry = known.get((fixture_id, failure["category"]))
        if known_entry:
            failure["known"] = {
                "id": known_entry.get("id"),
                "issue": known_entry.get("issue"),
                "reason": known_entry.get("reason"),
            }

    if not failures:
        status = "passed"
    elif all("known" in failure for failure in failures):
        status = "known"
    else:
        status = "failed"

    return {
        "id": fixture_id,
        "title": fixture.get("title", fixture_id),
        "target": target,
        "categories": fixture.get("categories", []),
        "status": status,
        "metrics": metrics,
        "failures": failures,
    }


def run_converter_commands(
    manifest: dict[str, Any],
    *,
    manifest_path: Path,
    results_dir: Path,
    command: str,
) -> dict[str, CommandFailure]:
    results_dir.mkdir(parents=True, exist_ok=True)
    failures: dict[str, CommandFailure] = {}
    for fixture in manifest.get("fixtures", []):
        fixture_id = fixture["id"]
        source = fixture.get("source", "")
        source_path = manifest_path.parent / source if isinstance(source, str) and source else Path("")
        env = os.environ.copy()
        env.update(
            {
                "PDFBOX_CONVERSION_FIXTURE_ID": fixture_id,
                "PDFBOX_CONVERSION_SOURCE": str(source_path),
                "PDFBOX_CONVERSION_RESULT_DIR": str(results_dir),
                "PDFBOX_CONVERSION_TARGET": str(fixture.get("target", "")),
            }
        )
        formatted = command.format(
            fixture_id=shlex.quote(fixture_id),
            source=shlex.quote(str(source_path)),
            result_dir=shlex.quote(str(results_dir)),
            target=shlex.quote(str(fixture.get("target", ""))),
        )
        completed = subprocess.run(
            formatted,
            cwd=ROOT,
            env=env,
            shell=True,
            encoding="utf-8",
            text=True,
            capture_output=True,
            check=False,
        )
        if completed.returncode != 0:
            failures[fixture_id] = CommandFailure(
                exit_code=completed.returncode,
                stdout=completed.stdout,
                stderr=completed.stderr,
            )
    return failures


def summarize_results(results: list[dict[str, Any]]) -> dict[str, Any]:
    status_counts = {status: 0 for status in STATUS_ORDER}
    category_counts: Counter[str] = Counter()
    coverage_values: list[float] = []

    for result in results:
        status_counts[result["status"]] += 1
        coverage = result["metrics"].get("textCoverage")
        if isinstance(coverage, (int, float)):
            coverage_values.append(float(coverage))
        for failure in result["failures"]:
            category_counts[failure["category"]] += 1

    metrics = {
        "minimumTextCoverage": round(min(coverage_values), 6) if coverage_values else None,
        "averageTextCoverage": round(sum(coverage_values) / len(coverage_values), 6) if coverage_values else None,
    }

    return {
        "fixtures": len(results),
        "status": status_counts,
        "categories": dict(sorted(category_counts.items())),
        "metrics": metrics,
    }


def check_ratchet(summary: dict[str, Any], baseline: dict[str, Any] | None) -> dict[str, Any]:
    if baseline is None:
        return {"passed": True, "failures": []}

    failures: list[str] = []
    for status, maximum in baseline.get("maxStatus", {}).items():
        actual = int(summary["status"].get(status, 0))
        if actual > int(maximum):
            failures.append(f"status {status} count {actual} exceeds ratchet maximum {maximum}")

    for category, maximum in baseline.get("maxCategories", {}).items():
        actual = int(summary["categories"].get(category, 0))
        if actual > int(maximum):
            failures.append(f"category {category} count {actual} exceeds ratchet maximum {maximum}")

    for metric, minimum in baseline.get("minMetrics", {}).items():
        actual = summary["metrics"].get(metric)
        if actual is None:
            failures.append(f"metric {metric} is missing")
        elif float(actual) + 1e-12 < float(minimum):
            failures.append(f"metric {metric} value {actual} is below ratchet minimum {minimum}")

    return {"passed": not failures, "failures": failures}


def evaluate_manifest(
    manifest: dict[str, Any],
    *,
    manifest_path: Path,
    results_dir: Path,
    known_divergences: dict[tuple[str, str], dict[str, Any]],
    ratchet_baseline: dict[str, Any] | None,
    command_failures: dict[str, CommandFailure] | None = None,
) -> dict[str, Any]:
    fixtures = manifest.get("fixtures", [])
    if not isinstance(fixtures, list):
        raise ValueError(f"{manifest_path} fixtures must be an array")

    results = [
        evaluate_fixture(
            fixture,
            results_dir=results_dir,
            known=known_divergences,
            command_failure=(command_failures or {}).get(fixture["id"]),
        )
        for fixture in fixtures
    ]
    summary = summarize_results(results)
    ratchet = check_ratchet(summary, ratchet_baseline)

    return {
        "schema": 1,
        "generatedAtUtc": utc_now(),
        "manifest": str(manifest_path),
        "resultsDir": str(results_dir),
        "summary": summary,
        "ratchet": ratchet,
        "fixtures": results,
    }


def render_summary_markdown(comparison: dict[str, Any]) -> str:
    summary = comparison["summary"]
    lines = [
        "# Conversion Quality Summary",
        "",
        f"- Fixtures: {summary['fixtures']}",
        f"- Passed: {summary['status'].get('passed', 0)}",
        f"- Known: {summary['status'].get('known', 0)}",
        f"- Failed: {summary['status'].get('failed', 0)}",
        f"- Minimum text coverage: {summary['metrics'].get('minimumTextCoverage')}",
        f"- Average text coverage: {summary['metrics'].get('averageTextCoverage')}",
        f"- Ratchet: {'passed' if comparison['ratchet']['passed'] else 'failed'}",
        "",
    ]

    if comparison["ratchet"]["failures"]:
        lines.extend(["## Ratchet Failures", ""])
        lines.extend(f"- {failure}" for failure in comparison["ratchet"]["failures"])
        lines.append("")

    lines.extend(
        [
            "## Fixtures",
            "",
            "| Fixture | Target | Status | Text Coverage | Broken References | Diagnostics | Failure Categories |",
            "| --- | --- | --- | ---: | ---: | ---: | --- |",
        ]
    )
    for result in comparison["fixtures"]:
        categories = ", ".join(failure["category"] for failure in result["failures"]) or "-"
        coverage = result["metrics"].get("textCoverage")
        lines.append(
            "| {id} | {target} | {status} | {coverage} | {broken} | {diagnostics} | {categories} |".format(
                id=result["id"],
                target=result["target"],
                status=result["status"],
                coverage="-" if coverage is None else coverage,
                broken=result["metrics"].get("brokenLocalReferences", 0),
                diagnostics=result["metrics"].get("diagnostics", 0),
                categories=categories,
            )
        )

    lines.append("")
    return "\n".join(lines)


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Evaluate PDF conversion output quality against a fixture manifest.")
    parser.add_argument("--manifest", type=Path, required=True, help="JSON fixture manifest.")
    parser.add_argument("--results-dir", type=Path, required=True, help="Directory containing converter outputs.")
    parser.add_argument("--out-dir", type=Path, required=True, help="Directory for comparison.json and summary.md.")
    parser.add_argument("--known-divergences", type=Path, help="Reviewed known-divergence ledger.")
    parser.add_argument("--ratchet-baseline", type=Path, help="Ratchet baseline JSON.")
    parser.add_argument(
        "--converter-command",
        help=(
            "Optional shell command to run once per fixture before evaluation. "
            "Supports {fixture_id}, {source}, {result_dir}, and {target} placeholders."
        ),
    )
    parser.add_argument("--fail-on-unexpected", action="store_true", help="Return non-zero when any fixture fails.")
    parser.add_argument("--fail-on-regression", action="store_true", help="Return non-zero when ratchet checks fail.")
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv or sys.argv[1:])
    manifest = load_json(args.manifest)
    known = load_known_divergences(args.known_divergences)
    baseline = load_json(args.ratchet_baseline) if args.ratchet_baseline else None

    command_failures: dict[str, CommandFailure] = {}
    if args.converter_command:
        command_failures = run_converter_commands(
            manifest,
            manifest_path=args.manifest,
            results_dir=args.results_dir,
            command=args.converter_command,
        )

    comparison = evaluate_manifest(
        manifest,
        manifest_path=args.manifest,
        results_dir=args.results_dir,
        known_divergences=known,
        ratchet_baseline=baseline,
        command_failures=command_failures,
    )

    write_json(args.out_dir / "comparison.json", comparison)
    write_text(args.out_dir / "summary.md", render_summary_markdown(comparison))

    if args.fail_on_unexpected and comparison["summary"]["status"].get("failed", 0) > 0:
        return 1
    if args.fail_on_regression and not comparison["ratchet"]["passed"]:
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
