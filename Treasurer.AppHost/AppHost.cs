var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Treasurer_Api>("api")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.Treasurer_Web>("web")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
