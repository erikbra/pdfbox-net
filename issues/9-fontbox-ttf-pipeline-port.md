### Title
Port `fontbox/ttf/**` tables and TrueType/OpenType parsing pipeline

### Depends on
- #6 `fontbox/util` follow-up
- #7 `fontbox/encoding` port
- #8 `fontbox/type1` + `fontbox/cff` port

### Scope
- Port TrueType/OpenType table and parser pipeline classes in `org.apache.fontbox.ttf`.
- Add fixture-driven tests for table parsing and pipeline behavior.

### Exit criteria
- TTF parser pipeline compiles with prior FontBox layers.
- Fixture-driven tests pass for core table parsing paths.
- `dotnet test` remains green.
