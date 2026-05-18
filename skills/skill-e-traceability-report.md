# Skill E - Traceability and parity reporting

## Purpose
Produce auditable status for source-to-target mappings and sync health.

## Inputs
- Current source->target mapping data
- File provenance metadata
- Latest analyzed upstream revision

## Output
- Machine-readable report (for example JSON or CSV) with statuses:
  - `in-sync`
  - `needs-sync`
  - `deleted-upstream`
  - `new-upstream`
  - `needs-manual-sync`

## Notes
- Run after Skills B/C/D to publish current project sync state.

## Required report schema
Each report row/object must contain:
- `source_path`
- `target_path`
- `source_commit`
- `sync_commit`
- `port_mode`
- `status`
- `last_checked_utc`
- `note`

## Status rules
- `in-sync`: source+target pair synced to latest tracked upstream commit.
- `needs-sync`: upstream changed after `sync_commit`.
- `deleted-upstream`: mapped source no longer exists upstream.
- `new-upstream`: upstream file exists with no mapping yet.
- `needs-manual-sync`: automated sync stopped due to unresolved conflict.
