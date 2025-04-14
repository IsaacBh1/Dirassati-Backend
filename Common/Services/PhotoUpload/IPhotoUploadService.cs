using Microsoft.AspNetCore.Http;

namespace Dirassati_Backend.Common.Services.PhotoUpload;

public interface IPhotoUploadService
{
    Task<PhotoUploadResult?> UploadPhoto(IFormFile formFile);
    Task<string> DeletePhoto(string PublicId);

}
