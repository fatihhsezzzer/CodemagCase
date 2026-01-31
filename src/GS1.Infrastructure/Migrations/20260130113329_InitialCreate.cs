using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GS1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GLN = table.Column<string>(type: "nchar(13)", fixedLength: true, maxLength: 13, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GTIN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductionQuantity = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SerialNumberStart = table.Column<long>(type: "bigint", nullable: false),
                    LastSerialNumber = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemsPerBox = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    BoxesPerPallet = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SSCCs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SSCCCode = table.Column<string>(type: "nchar(18)", fixedLength: true, maxLength: 18, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentSSCCId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GS1DataMatrix = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSCCs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SSCCs_SSCCs_ParentSSCCId",
                        column: x => x.ParentSSCCId,
                        principalTable: "SSCCs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SSCCs_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SerialNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Serial = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GS1DataMatrix = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PrintedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SSCCId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerialNumbers_SSCCs_SSCCId",
                        column: x => x.SSCCId,
                        principalTable: "SSCCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SerialNumbers_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyName",
                table: "Customers",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GLN",
                table: "Customers",
                column: "GLN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CustomerId",
                table: "Products",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_GTIN",
                table: "Products",
                column: "GTIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductName",
                table: "Products",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_SSCCId",
                table: "SerialNumbers",
                column: "SSCCId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_Status",
                table: "SerialNumbers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_WorkOrderId",
                table: "SerialNumbers",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_WorkOrderId_Serial",
                table: "SerialNumbers",
                columns: new[] { "WorkOrderId", "Serial" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SSCCs_ParentSSCCId",
                table: "SSCCs",
                column: "ParentSSCCId");

            migrationBuilder.CreateIndex(
                name: "IX_SSCCs_SSCCCode",
                table: "SSCCs",
                column: "SSCCCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SSCCs_Type",
                table: "SSCCs",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_SSCCs_WorkOrderId",
                table: "SSCCs",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_BatchNumber",
                table: "WorkOrders",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductId",
                table: "WorkOrders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Status",
                table: "WorkOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                table: "WorkOrders",
                column: "WorkOrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerialNumbers");

            migrationBuilder.DropTable(
                name: "SSCCs");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
