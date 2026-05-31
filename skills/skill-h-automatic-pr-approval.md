# Skill H - Automatic PR approval checklist

Use this checklist to review issue-driven PRs in strict sequence, one issue at a time. Only move to the next issue after the current one is completed and approved.

## Approval criteria

1. **Issue closure is explicit**  
   The PR references the target issue and clearly states what scope is completed.

2. **Scope matches issue**  
   The changes are tightly focused on that issue's defined deliverables, with no scope creep.

3. **Parity/behavior correctness**  
   Ported logic matches upstream PDFBox semantics in the touched area.

4. **Tests added/updated and passing**  
   Targeted tests for new behavior are present and no regressions are introduced.

5. **No remaining stubs/placeholders in touched scope**  
   If the issue is marked complete, no TODO/stub gaps remain in the touched area.

6. **Required artifacts/reports updated**  
   Parity/conversion/traceability artifacts are refreshed where applicable.

7. **PR description quality**  
   The PR description includes summary, concrete change list, and validation evidence.

8. **Apache license provenance header present**  
   All ported source files include the original Apache 2.0 provenance header with upstream attribution.

9. **AI-led approval gate**  
   The orchestrator agent performs the final checklist review and approves when all criteria pass.
