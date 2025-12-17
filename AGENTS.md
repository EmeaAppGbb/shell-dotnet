# AGENTS.md

> **AI Agent Instructions for agentic-shell-dotnet**  
> This document provides comprehensive, prescriptive guidance for AI coding agents working on this project.  
> Human developers should refer to [README.md](README.md) and `/specs/docs/` for general documentation.

---

## Project Overview

**agentic-shell-dotnet** is a microservices-based AI agent application demonstrating the Microsoft Agent Framework. The application provides an intelligent chat interface powered by Azure OpenAI, featuring:

- **Frontend**: Next.js 16 + React 19 + TypeScript + CopilotKit
- **Backend**: ASP.NET Core 10 + Microsoft Agent Framework
- **Orchestration**: .NET Aspire for local development
- **Deployment**: Azure Container Apps with Azure AI services
- **Status**: Prototype/Demo (not production-ready)

**Key Characteristics:**
- Workflow-based agent architecture using Microsoft Agent Framework
- Preview/beta software stack (70% of dependencies)
- Clean microservices separation with AGUI protocol
- Infrastructure as Code with Azure Bicep
- No authentication, testing, or production hardening yet implemented

---

## Architecture Quick Reference

### System Components

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ HTTPS
       ▼
┌─────────────────────────────┐
│   agentic-ui (Frontend)     │
│   Next.js 16 + React 19     │
│   Port: 3000                │
└──────────┬──────────────────┘
           │ HTTP
           │ /api/copilotkit
           ▼
┌─────────────────────────────┐
│  agentic-api (Backend)      │
│  ASP.NET Core 10 + Agents   │
│  Port: 8080 (5149 local)    │
└──────────┬──────────────────┘
           │ SDK calls
           ▼
┌─────────────────────────────┐
│    Azure AI Services        │
│  - Azure OpenAI (GPT-5 Mini)│
│  - Azure AI Foundry         │
│  - Cosmos DB (provisioned)  │
│  - AI Search (provisioned)  │
└─────────────────────────────┘
```

### Repository Structure

```
/
├── src/
│   ├── agentic-api/              # Backend API service (.NET 10)
│   │   ├── Program.cs            # Application startup
│   │   ├── AGUIWorkflowAgent.cs  # AGUI protocol adapter
│   │   └── Workflows/            # Agent workflow implementations
│   │       └── DummyWorkflow.cs  # Demo workflow
│   └── agentic-ui/               # Frontend web app (Next.js 16)
│       ├── app/
│       │   ├── page.tsx          # Main landing page with chat
│       │   └── api/copilotkit/   # Backend connection endpoint
│       └── package.json
├── tests/
│   └── agentic-api-tests/        # Backend unit tests (xUnit)
├── infra/                        # Infrastructure as Code (Bicep)
│   ├── main.bicep                # Main deployment template
│   ├── resources.bicep           # Azure resources definition
│   └── modules/                  # Reusable Bicep modules
├── specs/                        # Product specifications
│   ├── docs/                     # Reverse engineering documentation
│   │   ├── architecture/         # Architecture documentation
│   │   ├── technology/           # Technology stack analysis
│   │   └── infrastructure/       # Deployment documentation
│   └── features/                 # Feature specifications
│       ├── ai-chat-interface.md
│       └── marketing-campaign-workflow.md
├── apphost.cs                    # .NET Aspire orchestration
├── azure.yaml                    # Azure Developer CLI config
└── AGENTS.md                     # This file
```

---

## Setup Commands

### Prerequisites

**Required:**
- **.NET 10.0 SDK** - Install from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Node.js 20.x LTS** - Install from [nodejs.org](https://nodejs.org/)
- **Azure CLI** - Install from [learn.microsoft.com/cli/azure/install-azure-cli](https://learn.microsoft.com/cli/azure/install-azure-cli)
- **Azure Developer CLI (azd)** - Install from [learn.microsoft.com/azure/developer/azure-developer-cli/install-azd](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)

**Optional (for development):**
- **Docker Desktop** - For dev container support
- **Visual Studio Code** - Recommended IDE
- **.NET Aspire workload** - Run: `dotnet workload install aspire`

### Initial Setup

```bash
# 1. Authenticate to Azure
az login
azd auth login

# 2. Provision Azure resources (one-time setup)
azd provision
# Follow prompts to select subscription and location
# This will automatically populate apphost.settings.json with Azure OpenAI configuration
```

### Local Development

```bash
# Run with .NET Aspire (required for proper environment variable injection)
aspire run
# Opens Aspire dashboard at http://localhost:15888
# Frontend available at http://localhost:3000
# Backend available at http://localhost:5149
```

### Build Commands

```bash
# Build all services (recommended)
./build.sh

# Or build services individually:

# Backend build
cd src/agentic-api
dotnet restore
dotnet build

# Frontend build
cd src/agentic-ui
npm install
npm run build

# Build Docker images locally
cd src/agentic-api
docker build -t agentic-api:latest .

cd src/agentic-ui
docker build -t agentic-ui:latest .
```

### Deployment

```bash
# Deploy to Azure (builds + pushes + deploys)
azd deploy

# Deploy specific service
azd deploy agentic-api
azd deploy agentic-ui

# Full provision + deploy
azd up
```

### Testing Commands

**Backend - xUnit testing project (located in tests/agentic-api-tests):**

```bash
# Run backend tests
cd tests/agentic-api-tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run from project root
dotnet test tests/agentic-api-tests/agentic-api-tests.csproj
```

**Frontend - Tests configured (Jest + React Testing Library):**

```bash
cd src/agentic-ui
npm test                    # Run tests in watch mode
npm run test:ci             # Run tests once (CI mode)
npm run test:coverage       # Run with coverage report
```

---

## Technology Stack & Versions

### Backend (.NET)

**Critical: All backend dependencies use EXACT versions. Do not change versions without testing.**

| Package | Version | Status | Notes |
|---------|---------|--------|-------|
| `.NET SDK` | `10.0` | Stable | Target framework: net10.0 |
| `Microsoft.Agents.AI.Workflows` | `1.0.0-preview.251125.1` | ⚠️ Preview | Core workflow orchestration |
| `Microsoft.Agents.AI.Hosting` | `1.0.0-preview.251125.1` | ⚠️ Preview | Agent hosting infrastructure |
| `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore` | `1.0.0-preview.251125.1` | ⚠️ Preview | AGUI protocol integration |
| `Microsoft.Agents.AI.Hosting.OpenAI` | `1.0.0-alpha.251125.1` | ⚠️ Alpha | OpenAI-specific hosting |
| `Azure.AI.OpenAI` | `2.5.0-beta.1` | ⚠️ Beta | Azure OpenAI SDK |
| `Azure.Identity` | `1.18.0-beta.1` | ⚠️ Beta | Authentication |
| `Aspire.Azure.AI.Inference` | `13.0.0-preview.1.25560.3` | ⚠️ Preview | Aspire AI integration |

### Frontend (Node.js)

**Critical: Package versions use caret ranges (^). Lock file ensures deterministic builds.**

| Package | Version | Status | Notes |
|---------|---------|--------|-------|
| `next` | `16.0.3` | Stable | React framework with SSR |
| `react` | `19.2.0` | Stable | Core React library |
| `react-dom` | `19.2.0` | Stable | React DOM rendering |
| `@copilotkit/react-core` | `^1.10.6` | Stable | Core CopilotKit integration |
| `@copilotkit/react-ui` | `^1.10.6` | Stable | UI components |
| `@ag-ui/client` | `^0.0.41` | ⚠️ Pre-1.0 | Microsoft Agent Framework client |
| `tailwindcss` | `^4` | Stable | Utility-first CSS |
| `typescript` | `^5` | Stable | Type safety |

### Infrastructure

- **Orchestration**: .NET Aspire 13.0.0
- **IaC**: Azure Bicep (latest)
- **Cloud Platform**: Azure (Container Apps, OpenAI, AI Foundry, Cosmos DB, AI Search)

---

## Development Workflow

### Adding a New Agent Workflow

**Example: Create a "GreetingWorkflow"**

1. **Create workflow file**: `src/agentic-api/Workflows/GreetingWorkflow.cs`

```csharp
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace agentic_api.Workflows;

// Executor 1: Handle chat input
public class GreetingChatInputExecutor(ILogger<GreetingChatInputExecutor> logger)
    : ExecutorBase<IConversationUpdate, UserInputEvent>(logger)
{
    protected override ValueTask ExecuteAsync(
        IConversationUpdate input,
        CancellationToken cancellationToken)
    {
        var userMessage = input switch
        {
            ChatMessage msg => msg.Text,
            TurnToken token => token.Text,
            _ => "Hello"
        };

        Logger.LogInformation("User said: {Message}", userMessage);
        
        var userEvent = new UserInputEvent
        {
            Input = userMessage
        };
        
        return ValueTask.FromResult(new ExecutionResult<UserInputEvent>(userEvent));
    }
}

// Executor 2: Generate greeting response
public class GreetingGeneratorExecutor(
    ILogger<GreetingGeneratorExecutor> logger,
    IChatClient chatClient)
    : ExecutorBase<UserInputEvent, WorkflowOutputEvent>(logger)
{
    protected override async ValueTask ExecuteAsync(
        UserInputEvent input,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
            You are a friendly AI assistant. Greet the user warmly.
            User message: {input.Input}
            """;

        var response = await chatClient.CompleteAsync(
            prompt, 
            cancellationToken: cancellationToken);

        Logger.LogInformation("AI response: {Response}", response.Message.Text);

        var output = new WorkflowOutputEvent(response.Message.Text ?? "Hello!");
        return new ExecutionResult<WorkflowOutputEvent>(output);
    }
}

// Factory to build the workflow
public class GreetingWorkflowFactory(
    ILogger<GreetingChatInputExecutor> inputLogger,
    ILogger<GreetingGeneratorExecutor> generatorLogger,
    IChatClient chatClient)
{
    public Workflow BuildWorkflow(string name)
    {
        var inputExecutor = new GreetingChatInputExecutor(inputLogger);
        var generatorExecutor = new GreetingGeneratorExecutor(generatorLogger, chatClient);
        
        return new WorkflowBuilder(inputExecutor)
            .WithName(name)
            .AddEdge(inputExecutor, generatorExecutor)
            .WithOutputFrom(generatorExecutor)
            .Build();
    }
}
```

2. **Register workflow in `Program.cs`** - This registers the workflow with the AGUI endpoint as an agent:

```csharp
// Add factory registration
builder.Services.AddSingleton<GreetingWorkflowFactory>();

// Register workflow as AI agent
// This makes the workflow accessible via the AGUI endpoint (/agui)
builder.AddWorkflow("GreetingWorkflow", (sp, name) => 
    sp.GetRequiredService<GreetingWorkflowFactory>().BuildWorkflow(name))
.AddAsAIAgent();  // Wraps workflow with AGUIWorkflowAgent for protocol compatibility
```

**Important**: The `.AddAsAIAgent()` extension method:
- Wraps your workflow with `AGUIWorkflowAgent` for AGUI protocol compatibility
- Registers the agent with the AGUI endpoint (configured via `app.MapAGUI()` in Program.cs)
- Makes the workflow accessible to the frontend via `/api/copilotkit`

3. **Test locally**: Run `aspire run` and interact via chat UI.

### Using IImageGenerator for Text-to-Image Generation

**IImageGenerator** is a client from Microsoft.Extensions.AI that provides text-to-image generation capabilities using any Azure AI Foundry model that supports OpenAI's image generation API (such as Flux, GPT-Image, DALL-E, or other compatible models).

#### Step 1: Configure Image Model Deployment Name

**The image model deployment is automatically provisioned** when running `azd provision`. The infrastructure scripts (in `infra/` directory) create the image model deployment and inject the deployment name into your configuration.

**Local development (apphost.settings.json)** - populated automatically by `azd provision`:

```bash
# This file is auto-generated by azd provision
{
  "openAiEndpoint": "https://YOUR-RESOURCE.openai.azure.com/",
  "openAiDeployment": "gpt-5-mini",
  "imageModelDeployment": "<auto-generated-deployment-name>"  // Injected by provisioning script
}

# Environment variable (for Azure Container Apps) - set automatically during deployment
AZURE_IMAGE_MODEL_DEPLOYMENT_NAME=<auto-generated-deployment-name>
```

**Update `Program.cs` configuration** to read the deployment name:

```csharp
string imageDeploymentName = builder.Configuration["AZURE_IMAGE_MODEL_DEPLOYMENT_NAME"]
    ?? throw new InvalidOperationException("AZURE_IMAGE_MODEL_DEPLOYMENT_NAME is not set.");
```

#### Step 2: Register IImageGenerator in Dependency Injection

**In `Program.cs`**, register the IImageGenerator service:

```csharp
// Register IImageGenerator for text-to-image generation
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
builder.Services.AddSingleton(_ =>
    new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        .GetImageClient(imageDeploymentName)
        .AsIImageGenerator());
#pragma warning restore MEAI001
```

**Important notes:**
- Use `#pragma warning disable MEAI001` because IImageGenerator is a preview API
- Register as `Singleton` for optimal performance
- Use `DefaultAzureCredential` for authentication (same as IChatClient)
- Call `.AsIImageGenerator()` extension method to convert to the standard interface

#### Step 3: Inject IImageGenerator into Executors

**In your workflow executor**, inject IImageGenerator via constructor:

```csharp
public class MyExecutor : Executor<InputEvent, OutputEvent>
{
    private readonly ILogger<MyExecutor> _logger;
    private readonly IChatClient _chatClient;
    private readonly IImageGenerator _imageGenerator;

    public MyExecutor(
        ILogger<MyExecutor> logger, 
        IChatClient chatClient, 
        IImageGenerator imageGenerator) 
        : base("MyExecutor")
    {
        _logger = logger;
        _chatClient = chatClient;
        _imageGenerator = imageGenerator;
    }

    public override async ValueTask<OutputEvent> HandleAsync(
        InputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // Your executor logic here
    }
}
```

**In your workflow factory**, ensure IImageGenerator is injected:

```csharp
public class MyWorkflowFactory
{
    private readonly ILogger<MyExecutor> _logger;
    private readonly IChatClient _chatClient;
    private readonly IImageGenerator _imageGenerator;

    public MyWorkflowFactory(
        ILogger<MyExecutor> logger,
        IChatClient chatClient,
        IImageGenerator imageGenerator)
    {
        _logger = logger;
        _chatClient = chatClient;
        _imageGenerator = imageGenerator;
    }

    public Workflow BuildWorkflow(string name)
    {
        var executor = new MyExecutor(_logger, _chatClient, _imageGenerator);
        
        return new WorkflowBuilder(executor)
            .WithName(name)
            .WithOutputFrom(executor)
            .Build();
    }
}
```

#### Step 4: Generate Images in Executors

**Generate an image from a text prompt**:

```csharp
public override async ValueTask<OutputEvent> HandleAsync(
    InputEvent input,
    IWorkflowContext context,
    CancellationToken cancellationToken = default)
{
    try
    {
        // Step 1: Configure image generation options
        var options = new ImageGenerationOptions
        {
            MediaType = "image/png",
            ResponseFormat = ImageGenerationResponseFormat.Hosted  // or .Base64
        };

        // Step 2: Generate image with a prompt
        string prompt = "A futuristic city at sunset with flying cars";
        _logger.LogInformation("Generating image with prompt: {Prompt}", prompt);
        
        var imageResponse = await _imageGenerator.GenerateImagesAsync(
            prompt, 
            options, 
            cancellationToken);

        // Step 3: Extract the image URL or data
        var dataContent = imageResponse.Contents.OfType<DataContent>().FirstOrDefault();
        
        if (dataContent?.Uri != null)
        {
            _logger.LogInformation("Image generated at URL: {Url}", dataContent.Uri);
            
            // Return the image URL in your response
            return new OutputEvent 
            { 
                Text = "Image generated successfully!",
                ImageUrl = dataContent.Uri.ToString()
            };
        }
        else
        {
            _logger.LogWarning("Image generation failed: no data content returned");
            return new OutputEvent { Text = "Failed to generate image" };
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error generating image");
        throw;
    }
}
```

#### Step 5: Include Images in AgentMessage for UI Display

**To send images back to the UI**, include the image URL in the AgentMessage:

```csharp
// Example: Send image URL back to UI via AgentMessage
await context.YieldOutputAsync(new AgentMessage 
{ 
    Text = "Here's your generated image:",
    ImageUrl = dataContent.Uri.ToString()  // Include image URL
});

// Or in the final response
return new AgentRunResponse 
{ 
    Text = "Image created successfully!",
    ImageUrl = dataContent.Uri.ToString()
};
```

**Frontend rendering** (in CustomMessageRenderer.tsx):

```typescript
// The frontend should handle AgentMessage with imageUrl property
interface AgentMessage {
  text?: string;
  imageUrl?: string;
  // other properties...
}

export function CustomMessageRenderer({ message }: { message: AgentMessage }) {
  return (
    <div className="agent-message">
      {message.text && <p>{message.text}</p>}
      {message.imageUrl && (
        <img 
          src={message.imageUrl} 
          alt="Generated by AI" 
          className="max-w-full rounded-lg mt-2"
        />
      )}
    </div>
  );
}
```

#### Image Generation Options

**Available options for ImageGenerationOptions**:

```csharp
var options = new ImageGenerationOptions
{
    // Image format - "image/png" or "image/jpeg"
    MediaType = "image/png",
    
    // Response format - Hosted (URL) or Base64 (embedded data)
    ResponseFormat = ImageGenerationResponseFormat.Hosted,  // or .Base64
    
    // Image size (model-dependent)
    // Common sizes: "1024x1024", "1024x1792", "1792x1024"
    // Available sizes depend on your deployed model
    Size = "1024x1024",
    
    // Image quality - "standard" or "hd" (model-dependent)
    Quality = "standard",
    
    // Style - "natural" or "vivid" (model-dependent)
    Style = "natural",
    
    // Number of images to generate (model-dependent)
    Count = 1
};
```

#### Complete Example from DummyWorkflow

**See DummyWorkflow.cs for a working example**:

```csharp
public class GreetingExecutor : Executor<UserInputEvent, AgentRunResponse>
{
    private readonly ILogger<GreetingExecutor> _logger;
    private readonly AIAgent _agent;
    private readonly IImageGenerator _imageGenerator;

    public GreetingExecutor(
        ILogger<GreetingExecutor> logger, 
        IChatClient chatClient, 
        IImageGenerator imageGenerator) 
        : base("Greeting")
    {
        _logger = logger;
        _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "GreetingAgent",
            Instructions = "You are a friendly AI assistant."
        });
        _imageGenerator = imageGenerator;
    }

    public override async ValueTask<AgentRunResponse> HandleAsync(
        UserInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate image
            var options = new ImageGenerationOptions
            {
                MediaType = "image/png",
                ResponseFormat = ImageGenerationResponseFormat.Hosted
            };
            
            string prompt = "A tennis court in a jungle";
            var response = await _imageGenerator.GenerateImagesAsync(
                prompt, 
                options, 
                cancellationToken);
                
            var dataContent = response.Contents.OfType<DataContent>().First();
            
            // Generate text response
            var agentResponse = await _agent.RunAsync(
                new ChatMessage(ChatRole.User, input.Input), 
                cancellationToken: cancellationToken);
            
            var responseText = agentResponse.Text ?? "Hi there!";
            
            _logger.LogInformation(
                "AI agent responded with: {Response}, image was created at {Uri}", 
                responseText, 
                dataContent.Uri);

            return new AgentRunResponse 
            { 
                Text = responseText,
                ImageUrl = dataContent.Uri?.ToString()  // Include image URL
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in greeting executor");
            return new AgentRunResponse 
            { 
                Text = "Hi! I had trouble processing your message, but I'm here to help!" 
            };
        }
    }
}
```

#### Best Practices

1. **Error Handling**: Always wrap image generation in try-catch blocks
2. **Logging**: Log prompts and image URLs for debugging
3. **Response Format**: Use `Hosted` for URLs (preferred for UI display), `Base64` for embedded data
4. **Prompt Engineering**: Be specific in prompts for better results
5. **Cost Management**: Image generation charges per image, monitor usage and costs
6. **Timeout Handling**: Image generation can take 10-30 seconds, ensure proper timeout configuration

#### Environment Variables Summary

**Required environment variables for image generation** (automatically configured by `azd provision`):

```bash
# Backend (agentic-api)
AZURE_OPENAI_ENDPOINT=https://YOUR-RESOURCE.openai.azure.com/
AZURE_IMAGE_MODEL_DEPLOYMENT_NAME=<auto-generated>  # Injected by infrastructure provisioning
```

**apphost.settings.json** (automatically populated by `azd provision`):

```json
{
  "openAiEndpoint": "https://YOUR-RESOURCE.openai.azure.com/",
  "openAiDeployment": "gpt-5-mini",
  "imageModelDeployment": "<auto-generated>"  // Provisioned by infra scripts
}
```

**Note**: You don't need to manually configure these values. The `azd provision` command automatically:
1. Creates the Azure OpenAI image model deployment
2. Generates a unique deployment name
3. Injects the deployment name into `apphost.settings.json` (local) and Container Apps environment variables (production)

### Implementing Human-in-the-Loop Approval

**Human-in-the-Loop (HITL)** allows your AI agents to request user approval before proceeding with certain actions. This is essential for workflows that require human oversight, such as content approval, data validation, or critical decision-making.

The implementation requires coordination between backend (agent workflow) and frontend (approval UI):

#### Backend Implementation

##### Step 1: Use ApprovalRequestHelper to Create Approval Requests

The `ApprovalRequestHelper` utility class simplifies creating approval requests in your executors.

**Helper class** (`src/agentic-api/ApprovalRequestHelper.cs`):

```csharp
using Microsoft.Extensions.AI;

namespace agentic_api.Workflows;

/// <summary>
/// Helper class for creating human-in-the-loop approval requests in workflows.
/// </summary>
public static class ApprovalRequestHelper
{
    /// <summary>
    /// Creates a function approval request that prompts the user to approve an item.
    /// </summary>
    /// <param name="functionName">The name of the approval function to call</param>
    /// <param name="arguments">The arguments dictionary containing the data to be approved</param>
    /// <returns>A FunctionApprovalRequestContent that will prompt the user for approval</returns>
    public static FunctionApprovalRequestContent CreateApprovalRequest(
        string functionName,
        Dictionary<string, object?> arguments)
    {
        return new FunctionApprovalRequestContent(
            Guid.NewGuid().ToString(),
            new FunctionCallContent(
                functionName,
                functionName,
                arguments: arguments
            )
        );
    }
}
```

##### Step 2: Return Approval Requests from Executors

In your workflow executor, return a `FunctionApprovalRequestContent` instead of a regular response when you need user approval.

**Example pattern**:

```csharp
public sealed class TextGeneratorExecutor : Executor<UserInputEvent, AIContent>
{
    private readonly ILogger<TextGeneratorExecutor> _logger;
    private readonly AIAgent _agent;

    public TextGeneratorExecutor(
        ILogger<TextGeneratorExecutor> logger, 
        IChatClient chatClient) 
        : base("TextGenerator")
    {
        _logger = logger;
        _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "TextGeneratorAgent",
            Instructions = "You are a helpful AI assistant..."
        });
    }

    public override async ValueTask<AIContent> HandleAsync(
        UserInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating text content for: {Input}", input.Input);

            // Generate content using AI agent
            var agentResponse = await _agent.RunAsync(
                new ChatMessage(ChatRole.User, input.Input), 
                cancellationToken: cancellationToken);

            var responseText = agentResponse.Text ?? "Generated content";
            
            // Return approval request instead of direct response
            return ApprovalRequestHelper.CreateApprovalRequest(
                functionName: "approve_copyright_command",  // Must match frontend hook name
                arguments: new Dictionary<string, object?>
                {
                    { "copyright", responseText }  // Data to be approved
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text content");
            return new TextContent("Error occurred");
        }
    }
}
```

**Key points**:
- The `functionName` parameter must match the `name` property in the frontend's `useHumanInTheLoop` hook
- The `arguments` dictionary contains the data that needs approval
- Argument keys must match the `parameters` defined in the frontend hook

##### Step 3: Handle Approval Responses in Input Executor

When the user approves or rejects content, the response comes back as a `FunctionResultContent` in the chat messages. Your input executor should check for these approval results and route the workflow accordingly.

**Example pattern**:

```csharp
public sealed class DummyChatInputExecutor : Executor
{
    private readonly ILogger<DummyChatInputExecutor> _logger;

    public DummyChatInputExecutor(ILogger<DummyChatInputExecutor> logger) 
        : base("DummyChatInput")
    {
        _logger = logger;
    }

    protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder
            .AddHandler<List<ChatMessage>, UserInputEvent>(HandleChatMessagesAsync);

    private async ValueTask<UserInputEvent> HandleChatMessagesAsync(
        List<ChatMessage> messages,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var lastUserMessage = messages.LastOrDefault(m => m.Role == ChatRole.User);
        
        // Check for approval response from user
        var approvalMessage = messages.LastOrDefault(m => m.Role == ChatRole.Tool);
        var functionResult = approvalMessage?.Contents
            .OfType<FunctionResultContent>()
            .FirstOrDefault();

        // Check if text was approved
        var textApproved = functionResult?.Result?.ToString()?.Contains("text-approved");
        var textRejected = functionResult?.Result?.ToString()?.Contains("text-rejected");
        
        // Check if image was approved
        var imageApproved = functionResult?.Result?.ToString()?.Contains("image-approved");
        var imageRejected = functionResult?.Result?.ToString()?.Contains("image-rejected");

        // Route based on approval status
        if (textApproved == true)
        {
            _logger.LogInformation("Text content approved by user.");
            return new UserInputEvent 
            { 
                Input = lastUserMessage?.Text ?? "", 
                NextStep = WorkflowSteps.GenerateImage 
            };
        }
        else if (textRejected == true)
        {
            _logger.LogInformation("Text content rejected by user.");
            return new UserInputEvent 
            { 
                Input = lastUserMessage?.Text ?? "", 
                NextStep = WorkflowSteps.RegenerateText 
            };
        }
        else if (imageApproved == true)
        {
            _logger.LogInformation("Image content approved by user.");
            return new UserInputEvent 
            { 
                Input = lastUserMessage?.Text ?? "", 
                NextStep = WorkflowSteps.Finalize 
            };
        }
        else
        {
            // No approval detected, start workflow
            _logger.LogInformation("Starting workflow");
            return new UserInputEvent 
            { 
                Input = lastUserMessage?.Text ?? "", 
                NextStep = WorkflowSteps.GenerateText 
            };
        }
    }
}
```

**Key points**:
- Approval responses come back as `ChatRole.Tool` messages containing `FunctionResultContent`
- The `Result` property contains the response value from the frontend (`respond()` function)
- Use the response value to determine the next step in your workflow
- Handle both approval and rejection cases

##### Step 4: Configure AGUIWorkflowAgent to Forward Approval Requests

The `AGUIWorkflowAgent` adapter automatically converts `FunctionApprovalRequestContent` to `FunctionCallContent` for the frontend. This is already configured in the project.

**AGUIWorkflowAgent.cs** (no changes needed, for reference):

```csharp
public class AGUIWorkflowAgent : DelegatingAIAgent
{
    public override async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var update in this.InnerAgent.RunStreamingAsync(
            messages, thread, options, cancellationToken))
        {
            switch (update.RawRepresentation)
            {
                case WorkflowOutputEvent outputEvent:
                    yield return ExtractFunctionCallUpdate(update, outputEvent.Data);
                    break;
                default:
                    yield return ExtractFunctionCallUpdate(update, update.RawRepresentation);
                    break;
            }
        }
    }

    private static AgentRunResponseUpdate ExtractFunctionCallUpdate(
        AgentRunResponseUpdate update, 
        object? data)
    {
        IList<AIContent>? updatedContents = null;
        var content = data;

        // Convert FunctionApprovalRequestContent to FunctionCallContent
        if (content is FunctionApprovalRequestContent request)
        {
            updatedContents ??= [.. update.Contents];
            var functionCall = request.FunctionCall;
            var approvalId = request.Id;

            updatedContents.Add(new FunctionCallContent(
                callId: approvalId,
                name: functionCall.Name,
                arguments: functionCall.Arguments));
        }
        
        // ... rest of the code
    }
}
```

**Important**: This adapter is already registered when you call `.AddAsAIAgent()` in `Program.cs`, so no additional configuration is needed.

#### Frontend Implementation

##### Step 1: Add useHumanInTheLoop Hook

In your frontend component (typically `app/page.tsx`), use the `useHumanInTheLoop` hook from CopilotKit to create an approval UI.

**Example pattern for text approval**:

```typescript
import { useHumanInTheLoop } from "@copilotkit/react-core";
import { useState } from "react";

export default function Page() {
  const [approvedContent, setApprovedContent] = useState<string | null>(null);

  // Human-in-the-loop tool for text content approval
  useHumanInTheLoop({
    name: "approve_copyright_command",  // Must match backend functionName
    description: "Ask the user to approve the generated text content",
    parameters: [
      {
        name: "copyright",  // Must match backend argument key
        type: "string",
        description: "The text content to review and approve",
        required: true,
      },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;
      
      return (
        <div className="approval-container">
          <p className="approval-title">
            📝 Text Content Approval Required
          </p>
          <p className="approval-description">
            Please review the generated text content below:
          </p>
          
          {/* Display content for approval */}
          <div className="content-preview">
            <pre>{args.copyright}</pre>
          </div>
          
          {/* Approval buttons */}
          <div className="button-group">
            <button 
              onClick={() => {
                setApprovedContent(args.copyright);
                respond("text-approved");  // Send approval response
              }}
              className="button-approve"
            >
              ✓ Approve
            </button>
            <button 
              onClick={() => {
                setApprovedContent(null);
                respond("text-rejected");  // Send rejection response
              }}
              className="button-reject"
            >
              ✗ Deny
            </button>
          </div>
        </div>
      );
    },
  });

  return (
    <CopilotSidebar>
      {/* Your page content */}
      {approvedContent && (
        <div className="approved-content">
          <h2>Approved Content</h2>
          <p>{approvedContent}</p>
        </div>
      )}
    </CopilotSidebar>
  );
}
```

**Key points**:
- `name` must match the `functionName` from backend's `ApprovalRequestHelper.CreateApprovalRequest()`
- `parameters` array must match the `arguments` dictionary keys from backend
- `render` function receives `args` (the data to approve) and `respond` (callback to send response)
- Call `respond()` with a string value that your backend can check for in the input executor
- Use descriptive response values like `"text-approved"`, `"text-rejected"`, `"image-approved"`, etc.

##### Step 2: Add Multiple Approval Types

You can have multiple `useHumanInTheLoop` hooks for different approval types.

**Example pattern for image approval**:

```typescript
export default function Page() {
  const [approvedImage, setApprovedImage] = useState<string | null>(null);

  // Human-in-the-loop tool for image content approval
  useHumanInTheLoop({
    name: "approve_design_command",  // Different function name
    description: "Ask the user to approve the generated image content",
    parameters: [
      {
        name: "design",  // Different parameter name
        type: "string",
        description: "The image URL to review and approve",
        required: true,
      },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;
      
      return (
        <div className="approval-container">
          <p className="approval-title">
            🖼️ Image Content Approval Required
          </p>
          <p className="approval-description">
            Please review the generated image below:
          </p>
          
          {/* Display image for approval */}
          <div className="content-preview">
            <img 
              src={args.design} 
              alt="Generated image for approval"
            />
          </div>
          
          {/* Approval buttons */}
          <div className="button-group">
            <button 
              onClick={() => {
                setApprovedImage(args.design);
                respond("image-approved");
              }}
              className="button-approve"
            >
              ✓ Approve
            </button>
            <button 
              onClick={() => {
                setApprovedImage(null);
                respond("image-rejected");
              }}
              className="button-reject"
            >
              ✗ Reject
            </button>
          </div>
        </div>
      );
    },
  });

  return (
    <CopilotSidebar>
      {/* Display approved image */}
      {approvedImage && (
        <div className="approved-image">
          <h2>Approved Image</h2>
          <img src={approvedImage} alt="Approved content" />
        </div>
      )}
    </CopilotSidebar>
  );
}
```

#### Complete Workflow Example

Here's how the pieces work together in a complete workflow:

**Backend workflow** (`DummyWorkflow.cs`):

```csharp
public Workflow BuildWorkflow(string name)
{
    var chatInput = new DummyChatInputExecutor(_inputLogger);
    var textGenerator = new TextGeneratorExecutor(_textGeneratorLogger, _chatClient);
    var imageGenerator = new ImageGeneratorExecutor(_imageGeneratorLogger, _chatClient, _imageGenerator);
    var final = new FinalExecutor(_finalLogger);

    // Build workflow with conditional routing
    var workflowBuilder = new WorkflowBuilder(chatInput)
        .WithName(name)
        .AddSwitch(chatInput, switchBuilder =>
            switchBuilder
                .AddCase(input => input?.NextStep == WorkflowSteps.GenerateText, textGenerator)
                .AddCase(input => input?.NextStep == WorkflowSteps.GenerateImage, imageGenerator)
                .AddCase(input => input?.NextStep == WorkflowSteps.Finalize, final)
                .WithDefault(textGenerator)
        )
        .WithOutputFrom(textGenerator)   // Register as output source
        .WithOutputFrom(imageGenerator)  // Register as output source
        .WithOutputFrom(final);          // Register as output source

    return workflowBuilder.Build();
}
```

**Workflow execution flow**:

1. **User sends message** → `DummyChatInputExecutor` receives message list
2. **Input executor checks for approvals** → Routes to appropriate executor
3. **Executor generates content** → Returns `FunctionApprovalRequestContent`
4. **AGUIWorkflowAgent converts to FunctionCallContent** → Sends to frontend
5. **Frontend displays approval UI** → User approves or rejects
6. **Frontend calls `respond()`** → Sends response back as `FunctionResultContent`
7. **Input executor receives approval response** → Routes to next step
8. **Workflow continues** → Repeats for next approval or completes

#### Best Practices

1. **Naming Conventions**: Use descriptive, action-oriented function names like `approve_copyright_command`, `approve_design_command`, `validate_data_command`

2. **Response Values**: Use clear, searchable response values like `"text-approved"`, `"image-rejected"` that are easy to check in the backend

3. **Error Handling**: Always wrap approval logic in try-catch blocks and provide fallback behavior

4. **User Experience**: 
   - Provide clear instructions in approval UI
   - Show preview of content being approved
   - Use visual indicators (colors, icons) for approval/rejection
   - Display approved content on the page for user confirmation

5. **Security**: Validate approval responses in backend, don't trust frontend approval without verification

6. **Logging**: Log all approval requests and responses for audit trails

7. **Timeout Handling**: Consider adding timeouts for approval requests if user doesn't respond

8. **State Management**: Use React state to track approved/rejected content and update UI accordingly

#### Troubleshooting

**Problem**: Approval UI not appearing

**Solutions**:
- Verify `functionName` in backend matches `name` in frontend hook exactly
- Check that `AGUIWorkflowAgent` is properly registered (automatic with `.AddAsAIAgent()`)
- Verify workflow executor is registered as output source with `.WithOutputFrom()`
- Check browser console for React errors

**Problem**: Approval response not reaching backend

**Solutions**:
- Verify `respond()` is being called with a string value
- Check that backend input executor is looking for the correct response value
- Ensure `ChatRole.Tool` messages are being checked in input executor
- Add logging to see what messages are being received

**Problem**: Workflow not progressing after approval

**Solutions**:
- Verify input executor is checking `FunctionResultContent.Result` property
- Ensure workflow routing logic handles approval cases correctly
- Check that `NextStep` enum or routing flags are being set correctly
- Add logging to trace workflow execution path

### Adding a Frontend Component

**Example: Add a custom chat header**

1. **Create component**: `src/agentic-ui/app/components/ChatHeader.tsx`

```typescript
export function ChatHeader() {
  return (
    <div className="bg-gradient-to-r from-blue-500 to-purple-600 p-4 text-white">
      <h1 className="text-2xl font-bold">AI Assistant</h1>
      <p className="text-sm opacity-90">Powered by Azure OpenAI</p>
    </div>
  );
}
```

2. **Use in page**: `src/agentic-ui/app/page.tsx`

```typescript
import { ChatHeader } from "./components/ChatHeader";

export default function Home() {
  return (
    <CopilotKit runtimeUrl="/api/copilotkit">
      <ChatHeader />
      <CopilotSidebar
        defaultOpen={true}
        labels={{
          title: "AI Assistant",
          initial: "Hi! How can I help you today?",
        }}
      >
        {/* existing content */}
      </CopilotSidebar>
    </CopilotKit>
  );
}
```

3. **Test**: Run `npm run dev` in `src/agentic-ui/`

### Modifying Infrastructure

**Example: Add Azure Cache for Redis**

1. **Add module to `infra/resources.bicep`**:

```bicep
module redis 'br/public:avm/res/cache/redis:0.1.0' = {
  name: 'redis-${resourceToken}'
  params: {
    name: 'redis-${resourceToken}'
    location: location
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0
    }
  }
}
```

2. **Add connection string output**:

```bicep
output redisConnectionString string = redis.outputs.connectionString
```

3. **Update Container App environment variables** in `main.bicep`:

```bicep
{
  name: 'REDIS_CONNECTION_STRING'
  value: resources.outputs.redisConnectionString
}
```

4. **Deploy**: Run `azd deploy`

---

## Code Style & Conventions

### Backend (.NET)

**File naming**: PascalCase for classes, folders, and files
- ✅ `DummyWorkflow.cs`, `AGUIWorkflowAgent.cs`
- ❌ `dummyWorkflow.cs`, `agui-workflow-agent.cs`

**Code style**:
- **Nullable reference types**: Enabled (required)
- **Implicit usings**: Enabled
- **Target framework**: `net10.0`
- **Naming**: PascalCase for public members, camelCase with underscore prefix for private fields
- **Async**: Always use `async`/`await`, never `.Result` or `.Wait()`
- **DI**: Constructor injection for all dependencies
- **Logging**: Use `ILogger<T>` for structured logging

**Example pattern**:

```csharp
public class MyService(ILogger<MyService> logger, IChatClient chatClient)
{
    private readonly ILogger<MyService> _logger = logger;
    private readonly IChatClient _chatClient = chatClient;

    public async Task<string> ProcessAsync(string input, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing input: {Input}", input);
        
        try
        {
            var result = await _chatClient.CompleteAsync(input, cancellationToken: cancellationToken);
            return result.Message.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing input");
            throw;
        }
    }
}
```

### Frontend (TypeScript/React)

**File naming**: kebab-case for files, PascalCase for React components
- ✅ `chat-header.tsx`, `user-profile.tsx`
- Component: `export function ChatHeader() {}`

**Code style**:
- **TypeScript**: Strict mode enabled
- **React 19**: Use hooks, functional components only
- **JSX**: Use `.tsx` extension
- **Props**: Always type props explicitly
- **State**: Use `useState`, `useContext`, `useReducer`
- **Effects**: Minimize `useEffect`, prefer derived state
- **Styling**: Tailwind CSS utility classes

**Example pattern**:

```typescript
interface ChatMessageProps {
  message: string;
  sender: 'user' | 'ai';
  timestamp: Date;
}

export function ChatMessage({ message, sender, timestamp }: ChatMessageProps) {
  return (
    <div className={`p-4 rounded-lg ${
      sender === 'user' ? 'bg-blue-100' : 'bg-gray-100'
    }`}>
      <p className="text-sm text-gray-600">
        {timestamp.toLocaleTimeString()}
      </p>
      <p className="mt-2">{message}</p>
    </div>
  );
}
```

### Bicep (Infrastructure)

**File naming**: kebab-case
- ✅ `main.bicep`, `ai-project.bicep`, `fetch-container-image.bicep`

**Code style**:
- **Naming**: Use descriptive resource names with `${resourceToken}` suffix
- **Modules**: Use Azure Verified Modules (`br/public:avm/...`) when available
- **Outputs**: Expose all connection strings and endpoints as outputs
- **Parameters**: Use `@description` decorator for all parameters

**Example pattern**:

```bicep
@description('The location for all resources')
param location string = resourceGroup().location

@description('Unique token for resource naming')
param resourceToken string

module appServicePlan 'br/public:avm/res/web/serverfarm:0.1.0' = {
  name: 'plan-${resourceToken}'
  params: {
    name: 'plan-${resourceToken}'
    location: location
    sku: {
      name: 'B1'
      tier: 'Basic'
    }
  }
}

output appServicePlanId string = appServicePlan.outputs.resourceId
```

---

## Environment Variables

### Backend (agentic-api)

**Required for Azure OpenAI:**
```bash
AZURE_OPENAI_ENDPOINT=https://YOUR-RESOURCE.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-5-mini
```

**Required for local development authentication:**
```bash
# Use one of these authentication methods
AZURE_TENANT_ID=<your-tenant-id>
AZURE_CLIENT_ID=<your-client-id>
AZURE_CLIENT_SECRET=<your-client-secret>

# OR authenticate via Azure CLI
az login
```

**Optional (provisioned but unused):**
```bash
AZURE_COSMOS_ENDPOINT=https://YOUR-COSMOS.documents.azure.com:443/
AZURE_AI_SEARCH_ENDPOINT=https://YOUR-SEARCH.search.windows.net
AZURE_AI_PROJECT_ENDPOINT=https://YOUR-PROJECT.cognitiveservices.azure.com/
```

**Production (Azure Container Apps):**
- `AZURE_CLIENT_ID` - Automatically set by managed identity
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Automatically set

### Frontend (agentic-ui)

**Required:**
```bash
AGENT_API_URL=http://localhost:5149  # Local development
AGENT_API_URL=https://your-api.azurecontainerapps.io  # Production
```

**Optional:**
```bash
PORT=3000  # Default Next.js port
NODE_ENV=development  # or production
```

### Configuration Files

**Local development configuration:**
- Create `apphost.settings.json` (gitignored) with:

```json
{
  "openAiEndpoint": "https://YOUR-RESOURCE.openai.azure.com/",
  "openAiDeployment": "gpt-5-mini"
}
```

**Do NOT commit:**
- `apphost.settings.json` (gitignored)
- Any file containing Azure endpoints, keys, or secrets
- `.env` or `.env.local` files

---

## Testing Instructions

### Backend Testing Setup (xUnit)

**Testing project location**: `tests/agentic-api-tests/`

**Current state**: Backend testing framework is established with xUnit.

**To add new tests:**

```bash
# Navigate to test project
cd tests/agentic-api-tests

# Add additional testing packages as needed
dotnet add package FluentAssertions
dotnet add package Moq
dotnet add package Testcontainers

# Create test file: WorkflowTests.cs
```

**Example test pattern**:

```csharp
public class DummyWorkflowTests
{
    private readonly Mock<IChatClient> _mockChatClient;
    private readonly Mock<ILogger<GreetingExecutor>> _mockLogger;

    public DummyWorkflowTests()
    {
        _mockChatClient = new Mock<IChatClient>();
        _mockLogger = new Mock<ILogger<GreetingExecutor>>();
    }

    [Fact]
    public async Task Execute_WithValidInput_ReturnsGreeting()
    {
        // Arrange
        var executor = new GreetingExecutor(_mockLogger.Object, _mockChatClient.Object);
        var input = new UserInputEvent { Input = "Hello" };
        
        _mockChatClient
            .Setup(x => x.CompleteAsync(
                It.IsAny<string>(), 
                It.IsAny<ChatOptions>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatCompletion(new ChatMessage(ChatRole.Assistant, "Hi there!")));

        // Act
        var result = await executor.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Output.Text.Should().Contain("Hi there!");
    }
}
```

**Run tests**:
```bash
cd tests/agentic-api-tests
dotnet test
dotnet test --collect:"XPlat Code Coverage"  # With coverage
```

### Frontend Testing Setup (Jest + React Testing Library)

```bash
cd src/agentic-ui

# Install testing dependencies
npm install --save-dev @testing-library/react @testing-library/jest-dom jest-environment-jsdom

# Create jest.config.js
```

**jest.config.js**:

```javascript
const nextJest = require('next/jest')

const createJestConfig = nextJest({
  dir: './',
})

const customJestConfig = {
  setupFilesAfterEnv: ['<rootDir>/jest.setup.js'],
  testEnvironment: 'jest-environment-jsdom',
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/$1',
  },
}

module.exports = createJestConfig(customJestConfig)
```

**Example test pattern** (`__tests__/ChatHeader.test.tsx`):

```typescript
import { render, screen } from '@testing-library/react';
import { ChatHeader } from '@/app/components/ChatHeader';

describe('ChatHeader', () => {
  it('renders the title', () => {
    render(<ChatHeader />);
    expect(screen.getByText('AI Assistant')).toBeInTheDocument();
  });

  it('renders the subtitle', () => {
    render(<ChatHeader />);
    expect(screen.getByText('Powered by Azure OpenAI')).toBeInTheDocument();
  });
});
```

**Run tests**:
```bash
npm test              # Watch mode
npm run test:ci       # Run once
npm run test:coverage # With coverage
```

### Integration Testing

**Not yet implemented. Required setup:**

1. **Backend integration tests**: Test agent workflows end-to-end with test doubles for Azure services
2. **Frontend integration tests**: Test API integration with mock backend
3. **E2E tests**: Use Playwright to test full user flows

---

## Debugging

### Backend Debugging (VS Code)

**Launch configuration** (`.vscode/launch.json`):

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Debug Backend API",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/agentic-api/bin/Debug/net10.0/agentic-api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/agentic-api",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AZURE_OPENAI_ENDPOINT": "https://YOUR-RESOURCE.openai.azure.com/",
        "AZURE_OPENAI_DEPLOYMENT_NAME": "gpt-5-mini"
      }
    }
  ]
}
```

**Set breakpoints** in executors, Program.cs, or workflow code.

### Frontend Debugging (VS Code)

**Launch configuration**:

```json
{
  "name": "Debug Frontend (Chrome)",
  "type": "chrome",
  "request": "launch",
  "url": "http://localhost:3000",
  "webRoot": "${workspaceFolder}/src/agentic-ui",
  "sourceMapPathOverrides": {
    "webpack://_N_E/*": "${webRoot}/*"
  }
}
```

**Debug in browser**: Use Chrome/Edge DevTools, set breakpoints in TypeScript source.

### Common Issues

**Issue**: `Azure.Identity.AuthenticationFailedException: DefaultAzureCredential failed to retrieve a token`

**Solution**:
```bash
# Authenticate via Azure CLI
az login

# Verify authentication
az account show

# Set environment variables (or use apphost.settings.json)
export AZURE_OPENAI_ENDPOINT=https://YOUR-RESOURCE.openai.azure.com/
export AZURE_OPENAI_DEPLOYMENT_NAME=gpt-5-mini
```

**Issue**: Frontend shows "Failed to connect to agent"

**Solution**:
1. Verify backend is running: `curl http://localhost:5149/health`
2. Check `AGENT_API_URL` in frontend `.env.local`
3. Verify CORS configuration in `Program.cs`

**Issue**: `Microsoft.Agents.AI` namespace not found

**Solution**:
```bash
# Ensure .NET 10.0 SDK is installed
dotnet --version  # Should show 10.0.x

# Restore packages
cd src/agentic-api
dotnet restore
```

---

## Architecture Patterns

### Workflow Executor Pattern

**Every agent workflow follows this pattern:**

1. **Input Executor**: Receives external input (ChatMessage/TurnToken), converts to internal event
2. **Processing Executor(s)**: Handle business logic, call AI models, process data
3. **Output Executor**: Returns results to the framework

**Flow**:
```
User Input → ChatInputExecutor → [ProcessingExecutors...] → OutputExecutor → Response
```

### Streaming Messages to the UI

**When building workflows with Microsoft Agent Framework, to stream messages back to the UI:**

**1. Emit events from executors using `YieldOutputAsync`:**

At any point where you want to send intermediate results or status updates to the UI, call:

```csharp
await context.YieldOutputAsync(new AgentMessage 
{ 
    Text = "Your message content here",
    // Map additional properties as needed
});
```

This is particularly useful for:
- Multi-step workflows where each executor produces visible output
- Iterative processes (e.g., one agent creates an artifact, another reviews and approves it)
- Progress updates during long-running operations

**Example:**
```csharp
public class ReviewExecutor : ExecutorBase<ArtifactEvent, ReviewEvent>
{
    protected override async ValueTask ExecuteAsync(
        ArtifactEvent input,
        CancellationToken cancellationToken)
    {
        // Stream status to UI
        await context.YieldOutputAsync(new AgentMessage 
        { 
            Text = "Reviewing artifact..."
        });
        
        var review = await _reviewService.ReviewAsync(input.Artifact);
        
        // Stream result to UI
        await context.YieldOutputAsync(new AgentMessage 
        { 
            Text = $"Review complete: {review.Status}"
        });
        
        return new ExecutionResult<ReviewEvent>(new ReviewEvent { Review = review });
    }
}
```

**2. Register executors as output sources in the workflow builder:**

When building the workflow, mark any executor that calls `YieldOutputAsync` as an output source:

```csharp
var workflow = new WorkflowBuilder(inputExecutor)
    .AddEdge(inputExecutor, processingExecutor)
    .WithOutputFrom(processingExecutor)  // Register as output source
    .AddEdge(processingExecutor, reviewExecutor)
    .WithOutputFrom(reviewExecutor)      // Register as output source
    .Build();
```

**Important**: Every executor that calls `YieldOutputAsync` must be registered with `.WithOutputFrom()`, otherwise its messages won't be streamed to the UI.

**Example**:
```csharp
// 1. Input Executor
public class MyInputExecutor : ExecutorBase<IConversationUpdate, MyInputEvent>
{
    protected override ValueTask ExecuteAsync(IConversationUpdate input, ...)
    {
        var event = new MyInputEvent { Data = ExtractData(input) };
        return ValueTask.FromResult(new ExecutionResult<MyInputEvent>(event));
    }
}

// 2. Processing Executor
public class MyProcessingExecutor : ExecutorBase<MyInputEvent, MyOutputEvent>
{
    protected override async ValueTask ExecuteAsync(MyInputEvent input, ...)
    {
        var result = await ProcessData(input.Data);
        return new ExecutionResult<MyOutputEvent>(new MyOutputEvent { Result = result });
    }
}

// 3. Build Workflow
var workflow = new WorkflowBuilder(inputExecutor)
    .AddEdge(inputExecutor, processingExecutor)
    .WithOutputFrom(processingExecutor)
    .Build();
```

### AGUI Protocol Adapter Pattern

**AGUIWorkflowAgent** wraps workflows to make them compatible with AGUI protocol.

**Pattern**:
```csharp
public class AGUIWorkflowAgent : DelegatingAIAgent
{
    public AGUIWorkflowAgent(AIAgent innerAgent) : base(innerAgent) { }
    
    public override IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(...)
    {
        // Intercept workflow events
        // Convert to AGUI-compatible responses
        // Stream back to frontend
    }
}
```

**Usage**:
```csharp
builder.AddWorkflow("MyWorkflow", factoryMethod)
    .AddAsAIAgent();  // Automatically wraps with AGUIWorkflowAgent
```

### Dependency Injection Pattern

**All services use constructor injection:**

```csharp
// ❌ DON'T: Service locator anti-pattern
public class MyService
{
    public void DoWork()
    {
        var logger = ServiceProvider.GetService<ILogger>();  // WRONG
    }
}

// ✅ DO: Constructor injection
public class MyService(ILogger<MyService> logger, IChatClient chatClient)
{
    private readonly ILogger _logger = logger;
    private readonly IChatClient _chatClient = chatClient;
    
    public async Task DoWork()
    {
        _logger.LogInformation("Working...");
        await _chatClient.CompleteAsync("...");
    }
}
```

---

## Observability & Monitoring

### Local Development - .NET Aspire Dashboard

**Primary tool for local debugging and monitoring:**

When running `aspire run`, the Aspire dashboard opens automatically at `http://localhost:15888`

**Dashboard features:**
- **Resources view**: See all running services (agentic-api, agentic-ui) with health status
- **Console logs**: Real-time logs from both backend and frontend
- **Structured logs**: Filter by log level, timestamp, service, and search text
- **Traces**: Distributed tracing across services and dependencies
- **Metrics**: Request counts, duration, and custom metrics

**Accessing logs:**
```bash
# Start the application
aspire run

# Dashboard opens at http://localhost:15888
# Navigate to:
# - "Resources" tab to see service status
# - "Console" tab for real-time logs
# - "Structured" tab for filterable logs
# - "Traces" tab for request tracing
# - "Metrics" tab for performance data
```

**Adding structured logging:**

```csharp
public class MyExecutor(ILogger<MyExecutor> logger)
{
    public async Task ExecuteAsync(...)
    {
        // Structured logging automatically appears in Aspire dashboard
        logger.LogInformation("Processing request for user {UserId}", userId);
        
        try
        {
            await DoWork();
            logger.LogInformation("Request completed in {Duration}ms", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing request");
            throw;
        }
    }
}
```

**Adding custom metrics:**

```csharp
using System.Diagnostics.Metrics;

public class MyExecutor
{
    private static readonly Meter _meter = new("AgenticApi.Workflows");
    private static readonly Counter<long> _requestCounter = _meter.CreateCounter<long>("workflow_requests");
    private static readonly Histogram<double> _duration = _meter.CreateHistogram<double>("workflow_duration_ms");

    public async Task ExecuteAsync(...)
    {
        _requestCounter.Add(1);
        var sw = Stopwatch.StartNew();
        
        try
        {
            await DoWork();
            _duration.Record(sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing request");
            throw;
        }
    }
}
```

### Production - Application Insights

**For Azure deployments:**
- Connection string automatically injected into Container Apps
- Automatic request/dependency tracking
- Console logs forwarded to Application Insights

**Access production logs:**
```bash
# Azure Portal
az monitor app-insights query \
  --app <app-insights-name> \
  --analytics-query "traces | order by timestamp desc | take 100"
```

---

## Documentation Standards

### Required Documentation Updates

**When adding features, UPDATE these files:**

1. **`/specs/features/<feature-name>.md`** - Feature specification
2. **`/specs/docs/architecture/overview.md`** - If architecture changes
3. **`/specs/adr/NNNN-<decision>.md`** - Create new MADR for significant decisions
4. **`README.md`** - If setup or usage changes
5. **`AGENTS.md`** - This file (if agent instructions change)

### MADR (Architecture Decision Record) Template

**Location**: `/specs/adr/0001-decision-title.md`

```markdown
# [Number]. [Title]

Date: YYYY-MM-DD

## Status

Accepted | Proposed | Deprecated | Superseded by [NNNN-new-decision.md]

## Context and Problem Statement

[Describe the context and problem statement]

## Decision Drivers

* [driver 1]
* [driver 2]

## Considered Options

* [option 1]
* [option 2]
* [option 3]

## Decision Outcome

Chosen option: "[option 1]", because [justification].

### Consequences

**Positive:**
* [improvement or benefit]

**Negative:**
* [compromising quality attribute or trade-off]
```

### Documentation Commands

```bash
# Serve documentation locally (requires MkDocs)
mkdocs serve

# Build documentation
mkdocs build

# Deploy to GitHub Pages
mkdocs gh-deploy
```

---

## Dependency Management

### Version Pinning Strategy

**Backend**: Use EXACT versions (no wildcards)
```xml
<PackageReference Include="Microsoft.Agents.AI.Workflows" Version="1.0.0-preview.251125.1" />
```

**Frontend**: Use caret ranges, rely on lock file
```json
"@copilotkit/react-core": "^1.10.6"
```

### Update Strategy (Not Yet Implemented)

**Required: Add Dependabot configuration**

Create `.github/dependabot.yml`:

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/src/agentic-api"
    schedule:
      interval: "weekly"
    groups:
      microsoft-agents:
        patterns:
          - "Microsoft.Agents.*"

  - package-ecosystem: "npm"
    directory: "/src/agentic-ui"
    schedule:
      interval: "weekly"
    groups:
      copilotkit:
        patterns:
          - "@copilotkit/*"
      ag-ui:
        patterns:
          - "@ag-ui/*"
```

---

## CI/CD Pipeline

### Setup CI/CD with Azure Developer CLI

**Configure CI/CD pipeline:**

```bash
# Configure CI/CD pipeline (interactive)
azd pipeline config

# Follow prompts to:
# 1. Select provider (GitHub Actions or Azure DevOps)
# 2. Authenticate to provider
# 3. Configure deployment credentials
# 4. Set up pipeline configuration

# This will automatically create:
# - .github/workflows/azure-dev.yml (for GitHub Actions)
# - Required secrets in your repository
# - Service principal for Azure authentication
```

**What gets created:**

- **CI/CD workflow file**: Automatically generated with build, test, and deploy jobs
- **Azure credentials**: Service principal created and stored as repository secret
- **Deployment automation**: Triggered on push to main branch

**Verify pipeline:**

```bash
# Check pipeline status
azd pipeline config --show
```

---

## Known Limitations & Technical Debt

### Critical Gaps

1. ✅ **No User Authentication** - Public endpoints, no identity management
2. ⚠️ **Limited Test Coverage** - Backend testing framework established, needs test implementation
3. ✅ **No Input Validation** - Prompt injection and abuse possible
4. ✅ **No Rate Limiting** - Cost overruns and abuse possible
5. ✅ **No Error Handling** - Minimal try-catch coverage
6. ✅ **No Monitoring Dashboards** - Basic telemetry only

### Architectural Limitations

1. **Single Agent Only** - Only "DummyWorkflow" implemented
2. **No Data Persistence** - Cosmos DB provisioned but unused
3. **No RAG Capabilities** - AI Search provisioned but unused
4. **Stateless Design** - No conversation history or context retention

### Technical Debt

- **Unused Azure Resources**: Cosmos DB and AI Search provisioned but not used (~$75/month cost)
- **No SBOM Generation**: Supply chain security not tracked
- **No License Compliance**: Dependency licenses not audited
- **No Load Testing**: Performance characteristics unknown

---

## Quick Reference

### Common Tasks

| Task | Command |
|------|---------|
| Run locally (Aspire) | `aspire run` |
| Build all services | `./build.sh` |
| Build backend | `cd src/agentic-api && dotnet build` |
| Build frontend | `cd src/agentic-ui && npm run build` |
| Run backend tests | `cd tests/agentic-api-tests && dotnet test` |
| Run frontend tests | `cd src/agentic-ui && npm test` |
| Deploy to Azure | `azd deploy` |
| View logs | `azd logs` or Azure Portal |
| Update dependencies | `dotnet restore && npm update` |

### Important Directories

| Path | Purpose |
|------|---------|
| `src/agentic-api/` | Backend API service (.NET 10) |
| `src/agentic-ui/` | Frontend web app (Next.js 16) |
| `infra/` | Infrastructure as Code (Bicep) |
| `specs/docs/` | Comprehensive documentation |
| `specs/features/` | Feature specifications |
| `specs/adr/` | Architecture Decision Records |

### Important Files

| File | Purpose |
|------|---------|
| `apphost.cs` | .NET Aspire orchestration |
| `azure.yaml` | Azure Developer CLI config |
| `src/agentic-api/Program.cs` | Backend startup configuration |
| `src/agentic-api/AGUIWorkflowAgent.cs` | AGUI protocol adapter |
| `src/agentic-ui/app/page.tsx` | Frontend landing page |
| `infra/main.bicep` | Main infrastructure template |

### Azure Resources

| Resource | Purpose | Status |
|----------|---------|--------|
| Azure OpenAI | AI model hosting (GPT-5 Mini) | ✅ Used |
| Container Apps | Application hosting | ✅ Used |
| Azure AI Foundry | AI project management | ✅ Used |
| Application Insights | Telemetry and monitoring | ✅ Used |
| Cosmos DB | NoSQL database | ⚠️ Provisioned, unused |
| Azure AI Search | Vector search | ⚠️ Provisioned, unused |

---

## Getting Help

### Documentation Resources

- **Project Documentation**: `/specs/docs/`
- **Feature Specs**: `/specs/features/`
- **Architecture Decisions**: `/specs/adr/`
- **Microsoft Docs MCP**: Use `microsoft.docs.mcp` tool for official Microsoft documentation
- **Context7 MCP**: Use `context7` tool for library-specific documentation (fallback)

### External Resources

- **Microsoft Agent Framework**: [Limited preview documentation available]
- **.NET Aspire**: [learn.microsoft.com/aspire](https://learn.microsoft.com/aspire)
- **Azure Container Apps**: [learn.microsoft.com/azure/container-apps](https://learn.microsoft.com/azure/container-apps)
- **Next.js 16**: [nextjs.org/docs](https://nextjs.org/docs)
- **CopilotKit**: [docs.copilotkit.ai](https://docs.copilotkit.ai)

### Troubleshooting

**For authentication issues**: Check `DefaultAzureCredential` chain and run `az login`
**For build issues**: Verify .NET 10.0 SDK and Node.js 20.x are installed
**For deployment issues**: Run `azd auth login` and verify Azure subscription access
**For runtime issues**: Check Application Insights logs in Azure Portal

---

**Last Updated**: December 5, 2025  
**Project Status**: Prototype/Demo

**For human developers**: Refer to [README.md](README.md) for project overview and [/specs/docs/](specs/docs/) for comprehensive documentation.