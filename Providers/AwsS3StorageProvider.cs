using Amazon.S3;
using Amazon.S3.Model;
using CloudStorageAPI.Interfaces;
using CloudStorageAPI.Models;

namespace CloudStorageAPI.Providers
{
    // Implement the AWS S3 storage provider
    public class AwsS3StorageProvider : IStorageProvider
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<AwsS3StorageProvider> _logger;

        public AwsS3StorageProvider(string accessKey, string secretKey, string region, string bucketName, ILogger<AwsS3StorageProvider> logger)
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, config);
            _bucketName = bucketName;
            _logger = logger;
        }

        public async Task<FileOperationResponse> CreateFileAsync(string filePath, string fileName, byte[] fileContents)
        {
            try
            {
                var key = ConstructS3Key(filePath, fileName);

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = new MemoryStream(fileContents),
                    ContentType = GetContentType(fileName)
                };

                var response = await _s3Client.PutObjectAsync(request);

                _logger.LogInformation($"Successfully created file: {key} in AWS S3 bucket: {_bucketName}");
                return new FileOperationResponse { Successful = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating file {filePath}/{fileName} in AWS S3");
                return new FileOperationResponse
                {
                    Successful = false,
                    ErrorMessage = $"Failed to create file: {ex.Message}"
                };
            }
        }

        public async Task<FileDownloadResponse> DownloadFileAsync(string filePath, string fileName)
        {
            try
            {
                var key = ConstructS3Key(filePath, fileName);
                _logger.LogInformation($"AWS: Attempting to download object: '{key}' from bucket: '{_bucketName}'");

                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                var fileContents = memoryStream.ToArray();

                _logger.LogInformation($"AWS: Successfully downloaded {fileContents.Length} bytes from S3 key: {key}");
                return new FileDownloadResponse
                {
                    FileContents = fileContents
                };
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"AWS: File not found: {filePath}/{fileName} (S3 key: {ConstructS3Key(filePath, fileName)})");
                return new FileDownloadResponse
                {
                    ErrorMessage = "File not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AWS: Error downloading file {filePath}/{fileName}");
                return new FileDownloadResponse
                {
                    ErrorMessage = $"AWS S3 download failed: {ex.Message}"
                };
            }
        }

        public async Task<FileOperationResponse> DeleteFileAsync(string filePath, string fileName)
        {
            try
            {
                var key = ConstructS3Key(filePath, fileName);

                // Check if object exists first
                try
                {
                    await _s3Client.GetObjectMetadataAsync(_bucketName, key);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new FileOperationResponse
                    {
                        Successful = false,
                        ErrorMessage = "File not found"
                    };
                }

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);

                _logger.LogInformation($"Successfully deleted file: {key} from AWS S3 bucket: {_bucketName}");
                return new FileOperationResponse { Successful = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {filePath}/{fileName} from AWS S3");
                return new FileOperationResponse
                {
                    Successful = false,
                    ErrorMessage = $"Failed to delete file: {ex.Message}"
                };
            }
        }

        private static string ConstructS3Key(string filePath, string fileName)
        {
            // Ensure appropriate file path formatting no matter user input
            var normalizedPath = filePath.Trim('/');
            if (!string.IsNullOrEmpty(normalizedPath))
            {
                normalizedPath += "/";
            }

            return normalizedPath + fileName;
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _s3Client?.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
