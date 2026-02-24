#!/usr/bin/env bash
set -euo pipefail

# Target: move the existing template solution + src + tests under templates/Freya.Template/
TEMPLATE_DIR="templates/Freya.Template"
OLD_SLN="Freya.EnterpriseTemplate.sln"
NEW_SLN="Freya.Template.sln"

CI_FILE=".github/workflows/ci.yml"
CODEQL_FILE=".github/workflows/codeql.yml"

echo "== Freya monorepo restructure (clean) =="

# Sanity checks
if [ ! -d ".git" ]; then
  echo "❌ Run from the repo root (no .git found)."
  exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
  echo "❌ Working tree not clean. Commit/stash first."
  exit 1
fi

if [ ! -f "$OLD_SLN" ]; then
  echo "❌ Solution '$OLD_SLN' not found in repo root."
  exit 1
fi

if [ ! -d "src" ] || [ ! -d "tests" ]; then
  echo "❌ Expected 'src' and 'tests' folders in repo root."
  exit 1
fi

mkdir -p "$TEMPLATE_DIR"

echo "-> Moving solution + src + tests under $TEMPLATE_DIR ..."
git mv "$OLD_SLN" "$TEMPLATE_DIR/$NEW_SLN"
git mv "src" "$TEMPLATE_DIR/src"
git mv "tests" "$TEMPLATE_DIR/tests"

# Patch CI paths (macOS-compatible sed)
# Note: BSD sed needs -i '' for in-place edit.
if [ -f "$CI_FILE" ]; then
  echo "-> Patching $CI_FILE ..."

  # 1) Restore/build/test: point to moved solution
  sed -i '' "s|dotnet restore|dotnet restore $TEMPLATE_DIR/$NEW_SLN|g" "$CI_FILE"
  sed -i '' "s|dotnet build -c Release --no-restore|dotnet build $TEMPLATE_DIR/$NEW_SLN -c Release --no-restore|g" "$CI_FILE"
  sed -i '' "s|dotnet test -c Release --no-build|dotnet test $TEMPLATE_DIR/$NEW_SLN -c Release --no-build|g" "$CI_FILE"

  # 2) Publish path
  sed -i '' "s|dotnet publish src/Project.Api/Project.Api.csproj|dotnet publish $TEMPLATE_DIR/src/Project.Api/Project.Api.csproj|g" "$CI_FILE"

  # 3) CycloneDX uses solution path
  sed -i '' "s|dotnet-CycloneDX $OLD_SLN|dotnet-CycloneDX $TEMPLATE_DIR/$NEW_SLN|g" "$CI_FILE"
  sed -i '' "s|dotnet-CycloneDX $NEW_SLN|dotnet-CycloneDX $TEMPLATE_DIR/$NEW_SLN|g" "$CI_FILE"
else
  echo "⚠️  $CI_FILE not found; skipping CI patch."
fi

# Patch CodeQL build path if present
if [ -f "$CODEQL_FILE" ]; then
  echo "-> Patching $CODEQL_FILE ..."
  sed -i '' "s|dotnet build -c Release|dotnet build $TEMPLATE_DIR/$NEW_SLN -c Release|g" "$CODEQL_FILE"
else
  echo "ℹ️  $CODEQL_FILE not found; skipping CodeQL patch."
fi

echo
echo "✅ Restructure done."
echo "Next run:"
echo "  dotnet restore $TEMPLATE_DIR/$NEW_SLN"
echo "  dotnet build   $TEMPLATE_DIR/$NEW_SLN -c Release"
echo "  dotnet test    $TEMPLATE_DIR/$NEW_SLN -c Release"
