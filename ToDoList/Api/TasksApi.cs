using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
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

                if (string.IsNullOrWhiteSpace(taskDto.Description))
                {
                    return Results.BadRequest("Описание задачи обязательно");
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

                if (taskDto.Categories != null && taskDto.Categories.Any())
                {
                    foreach (var category in taskDto.Categories)
                    {
                        bool isValid;
                        if (category.IsGlobal)
                        {
                            isValid = await dbContext.GlobalCategories.AnyAsync(gc => gc.Id == category.Id);
                        }
                        else
                        {
                            isValid = await dbContext.UserCategories.AnyAsync(uc => uc.Id == category.Id && uc.UserId == userId);
                        }

                        if (!isValid)
                        {
                            return Results.BadRequest($"Категория с ID {category.Id} не найдена");
                        }

                        task.TaskCategories.Add(new TaskCategory
                        {
                            CategoryType = category.IsGlobal ? CategoryType.Global : CategoryType.User,
                            GlobalCategoryId = category.IsGlobal ? category.Id : null,
                            UserCategoryId = category.IsGlobal ? null : category.Id
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
                    Categories = task.TaskCategories
                        .Select(tc => new CategoryDto
                        {
                            Id = tc.GlobalCategoryId ?? tc.UserCategoryId ?? 0,
                            Name = tc.GlobalCategoryId != null ? tc.GlobalCategory.Name : tc.UserCategory.Name,
                            IsGlobal = tc.CategoryType == CategoryType.Global
                        })
                        .Where(c => c.Id != 0 && c.Name != null)
                        .ToList()
                };

                return Results.Created($"/api/tasks/{task.Id}", result);
            })
            .RequireAuthorization()
            .WithName("CreateTask")
            .WithOpenApi();

            // получение задачи по ID
            app.MapGet("/api/tasks/{id}", async (HttpContext httpContext,
                ApplicationDbContext dbContext,
                int id) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var task = await dbContext.Tasks
                    .Include(t => t.TaskCategories)
                    .ThenInclude(tc => tc.GlobalCategory)
                    .Include(t => t.TaskCategories)
                    .ThenInclude(tc => tc.UserCategory)
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    return Results.NotFound("Задача не найдена");
                }

                var result = new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    DueDate = task.DueDate,
                    CreatedAt = task.CreatedAt,
                    Categories = task.TaskCategories
                        .Select(tc => new CategoryDto
                        {
                            Id = tc.GlobalCategoryId ?? tc.UserCategoryId ?? 0,
                            Name = tc.GlobalCategoryId != null ? tc.GlobalCategory.Name : tc.UserCategory.Name,
                            IsGlobal = tc.CategoryType == CategoryType.Global
                        })
                        .Where(c => c.Id != 0 && c.Name != null)
                        .ToList()
                };

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetTaskById")
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

                if (taskDto.Description == null)
                {
                    return Results.BadRequest("Описание задачи обязательно");
                }
                task.Description = taskDto.Description;

                task.IsCompleted = taskDto.IsCompleted;
                task.DueDate = taskDto.DueDate;

                if (taskDto.Categories != null)
                {
                    task.TaskCategories.Clear();
                    foreach (var category in taskDto.Categories)
                    {
                        bool isValid;
                        if (category.IsGlobal)
                        {
                            isValid = await dbContext.GlobalCategories.AnyAsync(gc => gc.Id == category.Id);
                        }
                        else
                        {
                            isValid = await dbContext.UserCategories.AnyAsync(uc => uc.Id == category.Id && uc.UserId == userId);
                        }

                        if (!isValid)
                        {
                            return Results.BadRequest($"Категория с ID {category.Id} не найдена");
                        }

                        task.TaskCategories.Add(new TaskCategory
                        {
                            CategoryType = category.IsGlobal ? CategoryType.Global : CategoryType.User,
                            GlobalCategoryId = category.IsGlobal ? category.Id : null,
                            UserCategoryId = category.IsGlobal ? null : category.Id
                        });
                    }
                }

                await dbContext.SaveChangesAsync();
                return Results.Ok("Задача обновлена");
            })
            .RequireAuthorization()
            .WithName("UpdateTask")
            .WithOpenApi();

            // отмечает задачу выполненной
            app.MapPatch("/api/tasks/{id}/complete", async (HttpContext httpContext,
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

                task.IsCompleted = true;
                await dbContext.SaveChangesAsync();
                return Results.Ok("Задача отмечена как выполненная");
            })
            .RequireAuthorization()
            .WithName("CompleteTask")
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
                bool? isCompleted = null,
                string? searchQuery = null,
                string? sortBy = null,
                string? sortOrder = "asc",
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

                if (isCompleted.HasValue)
                {
                    query = query.Where(t => t.IsCompleted == isCompleted.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    query = query.Where(t =>
                        t.Title.ToLower().Contains(searchQuery.ToLower()) ||
                        (t.Description != null && t.Description.ToLower().Contains(searchQuery.ToLower())));
                }

                query = sortBy?.ToLower() switch
                {
                    "title" => sortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.Title)
                        : query.OrderBy(t => t.Title),
                    "duedate" => sortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.DueDate)
                        : query.OrderBy(t => t.DueDate),
                    "createdat" => sortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.CreatedAt)
                        : query.OrderBy(t => t.CreatedAt),
                    "iscompleted" => sortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.IsCompleted)
                        : query.OrderBy(t => t.IsCompleted),
                    _ => query.OrderBy(t => t.CreatedAt)
                };

                var totalCount = await query.CountAsync();
                var tasks = await query
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
                        Categories = t.TaskCategories
                            .Select(tc => new CategoryDto
                            {
                                Id = tc.GlobalCategoryId ?? tc.UserCategoryId ?? 0,
                                Name = tc.GlobalCategoryId != null ? tc.GlobalCategory.Name : tc.UserCategory.Name,
                                IsGlobal = tc.CategoryType == CategoryType.Global
                            })
                            .Where(c => c.Id != 0 && c.Name != null)
                            .ToList()
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