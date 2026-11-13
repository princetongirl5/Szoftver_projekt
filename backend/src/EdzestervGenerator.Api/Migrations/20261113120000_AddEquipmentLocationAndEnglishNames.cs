using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdzestervGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentLocationAndEnglishNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Exercises",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Location",
                table: "Equipment",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Equipment",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 0, "Dumbbell" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 0, "Kettlebell" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 0, "Pull-up bar" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 0, "Resistance band" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 0, "Mat" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 0, "Bench" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 0, "Jump rope" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 1, "Barbell" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 1, "EZ bar" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 1, "Incline bench" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 1, "Cable machine" });

            migrationBuilder.UpdateData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Location", "NameEn" },
                values: new object[] { 1, "Exercise ball" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Equipment");
        }
    }
}
