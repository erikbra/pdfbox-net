# Skill A - Initial mechanical conversion + provenance header

## Purpose
Create a first-pass C# file from an upstream PDFBox Java file and stamp required provenance metadata.

## Inputs
- Upstream Java file path
- Upstream commit SHA
- Target .NET module/path mapping

## Output
- New C# file with provenance header fields:
  - `PDFBOX_SOURCE_PATH`
  - `PDFBOX_SOURCE_COMMIT`
  - `PORT_MODE` (`mechanical` or `adapted`)
  - `PORT_LAST_SYNC_COMMIT`
- Apache 2.0 license header copied verbatim from upstream file (for mechanically ported files)
- Small separate two-line notice with:
  - copyright for C# port modifications/adaptations
  - statement that AI assistance was used in the conversion
- Documentation-style comments ported from JavaDoc where present

## Notes
- Default `PORT_MODE` should be `mechanical` for one-to-one conversion output.
- Set `PORT_MODE` to `adapted` only when behavior/API is intentionally changed from upstream mechanical parity.
- **Verbatim-first rule**: Port Java code and comments as verbatim as possible. Keep structure, ordering, and wording close to upstream unless C# language differences make a direct port impossible.
- Keep the Apache license block verbatim and place it before provenance metadata.
- Keep the copyright + AI conversion notice separate from the license text.
- Prefer preserving upstream inline test data setup over refactoring/extracting helpers when doing mechanical test conversions.
- **JavaBean accessors**: Port Java getter/setter methods as methods in the converted source (`getX` -> `GetX`, `setX` -> `SetX`, `isX` -> `IsX`, `hasX` -> `HasX`). Do not collapse them into C# properties inside the mechanical file. If an idiomatic .NET property is useful, add it later as a proxy in a sibling partial adapter file (for example `Foo.Properties.cs`) and mark the original type `partial`.
- **Closeable → IDisposable**: Java's `java.io.Closeable` maps to .NET's `System.IDisposable`. Any interface or class extending `Closeable` in Java must also extend `IDisposable` in C#. Add a default implementation `void IDisposable.Dispose() => Close();` to the C# interface so concrete classes satisfy `IDisposable` automatically through the interface default method. If multiple `IDisposable`-bearing interfaces are combined (e.g. a read+write super-interface), only one of them should provide the default `Dispose()` implementation to avoid diamond ambiguity — typically the read interface.
- **Header blank line**: In the provenance comment block at the top of each mechanically converted file, add a blank comment line (` *`) immediately after the "Mechanically converted..." line and before the `PDFBOX_SOURCE_*` fields.
- **API/type substitutions**: Before starting the conversion, consult **Skill G** for the authoritative Java→C# mapping table. Critical areas: `java.nio.ByteBuffer`, `FileChannel`, memory-mapped files, `BitSet`, `LinkedHashMap` LRU cache, `IOUtils`, threading helpers, exception types, and test assertion char-literal widening.
- **Required test parity step**: When a Java production class is converted, also convert the upstream Java tests that cover that class (from `pdfbox/src/test/java/...`) into `tests/PdfBox.Net.Tests/...` in the same work item whenever dependencies allow. If a test cannot be converted yet because a required dependency layer is not ported, record that limitation explicitly in conversion notes and traceability artifacts.

## Required provenance header format
Place this block at the **very top** of every converted C# file, before the Apache license header:

```csharp
/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: <upstream relative path>
 * PDFBOX_SOURCE_COMMIT: <upstream commit sha>
 * PORT_MODE: mechanical|adapted
 * PORT_LAST_SYNC_COMMIT: <upstream commit sha>
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements. ...
 */
```

> **Note**: all existing C# files in this port use block comment (`/* ... */`) format.
> Do **not** use single-line `//` comments for the provenance block.

## Required conversion record fields (per file)
- `source_path`
- `target_path`
- `source_commit`
- `port_mode`
- `sync_commit`
- `conversion_notes`
