var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PaperlessLabelGenerator>("api")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
