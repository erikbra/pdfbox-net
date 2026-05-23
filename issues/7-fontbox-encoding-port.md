### Title
Port `fontbox/encoding/**` with parity tests

### Depends on
- #6 `fontbox/util` follow-up

### Scope
- Port encoding tables and code/name lookup behavior in `org.apache.fontbox.encoding`.
- Add parity checks for representative encodings and fallback paths.

### Exit criteria
- Encoding classes compile against ported util layer.
- Lookup behavior tests pass for core encodings.
- `dotnet test` remains green.
