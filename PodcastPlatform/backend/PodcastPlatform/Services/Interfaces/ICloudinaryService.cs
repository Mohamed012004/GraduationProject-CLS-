namespace PodcastPlatform.Services.Interfaces;

public interface ICloudinaryService
{
    //专门用于图片（自动压缩、优化）
    Task<string> UploadImageAsync(IFormFile file);
    
    //专门用于音频文件
    
    Task<(string Url, TimeSpan Duration)> UploadAudioAsync(IFormFile file);
    
    //删除文件
    Task<bool> DeleteFileAsync(string publicUrl, string resourceType = "raw");

    }