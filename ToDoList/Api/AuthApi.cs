using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using ToDoList.DTO;
using ToDoList.Services;

namespace ToDoList.Api
{
    public static class AuthApi
    {
        public static void MapAuthApi(this WebApplication app)
        {
            // регистрация
            app.MapPost("/api/auth/register", async (RegisterDto registerDto, IAuthService authService) =>
            {
                if (registerDto == null)
                    return Results.BadRequest(new { Message = "Заполните данные для регистрации" });

                try
                {
                    var token = await authService.RegisterAsync(registerDto);
                    return Results.Ok(new { Token = token });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { Message = ex.Message });
                }
            })
            .WithName("Register")
            .WithOpenApi();

            // вход
            app.MapPost("/api/auth/login", async (LoginDto loginDto, IAuthService authService) =>
            {
                if (loginDto == null)
                    return Results.BadRequest(new { Message = "Заполните данные для входа" });

                try
                {
                    var token = await authService.LoginAsync(loginDto);
                    return Results.Ok(new { Token = token });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { Message = ex.Message });
                }
            })
            .WithName("Login")
            .WithOpenApi();

            // получение аккаунта пользователя
            app.MapGet("/api/auth/me", async (HttpContext httpContext, IAuthService authService) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!int.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest(new { Message = "Неверный идентификатор пользователя" });

                try
                {
                    var user = await authService.GetUserByIdAsync(userId);
                    if (user == null)
                        return Results.NotFound(new { Message = "Пользователь не найден" });

                    return Results.Ok(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username
                    });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { Message = ex.Message });
                }
            })
            .WithName("GetUser")
            .RequireAuthorization()
            .WithOpenApi();
        }
    }
}