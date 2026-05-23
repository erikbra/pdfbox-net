### Title
Port `fontbox/util/**` core utilities beyond `BoundingBox`

### Depends on
- Initial `BoundingBox` port in this branch

### Scope
- Port remaining `org.apache.fontbox.util` types needed by downstream encoding and parser packages.
- Keep parity close to upstream behavior and preserve provenance metadata.

### Exit criteria
- Util namespace compiles without new stubs in touched files.
- Focused util tests are added/updated and pass.
- `dotnet test` remains green.
