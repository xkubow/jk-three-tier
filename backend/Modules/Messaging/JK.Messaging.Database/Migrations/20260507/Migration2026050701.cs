using FluentMigrator;

namespace JK.Messaging.Database.Migrations._20260507;

[Migration(20260507010001)]
public class Migration2026050701: Migration
{
    public override void Up()
    {
        Delete.Column("TargetUrl").FromTable("ApiMessageTask");
    }

    public override void Down()
    {
        Alter.Table("ApiMessageTask").AddColumn("TargetUrl").AsString(2000).NotNullable().WithDefaultValue(string.Empty);
    }
}
