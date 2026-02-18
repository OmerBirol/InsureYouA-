using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsureYouAı.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorNameToArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Descripiton",
                table: "Abouts",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "Articles");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Abouts",
                newName: "Descripiton");
        }
    }
}
