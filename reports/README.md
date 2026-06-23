# reports/

This directory holds the canonical parity-tracking artefacts for the pdfbox-net port of Apache PDFBox.
All files are generated or maintained by the parity tooling and should be kept in sync after every implementation slice.

---

## File inventory

### `upstream-port-coverage-state.json`
**Purpose:** Compact machine-readable snapshot of the current port-coverage state.
Consumed by automation (e.g. upstream-watch workflows) to evaluate whether the 100 % parity gates are met.

Key fields: upstream head commit, total / mapped / missing Java-file counts, SHA-256 of the missing-paths list (for change detection), and a `target_100_percent_parity` block with boolean gate evaluations.

### `upstream-sync-state.json`
**Purpose:** Tracks upstream drift for automation — records the latest upstream commit seen and when the last port-coverage scan ran, so the upstream-watch pipeline can detect new commits on Apache PDFBox trunk.

Key fields: `latest_upstream_commit_seen`, `tracked_commit_updated_utc`, `last_port_coverage_scan_utc`, and the same total / ported / missing counts as the coverage-state file.

> **Overlap note:** Both `upstream-port-coverage-state.json` and `upstream-sync-state.json` contain the total / mapped / missing file counts and the upstream commit SHA.  
> The distinction is their consumer: the coverage-state file is the authoritative gate-evaluation record; the sync-state file is the lightweight sentinel used by the drift-detection automation.  Both are necessary.

---

### `all-upstream-coverage.json`
**Purpose:** Detailed aggregate coverage breakdown, split by upstream module (e.g. `pdfbox`, `fontbox`, `xmpbox`) and package family (e.g. `fontbox:ttf`, `pdfbox:pdmodel`).  Used when diagnosing *which* area of the codebase still has gaps.

Key fields: per-module and per-family `java_files` / `mapped` / `missing` / `pct` tables, plus traceability-status counts (`in-sync`, `partially-in-sync`, `partial`).

> **Overlap note:** The top-level totals in this file duplicate those in `upstream-port-coverage-state.json`.  The module/family breakdown is unique to this file.

---

### `upstream-file-comparison.json`
**Purpose:** Full file-by-file comparison between the current Apache PDFBox upstream Java inventory and this repository's mapped C# ports.

Key fields: one `rows` entry per scoped upstream Java file, including `source_path`, `module`, `family`, mapping evidence (`provenance` and/or `traceability`), target paths, and `gap_category`.

> **Overlap note:** This file expands the aggregate totals in `all-upstream-coverage.json` into an auditable per-file ledger.  It is the fastest way to identify metadata gaps such as missing provenance markers or missing traceability rows even when global mapping coverage is 100 %.

---

### `pdfbox-main-gap-analysis.md`
**Purpose:** Human-readable summary of the current coverage state — the narrative counterpart to `upstream-port-coverage-state.json` and `all-upstream-coverage.json`.  Intended for quick review in GitHub and PR descriptions.

Content: per-module table (Java files / mapped / missing / %), traceability-status summary, and the 100 % parity gate checklist.

> **Overlap note:** The numbers here are a formatted subset of `all-upstream-coverage.json` and `upstream-port-coverage-state.json`.  The file is redundant in a strict data sense but is kept because Markdown renders well in GitHub and provides at-a-glance status without parsing JSON.

---

### `pdfbox-runtime-gap-analysis.md`
**Purpose:** Human-readable summary of the behavioral/runtime gap scan comparing Apache PDFBox Java and PdfBox.Net on a 104-PDF corpus.

Content: operation pass/fail counts, per-operation timings, paired Java/.NET failure differences, runtime mismatch notes, source-file coverage by filename stem, and explicit stubbed/missing .NET implementation areas.

> **Overlap note:** This complements `pdfbox-main-gap-analysis.md`: that file tracks source mapping and traceability coverage, while this file tracks observed runtime behavior and implementation stubs.

---

### `traceability-parity-report.json`
**Purpose:** Per-file traceability ledger — one record for every C# file in the port.  Links each C# file back to its upstream Java source, records the port mode (`native-test`, `adapted`, `adapted-minimal`, …), the sync status (`in-sync`, `partially-in-sync`, `partial`), and a human-readable note explaining any divergence.

This is the **source of truth for per-file sync status** and is the file checked when evaluating the traceability gate for the parity lock.

Key fields: `source_path`, `target_path`, `source_commit`, `sync_commit`, `port_mode`, `status`, `last_checked_utc`, `note`.

---

### `conversion-records.json`
**Purpose:** Records the *conversion* step for each ported file — what was done when the Java source was first translated to C#.  Captures `conversion_notes` describing the mechanical translation choices made (e.g. idiom mappings, API surface decisions).

Key fields: `source_path`, `target_path`, `port_mode`, `sync_commit`, `conversion_notes`.

> **Overlap note:** `conversion-records.json` and `traceability-parity-report.json` share `source_path`, `target_path`, `port_mode`, and `sync_commit`.  
> They serve different purposes: `conversion-records.json` documents the *how* of the initial translation; `traceability-parity-report.json` tracks the *ongoing sync status*.  Both are needed.

---

### `normalization-records.json`
**Purpose:** Records post-conversion normalisation passes — mechanical clean-ups applied to already-ported C# files (e.g. Java syntax leftovers replaced with idiomatic C#, naming conventions, etc.).

Key fields: `source_path`, `target_path`, `normalization_applied`, `change_categories`, `semantic_risk`, `compiles_after_change`, `note`.

---

### `parity-execution-tracker.md`
**Purpose:** Human-readable project journal for the parity effort.  Documents the 100 % parity target definition, milestone (M1–M6) completion status, the mandatory per-slice execution checklist, and a chronological series of execution snapshots with gate-evaluation tables.

This file is **append-only** during active work: each implementation slice adds a new snapshot section rather than overwriting previous ones, providing a full audit trail of how the parity lock was reached.

---

## Summary: is anything redundant?

| File | Unique content | Overlaps with |
|---|---|---|
| `upstream-port-coverage-state.json` | Gate evaluation booleans, `missing_source_paths_sha256` | `upstream-sync-state.json` (totals), `all-upstream-coverage.json` (totals) |
| `upstream-sync-state.json` | Drift-detection fields (`latest_upstream_commit_seen`, `tracked_commit_updated_utc`) | `upstream-port-coverage-state.json` (totals) |
| `all-upstream-coverage.json` | Per-module / per-family breakdown | `upstream-port-coverage-state.json` (top-level totals), `pdfbox-main-gap-analysis.md` (rendered subset) |
| `upstream-file-comparison.json` | One row per upstream Java file with mapping evidence and gap category | `all-upstream-coverage.json` (totals), `traceability-parity-report.json` (traceability-backed target paths) |
| `pdfbox-main-gap-analysis.md` | Human-readable Markdown rendering | `all-upstream-coverage.json`, `upstream-port-coverage-state.json` |
| `pdfbox-runtime-gap-analysis.md` | Runtime behavior/timing gaps and implementation-stub findings | JSONL artifacts in `/tmp/pdfbox-gap-scan` |
| `traceability-parity-report.json` | Per-file sync status and notes | `conversion-records.json` (key fields) |
| `conversion-records.json` | Per-file conversion notes | `traceability-parity-report.json` (key fields) |
| `normalization-records.json` | Normalisation change categories, semantic risk | — |
| `parity-execution-tracker.md` | Milestone history, execution snapshots | — |

No file is purely redundant: each either has a unique consumer (automation vs. human review), a unique granularity (aggregate vs. per-module vs. per-file), or a unique temporal role (initial conversion vs. ongoing sync vs. drift detection).
