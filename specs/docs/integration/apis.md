# API Documentation

## Overview

This document describes all API endpoints, integration points, and external service dependencies for the agentic-shell-dotnet application.

## API Architecture

### API Style: Agent-First Architecture

**Pattern:** The application exposes an **Agent GUI (AGUI) protocol endpoint** rather than traditional REST APIs.

**Rationale:**
- Built for agent-to-agent and UI-to-agent communication
- Streaming support for real-time responses
- Event-driven architecture
- Compatible with CopilotKit and other agent frameworks

## Backend API Endpoints

### Service Base URL

**Production:** `https://agentic-api.<environment>.azurecontainerapps.io`

**Local Development:** `http://localhost:5149`

**Port Configuration:**
- Production: 8080 (internal), HTTPS (external)
- Local: 5149 (HTTP), 7126 (HTTPS)

### Endpoint Registration

**Location:** `src/agentic-api/Program.cs`

```csharp
app.MapOpenAIResponses();        // OpenAI response handler
app.MapOpenAIConversations();    // OpenAI conversation handler
app.MapAGUI("/", dummyAgent);    // AGUI protocol endpoint (root path)

if (builder.Environment.IsDevelopment())
{
    app.MapDevUI();  // Developer testing UI
}
```

### 1. AGUI Endpoint (Primary)

**Endpoint:** `POST /`

**Protocol:** AGUI (Agent GUI Protocol)

**Purpose:** Execute agent workflows and return responses

**Implementation:** `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore`

#### Request Format

**Content-Type:** `application/json`

**Request Body (Simplified):**
```json
{
  "messages": [
    {
      "role": "user",
      "content": "What is the weather today?"
    }
  ],
  "agent": "my_agent",
  "stream": true
}
```

**Key Fields:**
- `messages` - Array of chat messages with role and content
- `agent` - Agent identifier (unused in current implementation)
- `stream` - Boolean indicating streaming response preference

#### Response Format

**Content-Type:** `text/event-stream` (streaming) or `application/json` (non-streaming)

**Streaming Response (SSE format):**
```
data: {"type":"response_update","content":"Hello","role":"assistant"}

data: {"type":"response_update","content":" there","role":"assistant"}

data: {"type":"response_complete"}
```

**Non-Streaming Response:**
```json
{
  "response_id": "...",
  "message_id": "...",
  "content": "Hello there! How can I help you today?",
  "role": "assistant",
  "created_at": "2024-12-01T10:00:00Z"
}
```

#### Agent Processing Flow

**Workflow Execution:** `src/agentic-api/AGUIWorkflowAgent.cs`

1. Receive AGUI request
2. Extract messages
3. Pass to `AGUIWorkflowAgent.RunStreamingAsync()`
4. Execute `DummyWorkflow`
5. Convert `WorkflowOutputEvent` to AGUI response
6. Stream or return complete response

#### Error Responses

**Error Format:**
```json
{
  "error": {
    "code": "...",
    "message": "...",
    "details": "..."
  }
}
```

**HTTP Status Codes:**
- `200 OK` - Success
- `400 Bad Request` - Invalid input
- `500 Internal Server Error` - Processing error
- `503 Service Unavailable` - Backend services unavailable

**Current Error Handling:**
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error in greeting executor");
    return new AgentRunResponse { 
        Text = "Hi! I had trouble processing your message, but I'm here to help!" 
    };
}
```

**Limitations:**
- Generic error messages
- No specific error codes
- Limited error context
- No retry guidance

#### Authentication: NOT IMPLEMENTED

**Current State:** ❌ No authentication required

**Security Issues:**
- Public endpoint
- Anyone can send requests
- No rate limiting
- No usage tracking

#### Rate Limiting: NOT IMPLEMENTED

**Current State:** ❌ No rate limiting

**Risks:**
- Unlimited requests per IP/user
- Potential cost overruns
- DoS vulnerability
- Resource exhaustion

### 2. OpenAI Responses Endpoint

**Endpoint:** `POST /openai/responses` (assumed path)

**Purpose:** Handle OpenAI-compatible response requests

**Implementation:** `app.MapOpenAIResponses();`

**Source:** `Microsoft.Agents.AI.Hosting.OpenAI` package

**Status:** ⚠️ **Registered but not documented**

**Expected Usage:**
- OpenAI API-compatible endpoint
- For direct OpenAI client integration
- Alternative to AGUI protocol

**Documentation Gap:** Exact path, request/response format unknown

### 3. OpenAI Conversations Endpoint

**Endpoint:** `POST /openai/conversations` (assumed path)

**Purpose:** Manage OpenAI-compatible conversation sessions

**Implementation:** `app.MapOpenAIConversations();`

**Source:** `Microsoft.Agents.AI.Hosting.OpenAI` package

**Status:** ⚠️ **Registered but not documented**

**Expected Features:**
- Conversation history management
- Session-based interactions
- Context persistence

**Documentation Gap:** Exact path, request/response format unknown

### 4. DevUI Endpoint (Development Only)

**Endpoint:** `/devui` (assumed path)

**Purpose:** Developer testing and debugging interface

**Implementation:** `Microsoft.Agents.AI.DevUI`

**Availability:** Development environment only

**Features:**
- Test agent workflows
- Inspect workflow state
- Debug execution flow
- View logs and events

**Security:** ✅ Correctly restricted to development

## Frontend API Endpoints

### Service Base URL

**Production:** `https://agentic-ui.<environment>.azurecontainerapps.io`

**Local Development:** `http://localhost:3000`

### 1. CopilotKit Runtime Endpoint

**Endpoint:** `POST /api/copilotkit`

**Purpose:** Proxy agent requests from frontend to backend

**Implementation:** `src/agentic-ui/app/api/copilotkit/route.ts`

**Source Code:**
```typescript
import {
  CopilotRuntime,
  ExperimentalEmptyAdapter,
  copilotRuntimeNextJSAppRouterEndpoint,
} from "@copilotkit/runtime";
import { HttpAgent } from "@ag-ui/client";

const runtime = new CopilotRuntime({
  agents: {
    my_agent: new HttpAgent({ 
      url: process.env.AGENT_API_URL || "http://localhost:5149" 
    }),
  },
});

export const POST = async (req: NextRequest) => {
  const { handleRequest } = copilotRuntimeNextJSAppRouterEndpoint({
    runtime,
    serviceAdapter,
    endpoint: "/api/copilotkit",
  });
  return handleRequest(req);
};
```

#### Request Flow

1. CopilotKit component sends POST to `/api/copilotkit`
2. Next.js API route receives request
3. CopilotRuntime processes request
4. HttpAgent forwards to backend `AGENT_API_URL`
5. Backend processes and returns response
6. CopilotRuntime streams response back to client

#### Configuration

**Backend URL:** Environment variable `AGENT_API_URL`
- Local: `http://localhost:5149` (default)
- Production: Injected by Aspire/Azure

**Agent Identifier:** `my_agent` (hardcoded)

#### Request Format

**Forwarded from CopilotKit client library**

**Expected Structure:**
```json
{
  "messages": [...],
  "agent": "my_agent",
  "stream": true,
  "metadata": {...}
}
```

#### Response Format

**Streaming:** Server-Sent Events (SSE)

**Proxied from backend AGUI endpoint**

#### Authentication: NOT IMPLEMENTED

**Current State:** ❌ No authentication

**Implications:**
- Any user with the URL can send requests
- No user identity tracking
- No conversation privacy

#### Error Handling

**Current Implementation:** Relies on CopilotKit's error handling

**Error Flow:**
1. Backend error occurs
2. HttpAgent receives error response
3. CopilotRuntime catches error
4. Error forwarded to client
5. CopilotKit displays error (default behavior)

**Customization:** ❌ No custom error handling

## External API Dependencies

### 1. Azure OpenAI Service

**Service:** Azure Cognitive Services - OpenAI

**Purpose:** Large language model inference

**SDK:** `Azure.AI.OpenAI` v2.5.0-beta.1

**Implementation:** `src/agentic-api/Program.cs`

```csharp
builder.Services.AddSingleton<IChatClient>(_ =>
    new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        .GetChatClient(deploymentName)
        .AsIChatClient());
```

#### Configuration

**Required Environment Variables:**
- `AZURE_OPENAI_ENDPOINT` - Service endpoint URL
- `AZURE_OPENAI_DEPLOYMENT_NAME` - Model deployment name (default: `gpt5MiniDeployment`)

**Model Details:**
- **Name:** gpt-5-mini
- **Version:** 2025-08-07
- **SKU:** GlobalStandard
- **Capacity:** 10 units

#### API Operations

**Chat Completion:**
```csharp
var response = await _chatClient.CompleteAsync(
    new ChatMessage(ChatRole.User, userInput),
    cancellationToken: cancellationToken
);
```

**Request Format (Abstracted by SDK):**
```json
{
  "messages": [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Hello!"}
  ],
  "model": "gpt-5-mini",
  "temperature": 0.7,
  "max_tokens": 800,
  "stream": true
}
```

**Response Format (Abstracted by SDK):**
```json
{
  "id": "chatcmpl-...",
  "object": "chat.completion",
  "created": 1234567890,
  "model": "gpt-5-mini",
  "choices": [{
    "index": 0,
    "message": {
      "role": "assistant",
      "content": "Hello! How can I help you today?"
    },
    "finish_reason": "stop"
  }],
  "usage": {
    "prompt_tokens": 10,
    "completion_tokens": 9,
    "total_tokens": 19
  }
}
```

#### Authentication

**Method:** DefaultAzureCredential (Managed Identity)

**Credential Flow:**
1. SDK requests token for `https://cognitiveservices.azure.com/.default` scope
2. DefaultAzureCredential provides token
3. Token included in HTTP Authorization header
4. Azure OpenAI validates token and processes request

#### Rate Limits

**Current Configuration:** GlobalStandard SKU with 10 capacity units

**Typical Limits:**
- Requests per minute: ~60 per capacity unit
- Tokens per minute: ~100,000 per capacity unit

**Error Handling:**
- `429 Too Many Requests` - Rate limit exceeded
- SDK has built-in retry logic with exponential backoff

#### Error Scenarios

**Possible Errors:**
1. `401 Unauthorized` - Authentication failure
2. `403 Forbidden` - Insufficient permissions
3. `429 Too Many Requests` - Rate limit exceeded
4. `500 Internal Server Error` - Azure service error
5. `503 Service Unavailable` - Service temporarily unavailable

**Current Handling:** ⚠️ Limited error handling, relies on SDK defaults

### 2. Azure AI Foundry (Provisioned, Unused)

**Service:** Azure AI Foundry Project

**Purpose:** AI project management, connections, experimentation

**SDK:** `Azure.AI.Projects` v1.2.0-beta.4

**Status:** ⚠️ **Provisioned but NOT ACTIVELY USED in code**

**Environment Variable:**
- `AZURE_AI_PROJECT_ENDPOINT` - Injected but not used

**Potential Usage:**
- Project-level configuration
- Connection management
- Model deployment coordination
- Evaluation and monitoring

### 3. Azure Cosmos DB (Provisioned, Unused)

**Service:** Azure Cosmos DB for NoSQL

**Purpose:** Document database for persistent storage

**SDK:** No SDK reference in code (but provisioned via Bicep)

**Status:** ⚠️ **Provisioned but NOT USED in code**

**Environment Variable:**
- `AZURE_COSMOS_ENDPOINT` - Injected but not used

**Database:** `agentic-storage`

**Containers:** None created

**Potential Usage:**
- Conversation history persistence
- User profile storage
- Agent state management
- Audit logs

### 4. Azure AI Search (Provisioned, Unused)

**Service:** Azure AI Search (formerly Cognitive Search)

**Purpose:** Vector search, full-text search, RAG scenarios

**SDK:** No SDK reference in code (but provisioned via Bicep)

**Status:** ⚠️ **Provisioned but NOT USED in code**

**Environment Variable:**
- `AZURE_AI_SEARCH_ENDPOINT` - Injected but not used

**Potential Usage:**
- Document retrieval (RAG)
- Semantic search
- Vector search for embeddings
- Knowledge base

### 5. Azure Container Registry

**Service:** ACR for container image storage

**Purpose:** Store and manage Docker images

**Access:** Via managed identity with AcrPull role

**Images:**
- `agentic-api:latest`
- `agentic-ui:latest`

**Usage:** Deployment-time only (not runtime API)

### 6. Azure Monitor / Application Insights

**Service:** Application Insights

**Purpose:** Telemetry, logging, monitoring

**SDK:** Automatic via ASP.NET Core and Next.js integrations

**Configuration:**
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Injected into both services

**Telemetry Collected:**
- HTTP requests/responses
- Dependencies (Azure SDK calls)
- Exceptions
- Performance metrics
- Custom events (if implemented)

**API:** Application Insights REST API (not directly called by app)

## API Client Libraries

### Frontend to Backend

**Library:** `@ag-ui/client` v0.0.41

**Component:** `HttpAgent`

**Usage:**
```typescript
new HttpAgent({ 
  url: process.env.AGENT_API_URL || "http://localhost:5149" 
})
```

**Features:**
- HTTP-based agent communication
- AGUI protocol support
- Streaming response handling
- Error propagation

### Backend to Azure OpenAI

**Library:** `Azure.AI.OpenAI` v2.5.0-beta.1

**Abstraction:** `Microsoft.Extensions.AI` v10.0.1-preview.1.25571.5

**Interface:** `IChatClient`

**Usage:**
```csharp
var chatClient = new AzureOpenAIClient(endpoint, credential)
    .GetChatClient(deploymentName)
    .AsIChatClient();
```

**Benefits:**
- Unified AI client abstraction
- Testable interface
- Provider-agnostic code
- Built-in retry and resilience

## API Versioning: NOT IMPLEMENTED

**Current State:**
- No API versioning strategy
- No version headers
- No version in URLs
- Breaking changes would affect all clients

## API Documentation: PARTIAL

**OpenAPI/Swagger:**
- Package included: `Microsoft.AspNetCore.OpenApi`
- ❌ No Swagger UI endpoint exposed
- ❌ No OpenAPI spec generation configured
- ❌ No API documentation site

## CORS Configuration

**Implementation:** `infra/resources.bicep`

```bicep
corsPolicy: {
  allowedOrigins: [
    'https://agentic-ui.${containerAppsEnvironment.outputs.defaultDomain}'
  ]
  allowedMethods: [
    '*'
  ]
}
```

**Analysis:**
- ✅ Origin restricted to frontend domain
- ⚠️ Wildcard methods (overly permissive)
- ❌ No credentials policy
- ❌ No exposed headers configuration

## WebSocket/SignalR: NOT IMPLEMENTED

**Current State:**
- No WebSocket support
- No SignalR hubs
- All communication via HTTP

**Streaming Implementation:**
- Server-Sent Events (SSE) for response streaming
- HTTP long polling alternative

## API Security

### Authentication: NOT IMPLEMENTED

**Missing:**
- No API keys
- No JWT tokens
- No OAuth/OpenID Connect
- No client certificates

### Authorization: NOT IMPLEMENTED

**Missing:**
- No role-based access control
- No scopes or permissions
- No resource-level authorization

### Input Validation: MINIMAL

**Current State:**
```csharp
var lastUserMessage = messages.LastOrDefault(m => m.Role == ChatRole.User);
var userInput = lastUserMessage?.Text ?? "Hello";
```

**Validation Present:**
- ✅ Null coalescing for missing input
- ❌ No length limits
- ❌ No content filtering
- ❌ No injection prevention

### Rate Limiting: NOT IMPLEMENTED

**Missing:**
- No request throttling
- No per-user quotas
- No IP-based limits
- No cost controls

## API Testing

### Testing Status: ❌ NONE

**Missing:**
- Unit tests for API endpoints
- Integration tests for workflows
- Contract tests
- Load tests
- Security tests

**Recommendation:**
```csharp
// Example integration test
[Fact]
public async Task AgentEndpoint_ReturnsResponse()
{
    var client = _factory.CreateClient();
    var request = new { messages = new[] { 
        new { role = "user", content = "Hello" } 
    }};
    
    var response = await client.PostAsJsonAsync("/", request);
    
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<AgentResponse>();
    Assert.NotNull(result.Content);
}
```

## Monitoring & Observability

### Application Insights Integration

**Automatic Telemetry:**
- Request duration
- Response status codes
- Dependency calls (Azure OpenAI)
- Exception tracking

**Custom Telemetry:** ❌ Not implemented

**Recommendation:**
```csharp
_telemetryClient.TrackEvent("AgentInvoked", new Dictionary<string, string>
{
    { "agent", "DummyWorkflow" },
    { "userId", userId },
    { "messageLength", userInput.Length.ToString() }
});
```

### Logging

**Current Implementation:**
```csharp
_logger.LogInformation("Dummy Workflow started with input: {Input}", userInput);
_logger.LogInformation("AI agent responded with: {Response}", responseText);
_logger.LogError(ex, "Error in greeting executor");
```

**Structured Logging:** ✅ Present

**Gaps:**
- No request IDs for tracing
- No correlation IDs
- No performance metrics
- No business metrics

## API Performance

### Current Performance: ❓ UNMEASURED

**No Performance Testing:**
- No load tests conducted
- No latency measurements
- No throughput benchmarks
- No capacity planning

**Expected Latency:**
- Frontend to backend: < 100ms (network)
- Backend to Azure OpenAI: 1-3 seconds (AI inference)
- Total user-perceived latency: 1-3.5 seconds

**Optimization Opportunities:**
- Response caching
- Request batching
- Connection pooling
- CDN for static assets

## API Limitations

### Known Limitations

1. **No Conversation Persistence**
   - Conversations lost on page refresh
   - No session management
   - No conversation retrieval

2. **Single Agent Only**
   - Hardcoded "DummyWorkflow"
   - No agent selection
   - No multi-agent orchestration

3. **No Batch Operations**
   - Single request processing only
   - No bulk operations
   - No queuing system

4. **No Offline Support**
   - Requires constant connectivity
   - No local caching
   - No offline queue

5. **No Webhooks**
   - No event notifications
   - No async processing callbacks
   - No integration webhooks
   - Bulk request processing
   - Queue-based architecture
   - Background job processing
