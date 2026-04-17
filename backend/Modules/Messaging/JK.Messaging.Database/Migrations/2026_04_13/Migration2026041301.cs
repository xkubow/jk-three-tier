using FluentMigrator;

namespace JK.Messaging.Database.Migrations._2026_04_13;

[Migration(20260413100001)]
public class Migration2026041301 : Migration
{
    public override void Up()
    {
        Execute.EmbeddedScript("Migration2026041301_Up.sql");
    }

    public override void Down()
    {
        Delete.Table("ApiMessageTask");
    }
}
