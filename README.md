# pdfbox-net

A mechanical port of [Apache PDFBox](https://pdfbox.apache.org/) to modern .NET.

## Status: Apache PDFBox 3.0 Core Parity

This branch targets the Apache PDFBox `origin/3.0` line. It has reached core
3.0 parity for the scoped non-Preflight modules: source coverage is complete,
the generated public/protected API surface has no unreviewed deltas, and the
runtime parity corpus is green against Apache PDFBox 3.0.

Preflight and `preflight-app` are intentionally excluded from this branch's
core parity claim. That means PDF/A validation is not included yet, even though
ordinary PDF load, parse, text, render, save, merge, encryption, FontBox, IO,
and XmpBox behavior are covered by the 3.0 parity reports.

| Upstream module | Java files | Ported C# files | Missing | Coverage |
|---|---:|---:|---:|---:|
| `benchmark` | 4 | 4 | 0 | 100% |
| `debugger` | 91 | 91 | 0 | 100% |
| `examples` | 94 | 94 | 0 | 100% |
| `fontbox` | 143 | 143 | 0 | 100% |
| `io` | 18 | 18 | 0 | 100% |
| `pdfbox` | 621 | 621 | 0 | 100% |
| `tools` | 26 | 26 | 0 | 100% |
| `xmpbox` | 74 | 74 | 0 | 100% |
| **TOTAL** | **1,071** | **1,071** | **0** | **100%** |

3.0 readiness summary:

- Apache upstream ref: `origin/3.0` at
  `ea68b6feae80e671b3d26565b12eccc79e74d967`
- Source coverage: 1071 / 1071 scoped production Java files
- API review: 0 unreviewed public/protected API deltas
- Runtime parity: 1027 matches, 0 known gaps, 0 unexpected gaps
- Package default: `3.0.8-preview`

The remaining known 3.0 work is product-surface work rather than unreviewed
core source/API/runtime parity: full `pdfbox-app` CLI option parity, debugger UI
scope, and deterministic examples coverage for PDF/A and advanced signature
flows. See [`reports/pdfbox-3.0-release-readiness.md`](reports/pdfbox-3.0-release-readiness.md)
for the full gate decision.

## Projects

The solution (`PdfBoxNet.slnx`) contains the core port, optional backend
packages, examples, tests, and development tools:

| Project | Description |
|---|---|
| `PdfBox.Net.IO` | Low-level random-access IO primitives (ported from `io` module) |
| `PdfBox.Net.FontBox` | Font handling — AFM, CFF, CMap, TTF, Type1 (ported from `fontbox` module) |
| `PdfBox.Net.XmpBox` | XMP metadata reading/writing (ported from `xmpbox` module) |
| `PdfBox.Net` | Core PDF library — COS, filters, parser, writer, pdmodel, text, rendering, tools (ported from `pdfbox` module) |
| `PdfBox.Net.Cryptography` | Optional BouncyCastle-backed public-key security provider |
| `PdfBox.Net.SkiaSharp` | Complete SkiaSharp rendering backend |
| `PdfBox.Net.SystemDrawing` | Windows `System.Drawing` helpers and print-spooler backend |
| `PdfBox.Net.ImageSharp` | ImageSharp backend experiment |
| `PdfBox.Net.MauiGraphics` | Microsoft.Maui.Graphics backend experiment |
| `PdfBox.Net.Examples` | Ported Apache PDFBox examples; non-packable by default |
| `PdfBox.Net.Debugger` | Ported debugger inspection models; non-packable by default |
| `PdfBox.Net.Tests` | xUnit v3 tests for all non-XmpBox modules |
| `PdfBox.Net.XmpBox.Tests` | xUnit v3 tests for `PdfBox.Net.XmpBox` |
| `PdfBox.Net.Benchmark` | BenchmarkDotNet benchmarks (ported from `benchmark` module, located under `src/PdfBox.Net.Benchmark/`) |

## Requirements

- .NET 10.0 SDK or later
- Optional backend packages for rendering, cryptography, or platform printing.
  `PdfBox.Net.SkiaSharp` is the complete rendering backend today.

## Optional Backends

Rendering, public-key cryptography, and printer submission are isolated behind
small backend/provider contracts so the mechanical Java port stays portable.

- `PdfBox.Net.SkiaSharp` is the complete rendering backend today.
- `PdfBox.Net.Cryptography` provides BouncyCastle-backed public-key encryption
  support.
- `PdfBox.Net.SystemDrawing` provides Windows `System.Drawing` image helpers and
  a Windows print-spooler backend. Register a complete rendering backend before
  printing real pages.

See [`reports/printing-backend-policy-2026-06-28.md`](reports/printing-backend-policy-2026-06-28.md)
for the printing support matrix and platform limitations.

## Build and test

```sh
dotnet restore PdfBoxNet.slnx
dotnet build PdfBoxNet.slnx --configuration Release
dotnet test PdfBoxNet.slnx --configuration Release
```

CI runs on every push and pull request via `.github/workflows/ci.yml`.

## Running benchmarks

The `PdfBox.Net.Benchmark` project uses [BenchmarkDotNet](https://benchmarkdotnet.org/) and mirrors the Java [JMH](https://github.com/openjdk/jmh) benchmarks from the upstream `benchmark` module.

### 1 — Download the PDF fixtures

Benchmarks operate on large real-world PDF files that live under `target/pdfs/`.
Run the provided download script to fetch them automatically:

```sh
# Linux / macOS
bash scripts/download-benchmark-pdfs.sh

# Windows (PowerShell)
.\scripts\download-benchmark-pdfs.ps1
```

| File | Benchmark class | Auto-download |
|---|---|:---:|
| `target/pdfs/849-42-94772-1-10-20210818.pdf` | `LoadAndSaveBenchmarks` (medium) | ✅ |
| `target/pdfs/506-42-86246-2-10-20190822.pdf` | `LoadAndSaveBenchmarks` (large) | ✅ |
| `target/pdfs/eci_altona-test-suite-v2_technical2_x4.pdf` | `RenderingBenchmarks` (Altona) | ✅ |
| `target/pdfs/PDF32000_2008.pdf` | `RenderingBenchmarks` + `TextExtractionBenchmarks` | ✅ |
| `target/pdfs/Ghent_PDF_Output_Suite_V50_Full/…/Ghent_PDF-Output-Test-V50_CMYK_X4.pdf` | `RenderingBenchmarks` (Ghent) | ✅ Playwright |

The **Ghent PDF Output Suite** requires accepting a license agreement on the GWG website.
`scripts/download-ghent-pdf.mjs` automates this with a headless [Playwright](https://playwright.dev/) browser:

```sh
# Install Playwright once
npm install playwright
npx playwright install chromium

# Download and extract the Ghent suite
node scripts/download-ghent-pdf.mjs
```

The `download-benchmark-pdfs.sh` / `.ps1` scripts call this automatically when `node` is available.

### 2 — Run the benchmarks

To run all benchmarks in Release mode:

```sh
dotnet run --project src/PdfBox.Net.Benchmark --configuration Release
```

To run a specific benchmark class:

```sh
dotnet run --project src/PdfBox.Net.Benchmark --configuration Release -- --filter "*LoadAndSave*"
```

### CI pipeline

The `benchmarks` workflow (`.github/workflows/benchmarks.yml`) can be triggered manually from the **Actions** tab. It installs Node.js and Playwright, downloads all PDF fixtures (including the Ghent suite), builds the project in Release mode, runs all benchmarks, and uploads the BenchmarkDotNet JSON results as a 90-day artifact.

## Provenance and traceability

Every ported file carries a provenance header recording its upstream origin:

```csharp
// PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/.../Foo.java
// PDFBOX_SOURCE_COMMIT: <sha>
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: <sha>
```

Traceability records are maintained in:
- `reports/conversion-records.json` — per-file conversion records
- `reports/normalization-records.json` — compile-normalization records
- `reports/traceability-parity-report.json` — parity status per upstream source path
- `reports/upstream-sync-state.json` — latest upstream commit tracked and coverage counters
- `reports/upstream-port-coverage-state.json` — canonical parity scan snapshot
- `reports/pdfbox-main-gap-analysis.md` — human-readable gap analysis
- `reports/pdfbox-3.0-source-coverage.md` — 3.0 source coverage excluding Preflight
- `reports/pdfbox-3.0-api-review.md` — 3.0 API review and accepted adaptations
- `reports/pdfbox-3.0-runtime-parity.md` — 3.0 runtime parity corpus result
- `reports/pdfbox-3.0-release-readiness.md` — 3.0 release-readiness gate decision

## Upstream sync automation

A daily scheduled workflow (`.github/workflows/upstream-sync-watch.yml`, 07:00 UTC) watches the `apache/pdfbox` `trunk` branch for new commits. When upstream drift is detected it:

1. Creates or updates a single "Upstream PDFBox has new commits to sync" issue.
2. Runs `tools/parity/generate_parity_inventory.py` to regenerate the parity inventory.
3. Refreshes `reports/upstream-sync-state.json`, `reports/upstream-port-coverage-state.json`, `reports/all-upstream-coverage.json`, and `reports/pdfbox-main-gap-analysis.md`.

The workflow can also be triggered manually via `workflow_dispatch`.

## Conversion methodology

Porting follows a set of defined skills documented in [`SKILLS.md`](SKILLS.md):

- **Skill A** — Initial mechanical conversion + provenance stamping
- **Skill B** — Upstream rewrite/update sync
- **Skill C** — Upstream deletion handling
- **Skill D** — Upstream new-file intake
- **Skill E** — Traceability and parity reporting
- **Skill F** — Compile-oriented normalization pass
- **Skill G** — Java → C# API and type mapping reference
- **Skill H** — Automatic PR approval checklist
- **Skill I** — Orchestrating sequential issue delivery

The approach is a **mechanical-first port** that stays close to upstream structure and API to make future re-syncs straightforward. Each converted file keeps the upstream Apache license header verbatim, adds a conversion note, and retains JavaDoc-derived XML documentation comments.

Two-lane strategy for ongoing .NET improvements:

1. **Mechanical lane** — Keep converted files close to upstream, track provenance fields, and re-sync via Skill B.
2. **Adaptation lane** — Place .NET-specific improvements in wrapper/adapter types or isolate them in bounded `PORT-LOCAL` regions so re-sync can preserve them.
