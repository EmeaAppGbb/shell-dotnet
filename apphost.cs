#:sdk Aspire.AppHost.Sdk@13.1.0
#:package Aspire.Hosting.JavaScript@13.1.0
#:package Aspire.Hosting.Azure.CosmosDB@13.1.0
#:package Aspire.Hosting.Python@13.1.0

var builder = DistributedApplication.CreateBuilder(args);

var cosmosName = builder.AddParameter("cosmosName");
var cosmosResourceGroup = builder.AddParameter("cosmosResourceGroup");

var cosmos = builder.AddAzureCosmosDB("cosmos-db")
    .AsExisting(cosmosName, cosmosResourceGroup);

var api = builder.AddCSharpApp("backend", "./src/backend")
          .WithReference(cosmos);

builder.AddViteApp("frontend", "./src/frontend")
    .WithEnvironment("BACKEND_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api)
    .PublishAsDockerFile();

// Documentation site using MkDocs
builder.AddPythonModule("docs", "./specs", "mkdocs")
    .WithArgs("serve", "--dev-addr", "0.0.0.0:8100")
    .WithHttpEndpoint(targetPort: 8100, name: "http")
    .ExcludeFromManifest();

builder.Build().Run();
