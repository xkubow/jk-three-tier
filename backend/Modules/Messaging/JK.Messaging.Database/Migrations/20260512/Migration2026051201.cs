using FluentMigrator;

namespace JK.Messaging.Database.Migrations._20260512;

[Migration(20260512010001)]
public class Migration2026051201 : Migration
{
    public override void Up()
    {
        Alter.Table("ApiMessageTask")
            .AddColumn("OriginalCorrelationId")
            .AsString(128)
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("OriginalCorrelationId").FromTable("ApiMessageTask");
    }
}
