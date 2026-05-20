namespace backend.DTOs.Auth;

public class VerifyOtpRequest
{
    public string Username { get; set; }
    public string OtpCode { get; set; }
}