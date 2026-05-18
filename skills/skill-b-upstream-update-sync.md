# Skill B - Upstream rewrite/update sync

## Purpose
Re-sync already tracked C# files when upstream PDFBox source files are rewritten or updated.

## Inputs
- Set of changed upstream Java files
- Current source->target mapping
- Latest upstream commit SHA

## Output
- Re-generated/updated mapped C# files
- Updated `PORT_LAST_SYNC_COMMIT` values
- Conflict flags where local adapted code diverges from mechanical baseline

## Notes
- Preserve only explicitly managed local regions bounded by:
  - `// PORT-LOCAL-START`
  - `// PORT-LOCAL-END`

## Conflict decision policy

| Conflict type | Condition | Action |
|---|---|---|
| none | Mechanical regeneration clean | Accept regenerated file |
| managed-region | Change overlaps a managed local region | Keep region, regenerate around region, flag review |
| semantic-divergence | Adapted code diverges from upstream behavior | Keep adapted code, set `PORT_MODE=adapted`, add sync note |
| unresolved | Automated merge cannot safely determine behavior | Mark `needs-manual-sync` and stop auto-apply |

## Required sync log fields (per file)
- `source_path`
- `target_path`
- `previous_sync_commit`
- `new_sync_commit`
- `conflict_type`
- `result_status` (`in-sync` | `needs-manual-sync`)
