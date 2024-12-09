using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Bookmark3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookmarks_AspNetUsers_UserId1",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId1",
                table: "Bookmarks");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Bookmarks");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Bookmarks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId",
                table: "Bookmarks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmarks_AspNetUsers_UserId",
                table: "Bookmarks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookmarks_AspNetUsers_UserId",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId",
                table: "Bookmarks");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Bookmarks",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Bookmarks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId1",
                table: "Bookmarks",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmarks_AspNetUsers_UserId1",
                table: "Bookmarks",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
