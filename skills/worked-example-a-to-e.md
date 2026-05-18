# Worked example (Skills A -> F -> E)

This is a concrete mini-flow showing required inputs/outputs.

## Input set
- Upstream file: `pdfbox/src/main/java/org/apache/pdfbox/util/Vector.java`
- Upstream commit: `ccd281cfecedcc0ad39709bece5e67b19a54e8db`
- Target path rule: `org/apache/pdfbox/**` -> `src/PdfBox/**`

## Skill A (initial conversion)

Output file: `src/PdfBox/Util/Vector.cs`

Required header:

```csharp
// PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/Vector.java
// PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
```

Skill A record example:

```json
{
  "source_path": "pdfbox/src/main/java/org/apache/pdfbox/util/Vector.java",
  "target_path": "src/PdfBox/Util/Vector.cs",
  "source_commit": "ccd281cfecedcc0ad39709bece5e67b19a54e8db",
  "port_mode": "mechanical",
  "sync_commit": "ccd281cfecedcc0ad39709bece5e67b19a54e8db",
  "conversion_notes": "Initial one-to-one conversion"
}
```

## Skill B (upstream update sync)

If upstream changes `Vector.java`, regenerate and update:
- `PORT_LAST_SYNC_COMMIT`
- sync log fields (`conflict_type`, `result_status`)

Example result status: `in-sync`.

## Skill C (upstream deletion)

If upstream deletes `Vector.java`:
- remove mapped `Vector.cs` when no downstream usage, **or**
- deprecate when .NET public API still depends on it.

## Skill D (new upstream file)

If upstream adds `Matrix.java`:
- create `src/PdfBox/Util/Matrix.cs`
- stamp required provenance header
- record intake status (`converted` or `backlog`)

## Skill F (compile-oriented normalization)

If `dotnet build` shows syntax/type errors in `Vector.cs`:
- apply limited compile-oriented fixes (syntax/type/API compatibility),
- keep provenance header fields unchanged,
- log normalization categories and semantic risk.

## Skill E (report row example)

```json
{
  "source_path": "pdfbox/src/main/java/org/apache/pdfbox/util/Vector.java",
  "target_path": "src/PdfBox/Util/Vector.cs",
  "source_commit": "ccd281cfecedcc0ad39709bece5e67b19a54e8db",
  "sync_commit": "ccd281cfecedcc0ad39709bece5e67b19a54e8db",
  "port_mode": "mechanical",
  "status": "in-sync",
  "last_checked_utc": "2026-05-18T00:00:00Z",
  "note": ""
}
```
