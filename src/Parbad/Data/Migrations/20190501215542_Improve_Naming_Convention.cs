using Microsoft.EntityFrameworkCore.Migrations;

namespace Parbad.Data.Migrations
{
    public partial class Improve_Naming_Convention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TbTransaction_TbPayment_PaymentId_PaymentTrackingNumber_PaymentToken",
                schema: "parbad",
                table: "TbTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "TransactionID",
                schema: "parbad",
                table: "TbTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TbTransaction_PaymentId_PaymentTrackingNumber_PaymentToken",
                schema: "parbad",
                table: "TbTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TbPayment",
                schema: "parbad",
                table: "TbPayment");

            migrationBuilder.DropColumn(
                name: "PaymentToken",
                schema: "parbad",
                table: "TbTransaction");

            migrationBuilder.DropColumn(
                name: "PaymentTrackingNumber",
                schema: "parbad",
                table: "TbTransaction");

            migrationBuilder.EnsureSchema(
                name: "Parbad");

            migrationBuilder.RenameTable(
                name: "TbTransaction",
                schema: "parbad",
                newName: "transaction",
                newSchema: "Parbad");

            migrationBuilder.RenameTable(
                name: "TbPayment",
                schema: "parbad",
                newName: "payment",
                newSchema: "Parbad");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "Parbad",
                table: "transaction",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Message",
                schema: "Parbad",
                table: "transaction",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Amount",
                schema: "Parbad",
                table: "transaction",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                schema: "Parbad",
                table: "transaction",
                newName: "updated_on");

            migrationBuilder.RenameColumn(
                name: "IsSucceed",
                schema: "Parbad",
                table: "transaction",
                newName: "is_succeed");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                schema: "Parbad",
                table: "transaction",
                newName: "created_on");

            migrationBuilder.RenameColumn(
                name: "AdditionalData",
                schema: "Parbad",
                table: "transaction",
                newName: "additional_data");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "Parbad",
                table: "transaction",
                newName: "transaction_id");

            migrationBuilder.RenameColumn(
                name: "Amount",
                schema: "Parbad",
                table: "payment",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Token",
                schema: "Parbad",
                table: "payment",
                newName: "token");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                schema: "Parbad",
                table: "payment",
                newName: "updated_on");

            migrationBuilder.RenameColumn(
                name: "TransactionCode",
                schema: "Parbad",
                table: "payment",
                newName: "transaction_code");

            migrationBuilder.RenameColumn(
                name: "IsPaid",
                schema: "Parbad",
                table: "payment",
                newName: "is_paid");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                schema: "Parbad",
                table: "payment",
                newName: "is_completed");

            migrationBuilder.RenameColumn(
                name: "GatewayName",
                schema: "Parbad",
                table: "payment",
                newName: "gateway_name");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                schema: "Parbad",
                table: "payment",
                newName: "created_on");

            migrationBuilder.RenameColumn(
                name: "TrackingNumber",
                schema: "Parbad",
                table: "payment",
                newName: "tracking_number");

            migrationBuilder.RenameColumn(
                name: "PaymentID",
                schema: "Parbad",
                table: "payment",
                newName: "payment_id");

            migrationBuilder.AlterColumn<string>(
                name: "gateway_name",
                schema: "Parbad",
                table: "payment",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "transaction_id",
                schema: "Parbad",
                table: "transaction",
                column: "transaction_id");

            migrationBuilder.AddPrimaryKey(
                name: "payment_id",
                schema: "Parbad",
                table: "payment",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_PaymentId",
                schema: "Parbad",
                table: "transaction",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_token",
                schema: "Parbad",
                table: "payment",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_tracking_number",
                schema: "Parbad",
                table: "payment",
                column: "tracking_number",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_payment_PaymentId",
                schema: "Parbad",
                table: "transaction",
                column: "PaymentId",
                principalSchema: "Parbad",
                principalTable: "payment",
                principalColumn: "payment_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transaction_payment_PaymentId",
                schema: "Parbad",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "transaction_id",
                schema: "Parbad",
                table: "transaction");

            migrationBuilder.DropIndex(
                name: "IX_transaction_PaymentId",
                schema: "Parbad",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "payment_id",
                schema: "Parbad",
                table: "payment");

            migrationBuilder.DropIndex(
                name: "IX_payment_token",
                schema: "Parbad",
                table: "payment");

            migrationBuilder.DropIndex(
                name: "IX_payment_tracking_number",
                schema: "Parbad",
                table: "payment");

            migrationBuilder.EnsureSchema(
                name: "parbad");

            migrationBuilder.RenameTable(
                name: "transaction",
                schema: "Parbad",
                newName: "TbTransaction",
                newSchema: "parbad");

            migrationBuilder.RenameTable(
                name: "payment",
                schema: "Parbad",
                newName: "TbPayment",
                newSchema: "parbad");

            migrationBuilder.RenameColumn(
                name: "type",
                schema: "parbad",
                table: "TbTransaction",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "message",
                schema: "parbad",
                table: "TbTransaction",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "amount",
                schema: "parbad",
                table: "TbTransaction",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "updated_on",
                schema: "parbad",
                table: "TbTransaction",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "is_succeed",
                schema: "parbad",
                table: "TbTransaction",
                newName: "IsSucceed");

            migrationBuilder.RenameColumn(
                name: "created_on",
                schema: "parbad",
                table: "TbTransaction",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "additional_data",
                schema: "parbad",
                table: "TbTransaction",
                newName: "AdditionalData");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                schema: "parbad",
                table: "TbTransaction",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "token",
                schema: "parbad",
                table: "TbPayment",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "amount",
                schema: "parbad",
                table: "TbPayment",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "updated_on",
                schema: "parbad",
                table: "TbPayment",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "transaction_code",
                schema: "parbad",
                table: "TbPayment",
                newName: "TransactionCode");

            migrationBuilder.RenameColumn(
                name: "tracking_number",
                schema: "parbad",
                table: "TbPayment",
                newName: "TrackingNumber");

            migrationBuilder.RenameColumn(
                name: "is_paid",
                schema: "parbad",
                table: "TbPayment",
                newName: "IsPaid");

            migrationBuilder.RenameColumn(
                name: "is_completed",
                schema: "parbad",
                table: "TbPayment",
                newName: "IsCompleted");

            migrationBuilder.RenameColumn(
                name: "gateway_name",
                schema: "parbad",
                table: "TbPayment",
                newName: "GatewayName");

            migrationBuilder.RenameColumn(
                name: "created_on",
                schema: "parbad",
                table: "TbPayment",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "payment_id",
                schema: "parbad",
                table: "TbPayment",
                newName: "PaymentID");

            migrationBuilder.AddColumn<string>(
                name: "PaymentToken",
                schema: "parbad",
                table: "TbTransaction",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "PaymentTrackingNumber",
                schema: "parbad",
                table: "TbTransaction",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "GatewayName",
                schema: "parbad",
                table: "TbPayment",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20);

            migrationBuilder.AddPrimaryKey(
                name: "TransactionID",
                schema: "parbad",
                table: "TbTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TbPayment",
                schema: "parbad",
                table: "TbPayment",
                columns: new[] { "PaymentID", "TrackingNumber", "Token" });

            migrationBuilder.CreateIndex(
                name: "IX_TbTransaction_PaymentId_PaymentTrackingNumber_PaymentToken",
                schema: "parbad",
                table: "TbTransaction",
                columns: new[] { "PaymentId", "PaymentTrackingNumber", "PaymentToken" });

            migrationBuilder.AddForeignKey(
                name: "FK_TbTransaction_TbPayment_PaymentId_PaymentTrackingNumber_PaymentToken",
                schema: "parbad",
                table: "TbTransaction",
                columns: new[] { "PaymentId", "PaymentTrackingNumber", "PaymentToken" },
                principalSchema: "parbad",
                principalTable: "TbPayment",
                principalColumns: new[] { "PaymentID", "TrackingNumber", "Token" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
