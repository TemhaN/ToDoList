using Microsoft.AspNetCore.Http.HttpResults;
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
        }
    }
}