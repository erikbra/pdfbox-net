#!/bin/zsh

setopt null_glob

for FILE in issues/*.md; do
  echo "Processing $FILE..."

  # A path can have multiple historical "add" commits if it was deleted/re-added;
  # keep only the first hash so downstream git commands receive a single revision.
  COMMIT=$(git log --diff-filter=A --pretty=format:'%H' -- "$FILE" | head -n 1)
  COMMIT_DATE=""
  if [[ -n "$COMMIT" ]]; then
    COMMIT_DATE=$(git show -s --format='%cI' "$COMMIT")  # ISO-8601 commit date
  fi
  TITLE=$(awk '
    /^# +/ {
      line = $0
      sub(/^# +/, "", line)
      sub(/^Issue[[:space:]]+[0-9]+[[:space:]]+[^[:alnum:][:space:]][[:space:]]*/, "", line)
      print line
      exit
    }
  ' "$FILE")

  if [[ -z "$TITLE" ]]; then
    echo "Skipping $FILE: could not determine title."
    continue
  fi

  ISSUE_BODY=$(cat "$FILE")
  if [[ -n "$COMMIT" ]]; then
    ISSUE_BODY+="

---
Introduced in commit: $COMMIT"
  fi

  if ! ISSUE_URL=$(gh issue create \
    --repo erikbra/pdfbox-net \
    --title "$TITLE" \
    --body "$ISSUE_BODY" ); then
    echo "Failed to create issue for $FILE; leaving file in place."
    continue
  fi

#  if [[ -n "$COMMIT" && -n "$COMMIT_DATE" ]]; then
#    gh issue close "$ISSUE_URL" \
#      --reason "completed" \
#      --comment "Closed as completed. Related commit date: $COMMIT_DATE (commit $COMMIT)."
#  else
#    gh issue close "$ISSUE_URL" --reason "completed"
#  fi

  rm "$FILE"
done