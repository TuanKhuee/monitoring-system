namespace backend.DTOs.Project;

public class UpdateProjectRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string ProjectCode { get; set; }
    public string ProjectUrl { get; set; }
    public string RepositoryUrl {get;set;}
    public DateTime UpdatedDate { get; set; }
}