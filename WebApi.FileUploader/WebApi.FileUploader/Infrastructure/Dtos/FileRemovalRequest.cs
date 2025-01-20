using System.ComponentModel.DataAnnotations;

namespace WebApi.FileUploader.Infrastructure.Dtos;

public class FileRemovalRequest
{
    [Required]
    public string Client { get; set; }

    [Required]
    public string Resource { get; set; }

    [Required]
    public string ResourceId { get; set; }
}