using FluentMigrator;

namespace JK.Messaging.Database.Migrations._20260506;

[Migration(20260506010001)]
public class Migration2026050601 : Migration
{
    public override void Up()
    {
        Create.Table("ApiMessageRecurringTask")
            .WithColumn("Id").AsString(200).PrimaryKey()
            .WithColumn("TaskName").AsString(200).NotNullable()
            .WithColumn("CronExpression").AsString(200).NotNullable()
            .WithColumn("IsEnabled").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("LastRunAtUtc").AsDateTimeOffset().Nullable()
            .WithColumn("NextRunAtUtc").AsDateTimeOffset().Nullable()
            .WithColumn("CreatedAtUtc").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAtUtc").AsDateTimeOffset().Nullable();

        Create.Index("IX_ApiMessageRecurringTask_NextRunAtUtc")
            .OnTable("ApiMessageRecurringTask")
            .OnColumn("NextRunAtUtc");
    }

    public override void Down()
    {
        Delete.Table("ApiMessageRecurringTask");
    }
}
