# Java logging migration audit

This report records the exhaustive logging audit for Apache PDFBox commit
`ddb7e78992bebc36140ba0d864c8212ec5da697b`, the Java baseline tracked by the
`release/3.0` branch. The row-level source of truth is
[`java-logging-audit.csv`](java-logging-audit.csv), with machine-readable totals in
[`java-logging-audit.summary.json`](java-logging-audit.summary.json) and reviewed
absent-region explanations in
[`java-logging-absent-regions.json`](java-logging-absent-regions.json). Adapted regions
that land beyond their primary provenance target are recorded in
[`java-logging-target-overrides.json`](java-logging-target-overrides.json).

## Scope and method

- Scanned all 1,077 tracked `*/src/main/java/**/*.java` production files, excluding
  `pdfbox-layout-fop`, `preflight`, and `preflight-app` in accordance with the release
  parity policy.
- Detected Apache Commons Logging fields, direct level calls, enabled-level guards,
  logger arguments passed to helpers, logger monitor locks, and the differently named
  logger parameter/call in `IOUtils`.
- Mapped Java files to C# files from provenance headers, conversion records, and the
  traceability report.
- Matched reachable diagnostics to `Microsoft.Extensions.Logging` calls, retaining their
  level, exception object, control flow, and structured named values.
- Classified a Java row as `absent-region` only when the containing Java behavior has no
  corresponding target implementation. These decisions are explicit and line-addressable
  in the disposition ledger.

The generator is reproducible with:

```bash
python3 tools/parity/generate_java_logging_audit.py \
  --upstream-root /path/to/apache/pdfbox \
  --upstream-ref ddb7e78992bebc36140ba0d864c8212ec5da697b
```

It exits non-zero if any inventory row is unaccounted.

## Baseline inventory

| Item | Count |
|---|---:|
| Logger fields | 209 |
| Direct logger calls | 838 |
| Enabled-level guards | 50 |
| Logger helper passes | 10 |
| `synchronized(LOG)` monitors | 4 |
| Differently named logger parameter/call rows | 2 |
| Total inventory rows | 1,113 |

All 209 fields use Apache Commons Logging's `Log` type and the name `LOG`; one file
contains two fields in nested classes. The only differently named logger use is the
`Log logger` parameter in Java `IOUtils.closeAndLogException`.

### Direct calls by level

| Level | Count |
|---|---:|
| Trace | 7 |
| Debug | 219 |
| Info | 82 |
| Warn | 314 |
| Error | 216 |

### Direct calls by module and disposition

| Module | Migrated | Absent region | Total |
|---|---:|---:|---:|
| debugger | 7 | 24 | 31 |
| examples | 29 | 58 | 87 |
| fontbox | 115 | 17 | 132 |
| io | 8 | 4 | 12 |
| pdfbox | 297 | 273 | 570 |
| tools | 0 | 6 | 6 |
| **Total** | **456** | **382** | **838** |

## Migration result

| Inventory kind | Migrated | Absent region | Total |
|---|---:|---:|---:|
| Logger fields | 207 | 2 | 209 |
| Direct calls | 456 | 382 | 838 |
| Enabled-level guards | 30 | 20 | 50 |
| Helper logger passes | 7 | 3 | 10 |
| Logger monitor locks | 0 | 4 | 4 |
| Differently named parameter/call rows | 2 | 0 | 2 |
| **All rows** | **702** | **411** | **1,113** |

The generated summary reports zero unaccounted rows. Every reachable target diagnostic
resolves its typed logger dynamically through `PdfBoxLogging.CreateLogger<T>()`, so an
application-installed factory is observed even after the target type has initialized.
The default remains provider-neutral and silent through `NullLoggerFactory`.

## Verification

Representative tests cover:

- the exact log levels used by `PostScriptTable`;
- factory installation after `PostScriptTable` type initialization;
- a structured `ResourceName` value and attached `IOException` from
  `IOUtils.CloseAndLogException`;
- silent defaults through `NullLoggerFactory`;
- the PDFBOX-4322 identity-CMap regression safeguard included in issue #930.

No Serilog or other concrete logging provider is referenced. Verification commands are:

```bash
dotnet restore PdfBoxNet.slnx
dotnet build PdfBoxNet.slnx --configuration Release --no-restore
dotnet test PdfBoxNet.slnx --configuration Release --no-build --no-restore
python3 tools/parity/generate_java_logging_audit.py \
  --upstream-root /path/to/apache/pdfbox \
  --upstream-ref ddb7e78992bebc36140ba0d864c8212ec5da697b
git diff --check
```

The final Release build completed with zero errors. The complete solution test run passed
1,466 tests, skipped 7 platform/fixture tests, and had no failures. The API-surface ratchet
passed with no unreviewed regression. The release runtime ratchet compared 150 PDFs and 80
merge pairs, producing 691 matches, zero known divergences, and zero unexpected divergences.
The logging audit exited successfully with all 1,113 rows accounted.
