using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTTelemetryLab.Migrations
{
    /// <inheritdoc />
    public partial class AddAxisField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Axis",
                table: "Records",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Axis",
                table: "Records");
        }
    }
}
