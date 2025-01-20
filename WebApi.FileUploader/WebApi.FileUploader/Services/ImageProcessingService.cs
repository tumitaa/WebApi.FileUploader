using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using WebApi.FileUploader.Infrastructure.Dtos;
using WebApi.FileUploader.Services.Interfaces;

namespace WebApi.FileUploader.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly FileUploadSettings _settings;

        public ImageProcessingService(FileUploadSettings settings)
        {
            _settings = settings;
        }

        public async Task<MemoryStream> OptimizeImageAsync(MemoryStream originalImage, string extension, long maxSizeInBytes)
        {
            originalImage.Position = 0;
            using var image = await Image.LoadAsync(originalImage);

            int quality = _settings.InitialImageQuality;
            var output = new MemoryStream();

            while (quality > _settings.MinImageQuality)
            {
                output.SetLength(0);
                if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    var encoder = new JpegEncoder { Quality = quality };
                    await image.SaveAsync(output, encoder);
                }
                else if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    await image.SaveAsPngAsync(output);
                }

                if (output.Length <= maxSizeInBytes)
                    return output;

                quality -= _settings.QualityReductionStep;
            }

            return output.Length <= maxSizeInBytes ? output : null;
        }
    }
}