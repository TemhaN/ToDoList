using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDoList.Data;
using ToDoList.DTO;
using ToDoList.Models;

namespace ToDoList.Api
{
    public static class TasksApi
    {
        public static void MapTasksApi(this WebApplication app)
        {
            // публикация задачи
            app.MapPost("/api/tasks", async (HttpContext httpContext,
                ApplicationDbContext dbContext,
                TaskCreateDto taskDto) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (string.IsNullOrWhiteSpace(taskDto.Title))
                {
                    return Results.BadRequest("Название задачи обязательно");
                }

                var task = new TaskItem
                {
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    UserId = userId,
                    DueDate = taskDto.DueDate,
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false
                };
                
                if (taskDto.CategoryIds != null && taskDto.CategoryIds.Any())
                {
                    foreach (var categoryId in taskDto.CategoryIds)
                    {
                        var categoryType = await dbContext.GlobalCategories.AnyAsync(gc => gc.Id == categoryId)
                            ? CategoryType.Global
                            : CategoryType.User;

                        if (categoryType == CategoryType.User &&
                            !await dbContext.UserCategories.AnyAsync(uc => uc.Id == categoryId && uc.UserId == userId))
                        {
                            return Results.BadRequest($"Категория с ID {categoryId} не найдена");
                        }

                        task.TaskCategories.Add(new TaskCategory
                        {
                            CategoryType = categoryType,
                            GlobalCategoryId = categoryType == CategoryType.Global ? categoryId : null,
                            UserCategoryId = categoryType == CategoryType.User ? categoryId : null
                        });
                    }
                }

                dbContext.Tasks.Add(task);
                await dbContext.SaveChangesAsync();

                await dbContext.Entry(task)
                    .Collection(t => t.TaskCategories)
                    .Query()
                    .Include(tc => tc.GlobalCategory)
                    .Include(tc => tc.UserCategory)
                    .LoadAsync();

                var result = new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    DueDate = task.DueDate,
                    CreatedAt = task.CreatedAt,
                    CategoryName = task.TaskCategories
                        .Select(tc => tc.GlobalCategoryId != null ? tc.GlobalCategory.Name : tc.UserCategory.Name)
                        .FirstOrDefault()
                };

                return Results.Created($"/api/tasks/{task.Id}", result);
            })
            .RequireAuthorization()
            .WithName("CreateTask")
            .WithOpenApi();

            // обновление задачи
            app.MapPut("/api/tasks/{id}", async (HttpContext httpContext,
                ApplicationDbContext dbContext,
                int id,
                TaskUpdateDto taskDto) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var task = await dbContext.Tasks
                    .Include(t => t.TaskCategories)
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    return Results.NotFound("Задача не найдена");
                }

                if (!string.IsNullOrWhiteSpace(taskDto.Title))
                {
                    task.Title = taskDto.Title;
                }

                task.Description = taskDto.Description;
                task.IsCompleted = taskDto.IsCompleted;
                task.DueDate = taskDto.DueDate;

                if (taskDto.CategoryIds != null)
                {
                    task.TaskCategories.Clear();
                    foreach (var categoryId in taskDto.CategoryIds)
                    {
                        var categoryType = await dbContext.GlobalCategories.AnyAsync(gc => gc.Id == categoryId)
                            ? CategoryType.Global
                            : CategoryType.User;

                        if (categoryType == CategoryType.User &&
                            !await dbContext.UserCategories.AnyAsync(uc => uc.Id == categoryId && uc.UserId == userId))
                        {
                            return Results.BadRequest($"Категория с ID {categoryId} не найдена");
                        }

                        task.TaskCategories.Add(new TaskCategory
                        {
                            CategoryType = categoryType,
                            GlobalCategoryId = categoryType == CategoryType.Global ? categoryId : null,
                            UserCategoryId = categoryType == CategoryType.User ? categoryId : null
                        });
                    }
                }

                await dbContext.SaveChangesAsync();
                return Results.Ok("Задача обновлена");
            })
            .RequireAuthorization()
            .WithName("UpdateTask")
            .WithOpenApi();

            // удаление задачи
            app.MapDelete("/api/tasks/{id}", async (HttpContext httpContext,
                ApplicationDbContext dbContext,
                int id) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var task = await dbContext.Tasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    return Results.NotFound("Задача не найдена");
                }

                dbContext.Tasks.Remove(task);
                await dbContext.SaveChangesAsync();
                return Results.Ok("Задача удалена");
            })
            .RequireAuthorization()
            .WithName("DeleteTask")
            .WithOpenApi();

            // получение списка задач 
            app.MapGet("/api/tasks", async (HttpContext httpContext,
                ApplicationDbContext dbContext,
                int? categoryId = null,
                int page = 1,
                int pageSize = 10) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentDate = DateTime.UtcNow;

                var overdueTasks = await dbContext.Tasks
                    .Where(t => t.UserId == userId &&
                               !t.IsCompleted &&
                               t.DueDate.HasValue &&
                               t.DueDate < currentDate)
                    .ToListAsync();

                if (overdueTasks.Any())
                {
                    overdueTasks.ForEach(t => t.IsCompleted = true);
                    await dbContext.SaveChangesAsync();
                }

                var query = dbContext.Tasks
                    .Include(t => t.TaskCategories)
                        .ThenInclude(tc => tc.GlobalCategory)
                    .Include(t => t.TaskCategories)
                        .ThenInclude(tc => tc.UserCategory)
                    .Where(t => t.UserId == userId);

                if (categoryId.HasValue)
                {
                    query = query.Where(t =>
                        t.TaskCategories.Any(tc =>
                            (tc.GlobalCategoryId == categoryId && tc.CategoryType == CategoryType.Global) ||
                            (tc.UserCategoryId == categoryId && tc.CategoryType == CategoryType.User)));
                }

                var totalCount = await query.CountAsync();
                var tasks = await query
                    .OrderBy(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        IsCompleted = t.IsCompleted,
                        DueDate = t.DueDate,
                        CreatedAt = t.CreatedAt,
                        CategoryName = t.TaskCategories
                            .Where(tc => tc.GlobalCategoryId != null || tc.UserCategoryId != null)
                            .Select(tc =>
                                tc.GlobalCategoryId != null
                                    ? tc.GlobalCategory.Name
                                    : tc.UserCategory.Name)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Results.Ok(new PagedResult<TaskDto>
                {
                    Items = tasks,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            })
            .RequireAuthorization()
            .WithName("GetUserTasks")
            .WithOpenApi();
        }
    }
}