using FluentMigrator;

namespace JK.Configuration.Database.Migrations._2026;

[Migration(20260401110001)]
public class ListSupport_210401: Migration {
    public override void Up()
    {
        Alter.Table("Configuration").AddColumn("IsList").AsBoolean().NotNullable().WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete.Column("IsList").FromTable("Configuration");
    }
}