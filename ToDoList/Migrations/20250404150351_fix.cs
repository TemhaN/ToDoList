using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ToDoList.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TaskCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "CreatedAt", "Description", "DueDate", "IsCompleted", "Title", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Подготовить месячный отчет для руководства", new DateTime(2025, 4, 7, 0, 0, 0, 0, DateTimeKind.Utc), false, "Завершить отчет", 1 },
                    { 2, new DateTime(2025, 4, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Закончить онлайн-курс по C#", new DateTime(2025, 4, 11, 0, 0, 0, 0, DateTimeKind.Utc), false, "Пройти курс", 1 },
                    { 3, new DateTime(2025, 4, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Сходить в магазин за продуктами на неделю", new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Utc), false, "Купить продукты", 1 }
                });

            migrationBuilder.InsertData(
                table: "TaskCategories",
                columns: new[] { "Id", "CategoryType", "GlobalCategoryId", "TaskId", "UserCategoryId" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, null },
                    { 2, 1, 2, 2, null },
                    { 3, 1, 3, 3, null }
                });
        }
    }
}
