using FluentMigrator;

namespace JK.Messaging.Database.Migrations;

[Migration(20260407130002)]
public class CreateOrleansPersistence : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("20260407130002_CreateOrleansPersistence_Up.sql");
    }

    public override void Down()
    {
        Execute.EmbeddedScript("20260407130002_CreateOrleansPersistence_Down.sql");
    }
}
