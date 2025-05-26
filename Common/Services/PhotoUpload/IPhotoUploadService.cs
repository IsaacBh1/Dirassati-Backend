namespace Dirassati_Backend.Common.Services.PhotoUpload;

public interface IPhotoUploadService
{
    Task<Result<PhotoUploadResult,string>> UploadPhotoAsync(IFormFile formFile);
    Task<string> DeletePhoto(string publicId);

}
