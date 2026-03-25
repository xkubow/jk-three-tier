#!/bin/sh
set -e

: "${DB_PASSWORD:?DB_PASSWORD is required}"

echo "Running Configuration migrations..."
dotnet /app/cli/JK.Migrations.Cli.dll \
  --connection "Host=postgres;Port=5432;Database=jk_configuration;Username=postgres;Password=${DB_PASSWORD}" \
  --assembly "/app/modules/configuration/JK.Configuration.Database.dll" \
  --ensure-db

echo "Running Order migrations..."
dotnet /app/cli/JK.Migrations.Cli.dll \
  --connection "Host=postgres;Port=5432;Database=jk_order;Username=postgres;Password=${DB_PASSWORD}" \
  --assembly "/app/modules/order/JK.Order.Database.dll" \
  --ensure-db

echo "All migrations finished."