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
- Preserve explicitly managed local regions if that policy is used.
