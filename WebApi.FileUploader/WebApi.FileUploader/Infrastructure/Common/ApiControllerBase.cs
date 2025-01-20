using Microsoft.AspNetCore.Mvc;

namespace WebApi.FileUploader.Infrastructure.Common
{
    public abstract class ApiControllerBase : ControllerBase
    {
        public string Client
        {
            get
            {
                return HttpContext.User.FindFirst("Hash")?.Value;
            }
        }
    }
}