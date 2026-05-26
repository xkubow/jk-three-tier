using FluentMigrator;

namespace JK.Platform.LongRunningTasks.Database.Migrations;

[Migration(20260515120002)]
public class LongRunningTask_ParentChild : Migration
{
    public override void Up()
    {
        Alter.Table("LongRunningTask")
            .AddColumn("ParentTaskId").AsString(36).Nullable()
            .AddColumn("TotalItems").AsInt64().Nullable()
            .AddColumn("ProcessedItems").AsInt64().NotNullable().WithDefaultValue(0)
            .AddColumn("FailedItems").AsInt64().NotNullable().WithDefaultValue(0)
            .AddColumn("ProgressPercent").AsDecimal(9, 4).Nullable()
            .AddColumn("ChunkNumber").AsInt32().Nullable()
            .AddColumn("ChunkSize").AsInt32().Nullable()
            .AddColumn("ExternalCursor").AsString(512).Nullable()
            .AddColumn("CorrelationId").AsString(128).Nullable();

        Create.Index("IX_LongRunningTask_ParentTaskId")
            .OnTable("LongRunningTask")
            .OnColumn("ParentTaskId");

        Create.Index("IX_LongRunningTask_Status_NextRunAtUtc")
            .OnTable("LongRunningTask")
            .OnColumn("Status").Ascending()
            .OnColumn("NextRunAtUtc").Ascending();

        Create.Index("IX_LongRunningTask_TaskName")
            .OnTable("LongRunningTask")
            .OnColumn("TaskName");

        Create.Index("IX_LongRunningTask_CorrelationId")
            .OnTable("LongRunningTask")
            .OnColumn("CorrelationId");
    }

    public override void Down()
    {
        Delete.Index("IX_LongRunningTask_CorrelationId").OnTable("LongRunningTask");
        Delete.Index("IX_LongRunningTask_TaskName").OnTable("LongRunningTask");
        Delete.Index("IX_LongRunningTask_Status_NextRunAtUtc").OnTable("LongRunningTask");
        Delete.Index("IX_LongRunningTask_ParentTaskId").OnTable("LongRunningTask");

        Delete.Column("CorrelationId").FromTable("LongRunningTask");
        Delete.Column("ExternalCursor").FromTable("LongRunningTask");
        Delete.Column("ChunkSize").FromTable("LongRunningTask");
        Delete.Column("ChunkNumber").FromTable("LongRunningTask");
        Delete.Column("ProgressPercent").FromTable("LongRunningTask");
        Delete.Column("FailedItems").FromTable("LongRunningTask");
        Delete.Column("ProcessedItems").FromTable("LongRunningTask");
        Delete.Column("TotalItems").FromTable("LongRunningTask");
        Delete.Column("ParentTaskId").FromTable("LongRunningTask");
    }
}
