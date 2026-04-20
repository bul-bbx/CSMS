using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSMS.Migrations
{
    /// <inheritdoc />
    public partial class M2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceAtTime",
                table: "RepairParts",
                newName: "TotalPrice");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RepairParts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "RepairParts");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "RepairParts",
                newName: "PriceAtTime");
        }
    }
}
