### Title
Complete remaining filter, pdfparser, and pdfwriter parity

### Depends on
- Current parser/writer baseline through issues #37-#41
- Graphics closeout path #53-#57 for image/filter interactions where relevant

### Background
The remaining `filter`, `pdfparser`, and `pdfwriter` gaps are the last low-level blockers before
full `Loader`, FDF, multipdf, signatures, and incremental-save parity can be completed safely.

### Scope
- Port the remaining `filter` files required for ASCII85, CCITT helper streams, TIFF extension,
  decoder-stream parity, and shared filter abstractions.
- Port the remaining `pdfparser` files needed for recovery, FDF parsing, xref stream support, and
  base lexical flow completeness.
- Port the remaining `pdfwriter` files needed for save/incremental-save/object-stream parity.
- Keep `Loader` follow-up aligned with this wave instead of adding ad-hoc parser entry points.

### Expected test scope
- Parser and writer fixture tests for xref, object streams, recovery, and FDF-relevant paths.
- Filter-specific tests for ASCII85 and CCITT stream semantics.

### Entry criteria
- Current parser/document load pipeline is stable and green.

### Exit criteria
- Remaining `filter`, `pdfparser`, and `pdfwriter` files are ported or explicitly deferred with
  rationale tied to a downstream issue.
- `Loader` prerequisites are complete for a later dedicated closeout.

### Risk register
- Recovery parsing and xref repair paths are edge-case dense.
- Writer/object-stream work can expose latent COS ownership bugs.

### PR slicing rule
- First PR: remaining filters.
- Second PR: parser completeness.
- Third PR: writer completeness and `Loader` readiness backfill.

### Definition of done
- `dotnet build` passes.
- Targeted parser/filter/writer tests pass.
- Traceability and conversion artifacts cover all touched mappings.
