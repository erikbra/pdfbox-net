# Skill F - Compile-oriented normalization pass

## Purpose
Apply a constrained, post-mechanical normalization pass so converted files are closer to valid, compilable C# while preserving provenance and parity intent.

## When to run
- After Skill A for newly converted files.
- After Skill B for updated files with significant syntax/API drift.

## Inputs
- Mechanically converted C# files (with provenance headers)
- Current compile diagnostics (for example `dotnet build` output)
- Mapping/provenance records

## Output
- Updated C# files with compile-oriented fixes
- Normalization record per file
- Updated status for Skill E reporting

## Allowed normalization categories
- Java syntax leftovers -> C# syntax equivalents.
- Namespace/type qualification fixes needed for C# compilation.
- Primitive/collection/type alias fixes required by C# type system.
- Exception/using/disposable pattern adjustments only when required to compile.

## Disallowed changes in this skill
- Functional redesign of behavior.
- Public API redesign for ".NET-feeling" ergonomics.
- Silent semantic changes without note.

## Required normalization record fields (per file)
- `source_path`
- `target_path`
- `normalization_applied` (`true` | `false`)
- `change_categories` (list)
- `semantic_risk` (`low` | `medium` | `high`)
- `compiles_after_change` (`true` | `false`)
- `note`
