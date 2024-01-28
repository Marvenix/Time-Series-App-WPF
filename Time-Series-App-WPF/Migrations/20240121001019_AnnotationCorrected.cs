using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Time_Series_App_WPF.Migrations
{
    /// <inheritdoc />
    public partial class AnnotationCorrected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Annotations",
                type: "TEXT",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "Color",
                table: "Annotations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 9);
        }
    }
}
