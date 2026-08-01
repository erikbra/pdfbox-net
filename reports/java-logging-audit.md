# Java logging migration audit

This report records the exhaustive logging audit for Apache PDFBox commit
`fee11b453d66725c2b3a28b6f862a8dc24d33177`, the Java baseline tracked by this port.
The row-level source of truth is [`java-logging-audit.csv`](java-logging-audit.csv), with
machine-readable totals in [`java-logging-audit.summary.json`](java-logging-audit.summary.json)
and reviewed absent-region explanations in
[`java-logging-absent-regions.json`](java-logging-absent-regions.json). Adapted parser regions
that land beyond their primary provenance target are recorded in
[`java-logging-target-overrides.json`](java-logging-target-overrides.json).

## Scope and method

- Scanned all 1,073 tracked `*/src/main/java/**/*.java` production files, excluding the
  canonical `pdfbox-layout-fop` subtree in accordance with the repository parity policy.
- Detected static `Logger` fields regardless of field name, direct level calls, enabled-level
  guards, logger arguments passed to `IOUtils.closeAndLogException`, logger monitor locks, and
  the differently named logger parameter/call in `IOUtils`.
- Mapped Java files to C# files from provenance headers, conversion records, and the
  traceability report.
- Matched reachable diagnostics to `Microsoft.Extensions.Logging` calls, retaining their
  level, exception object, control flow, and structured named values.
- Classified a Java row as `absent-region` only when the whole containing Java behavior has no
  corresponding target implementation. These decisions are explicit and line-addressable in
  the disposition ledger.

The generator is reproducible with:

```bash
python3 tools/parity/generate_java_logging_audit.py \
  --upstream-root /path/to/apache/pdfbox \
  --upstream-ref fee11b453d66725c2b3a28b6f862a8dc24d33177
```

It exits non-zero if any inventory row is unaccounted.

## Baseline inventory

| Item | Count |
|---|---:|
| Logger fields | 208 |
| Direct logger calls | 842 |
| Enabled-level guards | 8 |
| `IOUtils.closeAndLogException` logger passes | 10 |
| `synchronized(LOG)` monitors | 4 |
| Differently named logger parameter/call rows | 2 |
| Total inventory rows | 1,074 |

All 208 fields use Log4j 2's `Logger` type and the name `LOG`; one file contains two fields in
nested classes. No SLF4J or `java.util.logging.Logger` field occurs in the tracked production
baseline. The only differently named logger use is the `Logger logger` parameter in Java
`IOUtils.closeAndLogException`.

### Direct calls by level

| Level | Count |
|---|---:|
| Trace | 7 |
| Debug | 226 |
| Info | 83 |
| Warn | 313 |
| Error | 213 |

### Direct calls by module and disposition

| Module | Migrated | Absent region | Total |
|---|---:|---:|---:|
| debugger | 7 | 23 | 30 |
| examples | 29 | 58 | 87 |
| fontbox | 121 | 17 | 138 |
| io | 8 | 4 | 12 |
| pdfbox | 298 | 271 | 569 |
| tools | 0 | 6 | 6 |
| **Total** | **463** | **379** | **842** |

## Migration result

| Inventory kind | Migrated | Absent region | Total |
|---|---:|---:|---:|
| Logger fields | 206 | 2 | 208 |
| Direct calls | 463 | 379 | 842 |
| Enabled-level guards | 4 | 4 | 8 |
| Helper logger passes | 7 | 3 | 10 |
| Logger monitor locks | 1 | 3 | 4 |
| Differently named parameter/call rows | 2 | 0 | 2 |
| **All rows** | **683** | **391** | **1,074** |

The generated summary reports zero unaccounted rows. Every
reachable target diagnostic resolves its typed logger dynamically through
`PdfBoxLogging.CreateLogger<T>()`, so an application-installed factory is observed even after
the target type has initialized. Java logger monitors are never translated to locks on the
dynamic logger; a dedicated lock is used where the corresponding target region exists.

Java configuration-only code was also reviewed. The `Rendering` and `TextExtraction` benchmark
classes disable Log4j/JUL output but declare no logger, and the debugger's `DebugLogAppender` is
a Java-specific Log4j appender rather than a logger field/use. They therefore do not create
inventory rows or provider dependencies in the .NET port.

## Verification

Representative tests cover:

- the exact log levels used by `PostScriptTable`;
- factory installation after `PostScriptTable` type initialization;
- a structured `ResourceName` value and attached `IOException` from
  `IOUtils.CloseAndLogException`;
- silent defaults through `NullLoggerFactory`.

The repository remains provider-neutral: no Serilog or other concrete logging provider is
referenced.

Final verification commands:

```bash
dotnet restore PdfBoxNet.slnx
dotnet build PdfBoxNet.slnx --no-restore
dotnet test PdfBoxNet.slnx --no-build --no-restore
python3 tools/parity/generate_java_logging_audit.py \
  --upstream-root /path/to/apache/pdfbox \
  --upstream-ref fee11b453d66725c2b3a28b6f862a8dc24d33177
git diff --check
```

The final solution build completed with zero warnings and zero errors. The final complete test
run passed 1,529 tests with 14 platform/fixture tests skipped and no failures. An earlier test
invocation hit the repository's GC-sensitive COSName weak-cache test; that test passed on its
immediate isolated rerun and the subsequent complete solution run. The audit command exited zero
with all 1,074 rows accounted.
