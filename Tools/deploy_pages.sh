#!/bin/bash
# Publishes Builds/WebGL to GitHub Pages (repo ShadowBaby22/Bring-Your-Own-Chai).
#   bash Tools/deploy_pages.sh staging   -> https://shadowbaby22.github.io/Bring-Your-Own-Chai/       (team playtest link)
#   bash Tools/deploy_pages.sh dev       -> https://shadowbaby22.github.io/Bring-Your-Own-Chai/dev/   (work in progress)
# Branches: main = what we submit, staging = the playtest link, dev = work in progress.
set -euo pipefail
TARGET="${1:-}"
[[ "$TARGET" == "staging" || "$TARGET" == "dev" ]] || { echo "usage: $0 staging|dev"; exit 1; }
# The playtest link only gets builds whose code passes the regression suite (SKIP_TESTS=1 to override).
if [ "$TARGET" == "staging" ] && [ "${SKIP_TESTS:-0}" != "1" ]; then
  bash "$(dirname "$0")/test.sh" || { echo "Regression suite failed: not deploying to staging."; exit 1; }
fi
cd "$(dirname "$0")/../Builds"
REPO="ShadowBaby22/Bring-Your-Own-Chai"
SITE="pages-site"   # a git working copy that only ever holds the built game

if [ ! -d "$SITE/.git" ]; then
  git clone "https://github.com/$REPO.git" "$SITE"
fi
git -C "$SITE" pull -q --rebase origin main || true

if [ "$TARGET" == "dev" ]; then
  rm -rf "$SITE/dev" && mkdir -p "$SITE/dev" && cp -R WebGL/. "$SITE/dev/"
else
  # Replace the site root with the fresh build, keeping .git and the dev/ preview.
  find "$SITE" -mindepth 1 -maxdepth 1 ! -name .git ! -name dev -exec rm -rf {} +
  cp -R WebGL/. "$SITE/"
fi
touch "$SITE/.nojekyll"   # serve files as-is, no Jekyll processing
cat > "$SITE/README.md" <<'EOF'
# GRAP Stage 4 (Bring Your Own Chai)

**Play: https://shadowbaby22.github.io/Bring-Your-Own-Chai/** (phone or desktop, sound on, portrait)
Work-in-progress build: https://shadowbaby22.github.io/Bring-Your-Own-Chai/dev/

A game jam entry for the theme *Everything Is Temporary*. You have an 11-month rent agreement in Delhi NCR,
which means 11 days, to get one stamp from the Sarkari Daftar. Every skill you pick up comes with a lease.

This repo only holds the built web game.
EOF

cd "$SITE"
git add -A
if git diff --cached --quiet; then echo "Nothing changed; site already up to date."; exit 0; fi
STAMP=$(cat ../WebGL/build_info.txt 2>/dev/null || date)
git commit -q -m "Deploy $TARGET web build ($STAMP)

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
git push -q -u origin main
[ "$TARGET" == "dev" ] && echo "Pushed. Live in ~1 minute at https://shadowbaby22.github.io/Bring-Your-Own-Chai/dev/" \
                       || echo "Pushed. Live in ~1 minute at https://shadowbaby22.github.io/Bring-Your-Own-Chai/"
