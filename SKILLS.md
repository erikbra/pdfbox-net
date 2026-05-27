# SKILLS index (mechanical PDFBox -> .NET conversion)

This index splits the conversion workflow into small, focused skills.

## How to use

1. Start with **Skill A** for newly ported files.
2. On upstream updates, run one of:
   - **Skill B** for rewrites/updates
   - **Skill C** for deletions
   - **Skill D** for newly added upstream files
3. Run **Skill F** to apply compile-oriented normalization where needed.
4. Run **Skill E** after updates/normalization to produce traceability/parity status.
5. Consult **Skill G** whenever the Java source uses NIO, `BitSet`, `LinkedHashMap` LRU,
   `IOUtils`, Log4j, or any other API with a non-obvious C# mapping.
6. Apply **Skill H** when running orchestrated, issue-by-issue PR approval.
7. Port JavaDoc documentation comments from each upstream Java file into XML documentation
   comments in the converted C# file as part of the mechanical conversion/sync flow.
8. Port code and comments as verbatim as possible from the Java source; only adapt where
   required for C# correctness or unavoidable platform differences.

## Skill files

- [Skill A - Initial mechanical conversion + provenance header](skills/skill-a-initial-conversion.md)
- [Skill B - Upstream rewrite/update sync](skills/skill-b-upstream-update-sync.md)
- [Skill C - Upstream deletion handling](skills/skill-c-upstream-delete.md)
- [Skill D - Upstream new-file intake](skills/skill-d-upstream-new-file.md)
- [Skill E - Traceability and parity reporting](skills/skill-e-traceability-report.md)
- [Skill F - Compile-oriented normalization pass](skills/skill-f-compile-oriented-normalization.md)
- [Skill G - Java → C# API and type mapping reference](skills/skill-g-java-csharp-api-mappings.md)
- [Skill H - Automatic PR approval checklist](skills/AUTOMATIC-PR-APPROVAL.md)
- [Worked example - end-to-end flow A->F->E](skills/worked-example-a-to-f-to-e.md)

## Pilot evaluation

- [Delegated pilot run against Apache PDFBox (`pdfbox` module)](skills/pilot-evaluation-apache-pdfbox.md)
- [Delegated pilot run #2 (24 files, post-adjustment)](skills/pilot-evaluation-apache-pdfbox-round2.md)
