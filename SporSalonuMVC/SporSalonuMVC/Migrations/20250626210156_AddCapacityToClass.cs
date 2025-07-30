using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCapacityToClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Classes");
        }
    }
}
