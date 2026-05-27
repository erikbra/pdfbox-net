# Skill I - Orchestrating sequential issue delivery

Use this process to run issues in strict sequence (66, 67, 68, ...), one at a time.

## Orchestration process

1. **Select next issue in sequence**  
   Start with the next numeric issue in line. Do not skip or parallelize unless explicitly directed.

2. **Read issue scope and acceptance criteria**  
   Confirm what "done" means before implementation starts.

3. **Start a new agent session for the issue**  
   Start a new agent session with the issue description as context. The orchestrator session should coordinate and review, not implement the issue directly.

4. **Implement issue changes only**  
   Keep work tightly scoped to the selected issue.

5. **Run validation**  
   Run required build/tests/checks and ensure no regressions.

6. **Open/update PR for the issue**  
   Ensure the PR references the issue and includes clear validation evidence.

7. **Review PR using Skill H**  
   Apply all criteria from `AUTOMATIC-PR-APPROVAL.md` as a strict gate.

8. **Approve or reject with rationale**  
   - Approve only when all Skill H criteria pass.  
   - If any criterion fails, reject and list concrete gaps to fix.

9. **Merge after approval**  
   Merge only after approval is complete and no blockers remain.

10. **Start the next issue**  
   Move to the next numeric issue only after merge completion.

## Operating rules

- Single active issue at a time.
- Keep a clear pass/fail checklist record per issue.
- If scope ambiguity appears, pause and clarify before proceeding.
