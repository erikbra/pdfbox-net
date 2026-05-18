# Skill B - Upstream rewrite/update sync

## Purpose
Re-sync already tracked C# files when upstream PDFBox source files are rewritten or updated.

## Inputs
- Set of changed upstream Java files
- Current source->target mapping
- Latest upstream commit SHA
- Existing target file (including any `PORT-LOCAL` regions)

## Output
- Re-generated/updated mapped C# files
- Updated `PORT_LAST_SYNC_COMMIT` values
- Conflict flags where local adapted code diverges from mechanical baseline

## Notes
- Preserve only explicitly managed local regions bounded by:
  - `// PORT-LOCAL-START`
  - `// PORT-LOCAL-END`

## Sync workflow (required)
1. Regenerate the target C# file mechanically from the updated upstream Java file.
2. Compare regenerated output vs current target.
3. If `PORT-LOCAL` regions exist, lift those regions from current target into regenerated output.
4. Re-run formatting/syntax sanity checks on the merged target.
5. Update `PORT_LAST_SYNC_COMMIT` only when result status is `in-sync`.
6. If unresolved, keep previous target unchanged and emit a `needs-manual-sync` record.

## Conflict decision policy

| Conflict type | Condition | Action |
|---|---|---|
| none | Mechanical regeneration clean | Accept regenerated file |
| managed-region | Change overlaps a managed local region | Keep region, regenerate around region, flag review |
| semantic-divergence | Adapted code diverges from upstream behavior | Keep adapted code, set `PORT_MODE=adapted`, add sync note |
| unresolved | Automated merge cannot safely determine behavior | Mark `needs-manual-sync` and stop auto-apply |

## Managed-region guardrails
- A `PORT-LOCAL` region must be non-overlapping and fully bounded.
- Nested `PORT-LOCAL` regions are invalid and must be treated as `needs-manual-sync`.
- If a region marker is missing its pair, treat as `needs-manual-sync`.

## Required sync log fields (per file)
- `source_path`
- `target_path`
- `previous_sync_commit`
- `new_sync_commit`
- `conflict_type`
- `result_status` (`in-sync` | `needs-manual-sync`)
- `local_region_count`
- `sync_note`
