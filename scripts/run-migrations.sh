#!/bin/bash
set -e

if [ $# -lt 3 ]; then
  echo "Usage: $0 <connection-string> <db-project> <assembly-path>"
  exit 1
fi

CONNECTION_STRING="$1"
DB_PROJECT="$2"
ASSEMBLY_PATH="$3"
CLI_PROJECT="../backend/Tools/JK.Migrations.Cli"

echo "Building Migration CLI..."
dotnet build "$CLI_PROJECT" -c Local-Dev

echo "Building Database Project..."
dotnet build "$DB_PROJECT" -c Local-Dev

echo "Ensuring database exists and running migrations..."
dotnet run --project "$CLI_PROJECT" -- \
  --connection "$CONNECTION_STRING" \
  --assembly "$ASSEMBLY_PATH" \
  --ensure-db

echo "Done."