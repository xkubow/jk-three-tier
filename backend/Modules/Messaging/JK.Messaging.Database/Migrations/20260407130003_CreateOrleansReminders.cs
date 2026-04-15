using FluentMigrator;

namespace JK.Messaging.Database.Migrations;

[Migration(20260407130003)]
public class CreateOrleansReminders : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("20260407130003_CreateOrleansReminders_Up.sql");
    }

    public override void Down()
    {
        Execute.EmbeddedScript("20260407130003_CreateOrleansReminders_Down.sql");
    }
}
