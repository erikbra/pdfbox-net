# Skill C - Upstream deletion handling

## Purpose
Handle source files removed upstream and keep the .NET port in a consistent state.

## Inputs
- Set of deleted upstream Java files
- Current source->target mapping
- Deletion policy (`deprecate` or `remove`)

## Output
- Target C# files deprecated or removed per policy
- Mapping/parity records updated with deletion status
- Review record for each deletion decision

## Delete vs deprecate policy

| Condition | Action |
|---|---|
| Upstream file deleted and no downstream usage in .NET port | Remove target file |
| Upstream file deleted but public API is still referenced in .NET port | Deprecate target file and create migration note |
| Upstream file replaced by new upstream type | Deprecate old target, map to replacement via Skill D |

## Required deletion record fields (per file)
- `source_path`
- `target_path`
- `decision` (`remove` | `deprecate`)
- `reason`
- `reviewer_or_agent`
