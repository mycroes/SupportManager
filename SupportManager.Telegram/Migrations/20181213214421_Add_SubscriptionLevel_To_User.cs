using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportManager.Telegram.Migrations
{
    public partial class Add_SubscriptionLevel_To_User : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubscriptionLevel",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionLevel",
                table: "Users");
        }
    }
}
