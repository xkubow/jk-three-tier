using System.CommandLine;
using System.Runtime.Loader;
using AdysTech.CredentialManager;
using JK.Platform.Database.Migrations;
using JK.Migrations.Cli.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

var connectionOption = new Option<string>(
    name: "--connection",
    description: "The connection string to the database.")
{
    IsRequired = true
};

var assemblyOption = new Option<FileInfo>(
    name: "--assembly",
    description: "The path to the assembly containing the migrations.")
{
    IsRequired = true
};

var ensureDbOption = new Option<bool>(
    name: "--ensure-db",
    description: "Creates the target database if it does not exist.");

var rootCommand = new RootCommand("JK Migration CLI tool");
rootCommand.AddOption(connectionOption);
rootCommand.AddOption(assemblyOption);
rootCommand.AddOption(ensureDbOption);

rootCommand.SetHandler((connection, assemblyFile, ensureDb) =>
{
    if (!assemblyFile.Exists)
    {
        Console.WriteLine($"Error: Assembly file not found at {assemblyFile.FullName}");
        Environment.ExitCode = 1;
        return;
    }

    try
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connection);

        try
        {
            var credential = CredentialManager.GetCredentials("jk-db-password");
            if (credential != null)
            {
                Console.WriteLine("Using credentials from Windows Credential Manager: jk-db-password");
                connectionStringBuilder.Username = credential.UserName;
                connectionStringBuilder.Password = credential.Password;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not retrieve credentials from Windows Credential Manager (jk-db-password): {ex.Message}");
            Console.WriteLine("Falling back to credentials in connection string.");
        }

        var finalConnectionString = connectionStringBuilder.ConnectionString;

        if (ensureDb)
        {
            DbCreator.EnsureDbIsCreated(finalConnectionString);
        }

        var loadContext = new AssemblyLoadContext("MigrationsContext", isCollectible: true);

        try
        {
            var assembly = loadContext.LoadFromAssemblyPath(assemblyFile.FullName);

            var services = new ServiceCollection();
            services.AddLogging(lb => lb.AddConsole());
            services.AddBackendMigrations(finalConnectionString, assembly);

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.RunBackendMigrations();
        }
        finally
        {
            loadContext.Unload();
        }

        Console.WriteLine("Migrations executed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error executing migrations: {ex.Message}");

        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Error: {ex.InnerException.Message}");

            if (ex.InnerException.InnerException != null)
            {
                Console.WriteLine($"Root Error: {ex.InnerException.InnerException.Message}");
            }
        }

        Environment.ExitCode = 1;
    }
}, connectionOption, assemblyOption, ensureDbOption);

return await rootCommand.InvokeAsync(args);