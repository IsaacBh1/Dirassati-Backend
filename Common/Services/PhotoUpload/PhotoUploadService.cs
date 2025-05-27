using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Services.PhotoUpload;
using Dirassati_Backend.Configurations;

using Microsoft.Extensions.Options;

namespace Infrastructure.PhotoUpload;

public class PhotoUploadService : IPhotoUploadService
{
    private readonly ILogger<PhotoUploadService> _logger;
    private readonly Cloudinary _cloudinary;
    public PhotoUploadService(IOptions<CloudinaryConfig> options,ILogger<PhotoUploadService> logger)
    {
        _logger = logger;
        var account = new Account(options.Value.CloudName, options.Value.ApiKey, options.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }
    public async Task<string> DeletePhoto(string PublicId)
    {
        var deleteParams = new DeletionParams(PublicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        if (result.Error != null)
            throw new InvalidOperationException(result.Error.Message);
        return result.Result;
    }

public async Task<Result<PhotoUploadResult,string>> UploadPhotoAsync(IFormFile? formFile)
    {
        _logger.LogInformation("Starting photo upload process");
        var result = new Result<PhotoUploadResult, string>();
        
        if (formFile == null || formFile.Length == 0)
        {
            _logger.LogWarning("Upload attempt with empty or null file");
            return result.Failure("File is empty or not provided", 400);
        }
        
        _logger.LogInformation("Processing file: {FileName}, Size: {FileSize}KB", formFile.FileName, formFile.Length / 1024);
        
        try
        {
            await using var stream = formFile.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription { FileName = formFile.FileName, Stream = stream },
                Transformation = new Transformation().Height(500).Width(500).Crop("fill"),
            };
            
            _logger.LogInformation("Sending upload request to Cloudinary");
            var uploadResults = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResults.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {ErrorMessage}", uploadResults.Error.Message);
                return result.Failure(uploadResults.Error.Message, 500);
            }
            
            _logger.LogInformation("Photo uploaded successfully. PublicId: {PublicId}", uploadResults.PublicId);
            return result.Success(new PhotoUploadResult
            {
                PublicId = uploadResults.PublicId,
                Url = uploadResults.SecureUrl.AbsoluteUri
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during photo upload");
            return result.Failure($"Upload failed: {ex.Message}", 500);
        }
    }
}
