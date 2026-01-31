using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GS1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSSCCCodeLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SSCCCode",
                table: "SSCCs",
                type: "nvarchar(18)",
                maxLength: 18,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(18)",
                oldFixedLength: true,
                oldMaxLength: 18);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SSCCCode",
                table: "SSCCs",
                type: "nchar(18)",
                fixedLength: true,
                maxLength: 18,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(18)",
                oldMaxLength: 18);
        }
    }
}
