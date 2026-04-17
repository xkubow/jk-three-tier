using FluentMigrator;

namespace JK.Messaging.Database.Migrations;

[Migration(20260407130004)]
public class CreateOrleansClustering_20260413183701: Migration {
public override void Up()
{
    Execute.EmbeddedScript("20260413183701_CreateOrleansClustering_up.sql");
    Execute.EmbeddedScript("20260413183702_OrleansPatch_up.sql");
}

public override void Down()
{
}
}