using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECharge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_transaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "PulpalTransactions",
                newName: "PaidStatus");

            migrationBuilder.AddColumn<string>(
                name: "ChargerId",
                table: "PulpalTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateCharging",
                table: "PulpalTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StopDateCharging",
                table: "PulpalTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "TotalCostOfCharge",
                table: "PulpalTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargerId",
                table: "PulpalTransactions");

            migrationBuilder.DropColumn(
                name: "StartDateCharging",
                table: "PulpalTransactions");

            migrationBuilder.DropColumn(
                name: "StopDateCharging",
                table: "PulpalTransactions");

            migrationBuilder.DropColumn(
                name: "TotalCostOfCharge",
                table: "PulpalTransactions");

            migrationBuilder.RenameColumn(
                name: "PaidStatus",
                table: "PulpalTransactions",
                newName: "Status");
        }
    }
}
