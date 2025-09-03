using CloudStorageAPI.Interfaces;
using CloudStorageAPI.Models;
using CloudStorageAPI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CloudStorageAPI.Models;
using Xunit;

namespace CloudStorageAPI.Tests
{
    // Unite Tests for StorageProviderFactory
    public class StorageProviderFactoryTests
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<ILogger<StorageProviderFactory>> _mockLogger;
        private readonly StorageProviderFactory _factory;

        public StorageProviderFactoryTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockLogger = new Mock<ILogger<StorageProviderFactory>>();

            // Setup to return mock logs
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<StorageProviderFactory>)))
                              .Returns(_mockLogger.Object);

            var mockAzureLogger = new Mock<ILogger<CloudStorageAPI.Providers.AzureBlobStorageProvider>>();
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<CloudStorageAPI.Providers.AzureBlobStorageProvider>)))
                              .Returns(mockAzureLogger.Object);

            var mockAwsLogger = new Mock<ILogger<CloudStorageAPI.Providers.AwsS3StorageProvider>>();
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<CloudStorageAPI.Providers.AwsS3StorageProvider>)))
                              .Returns(mockAwsLogger.Object);

            _factory = new StorageProviderFactory(_mockServiceProvider.Object, _mockLogger.Object);
        }

        // Unit Test names provide descriptions of desired outcomes and tests
        [Fact]
        public void GetAvailableConnections_ShouldReturnDefaultConnections()
        {
            var connections = _factory.GetAvailableConnections().ToList();

            Assert.Contains("azure-dev", connections);
            Assert.Contains("aws-dev", connections);
            Assert.Equal(2, connections.Count);
        }

        [Fact]
        public void CreateProvider_WithEmptyConnectionName_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _factory.CreateProvider(""));
            Assert.Throws<ArgumentException>(() => _factory.CreateProvider(null));
        }

        [Fact]
        public void CreateProvider_WithInvalidConnectionName_ShouldThrowArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => _factory.CreateProvider("invalid-connection"));
            Assert.Contains("Connection 'invalid-connection' not found", exception.Message);
        }

        [Fact]
        public void AddConnection_WithValidConnection_ShouldAddConnection()
        {
            var connection = new StorageConnection
            {
                Name = "test-connection",
                Type = StorageType.AzureBlob,
                ConnectionString = "test-connection-string"
            };

            _factory.AddConnection(connection);
            var connections = _factory.GetAvailableConnections().ToList();

            Assert.Contains("test-connection", connections);
        }

        [Fact]
        public void AddConnection_WithNullConnection_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.AddConnection(null));
        }

        [Fact]
        public void RemoveConnection_WithExistingConnection_ShouldReturnTrue()
        {
            // Arrange
            var connection = new StorageConnection
            {
                Name = "temp-connection",
                Type = StorageType.AzureBlob,
                ConnectionString = "test"
            };
            _factory.AddConnection(connection);

            // Act
            var result = _factory.RemoveConnection("temp-connection");

            // Assert
            Assert.True(result);
            Assert.DoesNotContain("temp-connection", _factory.GetAvailableConnections());
        }

        [Fact]
        public void RemoveConnection_WithNonExistingConnection_ShouldReturnFalse()
        {
            // Act
            var result = _factory.RemoveConnection("non-existing");

            // Assert
            Assert.False(result);
        }
    }
}

namespace CloudStorageAPI.Tests
{
    // Unit tests for file operation models
    public class FileOperationTests
    {
        [Fact]
        public void FileOperationResponse_WhenSuccessfulTrue_ShouldBeValid()
        {
            var response = new FileOperationResponse
            {
                Successful = true,
                ErrorMessage = null
            };

            Assert.True(response.Successful);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void FileOperationResponse_WhenSuccessfulFalse_ShouldHaveErrorMessage()
        {
            var response = new FileOperationResponse
            {
                Successful = false,
                ErrorMessage = "Test error"
            };

            Assert.False(response.Successful);
            Assert.Equal("Test error", response.ErrorMessage);
        }

        [Fact]
        public void FileDownloadResponse_WithFileContents_ShouldBeSuccessful()
        {
            var fileContents = new byte[] { 1, 2, 3, 4, 5 };

            var response = new FileDownloadResponse
            {
                FileContents = fileContents,
                ErrorMessage = null
            };

            Assert.True(response.Successful);
            Assert.Equal(fileContents, response.FileContents);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void FileDownloadResponse_WithErrorMessage_ShouldNotBeSuccessful()
        {
            var response = new FileDownloadResponse
            {
                FileContents = null,
                ErrorMessage = "File not found"
            };

            Assert.False(response.Successful);
            Assert.Null(response.FileContents);
            Assert.Equal("File not found", response.ErrorMessage);
        }

        [Fact]
        public void CreateFileRequest_ShouldInitializeWithDefaults()
        {
            var request = new CreateFileRequest();

            Assert.Equal(string.Empty, request.ConnectionName);
            Assert.Equal(string.Empty, request.FilePath);
            Assert.Equal(string.Empty, request.FileName);
            Assert.Empty(request.FileContents);
        }

        [Fact]
        public void FileRequest_ShouldInitializeWithDefaults()
        {
            var request = new FileRequest();

            Assert.Equal(string.Empty, request.ConnectionName);
            Assert.Equal(string.Empty, request.FilePath);
            Assert.Equal(string.Empty, request.FileName);
        }

        [Theory]
        [InlineData(StorageType.AzureBlob)]
        [InlineData(StorageType.AmazonS3)]
        public void StorageConnection_ShouldSupportAllStorageTypes(StorageType storageType)
        {
            var connection = new StorageConnection
            {
                Name = "test",
                Type = storageType,
                ConnectionString = "test-connection"
            };

            Assert.Equal(storageType, connection.Type);
        }
    }
}
