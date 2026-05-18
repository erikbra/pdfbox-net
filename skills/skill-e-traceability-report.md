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

## Notes
- Run after Skills B/C/D to publish current project sync state.
