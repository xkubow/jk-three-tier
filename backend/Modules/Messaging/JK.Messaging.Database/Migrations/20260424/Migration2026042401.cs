using FluentMigrator;

namespace JK.Messaging.Database.Migrations._20260424;

[Migration(20260424010001)]
public class Migration2026042401: Migration
{
    public override void Up()
    {
        Alter.Table("ApiMessageTask").AddColumn("ConsumerResults").AsCustom("jsonb").Nullable();
    }

    public override void Down()
    {
        Delete.Column("ConsumerResults").FromTable("ApiMessageTask");
    }
}