using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ToDoList.Migrations
{
    /// <inheritdoc />
    public partial class fixtaskcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskCategories",
                table: "TaskCategories");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "TaskCategories",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "TaskCategories",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskCategories",
                table: "TaskCategories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategories_TaskId",
                table: "TaskCategories",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskCategories",
                table: "TaskCategories");

            migrationBuilder.DropIndex(
                name: "IX_TaskCategories_TaskId",
                table: "TaskCategories");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TaskCategories",
                newName: "CategoryId");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "TaskCategories",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskCategories",
                table: "TaskCategories",
                columns: new[] { "TaskId", "CategoryId", "CategoryType" });
        }
    }
}
