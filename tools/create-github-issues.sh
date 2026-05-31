#!/bin/zsh

for FILE in issues/*.md; do
  echo "Processing $FILE..."

  COMMIT=$(git log --diff-filter=A --pretty=format:'%H' -- "$FILE")
  COMMIT_DATE=$(git show -s --format='%cI' "$COMMIT")  # ISO-8601 commit date
  TITLE=$(awk '/^### Title/{getline; print; exit}' "$FILE")

  ISSUE_URL=$(gh issue create \
    --repo erikbra/pdfbox-net \
    --title "$TITLE" \
    --body "$(cat "$FILE")

---
Introduced in commit: $COMMIT" )

  gh issue close "$ISSUE_URL" \
    --reason "completed" \
    --comment "Closed as completed. Related commit date: $COMMIT_DATE (commit $COMMIT)."

  rm $FILE
done