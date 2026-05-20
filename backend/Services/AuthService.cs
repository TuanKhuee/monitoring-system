using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.DTOs.Auth;
using backend.Model;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using backend.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;

    public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings, IEmailService emailService, IOptions<EmailSettings> emailSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return false;
        }
        
        var otpCode = new Random().Next(100000, 999999).ToString();
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = "User",
            IsEmailVerified = false, 
            OtpCode = otpCode,
            OtpExpiry = DateTime.UtcNow.AddMinutes(5) 
        };
        await _userRepository.InsertOneAsync(newUser);
        
        string subject = "Xác Thực Tài Khoản Monitoring System";
        string body = $"<h3>Xin chào {request.Username},</h3>" +
                      $"<p>Mã OTP để kích hoạt tài khoản của bạn là: <strong>{otpCode}</strong></p>" +
                      $"<p>Mã này sẽ hết hạn sau 5 phút.</p>";
        await _emailService.SendEmailRegisterAsync(request.Email, subject, body);
        return true;
    }
    public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null) return false;
        if (user.IsEmailVerified) return true; 
        
        if (user.OtpCode == request.OtpCode && user.OtpExpiry > DateTime.UtcNow)
        {
            user.IsEmailVerified = true;
            user.OtpCode = null; 
            user.OtpExpiry = null;
            
            await _userRepository.ReplaceOneAsync(user.Id, user);
            return true;
        }
        return false; 
    }
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null) return null;
        
        if (!user.IsEmailVerified)
        {
            throw new UnauthorizedAccessException("Tài khoản chưa được xác thực email!");
        }
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid) return null;
        var token = GenerateJwtToken(user);
        return new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role
        };
    }
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
