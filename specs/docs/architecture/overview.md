# Architecture Overview

## System Overview

**agentic-shell-dotnet** is a microservices-based AI agent application that provides an intelligent chat interface powered by the Microsoft Agent Framework. The application consists of two main services orchestrated via .NET Aspire, deployed to Azure Container Apps, and integrated with Azure AI services.

## Architecture Style

### Pattern: **Microservices with Backend-for-Frontend**

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
           │
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

## High-Level Architecture

### Component Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                     Azure Cloud                              │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │       Azure Container Apps Environment             │    │
│  │                                                    │    │
│  │  ┌─────────────────┐      ┌─────────────────┐   │    │
│  │  │  agentic-ui     │◄────►│  agentic-api    │   │    │
│  │  │  Container App  │      │  Container App  │   │    │
│  │  │  Next.js        │      │  .NET 10        │   │    │
│  │  └─────────────────┘      └────────┬────────┘   │    │
│  │         │                           │             │    │
│  └─────────┼───────────────────────────┼─────────────┘    │
│            │                           │                   │
│            │                           ▼                   │
│            │              ┌─────────────────────────┐     │
│            │              │  Azure OpenAI Service   │     │
│            │              │  Deployment: gpt-5-mini │     │
│            │              └─────────────────────────┘     │
│            │                           │                   │
│            │                           ▼                   │
│            │              ┌─────────────────────────┐     │
│            │              │ Azure AI Foundry        │     │
│            │              │ Project & Connections   │     │
│            │              └─────────────────────────┘     │
│            │                                               │
│            ▼                           ▼                   │
│  ┌──────────────────┐    ┌─────────────────────────┐     │
│  │ Azure Monitor    │    │ Azure Cosmos DB         │     │
│  │ App Insights     │    │ (Serverless, SQL API)   │     │
│  │ Log Analytics    │    │ Database: agentic-storage│    │
│  └──────────────────┘    └─────────────────────────┘     │
│                                      │                     │
│                                      ▼                     │
│                           ┌─────────────────────────┐     │
│                           │ Azure AI Search         │     │
│                           │ (Basic tier)            │     │
│                           └─────────────────────────┘     │
│                                                            │
│  ┌──────────────────────────────────────────────────┐    │
│  │  Azure Container Registry                        │    │
│  │  Stores: agentic-api:latest, agentic-ui:latest  │    │
│  └──────────────────────────────────────────────────┘    │
│                                                            │
└────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│              Local Development (Aspire)                      │
│                                                              │
│  ┌─────────────┐                                            │
│  │  apphost.cs │ ◄─── Orchestrates local services          │
│  └──────┬──────┘                                            │
│         │                                                    │
│         ├──► Launch agentic-api (http://localhost:5149)    │
│         └──► Launch agentic-ui (http://localhost:3000)     │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

## Core Components

### 1. Frontend Service: agentic-ui

**Technology:** Next.js 16 + React 19 + TypeScript

**Location:** `src/agentic-ui/`

**Responsibilities:**
- Render the web user interface
- Provide chat interface via CopilotKit
- Proxy agent requests to backend
- Handle user interactions and state management

**Key Files:**
- `app/page.tsx` - Main landing page with chat sidebar
- `app/layout.tsx` - Root layout with CopilotKit provider
- `app/api/copilotkit/route.ts` - Backend agent connection endpoint
- `next.config.ts` - Next.js configuration (standalone mode)

**Port Configuration:**
- Production: 3000
- Local Development: 3000
- Exposed publicly via Container Apps ingress

**Environment Variables:**
- `AGENT_API_URL` - Backend API endpoint
- `PORT` - HTTP server port

### 2. Backend Service: agentic-api

**Technology:** ASP.NET Core 10 + Microsoft Agent Framework

**Location:** `src/agentic-api/`

**Responsibilities:**
- Host AI agent workflows
- Execute agent logic using Microsoft Agent Framework
- Integrate with Azure OpenAI Service
- Provide AGUI-compatible endpoints for frontend
- Handle agent execution, streaming, and event processing

**Key Files:**
- `Program.cs` - Application startup and configuration
- `AGUIWorkflowAgent.cs` - AGUI protocol adapter wrapper
- `Workflows/DummyWorkflow.cs` - Demo workflow implementation
- `appsettings.json` - Configuration settings

**Port Configuration:**
- Production: 8080
- Local Development: 5149 (HTTP), 7126 (HTTPS)
- Internal communication via Container Apps environment

**Environment Variables (Required):**
- `AZURE_OPENAI_ENDPOINT` - Azure OpenAI endpoint URL
- `AZURE_OPENAI_DEPLOYMENT_NAME` - Model deployment name
- `AZURE_CLIENT_ID` - Managed identity client ID (production)
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Telemetry

**Environment Variables (Provisioned but unused in current code):**
- `AZURE_COSMOS_ENDPOINT`
- `AZURE_AI_SEARCH_ENDPOINT`
- `AZURE_AI_PROJECT_ENDPOINT`

### 3. Orchestrator: .NET Aspire AppHost

**Technology:** C# Script (.NET Aspire 13.0)

**Location:** `apphost.cs`

**Responsibilities:**
- Local development orchestration
- Service discovery and configuration
- Environment variable injection
- Dependency management between services

**Configuration:**
- Parameters: `openAiEndpoint`, `openAiDeployment`
- Service references: agentic-api, agentic-ui
- Wait-for dependencies: UI waits for API to be ready

**Development Experience:**
- Single command to launch both services: Run `apphost.cs`
- Automatic service registration and discovery
- Dashboard for monitoring services

## Data Flow Architecture

### User Request Flow

```
1. User types message in browser
   │
   ▼
2. CopilotSidebar (React) captures input
   │
   ▼
3. CopilotKit runtime sends request to /api/copilotkit
   │
   ▼
4. Next.js API route (route.ts) forwards to AGENT_API_URL
   │
   ▼
5. HttpAgent (@ag-ui/client) sends HTTP POST to agentic-api
   │
   ▼
6. agentic-api receives request at AGUI endpoint (MapAGUI)
   │
   ▼
7. AGUIWorkflowAgent wrapper receives messages
   │
   ▼
8. DummyWorkflow executes:
   a. DummyChatInputExecutor extracts user message
   b. GreetingExecutor calls Azure OpenAI via IChatClient
   c. AI generates response
   │
   ▼
9. AGUIWorkflowAgent converts WorkflowOutputEvent to text
   │
   ▼
10. Response streams back through HTTP to frontend
    │
    ▼
11. CopilotKit renders AI response in sidebar
    │
    ▼
12. User sees response in chat interface
```

### Authentication Flow (Production)

```
1. Container App starts with User Assigned Managed Identity
   │
   ▼
2. DefaultAzureCredential detects managed identity
   │
   ▼
3. Automatic token acquisition for Azure services
   │
   ▼
4. Tokens used for:
   - Azure OpenAI API calls
   - Cosmos DB access (if used)
   - AI Search access (if used)
   - AI Foundry connections
```

### Local Development Authentication

```
1. Developer runs locally via Aspire
   │
   ▼
2. DefaultAzureCredential attempts (in order):
   a. Environment variables
   b. Visual Studio credential
   c. VS Code credential
   d. Azure CLI credential
   │
   ▼
3. First successful method provides token
   │
   ▼
4. Token used for Azure service authentication
```

## Design Patterns

### 1. Workflow Pattern (Microsoft Agent Framework)

**Implementation:** `DummyWorkflow.cs`

**Pattern Structure:**
```csharp
WorkflowBuilder
  .WithName("WorkflowName")
  .AddExecutor(ChatInputExecutor)
  .AddEdge(ChatInputExecutor → GreetingExecutor)
  .WithOutputFrom(GreetingExecutor)
  .Build()
```

**Executor Pattern:**
- **ChatInput Executor:** Accepts input and converts to internal events
- **Processing Executors:** Handle business logic and AI interactions
- **Output Executor:** Returns results to the framework

**Event-Driven:**
- Executors communicate via typed events (e.g., `UserInputEvent`)
- Events flow through the workflow graph
- Asynchronous processing with cancellation support

### 2. Adapter Pattern (AGUI Integration)

**Implementation:** `AGUIWorkflowAgent.cs`

**Purpose:** Adapt Microsoft Agent Framework to AGUI protocol

**Pattern:**
```csharp
public class AGUIWorkflowAgent : DelegatingAIAgent
{
    // Wraps inner agent
    public AGUIWorkflowAgent(AIAgent innerAgent) : base(innerAgent) { }
    
    // Intercepts and transforms events
    public override IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(...)
    {
        // Convert WorkflowOutputEvent → TextContent
        // Convert ExecutorCompletedEvent → TextContent
    }
}
```

**Responsibilities:**
- Convert workflow events to AGUI-compatible responses
- Handle streaming response transformation
- Serialize complex objects to JSON text
- Maintain agent metadata (ID, role, timestamps)

### 3. Factory Pattern (Workflow Construction)

**Implementation:** `DummyWorkflowFactory.cs`

**Purpose:** Encapsulate workflow creation with DI

**Pattern:**
```csharp
public class DummyWorkflowFactory
{
    private readonly ILogger<T> _logger;
    private readonly IChatClient _chatClient;
    
    // Dependencies injected
    public DummyWorkflowFactory(ILogger<T> logger, IChatClient chatClient) { }
    
    // Factory method
    public Workflow BuildWorkflow(string name)
    {
        var executor1 = new DummyChatInputExecutor(_logger);
        var executor2 = new GreetingExecutor(_logger, _chatClient);
        return new WorkflowBuilder(executor1)
            .AddEdge(executor1, executor2)
            .Build();
    }
}
```

### 4. Dependency Injection (ASP.NET Core)

**Implementation:** `Program.cs`

**Services Registered:**
```csharp
// HTTP client for external requests
builder.Services.AddHttpClient().AddLogging();

// AGUI support
builder.Services.AddAGUI();

// AI Chat client (Azure OpenAI)
builder.Services.AddSingleton<IChatClient>(_ => 
    new AzureOpenAIClient(endpoint, credential)
        .GetChatClient(deploymentName)
        .AsIChatClient());

// Workflow factory
builder.Services.AddSingleton<DummyWorkflowFactory>();

// OpenAI conversation/response handlers
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// Register workflow as AI agent
builder.AddWorkflow("DummyWorkflow", (sp, name) => 
    sp.GetRequiredService<DummyWorkflowFactory>().BuildWorkflow(name))
.AddAsAIAgent();
```

### 5. Repository Pattern (Not Implemented)

**Current State:** No repository or data access layer

**Observation:** Cosmos DB and AI Search are provisioned but unused

**Implication:** Future feature implementation will require DAL design

## Architectural Decisions

### 1. Microservices Over Monolith

**Decision:** Separate frontend and backend services

**Rationale:**
- Independent scaling of UI and agent processing
- Technology specialization (React for UI, .NET for AI)
- Clear separation of concerns
- Easier to add additional agent services in future

**Trade-offs:**
- Network latency between services
- More complex deployment
- Distributed system challenges

### 2. .NET Aspire for Orchestration

**Decision:** Use .NET Aspire for local development and service orchestration

**Rationale:**
- Simplified local development experience
- Automatic service discovery
- Built-in observability dashboard
- Cloud-agnostic abstractions

**Benefits:**
- Single command to run entire application
- Environment parity (local to cloud)
- Easier onboarding for developers

### 3. Microsoft Agent Framework

**Decision:** Build agents using Microsoft Agent Framework (preview)

**Rationale:**
- Native .NET integration
- Workflow-based agent design
- Extensible executor pattern
- Microsoft support and roadmap

**Risks:**
- Preview software with breaking changes
- Limited community and documentation
- Potential migration costs if framework changes

### 4. CopilotKit for Frontend

**Decision:** Use CopilotKit for chat UI instead of custom implementation

**Rationale:**
- Pre-built React components
- Agent integration abstractions
- Streaming support
- Active maintenance

**Trade-offs:**
- Third-party dependency
- Less UI customization
- Framework lock-in

### 5. Azure Container Apps for Hosting

**Decision:** Deploy both services to Azure Container Apps

**Rationale:**
- Serverless container platform
- Auto-scaling capabilities
- Integrated with Azure services
- Cost-effective for variable workloads

**Benefits:**
- No infrastructure management
- Automatic HTTPS and certificate management
- Built-in load balancing
- Easy integration with Azure AI services

## Non-Functional Architecture

### Scalability

**Current Design:**
- Stateless services (no session affinity required)
- Horizontal scaling configured in Container Apps
  - Min replicas: 1
  - Max replicas: 10
- Autoscaling based on HTTP traffic and CPU

**Limitations:**
- No caching layer
- No rate limiting implemented
- No load testing performed
- Database connection pooling not configured

### Reliability

**Current Implementation:**
- Health endpoints: Not explicitly defined
- Retry logic: DefaultAzureCredential has built-in retries
- Circuit breakers: Not implemented
- Graceful degradation: Not implemented

**Observability:**
- Application Insights integrated
- Logging configured (Console + App Insights)
- No custom metrics or dashboards
- No distributed tracing configuration

### Performance

**Optimizations Present:**
- Next.js standalone mode (smaller Docker image)
- Aspire service discovery (reduced latency)
- Streaming responses (lower perceived latency)

**Performance Concerns:**
- No caching strategy
- No CDN for static assets
- Cold start latency in Container Apps
- No performance testing results available

### Security

**Authentication:**
- Managed Identity for Azure services (production)
- DefaultAzureCredential for local development
- No user authentication implemented

**Authorization:**
- RBAC roles assigned to managed identities
- No application-level authorization
- No API keys or rate limiting

**Network Security:**
- HTTPS enforced by Container Apps
- Public endpoints (no private networking)
- CORS configured for frontend-backend communication

## Deployment Architecture

### Container Images

**Build Process:**
- Dockerfiles for both services
- Multi-stage builds for optimization
- Base images from Microsoft Container Registry

**Registry:**
- Azure Container Registry
- Images: `agentic-api:latest`, `agentic-ui:latest`
- Managed identity for ACR pull access

### Infrastructure Provisioning

**Tool:** Azure Bicep (IaC)

**Resources Created:**
- Resource Group
- Container Apps Environment
- 2 Container Apps (API + UI)
- Container Registry
- Azure OpenAI with deployment
- Azure AI Foundry Project
- Cosmos DB (serverless)
- Azure AI Search
- 2 Managed Identities
- Log Analytics Workspace
- Application Insights

**Configuration:**
- Parameterized via `main.parameters.json`
- Environment-based naming with `resourceToken`
- RBAC role assignments for managed identities

### Deployment Process

**Tool:** Azure Developer CLI (azd)

**Workflow:**
1. `azd provision` - Create Azure resources
2. Build Docker images
3. Push to Azure Container Registry
4. Update Container Apps with new images
5. `postprovision` hook - Additional configuration

**Configuration:** `azure.yaml`

## System Boundaries

### What's In Scope

- Chat-based AI interaction
- Agent workflow execution
- Azure OpenAI integration
- Web-based UI
- Development tooling and standards

### What's Out of Scope (Currently Unused)

- Cosmos DB persistence (provisioned but unused)
- Azure AI Search functionality (provisioned but unused)
- User authentication/authorization
- Multi-tenant support
- API rate limiting
- Monitoring and alerting
- Error tracking and reporting
- Performance optimization

### Future Extensibility Points

**Designed for:**
- Adding new workflow executors
- Registering multiple agents
- Integrating additional Azure AI services
- Implementing data persistence
- Adding vector search capabilities

**Extension Locations:**
- `src/agentic-api/Workflows/` - New workflow implementations
- `infra/` - Additional Azure resources
- `src/agentic-ui/app/` - New UI pages and features

## Architectural Constraints

### Technical Constraints

1. **Preview Software Dependency**
   - Microsoft Agent Framework is preview
   - Potential breaking changes
   - Limited documentation

2. **.NET 10.0 Requirement**
   - Latest .NET version required
   - May have compatibility issues
   - Limited hosting options

3. **Azure-Only Deployment**
   - Tightly coupled to Azure services
   - Azure OpenAI required
   - No multi-cloud strategy

4. **No Persistence Layer**
   - Stateless by design
   - No session management
   - No data storage implemented

### Business Constraints

1. **Demo/Prototype Nature**
   - "Dummy" workflow for testing
   - Not production-ready
   - Minimal error handling

2. **No SLA Guarantees**
   - No uptime monitoring
   - No disaster recovery
   - No backup strategy
