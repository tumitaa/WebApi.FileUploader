namespace WebApi.FileUploader.Infrastructure.Exceptions;

public class FileUploadException : Exception
{
    public int StatusCode { get; }

    public FileUploadException(string message, int statusCode = 400) 
        : base(message)
    {
        StatusCode = statusCode;
    }

    public FileUploadException(string message, Exception innerException, int statusCode = 400) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

public class FileNotFoundException : FileUploadException
{
    public FileNotFoundException(string fileName) 
        : base($"File not found: {fileName}", 404)
    {
    }
}

public class InvalidFileTypeException : FileUploadException
{
    public InvalidFileTypeException(string message) 
        : base(message, 400)
    {
    }
}

public class FileProcessingException : FileUploadException
{
    public FileProcessingException(string message, Exception innerException = null) 
        : base(message, innerException, 500)
    {
    }
} 