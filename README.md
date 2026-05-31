# pdfbox-net

A mechanical port of [Apache PDFBox](https://pdfbox.apache.org/) to modern .NET.

## Status: 100% parity achieved ✅

All **1,067** upstream Java source files from Apache PDFBox have been ported to C#.

| Upstream module | Java files | Ported C# files | Missing | Coverage |
|---|---:|---:|---:|---:|
| `benchmark` | 3 | 3 | 0 | 100% |
| `debugger` | 91 | 91 | 0 | 100% |
| `examples` | 94 | 94 | 0 | 100% |
| `fontbox` | 143 | 143 | 0 | 100% |
| `io` | 18 | 18 | 0 | 100% |
| `pdfbox` | 618 | 618 | 0 | 100% |
| `tools` | 26 | 26 | 0 | 100% |
| `xmpbox` | 74 | 74 | 0 | 100% |
| **TOTAL** | **1,067** | **1,067** | **0** | **100%** |

Parity baseline commit: `eeb5d611e0cea8beac3d7025a4dbccbef51d5caf` (Apache PDFBox `trunk`).
See [`reports/pdfbox-main-gap-analysis.md`](reports/pdfbox-main-gap-analysis.md) for the full gap analysis report.

## Projects

The solution (`PdfBoxNet.slnx`) contains four library projects and two test projects:

| Project | Description |
|---|---|
| `PdfBox.Net.IO` | Low-level random-access IO primitives (ported from `io` module) |
| `PdfBox.Net.FontBox` | Font handling — AFM, CFF, CMap, TTF, Type1 (ported from `fontbox` module) |
| `PdfBox.Net.XmpBox` | XMP metadata reading/writing (ported from `xmpbox` module) |
| `PdfBox.Net` | Core PDF library — COS, filters, parser, writer, pdmodel, text, rendering, tools (ported from `pdfbox` module) |
| `PdfBox.Net.Tests` | xUnit v3 tests for all non-XmpBox modules |
| `PdfBox.Net.XmpBox.Tests` | xUnit v3 tests for `PdfBox.Net.XmpBox` |

## Requirements

- .NET 10.0 SDK or later
- [SkiaSharp](https://github.com/mono/SkiaSharp) (used for rendering, replaces Java AWT/ImageIO)

## Build and test

```sh
dotnet restore PdfBoxNet.slnx
dotnet build PdfBoxNet.slnx --configuration Release
dotnet test PdfBoxNet.slnx --configuration Release
```

CI runs on every push and pull request via `.github/workflows/ci.yml`.

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

## Upstream sync automation

A daily scheduled workflow (`.github/workflows/upstream-sync-watch.yml`, 07:00 UTC) watches the `apache/pdfbox` `trunk` branch for new commits. When upstream drift is detected it:

1. Creates or updates a single "Upstream PDFBox has new commits to sync" issue.
2. Runs `tools/parity/generate_parity_inventory.py` to regenerate the parity inventory.
3. Refreshes `reports/upstream-sync-state.json`, `reports/upstream-port-coverage-state.json`, `reports/.all-upstream-coverage.json`, and `reports/pdfbox-main-gap-analysis.md`.

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
