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
