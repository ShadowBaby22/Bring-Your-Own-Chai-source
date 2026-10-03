#!/bin/bash
# Regression suite. No browser: the visible pane checks are in Docs/REGRESSION.md.
#   bash Tools/test.sh               compile check (all scripts, incl. View/UI) + every test group
#   bash Tools/test.sh shop nurse    only those test groups (data, art, shop, days, nurse, battle, balance)
#   bash Tools/test.sh --no-compile  skip the Unity compile check
# Exits non-zero on any failure. deploy_pages.sh staging runs this first.
set -uo pipefail
P="$(cd "$(dirname "$0")/.." && pwd)"
U=/Applications/Unity/Hub/Editor/6000.5.9f1/Unity.app/Contents/Resources/Scripting
DOTNET="$(command -v dotnet || echo "$U/DotNetSdk/dotnet")"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

COMPILE=1; ARGS=()
for a in "$@"; do [ "$a" == "--no-compile" ] && COMPILE=0 || ARGS+=("$a"); done

if [ $COMPILE == 1 ]; then
  echo "== Compile check (Core, Battle, View, Editor against Unity's DLLs)"
  OUT="$(mktemp -d)"
  LOG="$(bash "$P/Tools/compile.sh" "$OUT" 2>&1)"
  rm -rf "$OUT"
  if echo "$LOG" | grep -q "error CS" || ! echo "$LOG" | grep -q -- "--- runtime exit 0" || ! echo "$LOG" | grep -q -- "--- editor exit 0"; then
    echo "$LOG" | grep -v "warning CS" | tail -20
    echo "COMPILE FAILED"; exit 1
  fi
  echo "  ok    all scripts compile"
fi

echo "== Logic tests"
"$DOTNET" run -c Release --project "$P/Tools/tests/tests.csproj" -- ${ARGS[@]+"${ARGS[@]}"}
