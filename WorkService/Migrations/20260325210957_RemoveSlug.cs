using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Technologies_Slug",
                table: "Technologies");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Technologies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Technologies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_Slug",
                table: "Technologies",
                column: "Slug",
                unique: true);
        }
    }
}
