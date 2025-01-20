namespace WebApi.FileUploader.Infrastructure.Configuration
{
    public class S3Options
    {
        public const string SectionName = "S3";

        public string BucketName { get; set; }
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BaseUrl { get; set; }
    }
} 