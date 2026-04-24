using FluentMigrator;

namespace JK.Offer.Database.Migrations;

[Migration(20260420131501)]
public class Offer_Initial : Migration
{
    public override void Up()
    {
        Create.Table("Offer")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Number").AsString(100).NotNullable()
            .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Status").AsString(50).NotNullable()
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAt").AsDateTimeOffset().Nullable()
            .WithColumn("ExpiresAt").AsDateTimeOffset().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Offer");
    }
}
