### Title
Complete COS visitors and serialization interactions

### Depends on
- #27 COS stream lifecycle parity

### Scope
- Complete visitor pathways and serialization interactions across COS types.
- Ensure parser/writer low-level dependencies can consume COS objects consistently.
- Exclude higher-level `pdmodel` behavior.

### Expected test scope
- Add visitor traversal coverage for mixed COS object graphs.
- Add serialization-focused tests for deterministic output on representative fixtures.

### Exit criteria
- Visitor and serialization paths for scoped COS types are complete.
- Targeted visitor/serialization tests pass.
- Traceability + adaptation notes are updated where needed.
