using FluentMigrator;

namespace JK.Order.Database.Migrations;

[Migration(20260324113001)]
public class Order_Initial : Migration
{
    public override void Up()
    {
        Create.Table("Order")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Number").AsString(100).NotNullable()
            .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Status").AsString(50).NotNullable()
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAt").AsDateTimeOffset().Nullable();
    }

    public override void Down()
    {
        Delete.Table("Order");
    }
}
