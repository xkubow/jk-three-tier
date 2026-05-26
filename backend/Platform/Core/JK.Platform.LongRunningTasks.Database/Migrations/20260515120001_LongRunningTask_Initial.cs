using FluentMigrator;

namespace JK.Platform.LongRunningTasks.Database.Migrations;

[Migration(20260515120001)]
public class LongRunningTask_Initial : Migration
{
    public override void Up()
    {
        Create.Table("LongRunningTask")
            .WithColumn("Id").AsString(36).PrimaryKey()
            .WithColumn("TaskName").AsString(200).NotNullable()
            .WithColumn("PayloadJson").AsString(int.MaxValue).Nullable()
            .WithColumn("Status").AsString(50).NotNullable()
            .WithColumn("AttemptCount").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("MaxAttempts").AsInt32().NotNullable().WithDefaultValue(3)
            .WithColumn("ErrorMessage").AsString(int.MaxValue).Nullable()
            .WithColumn("CreatedAtUtc").AsDateTime().NotNullable()
            .WithColumn("StartedAtUtc").AsDateTime().Nullable()
            .WithColumn("CompletedAtUtc").AsDateTime().Nullable()
            .WithColumn("NextRunAtUtc").AsDateTime().Nullable()
            .WithColumn("LockedBy").AsString(256).Nullable()
            .WithColumn("LockedAtUtc").AsDateTime().Nullable();

        Create.Index("IX_LongRunningTask_Status")
            .OnTable("LongRunningTask")
            .OnColumn("Status");

        Create.Index("IX_LongRunningTask_NextRunAtUtc")
            .OnTable("LongRunningTask")
            .OnColumn("NextRunAtUtc");

        Create.Index("IX_LongRunningTask_LockedAtUtc")
            .OnTable("LongRunningTask")
            .OnColumn("LockedAtUtc");
    }

    public override void Down()
    {
        Delete.Table("LongRunningTask");
    }
}
