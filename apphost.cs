#:sdk Aspire.AppHost.Sdk@13.2.0
#:package Aspire.Hosting.JavaScript@13.2.0
#:package Aspire.Hosting.Azure.CosmosDB@13.2.0
#:package Aspire.Hosting.Python@13.2.0

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddCSharpApp("backend", "./src/backend");

builder.AddViteApp("frontend", "./src/frontend")
    .WithEnvironment("BACKEND_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api)
    .PublishAsDockerFile();

builder.Build().Run();
