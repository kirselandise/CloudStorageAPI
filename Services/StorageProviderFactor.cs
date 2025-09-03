using CloudStorageAPI.Interfaces;
using CloudStorageAPI.Models;
using CloudStorageAPI.Providers;

namespace CloudStorageAPI.Services
{
    // Provides connection to storage providers
    public class StorageProviderFactory : IStorageProviderFactory
    {
        private readonly Dictionary<string, StorageConnection> _connections;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StorageProviderFactory> _logger;

        public StorageProviderFactory(IServiceProvider serviceProvider, ILogger<StorageProviderFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            _connections = new Dictionary<string, StorageConnection>
            {

                // Hardcoded creditials as required by project
                ["azure-dev"] = new StorageConnection
                {
                    Name = "azure-dev",
                    Type = StorageType.AzureBlob,
                    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=cmvirtualizer;AccountKey=INPUT-HERE;EndpointSuffix=core.windows.net" // INPUT AZURE ACCOUNT KEY 
                },
                // Hardcoded creditials as required by project
                ["aws-dev"] = new StorageConnection
                {
                    Name = "aws-dev",
                    Type = StorageType.AmazonS3,
                    AccessKey = "", // INPUT AWS ACCESS KEY
                    SecretKey = "--", //INPUT AWS SECRET KEY
                    Region = "us-east-1",
                    BucketName = "cmbucketmccann"
                }
            };
        }

        // Uses user inputted 'connectionName'
        public IStorageProvider CreateProvider(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                throw new ArgumentException("Connection name cannot be null or empty", nameof(connectionName));
            }

            if (!_connections.TryGetValue(connectionName, out var connection))
            {
                throw new ArgumentException($"Connection '{connectionName}' not found. Available connections: {string.Join(", ", _connections.Keys)}", nameof(connectionName));
            }

            _logger.LogInformation($"Creating storage provider for connection: {connectionName} (Type: {connection.Type})");

            return connection.Type switch
            {
                StorageType.AzureBlob => CreateAzureBlobProvider(connection),
                StorageType.AmazonS3 => CreateAwsS3Provider(connection),
                _ => throw new NotSupportedException($"Storage type {connection.Type} is not supported")
            };
        }

        public IEnumerable<string> GetAvailableConnections()
        {
            return _connections.Keys;
        }

        // Creates Azure Blob Storage provider
        private IStorageProvider CreateAzureBlobProvider(StorageConnection connection)
        {
            if (string.IsNullOrEmpty(connection.ConnectionString))
            {
                throw new InvalidOperationException($"Connection string is required for Azure Blob storage connection '{connection.Name}'");
            }

            var logger = _serviceProvider.GetRequiredService<ILogger<AzureBlobStorageProvider>>();

            // Use 'files' as the container name - you can make this configurable
            return new AzureBlobStorageProvider(connection.ConnectionString, "cmcontainer", logger);
        }

        // Creates AWS S3 storage provider
        private IStorageProvider CreateAwsS3Provider(StorageConnection connection)
        {
            if (string.IsNullOrEmpty(connection.AccessKey) ||
                string.IsNullOrEmpty(connection.SecretKey) ||
                string.IsNullOrEmpty(connection.BucketName) ||
                string.IsNullOrEmpty(connection.Region))
            {
                throw new InvalidOperationException($"AccessKey, SecretKey, BucketName, and Region are required for AWS S3 connection '{connection.Name}'");
            }

            var logger = _serviceProvider.GetRequiredService<ILogger<AwsS3StorageProvider>>();

            return new AwsS3StorageProvider(
                connection.AccessKey,
                connection.SecretKey,
                connection.Region,
                connection.BucketName,
                logger);
        }

        public void AddConnection(StorageConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(connection.Name))
                throw new ArgumentException("Connection name cannot be null or empty");

            _connections[connection.Name] = connection;
            _logger.LogInformation($"Added/updated storage connection: {connection.Name}");
        }

        public bool RemoveConnection(string connectionName)
        {
            var removed = _connections.Remove(connectionName);
            if (removed)
            {
                _logger.LogInformation($"Removed storage connection: {connectionName}");
            }
            return removed;
        }
    }
}
