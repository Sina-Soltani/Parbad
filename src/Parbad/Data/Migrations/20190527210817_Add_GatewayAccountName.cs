using Microsoft.EntityFrameworkCore.Migrations;

namespace Parbad.Data.Migrations
{
    public partial class Add_GatewayAccountName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "gateway_account_name",
                schema: "Parbad",
                table: "payment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gateway_account_name",
                schema: "Parbad",
                table: "payment");
        }
    }
}
