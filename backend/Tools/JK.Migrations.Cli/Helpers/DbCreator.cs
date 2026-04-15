using System.Text.RegularExpressions;
using Npgsql;

namespace JK.Migrations.Cli.Helpers;

public static partial class DbCreator
{
    [GeneratedRegex("^[a-zA-Z0-9_]+$")]
    private static partial Regex ValidDatabaseNameRegex();

    public static void EnsureDbIsCreated(string connectionString)
    {
        Console.WriteLine($"connection string '{connectionString}'");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var targetBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = targetBuilder.Database;

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("Database name is missing in the connection string.");

        if (!ValidDatabaseNameRegex().IsMatch(databaseName))
            throw new InvalidOperationException(
                $"Database name '{databaseName}' contains unsupported characters.");

        var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres"
        };

        Console.WriteLine($"Checking database '{databaseName}'...");

        using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
        connection.Open();

        using var existsCommand = new NpgsqlCommand(
            "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @databaseName)",
            connection);

        existsCommand.Parameters.AddWithValue("databaseName", databaseName);

        var existsResult = existsCommand.ExecuteScalar();
        var exists = existsResult is bool value && value;

        if (exists)
        {
            Console.WriteLine($"Database '{databaseName}' already exists.");
            return;
        }

        var quotedDatabaseName = QuoteIdentifier(databaseName);

        Console.WriteLine($"Creating database '{databaseName}'...");

        using var createCommand = new NpgsqlCommand(
            $"CREATE DATABASE {quotedDatabaseName}",
            connection);

        createCommand.ExecuteNonQuery();

        Console.WriteLine($"Database '{databaseName}' created.");
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}