using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using agentic_api.Workflows;
using Microsoft.Agents.AI.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();

// Configure request timeout from configuration or use default
var timeoutSeconds = builder.Configuration.GetValue<int?>("RequestTimeoutSeconds") ?? 120;
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(timeoutSeconds)
    };
});

// Add health checks with basic readiness check
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

string endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"]
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

string deploymentName = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
    ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");

string imageDeploymentName = builder.Configuration["AZURE_IMAGE_MODEL_DEPLOYMENT_NAME"]
    ?? throw new InvalidOperationException("AZURE_IMAGE_MODEL_DEPLOYMENT_NAME is not set.");

// Register IChatClient
builder.Services.AddSingleton(_ =>
    new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        .GetChatClient(deploymentName)
        .AsIChatClient());

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Services.AddSingleton(_ =>
    new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        .GetImageClient(imageDeploymentName)
        .AsIImageGenerator());
#pragma warning restore MEAI001 // Ty

// Register the dummy workflow factory
builder.Services.AddSingleton<DummyWorkflowFactory>();

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

builder.AddWorkflow("DummyWorkflow", (sp, name) =>
{
    var factory = sp.GetRequiredService<DummyWorkflowFactory>();
    return factory.BuildWorkflow("DummyWorkflow");
}).AddAsAIAgent();

var app = builder.Build();

// Add request timeouts
app.UseRequestTimeouts();

// Get the dummy workflow and convert it to an agent
var dummyWorkflowFactory = app.Services.GetRequiredService<DummyWorkflowFactory>();
var dummyWorkflow = dummyWorkflowFactory.BuildWorkflow("DummyWorkflow");
var dummyAgent = new AGUIWorkflowAgent(dummyWorkflow.AsAgent(name: "DummyWorkflow"));

app.MapOpenAIResponses();
app.MapOpenAIConversations();

// Map the dummy workflow agent to the default AGUI endpoint
app.MapAGUI("/", dummyAgent);

// Map health check endpoint
app.MapHealthChecks("/health");

if (builder.Environment.IsDevelopment())
{
    // Map DevUI endpoint to /devui
    app.MapDevUI();
}


await app.RunAsync();