using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ToDoList.Migrations
{
    /// <inheritdoc />
    public partial class adtaskcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_GlobalCategories_GlobalCategoryId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_UserCategories_UserCategoryId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_GlobalCategoryId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserCategoryId",
                table: "Tasks");

            migrationBuilder.DeleteData(
                table: "GlobalCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "GlobalCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "GlobalCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "GlobalCategoryId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UserCategoryId",
                table: "Tasks");

            migrationBuilder.CreateTable(
                name: "TaskCategories",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CategoryType = table.Column<int>(type: "integer", nullable: false),
                    GlobalCategoryId = table.Column<int>(type: "integer", nullable: true),
                    UserCategoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategories", x => new { x.TaskId, x.CategoryId, x.CategoryType });
                    table.ForeignKey(
                        name: "FK_TaskCategories_GlobalCategories_GlobalCategoryId",
                        column: x => x.GlobalCategoryId,
                        principalTable: "GlobalCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskCategories_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskCategories_UserCategories_UserCategoryId",
                        column: x => x.UserCategoryId,
                        principalTable: "UserCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategories_GlobalCategoryId",
                table: "TaskCategories",
                column: "GlobalCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCategories_UserCategoryId",
                table: "TaskCategories",
                column: "UserCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskCategories");

            migrationBuilder.AddColumn<int>(
                name: "GlobalCategoryId",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserCategoryId",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                table: "GlobalCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Работа" },
                    { 2, "Учеба" },
                    { 3, "Дом и быт" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_GlobalCategoryId",
                table: "Tasks",
                column: "GlobalCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserCategoryId",
                table: "Tasks",
                column: "UserCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_GlobalCategories_GlobalCategoryId",
                table: "Tasks",
                column: "GlobalCategoryId",
                principalTable: "GlobalCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_UserCategories_UserCategoryId",
                table: "Tasks",
                column: "UserCategoryId",
                principalTable: "UserCategories",
                principalColumn: "Id");
        }
    }
}
