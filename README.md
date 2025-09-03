# Trinity McCann | 9/3/2025

Cloud Storage API is a unified REST API that abstracts Azure Blob Storage and AWS S3 operations, providing a single interface for file management (Create, Delete, and Download) across multiple cloud storage providers.

*This project was completed as a part of a 24 hour timed interview process.*

## How to Run
1) Unzip submitted file
2) Open file folder and solution project into IDE (Visual Studio Community 2022)
3) Build and Run
4) Swagger UI will automatically appear (Alternatively you can navigate to `https://localhost:[port]/`)
5) Interact and test Web App as necessary
6) Exit

## How to Test
1) Open file folder and solution project into IDE
2) Navigate to the Test tab and select 'Run Tests' or run 'StorageProviderFactorTests'
3) There are 15 automated tests, all are passing
4) Run Project and Manual Test on Swagger UI
    * Click dropdown button on Create
    * Run 
    ``` json
    {
    "connectionName": "aws-dev",
    "filePath": "new",
    "fileName": "file",
    "fileContents": "SGVsbG8sIFdvcmxkIQ=="
    }
    ```
    * Run 
    ``` json
    {
    "connectionName": "azure-dev",
    "filePath": "new",
    "fileName": "file",
    "fileContents": "SGVsbG8sIFdvcmxkIQ=="
    }
    ```
    * Click drop down on Download
    * Enter (connectionName, filePath, and fileName) = aws-dev, new, file
    * Download from link
    * Enter (connectionName, filePath, and fileName) = azure-dev, new, file\
    * Download from link
    * Click dropdown on Delete
    * Run 
    ``` json
    {
    "connectionName": "azure-dev",
    "filePath": "new",
    "fileName": "file"
    }
    ```
    * Run 
    ``` json
    {
    "connectionName": "azure-dev",
    "filePath": "new",
    "fileName": "file",
    "fileContents": "SGVsbG8sIFdvcmxkIQ=="
    }
    ```
    * All functions have been tested
    
## Testing Checklist
- [x] Create file in Azure Blob Storage
- [x] Create file in AWS S3
- [x] Download file from Azure Blob Storage
- [x] Download file from AWS S3
- [x] Delete file from Azure Blob Storage
- [x] Delete file from AWS S3
- [x] Test error scenarios (invalid connection, missing file)

## Development Time
The project was completed in 7 hours. Development time was calculated from AWS/Azure Account Creation to Final Code Edit. Deployment Time calculates time spent on documentation, technical writing, finalizations, and submission.

**Development Time:** 5 Hours

**Deployment Time:** 2 Hours

## Creditials
As outlined in the project requirements, due to the nature of a timed interview, creditials for both AWS and Azure are hardcoded in for development speed.

## Tech Stack
- **ASP.NET Core 8.0** - Web API framework
- **C# 12** - Programming language
- **Azure.Storage.Blobs (12.19.1)** - Azure Blob Storage client
- **AWSSDK.S3 (3.7.307)** - AWS S3 client
- **Visual Studio Community 2022** - IDE
- **Swashbuckle.AspNetCore (6.4.0)** - Swagger/OpenAPI documentation
- **Microsoft.AspNetCore.OpenApi (8.0.0)** - OpenAPI support
- **xUnit (2.6.1)** - Unit testing framework
- **Moq (4.20.69)** - Mocking framework
- **Microsoft.AspNetCore.Mvc.Testing (8.0.0)** - Integration testing
- **Microsoft.Extensions.DependencyInjection (8.0.0)** - Dependency injection
- **Microsoft.Extensions.Logging** - Logging framework

## Classes and Architecture

- **FilesController**: REST API endpoints for file operations
- **StorageModels**: Data transfer objects and request/response models
- **StorageProviderFactor**: Factory pattern for creating storage providers
- **AzureBlobStorageProvider**: Azure Blob Storage implementation
- **AwsS3StorageProvider**: AWS S3 implementation
- **StorageProviderFactorTests**: Testing Class
- **Program**: Main program file

## Scalability

The project is required to be easy to scale off off. I have provided the following to match the example it gave in the project description. 

To add a new storage provider (Google Cloud Storage):

1. **Create the Provider**
```csharp
public class GoogleCloudStorageProvider : IStorageProvider
{
    // Implement IStorageProvider methods
}
```

2. **Update StorageType Enum**
```csharp
public enum StorageType
{
    AzureBlob,
    AmazonS3,
    GoogleCloud  // Add new type
}
```

3. **Update Factory**
```csharp
// Add case in StorageProviderFactory.CreateProvider()
StorageType.GoogleCloud => CreateGoogleCloudProvider(connection),
```
