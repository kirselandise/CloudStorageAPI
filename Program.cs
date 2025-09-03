using CloudStorageAPI.Interfaces;
using CloudStorageAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IStorageProviderFactory, StorageProviderFactory>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    //This project utilizes Swagger to assist in UI for development and testing
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cloud Storage API",
        Version = "v1",
        Description = "A unified API for managing files across Azure Blob Storage and AWS S3",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Cloud Storage API",
            Email = "support@example.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Adds CORS to assist in local testing with Swagger UI
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add logging to assist in testing and debugging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cloud Storage API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Adds health check endpoint
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Version = "1.0.0"
    });
});

// Start up of program
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Cloud Storage API is starting up...");
logger.LogInformation("Swagger UI available at: https://localhost:[port]/");

app.Run();
