public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}
