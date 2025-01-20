namespace WebApi.FileUploader.Infrastructure.Dtos;

public class DeleteFileRequest
{
    public string Resource { get; set; }
    public string IdResource { get; set; }
    public string Filename { get; set; }
}