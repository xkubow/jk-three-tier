#!/bin/bash
set -e

# Configuration for Database Migration
CONNECTION_STRING="Host=172.27.76.33;Port=30432;Database=jk_offer"
CLI_PROJECT="../../../Tools/JK.Migrations.Cli"
DB_PROJECT="JK.Offer.Database.csproj"
ASSEMBLY_PATH="bin/Local-Dev/net9.0/JK.Offer.Database.dll"

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
