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
6. Apply **Skill H** when reviewing PRs for approval.
7. Apply **Skill I** to orchestrate issue-by-issue execution and merge flow.
8. Port JavaDoc documentation comments from each upstream Java file into XML documentation
   comments in the converted C# file as part of the mechanical conversion/sync flow.
9. Port code and comments as verbatim as possible from the Java source; only adapt where
   required for C# correctness or unavoidable platform differences.
10. When committing issue work, include a GitHub closing reference in the commit message
    body, e.g. `Closes #413`, so pushing the commit closes the tracked issue.
11. Keep external NuGet library dependencies isolated behind narrow internal interfaces
    or adapters. Do not let third-party API types leak through core PDFBox abstractions;
    this keeps dependency-specific functionality movable into separate packages later.
12. Keep JavaBean-style accessor methods in mechanically converted files. When adding a
    more idiomatic .NET property facade for `GetX`/`SetX`/`IsX`/`HasX`, mark the type
    `partial` and put the proxy property in a sibling partial class file such as
    `PDPage.Properties.cs`; do not replace the upstream-shaped methods in the
    mechanical file.

## Skill files

- [Skill A - Initial mechanical conversion + provenance header](skills/skill-a-initial-conversion.md)
- [Skill B - Upstream rewrite/update sync](skills/skill-b-upstream-update-sync.md)
- [Skill C - Upstream deletion handling](skills/skill-c-upstream-delete.md)
- [Skill D - Upstream new-file intake](skills/skill-d-upstream-new-file.md)
- [Skill E - Traceability and parity reporting](skills/skill-e-traceability-report.md)
- [Skill F - Compile-oriented normalization pass](skills/skill-f-compile-oriented-normalization.md)
- [Skill G - Java → C# API and type mapping reference](skills/skill-g-java-csharp-api-mappings.md)
- [Skill H - Automatic PR approval checklist](skills/skill-h-automatic-pr-approval.md)
- [Skill I - Orchestrating sequential issue delivery](skills/skill-i-orchestrating-issues.md)
- [Worked example - end-to-end flow A->F->E](skills/worked-example-a-to-f-to-e.md)
