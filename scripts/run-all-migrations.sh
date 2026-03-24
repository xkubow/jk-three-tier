#!/usr/bin/env bash
set -euo pipefail

# Runs all backend database migrations sequentially.
# Expected usage:
#   ./run-all-migrations.sh
#   ./run-all-migrations.sh Local-Dev
#
# Required env vars:
#   DB_PASSWORD=<postgres-password>
#
# Optional env vars:
#   DB_HOST=172.27.76.33
#   DB_PORT=30432
#   DB_USERNAME=postgres
#
# Run this script from the backend repository root.

CONFIGURATION="${1:-Local-Dev}"
CLI_PROJECT="./Tools/JK.Migrations.Cli/JK.Migrations.Cli.csproj"

DB_HOST="${DB_HOST:-172.27.76.33}"
DB_PORT="${DB_PORT:-30432}"
DB_USERNAME="${DB_USERNAME:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-}"

if [[ -z "$DB_PASSWORD" ]]; then
  echo "ERROR: DB_PASSWORD environment variable is required."
  echo "Example: DB_PASSWORD=change-me ./run-all-migrations.sh"
  exit 1
fi

MODULE_NAMES=(
  "Configuration"
  "Order"
)
MODULE_DATABASES=(
  "jk_configuration"
  "jk_order"
)
MODULE_PROJECTS=(
  "./Modules/Configuration/JK.Configuration.Database/JK.Configuration.Database.csproj"
  "./Modules/Order/JK.Order.Database/JK.Order.Database.csproj"
)
MODULE_ASSEMBLIES=(
  "./Modules/Configuration/JK.Configuration.Database/bin/${CONFIGURATION}/net9.0/JK.Configuration.Database.dll"
  "./Modules/Order/JK.Order.Database/bin/${CONFIGURATION}/net9.0/JK.Order.Database.dll"
)

echo ">>> Building migration CLI: ${CLI_PROJECT}"
dotnet build "$CLI_PROJECT" -c "$CONFIGURATION"

echo
for i in "${!MODULE_NAMES[@]}"; do
  NAME="${MODULE_NAMES[$i]}"
  DATABASE="${MODULE_DATABASES[$i]}"
  PROJECT="${MODULE_PROJECTS[$i]}"
  ASSEMBLY="${MODULE_ASSEMBLIES[$i]}"
  CONNECTION_STRING="Host=${DB_HOST};Port=${DB_PORT};Database=${DATABASE};Username=${DB_USERNAME};Password=${DB_PASSWORD};"

  echo ">>> Building ${NAME} database project: ${PROJECT}"
  dotnet build "$PROJECT" -c "$CONFIGURATION"

  echo ">>> Ensuring database exists and running ${NAME} migrations on ${DATABASE}"
  dotnet run --project "$CLI_PROJECT" --configuration "$CONFIGURATION" -- \
    --connection "$CONNECTION_STRING" \
    --assembly "$ASSEMBLY" \
    --ensure-db

  echo ">>> ${NAME} migrations finished"
  echo
done

echo "All migrations finished successfully."
