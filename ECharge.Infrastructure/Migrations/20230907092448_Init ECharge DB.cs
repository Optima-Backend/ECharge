using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECharge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitEChargeDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmountCharged = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CableState = table.Column<int>(type: "int", nullable: true),
                    MerchantOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmountRefunded = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Pan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChargePointSession",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChargerPointId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationInMinutes = table.Column<double>(type: "float", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    PricePerHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Chargingstatus = table.Column<int>(name: "Charging status", type: "int", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChargePointName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxVoltage = table.Column<int>(type: "int", nullable: true),
                    FCMToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxAmperage = table.Column<int>(type: "int", nullable: true),
                    EnergyConsumption = table.Column<double>(type: "float", nullable: true),
                    ProviderSessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinishReason = table.Column<int>(type: "int", nullable: true),
                    ProviderStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargePointSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargePointSession_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CableStateHooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChargePointId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Connector = table.Column<int>(type: "int", nullable: false),
                    CableState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CableStateHooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CableStateHooks_ChargePointSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChargePointSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusChangedHooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChargerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Connector = table.Column<int>(type: "int", nullable: false),
                    OrderUuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinishReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusChangedHooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusChangedHooks_ChargePointSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChargePointSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CableStateHooks_SessionId",
                table: "CableStateHooks",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargePointSession_OrderId",
                table: "ChargePointSession",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusChangedHooks_SessionId",
                table: "OrderStatusChangedHooks",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CableStateHooks");

            migrationBuilder.DropTable(
                name: "OrderStatusChangedHooks");

            migrationBuilder.DropTable(
                name: "ChargePointSession");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
