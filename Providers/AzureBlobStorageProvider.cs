using Azure.Storage.Blobs;
using CloudStorageAPI.Interfaces;
using CloudStorageAPI.Models;

namespace CloudStorageAPI.Providers
{
    // Implement the Azure Blob Storage provider
    public class AzureBlobStorageProvider : IStorageProvider
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly ILogger<AzureBlobStorageProvider> _logger;

        public AzureBlobStorageProvider(string connectionString, string containerName, ILogger<AzureBlobStorageProvider> logger)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = containerName;
            _logger = logger;
        }

        public async Task<FileOperationResponse> CreateFileAsync(string filePath, string fileName, byte[] fileContents)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobName = ConstructBlobName(filePath, fileName);

                var blobClient = containerClient.GetBlobClient(blobName);
                using var stream = new MemoryStream(fileContents);
                await blobClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation($"Successfully created file: {blobName} in Azure Blob Storage");
                return new FileOperationResponse { Successful = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating file {filePath}/{fileName} in Azure Blob Storage");
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
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobName = ConstructBlobName(filePath, fileName);
                var blobClient = containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                {
                    return new FileDownloadResponse
                    {
                        ErrorMessage = $"File not found in container '{_containerName}' at blob path '{blobName}'"
                    };
                }

                var response = await blobClient.DownloadContentAsync();
                var fileContents = response.Value.Content.ToArray();

                _logger.LogInformation($"Successfully downloaded file: {blobName} from Azure Blob Storage");
                return new FileDownloadResponse
                {
                    FileContents = fileContents
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file {filePath}/{fileName} from Azure Blob Storage");
                return new FileDownloadResponse
                {
                    ErrorMessage = $"Failed to download file: {ex.Message}"
                };
            }
        }
        public async Task<FileOperationResponse> DeleteFileAsync(string filePath, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobName = ConstructBlobName(filePath, fileName);
                var blobClient = containerClient.GetBlobClient(blobName);
                var response = await blobClient.DeleteIfExistsAsync();

                if (response.Value)
                {
                    _logger.LogInformation($"Successfully deleted file: {blobName} from Azure Blob Storage");
                    return new FileOperationResponse { Successful = true };
                }
                else
                {
                    return new FileOperationResponse
                    {
                        Successful = false,
                        ErrorMessage = "File not found"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {filePath}/{fileName} from Azure Blob Storage");
                return new FileOperationResponse
                {
                    Successful = false,
                    ErrorMessage = $"Failed to delete file: {ex.Message}"
                };
            }
        }

        private static string ConstructBlobName(string filePath, string fileName)
        {
            // Ensure appropriate file path formatting no matter user input
            var normalizedPath = filePath.Trim('/');
            if (!string.IsNullOrEmpty(normalizedPath))
            {
                normalizedPath += "/";
            }

            return normalizedPath + fileName;
        }
    }
}