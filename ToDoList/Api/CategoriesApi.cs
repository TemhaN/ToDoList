using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDoList.Data;
using ToDoList.DTO;
using ToDoList.Models;

namespace ToDoList.Api
{
    public static class CategoriesApi
    {
        public static void MapCategoriesApi(this WebApplication app)
        {
            // получает все глобальные категории
            app.MapGet("/api/categories/global", async (ApplicationDbContext dbContext) =>
            {
                var categories = await dbContext.GlobalCategories
                    .Select(gc => new CategoryDto
                    {
                        Id = gc.Id,
                        Name = gc.Name,
                        IsGlobal = true
                    })
                    .ToListAsync();

                return Results.Ok(categories);
            })
            .RequireAuthorization()
            .WithName("GetGlobalCategories")
            .WithOpenApi();

            // получает все категории пользователя
            app.MapGet("/api/categories/user", async (HttpContext httpContext, ApplicationDbContext dbContext) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var categories = await dbContext.UserCategories
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => new CategoryDto
                    {
                        Id = uc.Id,
                        Name = uc.Name,
                        IsGlobal = false
                    })
                    .ToListAsync();

                return Results.Ok(categories);
            })
            .RequireAuthorization()
            .WithName("GetUserCategories")
            .WithOpenApi();

            // публикует категорию пользователя
            app.MapPost("/api/categories/user", async (HttpContext httpContext,
                ApplicationDbContext dbContext,
                CategoryCreateDto categoryDto) =>
            {
                if (string.IsNullOrWhiteSpace(categoryDto.Name))
                {
                    return Results.BadRequest("Введите имя категории");
                }

                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (await dbContext.UserCategories.AnyAsync(uc =>
                    uc.UserId == userId &&
                    uc.Name.ToLower() == categoryDto.Name.ToLower()))
                {
                    return Results.BadRequest("Категория с таким именем уже существует");
                }

                var category = new UserCategory
                {
                    Name = categoryDto.Name,
                    UserId = userId
                };

                dbContext.UserCategories.Add(category);
                await dbContext.SaveChangesAsync();

                var result = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    IsGlobal = false
                };

                return Results.Created($"/api/categories/user/{category.Id}", result);
            })
            .RequireAuthorization()
            .WithName("CreateUserCategory")
            .WithOpenApi();

            // удаляет категорию пользователя
            app.MapDelete("/api/categories/user/{id}", async (HttpContext httpContext,
                ApplicationDbContext dbContext,
                int id) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var category = await dbContext.UserCategories
                    .FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == userId);

                if (category == null)
                {
                    return Results.NotFound("Категория не найдена");
                }

                var isInUse = await dbContext.TaskCategories
                    .AnyAsync(tc => tc.UserCategoryId == id);

                if (isInUse)
                {
                    return Results.BadRequest("Нельзя удалить категорию, у которой есть задачи");
                }

                dbContext.UserCategories.Remove(category);
                await dbContext.SaveChangesAsync();

                return Results.Ok("Категория удалена");
            })
            .RequireAuthorization()
            .WithName("DeleteUserCategory")
            .WithOpenApi();

            // обновляет категорию пользователя
            app.MapPut("/api/categories/user/{id}", async (HttpContext httpContext,
                ApplicationDbContext dbContext,
                int id,
                CategoryUpdateDto categoryDto) =>
            {
                if (string.IsNullOrWhiteSpace(categoryDto.Name))
                {
                    return Results.BadRequest("Введите новое имя категории");
                }

                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var category = await dbContext.UserCategories
                    .FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == userId);

                if (category == null)
                {
                    return Results.NotFound("Категория не найдена");
                }

                if (await dbContext.UserCategories.AnyAsync(uc =>
                    uc.UserId == userId &&
                    uc.Name.ToLower() == categoryDto.Name.ToLower() &&
                    uc.Id != id))
                {
                    return Results.BadRequest("Категория с таким именем уже существует");
                }

                category.Name = categoryDto.Name;
                await dbContext.SaveChangesAsync();

                return Results.Ok("Категория обновлена");
            })
            .RequireAuthorization()
            .WithName("UpdateUserCategory")
            .WithOpenApi();

            // пока мне не нужен
            //app.MapGet("/api/categories/user/search", async (HttpContext httpContext,
            //    ApplicationDbContext dbContext,
            //    string name) =>
            //{
            //    if (string.IsNullOrWhiteSpace(name))
            //    {
            //        return Results.BadRequest("Введите имя для поиска");
            //    }

            //    var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            //    var categories = await dbContext.UserCategories
            //        .Where(uc => uc.UserId == userId && uc.Name.Contains(name))
            //        .Select(uc => new CategoryDto
            //        {
            //            Id = uc.Id,
            //            Name = uc.Name,
            //            IsGlobal = false
            //        })
            //        .ToListAsync();

            //    return Results.Ok(categories);
            //})
            //.RequireAuthorization()
            //.WithName("SearchUserCategories")
            //.WithOpenApi();

        }
    }
}