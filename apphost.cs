#:sdk Aspire.AppHost.Sdk@13.1.0
#:package Aspire.Hosting.JavaScript@13.1.0
#:package Aspire.Hosting.Azure.CosmosDB@13.1.0

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

builder.Build().Run();
