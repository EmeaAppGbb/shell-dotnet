#:sdk Aspire.AppHost.Sdk@13.0.0
#:package Aspire.Hosting.JavaScript@13.0.0

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddCSharpApp("backend", "./src/backend");

builder.AddJavaScriptApp("agentic-ui", "./src/frontend")
    .WithRunScript("dev")
    .WithNpm(installCommand: "ci")
    .WithEnvironment("BACKEND_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
