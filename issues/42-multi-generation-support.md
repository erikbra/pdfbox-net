### Title
Multi-generation upstream PDFBox support — tracking and 3.x backport planning

### Goal
Track feasibility findings and define an actionable path for optionally supporting the
PDFBox 3.x stable generation alongside the current trunk (4.x) port.

### Background
See `reports/multi-generation-feasibility-assessment.md` for the full analysis.

The two generations share the same module structure and core PDF processing algorithms.
The known divergence points are limited to:
1. `COSName` caching mechanism (dual-map in 3.x vs. WeakReference + `Cleaner` in trunk)
2. `BaseParser` / `COSParser` logic distribution (parse-object logic in `BaseParser` for 3.x,
   moved to `COSParser` in trunk)
3. `SmallMap` present only in 3.x (`util/SmallMap.java`, ~10,790 bytes)
4. Logging framework (Commons Logging in 3.x vs. Log4j 2 in trunk — **no C# impact**)

### Criteria to start active 3.x port work

A 3.x backport slice should only be started when **at least one** of these is true:
- A confirmed end-user or downstream consumer explicitly requires 3.x behavioural parity.
- PDFBox 4.x is substantially delayed and 3.x becomes the long-term stable target.
- A regression in the trunk-based port is traced to a 3.x fix that was not forward-ported.

### Provenance header change (low-risk, do first)

Add an optional `PDFBOX_GENERATION` field to Skill A provenance headers:

```
PDFBOX_SOURCE_PATH: <upstream relative path>
PDFBOX_SOURCE_COMMIT: <upstream commit sha>
PDFBOX_GENERATION: 4.x           ← NEW (default for trunk-sourced files)
PORT_MODE: mechanical|adapted
PORT_LAST_SYNC_COMMIT: <upstream commit sha>
```

For any file ported from the 3.x branch, use `PDFBOX_GENERATION: 3.x`.
This makes generation tracking auditable without branching the repository.

### 3.x delta file list

When triggered, the 3.x backport slice covers:

| File | Change required | Effort |
|---|---|---|
| `src/PdfBox.Net/COS/COSName.cs` | Replace WeakReference/Cleaner with 3.x dual-map (nameMap + commonNameMap) | 0.5 day |
| `src/PdfBox.Net/PdfParser/BaseParser.cs` | Add parse-object methods back (COSArray, COSDictionary, COSString, etc.) | 1–2 days |
| `src/PdfBox.Net/PdfParser/COSParser.cs` | Remove methods migrated to BaseParser | 0.5 day |
| `src/PdfBox.Net/Util/SmallMap.cs` | New file — port `SmallMap.java` from 3.x | 0.5 day |
| Provenance header backfill | Update `PDFBOX_GENERATION` on affected files | 0.5 day |
| Test parity and report updates | Run tests, update JSON reports | 0.5 day |
| **Total** | | **3.5–5 days** |

### Current status

- [ ] Add `PDFBOX_GENERATION` field to Skill A documentation (`skills/skill-a-initial-conversion.md`)
- [ ] Add `PDFBOX_GENERATION` handling to Skill B documentation (`skills/skill-b-upstream-update-sync.md`)
- [ ] Apply `PDFBOX_GENERATION: 4.x` to all existing provenance headers (backfill)
- [ ] (Gated) Port `COSName` 3.x dual-map variant when requirement confirmed
- [ ] (Gated) Restore BaseParser parse-object logic for 3.x variant when requirement confirmed
- [ ] (Gated) Port `SmallMap` from 3.x when requirement confirmed
- [ ] (Gated) Update all affected provenance headers to `PDFBOX_GENERATION: 3.x`

### Feasibility verdict

> Feasible — low-to-moderate complexity, not ridiculous.

See `reports/multi-generation-feasibility-assessment.md` §7 for the full answer.
