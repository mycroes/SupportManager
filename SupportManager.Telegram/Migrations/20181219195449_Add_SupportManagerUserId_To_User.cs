using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportManager.Telegram.Migrations
{
    public partial class Add_SupportManagerUserId_To_User : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupportManagerUserId",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupportManagerUserId",
                table: "Users");
        }
    }
}
