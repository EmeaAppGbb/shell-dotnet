

#:sdk Aspire.AppHost.Sdk@13.0.0
#:package Aspire.Hosting.JavaScript@13.0.0
#:package Aspire.Hosting.Azure.CognitiveServices@13.0.0
#:package Aspire.Hosting.Azure.AIFoundry@13.0.0-preview.1.25560.3

var builder = DistributedApplication.CreateBuilder(args);

var openAiEndpoint = builder.AddParameter("openAiEndpoint");
var openAiDeployment = builder.AddParameter("openAiDeployment");
var imageModelDeployment = builder.AddParameter("imageModelDeployment");

var api = builder.AddCSharpApp("agentic-api", "./src/agentic-api")
    .WithEnvironment("AZURE_OPENAI_ENDPOINT", openAiEndpoint)
    .WithEnvironment("AZURE_OPENAI_DEPLOYMENT_NAME", openAiDeployment)
    .WithEnvironment("AZURE_IMAGE_MODEL_DEPLOYMENT_NAME", imageModelDeployment);

builder.AddJavaScriptApp("agentic-ui", "./src/agentic-ui")
    .WithRunScript("dev")
    .WithNpm(installCommand: "ci")
    .WithEnvironment("AGENT_API_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
