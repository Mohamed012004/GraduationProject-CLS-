using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using PodcastPlatform.Services.Interfaces;
using TagLib;

namespace PodcastPlatform.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var acc = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(acc);
    }

    // ==========================================
    // 1. UPLOAD IMAGE (With Cloud Magic)
    // ==========================================
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "podcast_images",
            // Automatically compress and optimize the image!
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception(uploadResult.Error.Message);

        return uploadResult.SecureUrl.ToString();
    }

    // ==========================================
    // 2. UPLOAD AUDIO (Raw File)
    // ==========================================
  

public async Task<(string Url, TimeSpan Duration)> UploadAudioAsync(IFormFile file)
{
    if (file.Length > 100 * 1024 * 1024)
        throw new Exception("Audio file cannot exceed 100MB");

    if (!file.ContentType.StartsWith("audio/"))
        throw new Exception("Invalid audio file");

    await using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);
    memoryStream.Position = 0;

    TimeSpan duration;

    try
    {
        using var tagFile = TagLib.File.Create(
            new StreamFileAbstraction(file.FileName, memoryStream, memoryStream)
        );

        duration = tagFile.Properties.Duration;
    }
    catch
    {
        throw new Exception("Failed to read audio metadata");
    }

    memoryStream.Position = 0;

    var uploadParams = new RawUploadParams
    {
        File = new FileDescription(file.FileName, memoryStream),
        Folder = "podcast_audio_files"
    };

    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

    if (uploadResult.Error != null)
        throw new Exception(uploadResult.Error.Message);

    // ✅ return TimeSpan instead of int
    return (uploadResult.SecureUrl.ToString(), duration);
}
    // ==========================================
    // 3. DELETE FILE
    // ==========================================
    public async Task<bool> DeleteFileAsync(string publicUrl, string resourceType = "raw")
    {
        var uri = new Uri(publicUrl);
        // Extract file name from URL (e.g., "myfile" from "https://.../myfile.mp3")
        var publicIdWithFolder = string.Join("", uri.Segments.Skip(2)).Split('.')[0];

        var deleteParams = new DeletionParams(publicIdWithFolder)
        {
            // Tell Cloudinary if it's an image or raw audio file
            ResourceType = resourceType == "image" ? ResourceType.Image : ResourceType.Raw
        };

        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}