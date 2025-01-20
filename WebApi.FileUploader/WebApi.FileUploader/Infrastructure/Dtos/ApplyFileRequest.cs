namespace WebApi.FileUploader.Infrastructure.Dtos;

public class ApplyFileRequest
{
    public string Resource { get; set; }
    public string IdResource { get; set; }
    public string NewIdResource { get; set; }
}