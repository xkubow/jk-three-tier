using FluentMigrator;

namespace JK.Messaging.Database.Migrations;

[Migration(20260407130001)]
public class CreateOrleansMain : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("20260407130001_CreateOrleansMain_Up.sql");
    }

    public override void Down()
    {
        Execute.EmbeddedScript("20260407130001_CreateOrleansMain_Down.sql");
    }
}
