using FluentMigrator;

namespace JK.Messaging.Database.Migrations;

[Migration(20260407120001)]
public class Messaging_Initial : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("20260407120001_Messaging_Initial_Up.sql");
    }

    public override void Down()
    {
        Execute.EmbeddedScript("20260407120001_Messaging_Initial_Down.sql");
    }
}
