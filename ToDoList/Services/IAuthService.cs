using ToDoList.DTO;

namespace ToDoList.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
        Task<UserDto> GetUserByIdAsync(int userId);
    }
}