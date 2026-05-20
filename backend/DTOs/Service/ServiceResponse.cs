namespace backend.DTOs.Service;

public class ServiceResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty; 
    public string ServiceName { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Protocol { get; set; } = "Http"; 
    public string? HealthEndpoint { get; set; } 
    public int IntervalSeconds { get; set; } = 60; 
    public bool IsActive { get; set; } 
    public DateTime CreatedAt { get; set; } 
}
