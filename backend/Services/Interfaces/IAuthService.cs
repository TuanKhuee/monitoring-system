using backend.DTOs.Auth;

namespace backend.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<bool> VerifyOtpAsync(VerifyOtpRequest request);
}