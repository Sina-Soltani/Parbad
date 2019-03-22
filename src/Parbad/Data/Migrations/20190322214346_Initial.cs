using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Parbad.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "parbad");

            migrationBuilder.CreateTable(
                name: "TbPayment",
                schema: "parbad",
                columns: table => new
                {
                    PaymentID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TrackingNumber = table.Column<long>(nullable: false),
                    Token = table.Column<string>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    TransactionCode = table.Column<string>(nullable: true),
                    GatewayName = table.Column<string>(nullable: true),
                    IsCompleted = table.Column<bool>(nullable: false),
                    IsPaid = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbPayment", x => new { x.PaymentID, x.TrackingNumber, x.Token });
                });

            migrationBuilder.CreateTable(
                name: "TbTransaction",
                schema: "parbad",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    IsSucceed = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    AdditionalData = table.Column<string>(nullable: true),
                    PaymentId = table.Column<long>(nullable: false),
                    PaymentTrackingNumber = table.Column<long>(nullable: false),
                    PaymentToken = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TransactionID", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TbTransaction_TbPayment_PaymentId_PaymentTrackingNumber_PaymentToken",
                        columns: x => new { x.PaymentId, x.PaymentTrackingNumber, x.PaymentToken },
                        principalSchema: "parbad",
                        principalTable: "TbPayment",
                        principalColumns: new[] { "PaymentID", "TrackingNumber", "Token" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TbTransaction_PaymentId_PaymentTrackingNumber_PaymentToken",
                schema: "parbad",
                table: "TbTransaction",
                columns: new[] { "PaymentId", "PaymentTrackingNumber", "PaymentToken" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbTransaction",
                schema: "parbad");

            migrationBuilder.DropTable(
                name: "TbPayment",
                schema: "parbad");
        }
    }
}
