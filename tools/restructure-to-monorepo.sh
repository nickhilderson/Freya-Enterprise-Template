#!/usr/bin/env bash
set -euo pipefail

TEMPLATE_DIR="templates/Freya.Template"
OLD_SLN="Freya.EnterpriseTemplate.sln"
NEW_SLN_NAME="Freya.Template.sln"

CI_FILE=".github/workflows/ci.yml"
CODEQL_FILE=".github/workflows/codeql.yml"

echo "== Freya monorepo restructure =="
echo

# 1) Sanity checks
if [ ! -d ".git" ]; then
  echo "❌ Not in a git repository root (no .git folder). Run this from repo root."
  exit 1
fi

if [ ! -f "$OLD_SLN" ]; then
  echo "❌ Solution '$OLD_SLN' not found in repo root."
  echo "   If your solution has a different name, update OLD_SLN in this script."
  exit 1
fi

if [ ! -d "src" ]; then
  echo "❌ 'src' folder not found in repo root."
  exit 1
fi

if [ ! -d "tests" ]; then
  echo "❌ 'tests' folder not found in repo root."
  exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
  echo "❌ Working tree is not clean."
  echo "   Commit or stash changes first."
  exit 1
fi

# 2) Create target structure
echo "-> Creating $TEMPLATE_DIR ..."
mkdir -p "$TEMPLATE_DIR"

# 3) Move solution + src + tests (together to preserve relative paths)
echo "-> Moving solution + src + tests under $TEMPLATE_DIR ..."
git mv "$OLD_SLN" "$TEMPLATE_DIR/$NEW_SLN_NAME"
git mv "src" "$TEMPLATE_DIR/src"
git mv "tests" "$TEMPLATE_DIR/tests"

# 4) Update CI paths
if [ -f "$CI_FILE" ]; then
  echo "-> Updating $CI_FILE paths ..."

  # Update build/test to target the solution explicitly
  perl -0777 -pi -e "
    s/dotnet restore\\b/dotnet restore $TEMPLATE_DIR\\/$NEW_SLN_NAME/g;
    s/dotnet build -c Release --no-restore/dotnet build $TEMPLATE_DIR\\/$NEW_SLN_NAME -c Release --no-restore/g;
    s/dotnet test -c Release --no-build/dotnet test $TEMPLATE_DIR\\/$NEW_SLN_NAME -c Release --no-build/g;
  " "$CI_FILE"

  # Update publish paths (Project.Api moved)
  perl -0777 -pi -e "
    s#dotnet publish\\s+src/Project\\.Api/Project\\.Api\\.csproj#dotnet publish $TEMPLATE_DIR/src/Project.Api/Project.Api.csproj#g;
  " "$CI_FILE"

  # Update CycloneDX invocation to point to moved solution
  perl -0777 -pi -e "
    s#dotnet-CycloneDX\\s+[^\\s]+\\.sln#dotnet-CycloneDX $TEMPLATE_DIR/$NEW_SLN_NAME#g;
  " "$CI_FILE"
else
  echo "⚠️  $CI_FILE not found, skipping CI update."
fi

# 5) Update CodeQL workflow paths (if present)
if [ -f "$CODEQL_FILE" ]; then
  echo "-> Updating $CODEQL_FILE build path ..."
  perl -0777 -pi -e "
    s/dotnet build -c Release/dotnet build $TEMPLATE_DIR\\/$NEW_SLN_NAME -c Release/g;
  " "$CODEQL_FILE"
else
  echo "ℹ️  $CODEQL_FILE not found, skipping CodeQL update."
fi

# 6) Quick verification commands (non-fatal)
echo
echo "== Next checks (run locally) =="
echo "dotnet restore $TEMPLATE_DIR/$NEW_SLN_NAME"
echo "dotnet build   $TEMPLATE_DIR/$NEW_SLN_NAME -c Release"
echo "dotnet test    $TEMPLATE_DIR/$NEW_SLN_NAME -c Release"
echo

echo "✅ Restructure complete."
echo "Now review git diff, then commit and push."