### Title
Port `fontbox/type1/**` and `fontbox/cff/**` parser stack

### Depends on
- #6 `fontbox/util` follow-up
- #7 `fontbox/encoding` port

### Scope
- Port Type1 and CFF parser/model classes in dependency order.
- Add parser-focused tests for representative fixtures and edge cases.

### Exit criteria
- Type1/CFF parser stack compiles and integrates with encoding/util layers.
- Parser tests pass for included fixtures.
- `dotnet test` remains green.
