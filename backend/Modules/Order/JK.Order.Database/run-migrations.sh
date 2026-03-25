#!/bin/bash

# Configuration for Database Migration
CONNECTION_STRING="Host=172.27.76.33;Port=30432;Database=jk_order"
CLI_PROJECT="../../../Tools/JK.Migrations.Cli"
DB_PROJECT="JK.Order.Database.csproj"
ASSEMBLY_PATH="bin/Debug/net9.0/JK.Order.Database.dll"

echo "Building Migration CLI..."
dotnet build "$CLI_PROJECT" -c Local-Dev

echo "Building Database Project..."
dotnet build "$DB_PROJECT" -c Local-Dev

echo "Running Migrations..."
dotnet run --project "$CLI_PROJECT" --connection "$CONNECTION_STRING" --assembly "$ASSEMBLY_PATH"

echo "Done."
