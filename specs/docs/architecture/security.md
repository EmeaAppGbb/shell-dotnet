# Security Architecture and Patterns

## Overview

This document analyzes the security posture of the agentic-shell-dotnet application, documenting implemented security patterns, identifying gaps, and assessing risks.

## Authentication Architecture

### Production Authentication (Azure)

#### Managed Identity Pattern

**Implementation:** Both Container Apps use User Assigned Managed Identities

**Configuration Location:** `infra/resources.bicep`

```bicep
module agenticApiIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.2.1' = {
  name: '${abbrs.managedIdentityUserAssignedIdentities}agenticApi-${resourceToken}'
  location: location
}
```

**Identity Assignment:**
```bicep
managedIdentities:{
  systemAssigned: false
  userAssignedResourceIds: [agenticApiIdentity.outputs.resourceId]
}
```

**Authentication Flow:**
1. Container App starts with managed identity
2. Azure platform injects identity credentials
3. `DefaultAzureCredential` automatically detects managed identity
4. Token acquisition happens transparently
5. Tokens used for Azure service authentication

**Services Authenticated:**
- Azure OpenAI Service
- Azure AI Foundry
- Cosmos DB (when used)
- Azure AI Search (when used)
- Azure Container Registry (for image pull)

### Local Development Authentication

**Implementation Location:** `src/agentic-api/Program.cs`

```csharp
new AzureOpenAIClient(
    new Uri(endpoint), 
    new DefaultAzureCredential()
)
```

**Credential Chain (DefaultAzureCredential attempts in order):**
1. Environment variables (`AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`)
2. Workload Identity (Kubernetes)
3. Managed Identity
4. Visual Studio credentials
5. Visual Studio Code credentials
6. Azure CLI credentials
7. Azure PowerShell credentials
8. Interactive browser authentication (last resort)

**Developer Authentication Setup:**
- Developers authenticate via `az login`
- Credentials cached locally
- No secrets in code or configuration
- Automatic credential discovery

### User Authentication: NOT IMPLEMENTED

**Current State:** No user authentication mechanism exists

**Implications:**
- Anyone with the URL can access the application
- No user identity tracking
- No conversation privacy or isolation
- No usage attribution or billing
- No access control or authorization

**Risk Level:** 🔴 **CRITICAL**

## Authorization Architecture

### Azure RBAC Implementation

#### Backend Service (agentic-api) Roles

**Managed Identity:** `agenticApiIdentity`

**Role Assignments (Resource Group Scope):**

1. **Azure AI Developer** (`64702f94-c441-49e6-a78b-ef80e0188fee`)
   ```bicep
   resource agenticApibackendRoleAzureAIDeveloperRG 
     'Microsoft.Authorization/roleAssignments@2020-04-01-preview'
   ```
   - **Purpose:** Access to Azure AI Foundry projects
   - **Permissions:** Read/write AI resources, manage connections

2. **Cognitive Services User** (`a97b65f3-24c7-4388-baec-2e87135dc908`)
   ```bicep
   resource agenticApibackendRoleCognitiveServicesUserRG 
     'Microsoft.Authorization/roleAssignments@2020-04-01-preview'
   ```
   - **Purpose:** Call Azure OpenAI APIs
   - **Permissions:** Read, list keys, call inference endpoints

3. **Search Index Data Contributor** (via module)
   - **Purpose:** Read/write to Azure AI Search indexes
   - **Permissions:** Query, upload, delete documents

4. **Search Service Contributor** (via module)
   - **Purpose:** Manage search service configuration
   - **Permissions:** Read/write search service settings

5. **Cosmos DB Built-in Data Contributor** (via module)
   - **Purpose:** Read/write Cosmos DB data
   - **Permissions:** CRUD operations on containers

6. **ACR Pull** (`7f951dda-4ed3-4680-a7ca-43fe172d538d`)
   - **Purpose:** Pull container images
   - **Permissions:** Read images from ACR

#### Frontend Service (agentic-ui) Roles

**Managed Identity:** `agenticUiIdentity`

**Role Assignments:**
- **ACR Pull** - Container image access
- **Search Index Data Contributor** - Search access (if needed)
- **Search Service Contributor** - Search management
- **Cosmos DB Data Contributor** - Database access (if needed)

#### User Principal Roles (Development)

**When `principalType == 'User'`:**

Assigns roles to the developer's Azure AD account for local development:

1. **Azure AI Developer** - AI Foundry access
2. **Cognitive Services User** - OpenAI access
3. **Search Index Data Contributor** - Search access
4. **Search Service Contributor** - Search management

### Application-Level Authorization: NOT IMPLEMENTED

**Current State:** No authorization logic in application code

**Missing:**
- No role-based access control (RBAC)
- No permission checks
- No resource ownership validation
- No API scopes or claims
- No tenant isolation

**Risk Level:** 🔴 **CRITICAL**

## Data Protection

### Data in Transit

#### HTTPS Enforcement

**Container Apps:**
- Automatic HTTPS ingress
- TLS certificates managed by Azure
- HTTP to HTTPS redirect (default)

**Configuration:** `infra/resources.bicep`
```bicep
ingressTargetPort: 8080  // Internal HTTP
// Azure Container Apps automatically provides HTTPS externally
```

**External URLs:**
- `https://agentic-ui.<environment>.azurecontainerapps.io`
- `https://agentic-api.<environment>.azurecontainerapps.io`

#### Service-to-Service Communication

**Frontend to Backend:**
- Uses HTTPS when deployed (Container Apps internal networking)
- Plain HTTP in local development

**Backend to Azure Services:**
- All Azure SDK calls use HTTPS
- TLS 1.2+ enforced by Azure services

### Data at Rest

#### Cosmos DB Encryption

**Configuration:** `infra/resources.bicep`

```bicep
module cosmos 'br/public:avm/res/document-db/database-account:0.8.1' = {
  params: {
    capabilitiesToAdd: [ 'EnableServerless' ]
  }
}
```

**Encryption:**
- Automatic encryption at rest (Azure-managed keys)
- No customer-managed keys (CMK) configured
- Transparent encryption (no code changes required)

**Status:** Provisioned but **UNUSED in current code**

#### Azure AI Search Encryption

**Encryption:**
- Automatic encryption at rest
- Azure-managed keys
- No CMK configured

**Status:** Provisioned but **UNUSED in current code**

#### Container Registry Encryption

- Default encryption enabled
- Image layers encrypted at rest
- No CMK configured

### Data in Use

**Current State:** No encryption for data in memory

**Azure OpenAI Processing:**
- Prompts and responses transmitted over HTTPS
- Data processed in Azure OpenAI service
- No local encryption of AI conversations

**Missing:**
- No conversation history persistence
- No PII detection or redaction
- No data classification
- No audit logging of data access

## Input Validation & Sanitization

### Frontend Input Validation

**Location:** `src/agentic-ui/app/page.tsx`

**Current Implementation:**
```typescript
<CopilotSidebar
  labels={{
    placeholder: "Ask me anything...",
  }}
  instructions="You are a helpful AI assistant..."
>
```

**Validation Present:** ❌ **NONE**

**Missing:**
- No input length limits
- No content filtering
- No profanity or harmful content detection
- No injection attack prevention
- No rate limiting on user input

### Backend Input Validation

**Location:** `src/agentic-api/Workflows/DummyWorkflow.cs`

```csharp
private async ValueTask<UserInputEvent> HandleChatMessagesAsync(
    List<ChatMessage> messages,
    IWorkflowContext context,
    CancellationToken cancellationToken = default)
{
    var lastUserMessage = messages.LastOrDefault(m => m.Role == ChatRole.User);
    var userInput = lastUserMessage?.Text ?? "Hello";
    // No validation or sanitization
    return new UserInputEvent { Input = userInput };
}
```

**Validation Present:** ❌ **NONE**

**Missing:**
- No input validation
- No SQL injection protection (N/A - no SQL used currently)
- No XSS protection (text-only responses)
- No prompt injection prevention
- No maximum token limits

**Risk Level:** 🟡 **MEDIUM** (limited attack surface due to simple implementation)

## API Security

### Endpoint Protection

#### Frontend API Routes

**Location:** `src/agentic-ui/app/api/copilotkit/route.ts`

```typescript
export const POST = async (req: NextRequest) => {
  const { handleRequest } = copilotRuntimeNextJSAppRouterEndpoint({
    runtime,
    serviceAdapter,
    endpoint: "/api/copilotkit",
  });
  return handleRequest(req);
};
```

**Security Issues:**
- ❌ No authentication required
- ❌ No authorization checks
- ❌ No rate limiting
- ❌ No request validation
- ❌ No CSRF protection
- ❌ No API keys

**Exposure:** Public endpoint accepting any request

#### Backend API Endpoints

**Location:** `src/agentic-api/Program.cs`

**Endpoints Registered:**
```csharp
app.MapOpenAIResponses();        // OpenAI response endpoint
app.MapOpenAIConversations();    // OpenAI conversation endpoint
app.MapAGUI("/", dummyAgent);    // AGUI endpoint at root
app.MapDevUI();                  // Dev UI (development only)
```

**Security Issues:**
- ❌ No authentication middleware
- ❌ No authorization policies
- ❌ No API versioning
- ❌ No rate limiting
- ✅ DevUI only in development mode

### CORS Configuration

**Location:** `infra/resources.bicep`

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

**Security Assessment:**
- ✅ Origin restricted to frontend URL
- ⚠️ Wildcard methods (`*`) - overly permissive
- ❌ No credentials policy specified
- ❌ No exposed headers configuration

**Improvement Needed:**
```bicep
corsPolicy: {
  allowedOrigins: ['https://agentic-ui...']
  allowedMethods: ['POST', 'OPTIONS']  // Be explicit
  allowCredentials: false
  maxAge: 3600
}
```

### API Documentation Security

**OpenAPI/Swagger:**
- OpenAPI package included: `Microsoft.AspNetCore.OpenApi`
- No explicit Swagger UI configuration found
- No API documentation endpoint exposed
- No security schemes defined in OpenAPI spec

## Secrets Management

### Production Secrets

**Current State:** ✅ **NO SECRETS IN CODE OR CONFIGURATION**

**Implementation:**
- All authentication via managed identity
- No connection strings in code
- No API keys stored
- No passwords or tokens

**Environment Variables (Not Secrets):**
```csharp
string endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"];
string deploymentName = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"];
```

These are **endpoints and names**, not secrets.

### Local Development Secrets

**Configuration:** `src/agentic-api/agentic-api.csproj`

```xml
<UserSecretsId>f55bcee4-7990-4eec-b22c-b148868634bd</UserSecretsId>
```

**User Secrets Support:**
- .NET User Secrets enabled
- Secrets stored outside repository
- Loaded automatically in Development environment

**Current Usage:** None detected (not using secrets currently)

### Azure Key Vault Integration

**Current State:** ❌ **NOT IMPLEMENTED**

**Missing:**
- No Key Vault provisioned
- No secret references in configuration
- No certificate storage
- No encryption key management

**Future Use Cases:**
- API keys for third-party services
- Database connection strings (if needed)
- Certificate management
- Encryption keys for CMK

## Network Security

### Public vs. Private Networking

**Current Configuration:**

#### Azure OpenAI
```bicep
properties: {
  publicNetworkAccess: 'Enabled'
  networkAcls: {
    defaultAction: 'Allow'
    virtualNetworkRules: []
    ipRules: []
  }
}
```

**Security Posture:** 🟡 Public access enabled

#### Cosmos DB
```bicep
params: {
  networkRestrictions: {
    ipRules: []
    virtualNetworkRules: []
    publicNetworkAccess: 'Enabled'
  }
}
```

**Security Posture:** 🟡 Public access enabled

#### Azure AI Search
```bicep
params: {
  publicNetworkAccess: 'Enabled'
}
```

**Security Posture:** 🟡 Public access enabled

#### Container Apps
- Public ingress enabled
- External HTTPS endpoints
- No private networking or VNet integration

### Virtual Network Integration: NOT IMPLEMENTED

**Missing:**
- No Virtual Network (VNet)
- No private endpoints
- No network security groups (NSGs)
- No application gateway or WAF
- No service endpoints

**Implications:**
- All services accessible from internet
- No network isolation
- No defense in depth
- Potential for unauthorized access

**Risk Level:** 🟡 **MEDIUM** (mitigated by RBAC)

## Security Headers

### Current Implementation

**Location:** `src/agentic-api/Program.cs`

**Security Headers:** ❌ **NOT IMPLEMENTED**

**Missing Headers:**
- `Strict-Transport-Security` (HSTS)
- `X-Content-Type-Options`
- `X-Frame-Options`
- `X-XSS-Protection`
- `Content-Security-Policy`
- `Referrer-Policy`
- `Permissions-Policy`

**Risk:** Potential for clickjacking, XSS, and other client-side attacks

### Next.js Security Headers

**Location:** `src/agentic-ui/next.config.ts`

**Security Headers:** ❌ **NOT CONFIGURED**

**Default Next.js Security:**
- Some headers set by framework
- Not explicitly configured in config file

## Logging and Auditing

### Audit Logging

**Current Implementation:**

**Backend Logging:** `src/agentic-api/Program.cs`
```csharp
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
```

**Log Levels:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**What's Logged:**
- Application startup events
- HTTP requests (ASP.NET Core default)
- Agent workflow events
- AI interactions

**What's NOT Logged:**
- User authentication events (no auth)
- Authorization decisions (no authz)
- Data access events
- Security events (login failures, suspicious activity)
- API usage per user

### Application Insights Integration

**Configuration:** Environment variable injection

```bicep
env: [
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: monitoring.outputs.applicationInsightsConnectionString
  }
]
```

**Telemetry Collected:**
- HTTP requests and responses
- Exceptions and errors
- Performance metrics
- Dependencies (Azure SDK calls)

**Missing:**
- Custom security events
- Business metric logging
- Audit trail
- User activity tracking

### Log Retention and Analysis

**Current Setup:**
- Log Analytics Workspace provisioned
- 30-day default retention (not specified in config)
- No log queries or alerts configured
- No SIEM integration

## Vulnerability Management

### Dependency Scanning: NOT CONFIGURED

**Current State:**
- No Dependabot configuration
- No automated security scanning
- No vulnerability alerts
- No supply chain security

**Missing Files:**
- `.github/dependabot.yml`
- `.github/workflows/security.yml`

### Container Image Scanning

**Current State:**
- No image scanning in CI/CD
- Azure Container Registry vulnerability scanning not enabled
- No admission control policies

**Risk:** Vulnerable base images or dependencies may be deployed

### Code Scanning: NOT CONFIGURED

**Missing:**
- No CodeQL analysis
- No static application security testing (SAST)
- No secret scanning
- No code quality gates
