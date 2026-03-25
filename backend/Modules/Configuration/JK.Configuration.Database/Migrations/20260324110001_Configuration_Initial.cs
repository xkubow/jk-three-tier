using FluentMigrator;

namespace JK.Configuration.Database.Migrations;

[Migration(20260324110001)]
public class Configuration_Initial : Migration
{
    public override void Up()
    {
        Create.Table("Configuration")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("MarketCode").AsString(50).Nullable()
            .WithColumn("ServiceCode").AsString(100).Nullable()
            .WithColumn("Key").AsString(500).NotNullable()
            .WithColumn("Value").AsCustom("text").NotNullable()
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAt").AsDateTimeOffset().Nullable()
            .WithColumn("CreatedBy").AsString(200).Nullable()
            .WithColumn("UpdatedBy").AsString(200).Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTimeOffset().Nullable();

        Execute.Sql(
            $"""
             CREATE INDEX "IX_Configuration_MarketCode_ServiceCode_Key"
             ON "Configuration" ("MarketCode", "ServiceCode", "Key")
             WHERE "IsDeleted" = false;
             """);
    }

    public override void Down()
    {
        Execute.Sql($"DROP INDEX IF EXISTS \"IX_Configuration_MarketCode_ServiceCode_Key\";");
        Delete.Table("Configuration");
    }
}
