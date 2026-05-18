# pdfbox-net

Porting Apache PDFBox to modern .NET.

## Feasibility summary

Based on Apache PDFBox's current structure (modules like `io`, `fontbox`, `xmpbox`, `pdfbox`, `tools`, `debugger`, and `examples`), this is a **medium-to-large porting effort**.

### 1) Least-effort, working port

Goal: get broad PDFBox feature coverage running on .NET quickly, while staying close to Java architecture/API.

- **Approach**
  - Keep package/module boundaries similar to upstream.
  - Do mostly mechanical translation of Java patterns to C#.
  - Minimize API redesign and prioritize compatibility and passing behavior tests.
- **Expected effort**
  - Roughly **4-8 engineer-months** (for a usable core) and typically **6-12 months calendar time** depending on team size and scope.
- **Pros**
  - Fastest path to "it works".
  - Easier to sync fixes from upstream PDFBox.
- **Cons**
  - API may feel Java-like in C#.
  - More technical debt and later cleanup cost.

### 2) More ".NET-feeling" port

Goal: provide idiomatic .NET developer experience while preserving PDF correctness.

- **Approach**
  - Redesign public APIs for .NET conventions (`Stream`, async where appropriate, `IDisposable`, nullable annotations, idiomatic naming, package split).
  - Keep algorithmic parity with PDFBox but refactor internals where .NET offers better primitives.
  - Build stronger .NET-first test, benchmarking, and compatibility layers.
- **Expected effort**
  - Roughly **10-18 engineer-months** and often **9-18 months calendar time**.
- **Pros**
  - Better long-term maintainability and adoption in .NET ecosystem.
  - Cleaner integration with modern tooling and performance tuning.
- **Cons**
  - Slower initial delivery.
  - Harder to keep one-to-one parity with upstream implementation details.

## Recommended direction

Use a **hybrid phased strategy**:
1. Start with least-effort compatibility for core functionality.
2. Stabilize with tests and sample corpus.
3. Incrementally introduce .NET-first APIs and internal refactors.

This reduces initial risk while still converging on an idiomatic .NET library.

## Design notes from review feedback

### Source traceability for one-to-one conversion

Yes — each converted file should include a small provenance header that references the upstream PDFBox source path (and ideally commit SHA) it came from. For example:

- Upstream path: `pdfbox/src/main/java/org/apache/pdfbox/.../Foo.java`
- Upstream commit: `<sha>`
- Port status: `mechanical` / `adapted`

This makes later upstream sync work much easier and enables tooling to diff .NET files against their Java origin.

### .NET-feeling wrapper on top of mechanical port

Yes — that is a practical and recommended architecture.

- Keep a lower-level compatibility layer close to PDFBox semantics.
- Add a higher-level .NET API wrapper that exposes idiomatic types/patterns.
- Translate exceptions and resource lifetimes at the wrapper boundary.

Main trade-off: some extra indirection/allocation can occur, but this is usually manageable if wrappers are thin and performance-critical paths can still access lower-level primitives directly.

### Proposed conversion "skills" (automation-ready)

Skill definitions are split into focused files in [`SKILLS.md`](SKILLS.md), including usage order and individual skill details for:
- initial conversion + provenance stamping
- upstream rewrite/update sync
- upstream deletion handling
- upstream new-file intake
- traceability/parity reporting

## Proposed project plan

### Phase 0 - Discovery and guardrails (1-2 weeks)
- Inventory PDFBox module dependencies and prioritize feature slices.
- Define target frameworks (e.g., `net8.0` / `netstandard2.1` if needed).
- Set quality gates: parser correctness, rendering checks, text extraction accuracy, memory/perf baselines.

### Phase 1 - Core foundations (4-8 weeks)
- Port low-level IO/COS primitives and font infrastructure first.
- Establish golden-file regression tests from real PDFs.
- Deliver minimal open/load/save pipeline.

### Phase 2 - Functional parity milestones (8-16 weeks)
- Add text extraction, metadata, forms, outlines, encryption/signing support by priority.
- Port CLI tooling equivalents for smoke validation.
- Track parity matrix against upstream PDFBox capabilities.

### Phase 3 - .NET API shaping (6-12 weeks, overlapping)
- Introduce idiomatic wrapper APIs while keeping compatibility layer.
- Apply .NET naming, nullability, disposable patterns, and optional async APIs.
- Add performance-focused refactors for hotspots.

### Phase 4 - Hardening and release (3-6 weeks)
- Cross-platform validation (Windows/Linux/macOS).
- Fuzz/robustness checks on malformed PDFs.
- Publish versioned NuGet packages and migration documentation.

## Immediate next steps

1. Create module-level backlog (`io`, `fontbox`, `xmpbox`, `pdfbox`) with complexity tags.
2. Build a representative PDF test corpus (happy path + malformed/security cases).
3. Implement a minimal vertical slice: open PDF -> read metadata/text -> save PDF.
4. Decide early whether public API parity or .NET idioms take priority for v1.
