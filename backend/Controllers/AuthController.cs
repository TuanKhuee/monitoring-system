using backend.DTOs.Auth;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        var isSuccess = await _authService.RegisterAsync(request);
        if (!isSuccess)
        {
            return BadRequest(new { message = "Username already exits" });
        }
        return Ok(new { message = "User registered successfully" });
    }
    
    [HttpPost("VerifyOtp")]
    public async Task<IActionResult> VerifyOtpAsync([FromBody] VerifyOtpRequest request)
    {
        var isSuccess = await _authService.VerifyOtpAsync(request);
        if (!isSuccess)
        {
            return BadRequest(new { message = "Invalid or expired OTP code." });
        }
        return Ok(new { message = "Email verified successfully. You can now log in." });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var authResponse = await _authService.LoginAsync(request);
        if (authResponse == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }
        return Ok(authResponse);
    }
}