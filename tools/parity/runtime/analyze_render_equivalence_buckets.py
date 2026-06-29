#!/usr/bin/env python3
"""Summarize reviewed render equivalence buckets from runtime parity artifacts."""

from __future__ import annotations

import argparse
import json
from collections import Counter, defaultdict
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


@dataclass(frozen=True)
class BucketInfo:
    category: str
    owner: str
    summary: str
    decision: str
    follow_up: str


BUCKETS: dict[str, BucketInfo] = {
    "render-visual-equivalence-match": BucketInfo(
        "render-visual-equivalence-match",
        "backend antialiasing / PageDrawer",
        "Same dimensions and bounded pixel drift under the generic visual-equivalence thresholds.",
        "Accepted backend raster adaptation; lower the ceiling to the current corpus count.",
        "No implementation issue unless a fixture shows semantic drift beyond antialiasing.",
    ),
    "render-foreground-shape-equivalence-match": BucketInfo(
        "render-foreground-shape-equivalence-match",
        "PageDrawer geometry, clipping, and backend rasterization",
        "Foreground masks overlap after dilation, but raster/color drift is larger than the generic visual threshold.",
        "Accepted shape-equivalence bucket with real geometry/clipping slices split out.",
        "#562",
    ),
    "render-image-mask-shape-equivalence-match": BucketInfo(
        "render-image-mask-shape-equivalence-match",
        "sampled images, stencil masks, and image masks",
        "Fixture-scoped image/mask rows preserve foreground shape while sampled-image raster details differ.",
        "Real renderer gap; reduce fixture by fixture.",
        "#559",
    ),
    "render-jbig2-decoder-raster-equivalence-match": BucketInfo(
        "render-jbig2-decoder-raster-equivalence-match",
        "JBIG2 decoder and bitonal image rasterization",
        "JBIG2 decoded foreground shape is preserved while decoder/backend raster pixels differ.",
        "Reviewed JBIG2 decoder raster bucket; keep separate from broader sampled-image and mask rows.",
        "#559",
    ),
    "render-pattern-transparency-raster-equivalence-match": BucketInfo(
        "render-pattern-transparency-raster-equivalence-match",
        "patterns, transparency groups, and form XObjects",
        "Fixture-scoped pattern/transparency rows remain visually bounded but not pixel-identical.",
        "Real renderer gap; reduce fixture by fixture.",
        "#560",
    ),
    "render-global-resource-text-raster-equivalence-match": BucketInfo(
        "render-global-resource-text-raster-equivalence-match",
        "LiveCycle global-resource text rendering",
        "GlobalResource merge fixtures preserve text content and foreground shape while text raster density differs.",
        "Reviewed fixture-family text raster bucket; keep separate from true pattern/transparency rows.",
        "#560",
    ),
    "render-form-widget-bbox-clipping-equivalence-match": BucketInfo(
        "render-form-widget-bbox-clipping-equivalence-match",
        "forms and widget appearance BBox clipping",
        "Widget appearance BBox-edge strokes and clipping preserve semantics while right/bottom edge pixels differ.",
        "Reviewed fixture-scoped form/widget clipping bucket; keep separate from broad text placement/raster rows.",
        "#558",
    ),
    "render-form-widget-raster-equivalence-match": BucketInfo(
        "render-form-widget-raster-equivalence-match",
        "forms and widget appearance rendering",
        "Widget appearance rows preserve semantics while text placement/raster details differ.",
        "Real form/widget renderer gap; reduce fixture by fixture.",
        "#558",
    ),
    "render-glyph-layout-equivalence-match": BucketInfo(
        "render-glyph-layout-equivalence-match",
        "fonts, glyph layout, and fallback rendering",
        "Glyph probe rows match identity with bounded glyph geometry, but rendered pixels still differ.",
        "Real glyph/font render fidelity gap; reduce fixture by fixture.",
        "#561",
    ),
    "render-sparse-equivalence-match": BucketInfo(
        "render-sparse-equivalence-match",
        "backend antialiasing on sparse content",
        "Sparse non-near-blank pages amplify tiny raster differences.",
        "Accepted backend raster adaptation; lower the ceiling to the current corpus count.",
        "No implementation issue unless the sparse fixture shows semantic drift.",
    ),
    "render-low-ink-equivalence-match": BucketInfo(
        "render-low-ink-equivalence-match",
        "backend antialiasing on near-blank content",
        "Near-blank pages are sensitive to tiny antialiasing differences.",
        "No current rows; lower the ceiling to zero.",
        "None.",
    ),
    "render-near-blank-threshold-equivalence-match": BucketInfo(
        "render-near-blank-threshold-equivalence-match",
        "backend near-blank threshold variance",
        "One runtime may cross the near-blank metric boundary while the raster drift remains sparse and bounded.",
        "No current rows; lower the ceiling to zero.",
        "None.",
    ),
    "render-low-mean-raster-drift-equivalence-match": BucketInfo(
        "render-low-mean-raster-drift-equivalence-match",
        "backend antialiasing / color-management drift",
        "Low average channel error with bounded large/moderate pixel differences.",
        "No current rows; lower the ceiling to zero.",
        "None.",
    ),
    "render-lossy-jpeg-decoder-equivalence-match": BucketInfo(
        "render-lossy-jpeg-decoder-equivalence-match",
        "JPEG decoder and color conversion",
        "JPEG decoding is lossy and backend-dependent while remaining visually bounded.",
        "Accepted decoder adaptation; keep the current ceiling.",
        "No implementation issue from #541.",
    ),
    "render-java-optional-jpx-reader-missing-match": BucketInfo(
        "render-java-optional-jpx-reader-missing-match",
        "optional Java JPEG 2000 provider",
        "Java renders blank because its optional JPX reader is unavailable while .NET renders visible pixels.",
        "Accepted optional-runtime difference for the fixture-scoped JPX policy.",
        "No default CI provider; see #542.",
    ),
}


def is_render_equivalence_category(category: str) -> bool:
    return category in BUCKETS


def metric(row: dict, runtime: str, name: str) -> object:
    runtime_payload = row.get(runtime)
    if not isinstance(runtime_payload, dict):
        return ""
    metrics = runtime_payload.get("metrics")
    if not isinstance(metrics, dict):
        return ""
    return metrics.get(name, "")


def diff_metric(row: dict, name: str) -> object:
    render_diff = row.get("renderDiff")
    if not isinstance(render_diff, dict):
        return ""
    return render_diff.get(name, "")


def likely_source_area(row: dict) -> str:
    category = str(row.get("category", ""))
    file_name = Path(str(row.get("file", ""))).name.lower()
    if category in {
        "render-form-widget-bbox-clipping-equivalence-match",
        "render-form-widget-raster-equivalence-match",
    }:
        return "forms"
    if category == "render-glyph-layout-equivalence-match":
        return "fonts/glyphs"
    if category in {
        "render-image-mask-shape-equivalence-match",
        "render-jbig2-decoder-raster-equivalence-match",
        "render-lossy-jpeg-decoder-equivalence-match",
    }:
        return "sampled images/masks"
    if category == "render-global-resource-text-raster-equivalence-match":
        return "fonts/glyphs"
    if category == "render-pattern-transparency-raster-equivalence-match":
        return "PageDrawer patterns/transparency"
    if category == "render-java-optional-jpx-reader-missing-match":
        return "optional runtime provider"
    if any(token in file_name for token in ("acroform", "field", "comb", "widget")):
        return "forms"
    if any(token in file_name for token in ("rot", "overlay", "crop", "landscape", "source")):
        return "geometry/clipping"
    if any(token in file_name for token in ("font", "glyph", "ligature", "cweb", "arxiv", "alignment", "control")):
        return "fonts/glyphs"
    if any(token in file_name for token in ("image", "jpeg", "jpg", "png", "jbig", "ccitt", "tiff", "raw")):
        return "sampled images/masks"
    if any(token in file_name for token in ("transparency", "globalresource", "survey", "tiger", "custom-render")):
        return "PageDrawer patterns/transparency"
    return "backend antialiasing"


def load_rows(out_dir: Path) -> tuple[dict, list[dict]]:
    comparison_path = out_dir / "comparison.json"
    payload = json.loads(comparison_path.read_text(encoding="utf-8"))
    rows = [
        row
        for row in payload.get("rows", [])
        if isinstance(row, dict)
        and row.get("op") == "render"
        and is_render_equivalence_category(str(row.get("category", "")))
    ]
    return payload, rows


def count_rows(rows: Iterable[dict]) -> dict[str, int]:
    counts = Counter(str(row.get("category", "")) for row in rows)
    return {category: counts.get(category, 0) for category in BUCKETS}


def source_area_counts(rows: Iterable[dict]) -> dict[str, dict[str, int]]:
    counts: dict[str, Counter] = defaultdict(Counter)
    for row in rows:
        counts[str(row.get("category", ""))][likely_source_area(row)] += 1
    return {category: dict(counter) for category, counter in sorted(counts.items())}


def baseline_counts(path: Path | None) -> dict[str, int]:
    if path is None:
        return {}
    payload = json.loads(path.read_text(encoding="utf-8"))
    categories = payload.get("maxCategories", {})
    if not isinstance(categories, dict):
        return {}
    return {category: int(categories.get(category, 0)) for category in BUCKETS}


def row_payload(row: dict) -> dict:
    return {
        "file": row.get("file"),
        "corpusCategory": row.get("corpusCategory"),
        "category": row.get("category"),
        "sourceArea": likely_source_area(row),
        "javaNonBg": metric(row, "java", "nonBg"),
        "dotnetNonBg": metric(row, "dotnet", "nonBg"),
        "javaNearBlank": metric(row, "java", "nearBlank"),
        "dotnetNearBlank": metric(row, "dotnet", "nearBlank"),
        "mean": diff_metric(row, "mean"),
        "rms": diff_metric(row, "rms"),
        "moderateDiffRatio": diff_metric(row, "moderateDiffRatio"),
        "largeDiffRatio": diff_metric(row, "largeDiffRatio"),
        "artifacts": row.get("artifacts", {}),
    }


def json_payload(
    out_dir: Path,
    comparison_payload: dict,
    rows: list[dict],
    baseline: dict[str, int],
    source_label: str | None,
    issue: str,
) -> dict:
    counts = count_rows(rows)
    return {
        "schema": 1,
        "source": {
            "issue": issue,
            "label": source_label,
            "outDir": out_dir.as_posix(),
            "comparisonGeneratedAtUtc": comparison_payload.get("summary", {}).get("generatedAtUtc"),
            "manifest": comparison_payload.get("summary", {}).get("manifest"),
        },
        "summary": {
            "counts": counts,
            "sourceAreaCounts": source_area_counts(rows),
            "ratchetLowering": {
                category: {
                    "previousCeiling": baseline.get(category, 0),
                    "currentCount": counts.get(category, 0),
                    "recommendedCeiling": counts.get(category, 0),
                }
                for category in BUCKETS
                if baseline.get(category, 0) != counts.get(category, 0)
            },
        },
        "buckets": [
            {
                "category": category,
                "owner": info.owner,
                "summary": info.summary,
                "decision": info.decision,
                "followUp": info.follow_up,
                "count": counts.get(category, 0),
                "previousCeiling": baseline.get(category, 0),
                "examples": [row_payload(row) for row in rows if row.get("category") == category][:8],
            }
            for category, info in BUCKETS.items()
        ],
    }


def format_number(value: object) -> str:
    if isinstance(value, float):
        return f"{value:.6g}"
    return str(value)


def markdown_report(payload: dict) -> str:
    source = payload["source"]
    summary = payload["summary"]
    lines = [
        "# Render Equivalence Bucket Review",
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
            "## Ratchet Lowering",
            "",
            "| Category | Previous ceiling | Current count | New ceiling |",
            "|---|---:|---:|---:|",
        ]
    )
    lowering = summary["ratchetLowering"]
    if lowering:
        for category, item in lowering.items():
            lines.append(
                f"| `{category}` | {item['previousCeiling']} | {item['currentCount']} | {item['recommendedCeiling']} |"
            )
    else:
        lines.append("| None |  |  |  |")

    lines.extend(
        [
            "",
            "## Bucket Summary",
            "",
            "| Category | Count | Owner / source area | Decision | Follow-up |",
            "|---|---:|---|---|---|",
        ]
    )
    for bucket in payload["buckets"]:
        lines.append(
            f"| `{bucket['category']}` | {bucket['count']} | {bucket['owner']} | {bucket['decision']} | {bucket['followUp']} |"
        )

    lines.extend(["", "## Source Area Grouping", "", "| Category | Source area | Rows |", "|---|---|---:|"])
    for category, counts in summary["sourceAreaCounts"].items():
        for source_area, count in sorted(counts.items(), key=lambda item: (-item[1], item[0])):
            lines.append(f"| `{category}` | {source_area} | {count} |")

    lines.extend(["", "## Artifact Evidence", ""])
    for bucket in payload["buckets"]:
        lines.append(f"### `{bucket['category']}`")
        lines.append("")
        lines.append(f"Owner/root cause: {bucket['summary']}")
        lines.append("")
        examples = bucket["examples"]
        if not examples:
            lines.append("No current rows.")
            lines.append("")
            continue
        lines.append("| File | Source area | Mean | RMS | Moderate | Large | Artifacts |")
        lines.append("|---|---|---:|---:|---:|---:|---|")
        for row in examples:
            artifacts = row.get("artifacts") if isinstance(row.get("artifacts"), dict) else {}
            java_artifact = artifacts.get("java", "")
            dotnet_artifact = artifacts.get("dotnet", "")
            artifact_text = f"`{java_artifact}` / `{dotnet_artifact}`"
            lines.append(
                "| `{file}` | {source} | {mean} | {rms} | {moderate} | {large} | {artifacts} |".format(
                    file=row.get("file", ""),
                    source=row.get("sourceArea", ""),
                    mean=format_number(row.get("mean", "")),
                    rms=format_number(row.get("rms", "")),
                    moderate=format_number(row.get("moderateDiffRatio", "")),
                    large=format_number(row.get("largeDiffRatio", "")),
                    artifacts=artifact_text,
                )
            )
        if len(examples) == 8:
            lines.append("| ... | Additional rows omitted from Markdown; see JSON report. | | | | | |")
        lines.append("")

    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Analyze render equivalence buckets from runtime parity artifacts.")
    parser.add_argument("--out-dir", required=True, type=Path, help="Runtime parity output directory containing comparison.json.")
    parser.add_argument("--baseline", type=Path, help="Ratchet baseline to compare against.")
    parser.add_argument("--report", required=True, type=Path, help="Markdown report path to write.")
    parser.add_argument("--json", dest="json_path", type=Path, help="Optional machine-readable JSON report path.")
    parser.add_argument("--source-label", help="Human-readable source label to include in the report, such as a CI run or PR number.")
    parser.add_argument("--issue", default="#541", help="Issue label to include in the Markdown and JSON reports.")
    args = parser.parse_args()

    comparison_payload, rows = load_rows(args.out_dir.resolve())
    payload = json_payload(Path(args.out_dir), comparison_payload, rows, baseline_counts(args.baseline), args.source_label, args.issue)

    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(markdown_report(payload), encoding="utf-8")
    if args.json_path is not None:
        args.json_path.parent.mkdir(parents=True, exist_ok=True)
        args.json_path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")

    print(json.dumps(payload["summary"], indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
