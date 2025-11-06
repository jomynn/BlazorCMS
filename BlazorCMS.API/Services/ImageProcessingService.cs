using BlazorCMS.Shared.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace BlazorCMS.API.Services;

public class ImageProcessingService : IImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(ILogger<ImageProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ConvertImageAsync(Stream inputStream, ImageConvertRequestDTO request)
    {
        try
        {
            using var image = await Image.LoadAsync(inputStream);

            // Resize if dimensions provided
            if (request.Width.HasValue || request.Height.HasValue)
            {
                var width = request.Width ?? image.Width;
                var height = request.Height ?? image.Height;

                if (request.MaintainAspectRatio)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Max
                    }));
                }
                else
                {
                    image.Mutate(x => x.Resize(width, height));
                }
            }

            // Convert to output format
            using var outputStream = new MemoryStream();
            IImageEncoder encoder = request.OutputFormat.ToLower() switch
            {
                "jpg" or "jpeg" => new JpegEncoder { Quality = request.Quality },
                "png" => new PngEncoder(),
                "gif" => new GifEncoder(),
                "webp" => new WebpEncoder { Quality = request.Quality },
                _ => new JpegEncoder { Quality = request.Quality }
            };

            await image.SaveAsync(outputStream, encoder);
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image");
            throw;
        }
    }

    public async Task<byte[]> ResizeImageAsync(Stream inputStream, int? width, int? height, bool maintainAspectRatio)
    {
        try
        {
            using var image = await Image.LoadAsync(inputStream);

            var targetWidth = width ?? image.Width;
            var targetHeight = height ?? image.Height;

            if (maintainAspectRatio)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(targetWidth, targetHeight),
                    Mode = ResizeMode.Max
                }));
            }
            else
            {
                image.Mutate(x => x.Resize(targetWidth, targetHeight));
            }

            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder());
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            throw;
        }
    }

    public async Task<byte[]> CreateVideoFromImageAsync(Stream inputStream, int durationSeconds, string outputFormat)
    {
        try
        {
            var tempImagePath = Path.GetTempFileName() + ".jpg";
            var tempVideoPath = Path.GetTempFileName() + $".{outputFormat}";

            // Save image to temp file
            using (var fileStream = File.Create(tempImagePath))
            {
                await inputStream.CopyToAsync(fileStream);
            }

            // Create video from image using FFMpeg
            await FFMpegArguments
                .FromFileInput(tempImagePath, false, options => options
                    .WithDuration(TimeSpan.FromSeconds(durationSeconds))
                    .ForceFormat("image2"))
                .OutputToFile(tempVideoPath, true, options => options
                    .WithVideoCodec("libx264")
                    .WithConstantRateFactor(23)
                    .WithFramerate(30))
                .ProcessAsynchronously();

            var videoBytes = await File.ReadAllBytesAsync(tempVideoPath);

            // Cleanup
            File.Delete(tempImagePath);
            File.Delete(tempVideoPath);

            return videoBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video from image");
            throw;
        }
    }
}
