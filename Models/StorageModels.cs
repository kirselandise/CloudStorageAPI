// File Operation responses and requests
namespace CloudStorageAPI.Models
{

    public class FileOperationResponse
    {
        public bool Successful { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class FileDownloadResponse
    {
        public byte[]? FileContents { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Successful => FileContents != null && FileContents.Length > 0 && string.IsNullOrEmpty(ErrorMessage);
    }

    public class CreateFileRequest
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public byte[] FileContents { get; set; } = Array.Empty<byte>();
    }

    public class FileRequest
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class StorageConnection
    {
        public string Name { get; set; } = string.Empty;
        public StorageType Type { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public string? AccessKey { get; set; }
        public string? SecretKey { get; set; }
        public string? Region { get; set; }
        public string? BucketName { get; set; }
    }

    public enum StorageType
    {
        AzureBlob,
        AmazonS3
    }
}

// Interface and UI with storage providers
namespace CloudStorageAPI.Interfaces
{
    using CloudStorageAPI.Models;

    public interface IStorageProvider
    {
        Task<FileOperationResponse> CreateFileAsync(string filePath, string fileName, byte[] fileContents);

        Task<FileDownloadResponse> DownloadFileAsync(string filePath, string fileName);

        Task<FileOperationResponse> DeleteFileAsync(string filePath, string fileName);
    }

    public interface IStorageProviderFactory
    {
        IStorageProvider CreateProvider(string connectionName);

        IEnumerable<string> GetAvailableConnections();
    }
}
