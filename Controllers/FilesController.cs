using CloudStorageAPI.Interfaces;
using CloudStorageAPI.Models;
using CloudStorageAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorageAPI.Controllers
{
    // Manages file operations across different cloud storage providers
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FilesController : ControllerBase
    {
        private readonly IStorageProviderFactory _storageProviderFactory;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IStorageProviderFactory storageProviderFactory, ILogger<FilesController> logger)
        {
            _storageProviderFactory = storageProviderFactory;
            _logger = logger;
        }

        // Creates a file in the specified storage system
        [HttpPost("create-file")]
        [ProducesResponseType(typeof(FileOperationResponse), 200)]
        [ProducesResponseType(typeof(FileOperationResponse), 400)]
        [ProducesResponseType(typeof(FileOperationResponse), 404)]
        [ProducesResponseType(typeof(FileOperationResponse), 500)]
        public async Task<ActionResult<FileOperationResponse>> CreateFile([FromBody] CreateFileRequest request)
        {
            try
            {
                // UI Validation
                if (string.IsNullOrEmpty(request.ConnectionName))
                {
                    _logger.LogWarning("Create file request missing connection name");
                    return BadRequest(new FileOperationResponse
                    {
                        Successful = false,
                        ErrorMessage = "Connection name is required"
                    });
                }

                if (string.IsNullOrEmpty(request.FileName))
                {
                    _logger.LogWarning("Create file request missing file name");
                    return BadRequest(new FileOperationResponse
                    {
                        Successful = false,
                        ErrorMessage = "File name is required"
                    });
                }

                if (request.FileContents == null || request.FileContents.Length == 0)
                {
                    _logger.LogWarning("Create file request missing file contents");
                    return BadRequest(new FileOperationResponse
                    {
                        Successful = false,
                        ErrorMessage = "File contents are required"
                    });
                }

                _logger.LogInformation($"Creating file: {request.FilePath}/{request.FileName} using connection: {request.ConnectionName}");

                // Logic Validation
                var provider = _storageProviderFactory.CreateProvider(request.ConnectionName);
                var result = await provider.CreateFileAsync(request.FilePath, request.FileName, request.FileContents);

                if (result.Successful)
                {
                    _logger.LogInformation($"File created successfully: {request.FilePath}/{request.FileName}");
                    return Ok(result);
                }
                else
                {
                    _logger.LogError($"Failed to create file: {result.ErrorMessage}");
                    return StatusCode(500, result);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid connection name provided");
                return NotFound(new FileOperationResponse
                {
                    Successful = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating file");
                return StatusCode(500, new FileOperationResponse
                {
                    Successful = false,
                    ErrorMessage = "An unexpected error occurred"
                });
            }
        }

        // Downloads a file from the specified storage system
        [HttpGet("download-file")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(FileDownloadResponse), 400)]
        [ProducesResponseType(typeof(FileDownloadResponse), 404)]
        [ProducesResponseType(typeof(FileDownloadResponse), 500)]
        public async Task<IActionResult> DownloadFile(
            [FromQuery] string connectionName,
            [FromQuery] string filePath,
            [FromQuery] string fileName)
        {
            try
            {
                // UI Validation
                if (string.IsNullOrEmpty(connectionName))
                {
                    _logger.LogWarning("Download file request missing connection name");
                    return BadRequest(new FileDownloadResponse
                    {
                        ErrorMessage = "Connection name is required"
                    });
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    _logger.LogWarning("Download file request missing file name");
                    return BadRequest(new FileDownloadResponse
                    {
                        ErrorMessage = "File name is required"
                    });
                }

                _logger.LogInformation($"Downloading file: {filePath}/{fileName} using connection: {connectionName}");

                // Logic Validation
                var provider = _storageProviderFactory.CreateProvider(connectionName);
                var result = await provider.DownloadFileAsync(filePath ?? "", fileName);

                if (result.Successful && result.FileContents != null)
                {
                    _logger.LogInformation($"File downloaded successfully: {filePath}/{fileName}");

                    var contentType = GetContentType(fileName);
                    return File(result.FileContents, contentType, fileName);
                }
                else if (result.ErrorMessage == "File not found")
                {
                    _logger.LogWarning($"File not found: {filePath}/{fileName}");
                    return NotFound(new FileDownloadResponse
                    {
                        ErrorMessage = "File not found"
                    });
                }
                else
                {
                    _logger.LogError($"Failed to download file: {result.ErrorMessage}");
                    return StatusCode(500, new FileDownloadResponse
                    {
                        ErrorMessage = result.ErrorMessage
                    });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid connection name provided");
                return NotFound(new FileDownloadResponse
                {
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error downloading file");
                return StatusCode(500, new FileDownloadResponse
                {
                    ErrorMessage = "An unexpected error occurred"
                });
            }
        }

        // Deletes a file from the specified storage system
        [HttpDelete("delete-file")]
        [ProducesResponseType(typeof(FileOperationResponse), 200)]
        [ProducesResponseType(typeof(FileOperationResponse), 400)]
        [ProducesResponseType(typeof(FileOperationResponse), 404)]
        [ProducesResponseType(typeof(FileOperationResponse), 500)]
        public async Task<ActionResult<FileOperationResponse>> DeleteFile([FromBody] FileRequest request)
        {
            try
            {
                // UI Validation
                if (string.IsNullOrEmpty(request.ConnectionName))
                {
                    _logger.LogWarning("Delete file request missing connection name");
                    return BadRequest(new FileOperationResponse
                    {
                        Successful = false,
                        ErrorMessage = "Connection name is required"
                    });
                }

                if (string.IsNullOrEmpty(request.FileName))
                {
                    _logger.LogWarning("Delete file request missing file name");
                    return BadRequest(new FileOperationResponse
                    {
                        Successful = false,
                        ErrorMessage = "File name is required"
                    });
                }

                _logger.LogInformation($"Deleting file: {request.FilePath}/{request.FileName} using connection: {request.ConnectionName}");

                // Logic Validation
                var provider = _storageProviderFactory.CreateProvider(request.ConnectionName);
                var result = await provider.DeleteFileAsync(request.FilePath, request.FileName);

                if (result.Successful)
                {
                    _logger.LogInformation($"File deleted successfully: {request.FilePath}/{request.FileName}");
                    return Ok(result);
                }
                else if (result.ErrorMessage == "File not found")
                {
                    _logger.LogWarning($"File not found for deletion: {request.FilePath}/{request.FileName}");
                    return NotFound(result);
                }
                else
                {
                    _logger.LogError($"Failed to delete file: {result.ErrorMessage}");
                    return StatusCode(500, result);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid connection name provided");
                return NotFound(new FileOperationResponse
                {
                    Successful = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting file");
                return StatusCode(500, new FileOperationResponse
                {
                    Successful = false,
                    ErrorMessage = "An unexpected error occurred"
                });
            }
        }

        // Lists available storage connections (testing and operating)
        [HttpGet("connections")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public ActionResult<IEnumerable<string>> GetConnections()
        {
            var connections = _storageProviderFactory.GetAvailableConnections();
            return Ok(connections);
        }

        // Debug for developer needs
        [HttpGet("debug-path")]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult<object> DebugPath([FromQuery] string filePath, [FromQuery] string fileName)
        {
            var normalizedPath = string.IsNullOrEmpty(filePath) ? "" : filePath.Trim('/');
            var finalPath = string.IsNullOrEmpty(normalizedPath) ? fileName : $"{normalizedPath}/{fileName}";

            return Ok(new
            {
                OriginalPath = filePath,
                OriginalFileName = fileName,
                NormalizedPath = normalizedPath,
                FinalBlobKey = finalPath,
                PathIsEmpty = string.IsNullOrEmpty(filePath),
                FileNameIsEmpty = string.IsNullOrEmpty(fileName)
            });
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
    }
}
