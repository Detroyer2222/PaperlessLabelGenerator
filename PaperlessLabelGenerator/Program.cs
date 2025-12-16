using PaperlessLabelGenerator.Core.Generators;
using PaperlessLabelGenerator.Core.Labels;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

builder.Services.AddScoped<ILabelDesignFactory, LabelDesignFactory>()
    .AddScoped<ILabelDocumentGenerator, LabelDocumentGenerator>();


builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

WebApplication app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Paperless Label Generator API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithOpenApiRoutePattern("/openapi/v1.json");
    });
}

// Don't force HTTPS redirect in Development to allow HTTP access
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

// Log startup info
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Paperless Label Generator API started");
logger.LogInformation("Available endpoints:");
logger.LogInformation("  POST /api/labels - Generate labels PDF");
logger.LogInformation("  GET /api/labels/formats - Get available label formats");
logger.LogInformation("API documentation at: http://localhost:8080/ or http://localhost:8081/");
logger.LogInformation("OpenAPI schema at: http://localhost:8080/openapi/v1.json or http://localhost:8081/openapi/v1.json");

app.Run();