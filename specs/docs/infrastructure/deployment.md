# Deployment Architecture and Infrastructure

## Overview

This document describes the deployment architecture, infrastructure provisioning, and operational aspects of the agentic-shell-dotnet application.

## Deployment Targets

### Production: Azure Container Apps

**Platform:** Azure Container Apps on Azure Cloud

**Region Configuration:** `infra/main.bicep`
```bicep
param location string  // Primary location
param aiDeploymentsLocation string  // AI services location
```

**Deployment Model:**
- Serverless containers
- Pay-per-use pricing
- Auto-scaling based on load
- Managed HTTPS and certificates

### Local Development: .NET Aspire

**Platform:** Local machine via .NET Aspire orchestration

**Orchestrator:** `apphost.cs`

**Benefits:**
- Single command to run entire application
- Automatic service discovery
- Integrated observability dashboard
- Cloud-agnostic development

## Infrastructure as Code

### Tooling: Azure Bicep

**Main Template:** `infra/main.bicep`
- Subscription-level deployment
- Resource group creation
- Module orchestration
- Output variables for configuration

**Resource Modules:** `infra/resources.bicep`
- Azure resources provisioning
- Network and identity configuration
- RBAC role assignments

**AI Services Module:** `infra/ai-project.bicep`
- Azure Cognitive Services account
- OpenAI model deployments
- Azure AI Foundry project

### Infrastructure Components

#### 1. Resource Organization

**Resource Group:** `rg-${environmentName}`

**Location:** Parameterized by environment

**Tags:**
```bicep
var tags = {
  'azd-env-name': environmentName
}
```

**Additional Service Tags:**
```bicep
tags: union(tags, { 'azd-service-name': 'agentic-api' })
```

#### 2. Compute Resources

##### Container Apps Environment

**Module:** `br/public:avm/res/app/managed-environment:0.4.5`

**Configuration:**
```bicep
params: {
  name: '${abbrs.appManagedEnvironments}${resourceToken}'
  logAnalyticsWorkspaceResourceId: monitoring.outputs.logAnalyticsWorkspaceResourceId
  zoneRedundant: false
}
```

**Capabilities:**
- Shared environment for both services
- Log Analytics integration
- Internal networking
- Auto-scaling platform

##### Backend Container App (agentic-api)

**Module:** `br/public:avm/res/app/container-app:0.8.0`

**Configuration:**
```bicep
params: {
  name: 'agentic-api'
  ingressTargetPort: 8080
  scaleMinReplicas: 1
  scaleMaxReplicas: 10
  containers: [{
    image: '...'
    resources: {
      cpu: json('0.5')
      memory: '1.0Gi'
    }
  }]
}
```

**Scaling Configuration:**
- **Min Replicas:** 1 (always at least one instance)
- **Max Replicas:** 10 (upper scaling limit)
- **CPU Allocation:** 0.5 vCPU per replica
- **Memory Allocation:** 1 GB per replica

**Network Configuration:**
- **Ingress:** HTTP/HTTPS on port 8080
- **External Access:** Yes
- **CORS Policy:** Configured for frontend origin

##### Frontend Container App (agentic-ui)

**Configuration:**
```bicep
params: {
  name: 'agentic-ui'
  ingressTargetPort: 3000
  scaleMinReplicas: 1
  scaleMaxReplicas: 10
  containers: [{
    resources: {
      cpu: json('0.5')
      memory: '1.0Gi'
    }
  }]
}
```

**Similar scaling and resource configuration as backend**

**Dependency:**
```bicep
uses: [agentic-api]  // Depends on backend API
```

#### 3. Container Registry

**Module:** `br/public:avm/res/container-registry/registry:0.1.1`

**Configuration:**
```bicep
params: {
  name: '${abbrs.containerRegistryRegistries}${resourceToken}'
  publicNetworkAccess: 'Enabled'
  roleAssignments: [
    // agentic-api managed identity: AcrPull role
    // agentic-ui managed identity: AcrPull role
  ]
}
```

**Access Control:**
- Managed identities granted `AcrPull` role
- No anonymous pull access
- Public network enabled for now

**Image Storage:**
- `agentic-api:latest`
- `agentic-ui:latest`

#### 4. Data Stores

##### Cosmos DB

**Module:** `br/public:avm/res/document-db/database-account:0.8.1`

**Configuration:**
```bicep
params: {
  name: '${abbrs.documentDBDatabaseAccounts}${resourceToken}'
  locations: [{
    failoverPriority: 0
    isZoneRedundant: false
    locationName: location
  }]
  sqlDatabases: [{
    name: 'agentic-storage'
    containers: []  // No containers defined
  }]
  capabilitiesToAdd: ['EnableServerless']
}
```

**Characteristics:**
- **Mode:** Serverless (pay-per-request)
- **API:** SQL API
- **Database:** `agentic-storage`
- **Containers:** None created
- **Replication:** Single region
- **Status:** ⚠️ **Provisioned but UNUSED in code**

**Access Control:**
- Built-in Data Contributor role assigned to both service identities
- User principal also granted access for development

##### Azure AI Search

**Module:** `br/public:avm/res/search/search-service:0.10.0`

**Configuration:**
```bicep
params: {
  name: '${abbrs.searchSearchServices}${resourceToken}'
  sku: 'basic'
  replicaCount: 1
  managedIdentities: { systemAssigned: true }
  disableLocalAuth: false
  publicNetworkAccess: 'Enabled'
}
```

**Characteristics:**
- **Tier:** Basic
- **Replicas:** 1
- **Authentication:** RBAC + API key (dual)
- **Status:** ⚠️ **Provisioned but UNUSED in code**

**Access Control:**
- Search Index Data Contributor
- Search Service Contributor
- Assigned to service identities and user principal

#### 5. AI Services

##### Azure Cognitive Services Account

**Module:** Custom `infra/ai-project.bicep`

**Configuration:**
```bicep
resource aiAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' = {
  sku: { name: 'S0' }
  kind: 'AIServices'
  properties: {
    allowProjectManagement: true
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}
```

**AI Model Deployment:**
```bicep
deployments: [{
  name: 'gpt5MiniDeployment'  // Parameter: deploymentName
  model: {
    name: 'gpt-5-mini'
    format: 'OpenAI'
    version: '2025-08-07'
  }
  sku: {
    name: 'GlobalStandard'
    capacity: 10
  }
}]
```

**Model Details:**
- **Model:** gpt-5-mini
- **Version:** 2025-08-07 (future date, likely placeholder)
- **SKU:** GlobalStandard
- **Capacity:** 10 units
- **Deployment Name:** `gpt5MiniDeployment` (default)

##### Azure AI Foundry Project

**Nested Resource:**
```bicep
resource project 'projects' = {
  name: envName
  properties: {
    description: '${envName} Project'
    displayName: '${envName}Project'
  }
}
```

**Purpose:**
- AI project management
- Connection management
- Model deployment coordination
- Experimentation workspace

**Search Connection:** `infra/modules/ai-search-conn.bicep`
- Connects AI Foundry to AI Search
- Enables RAG scenarios (if implemented)

#### 6. Identity & Access Management

##### Managed Identities

**Backend Identity:**
```bicep
module agenticApiIdentity 
  'br/public:avm/res/managed-identity/user-assigned-identity:0.2.1' = {
  name: '${abbrs.managedIdentityUserAssignedIdentities}agenticApi-${resourceToken}'
}
```

**Frontend Identity:**
```bicep
module agenticUiIdentity 
  'br/public:avm/res/managed-identity/user-assigned-identity:0.2.1' = {
  name: '${abbrs.managedIdentityUserAssignedIdentities}agenticUi-${resourceToken}'
}
```

**Usage:**
- No secrets in configuration
- Automatic token acquisition
- Access to Azure resources via RBAC

##### RBAC Role Assignments

**Backend Service Roles:**
1. Azure AI Developer (Resource Group scope)
2. Cognitive Services User (Resource Group scope)
3. Search Index Data Contributor
4. Search Service Contributor
5. Cosmos DB Data Contributor
6. ACR Pull

**Developer Roles (for local development):**
- Same roles assigned to user principal when `principalType == 'User'`

#### 7. Observability

##### Log Analytics Workspace

**Module:** `br/public:avm/ptn/azd/monitoring:0.1.0`

**Resources Created:**
```bicep
params: {
  logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
  applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
  applicationInsightsDashboardName: '${abbrs.portalDashboards}${resourceToken}'
}
```

**Components:**
- Log Analytics Workspace - Log aggregation
- Application Insights - Application telemetry
- Dashboard - Pre-built monitoring dashboard

**Integration:**
- Connection string injected into Container Apps
- Automatic telemetry collection
- Distributed tracing support

## Deployment Process

### Azure Developer CLI (azd)

**Configuration:** `azure.yaml`

```yaml
name: agentic-shell-dotnet
services:
  agentic-api:
    project: src/agentic-api
    host: containerapp
    language: dotnet
  agentic-ui:
    project: src/agentic-ui
    host: containerapp
    language: ts
```

### Deployment Workflow

#### 1. Infrastructure Provisioning

**Command:** `azd provision`

**Process:**
1. Read `azure.yaml` configuration
2. Load `infra/main.bicep` template
3. Create subscription deployment
4. Create resource group
5. Deploy all Azure resources via Bicep modules
6. Configure RBAC role assignments
7. Run post-provision hooks

**Post-Provision Hooks:** `azure.yaml`
```yaml
hooks:
  postprovision:
    posix:
      shell: sh
      run: ./infra/scripts/postprovision.sh
    windows:
      shell: pwsh
      run: ./infra/scripts/postprovision.ps1
```

**Hook Purpose:**
- Additional configuration steps
- Seeding data (if needed)
- Validation checks

#### 2. Application Deployment

**Command:** `azd deploy`

**Process:**

**For agentic-api (ASP.NET Core):**
1. Build .NET project (`dotnet build`)
2. Create Docker image using `Dockerfile`
3. Tag image: `<acr>.azurecr.io/agentic-api:latest`
4. Push to Azure Container Registry
5. Update Container App with new image

**For agentic-ui (Next.js):**
1. Install npm dependencies (`npm ci`)
2. Build Next.js app (`npm run build`)
3. Create Docker image using `Dockerfile`
4. Tag image: `<acr>.azurecr.io/agentic-ui:latest`
5. Push to Azure Container Registry
6. Update Container App with new image

#### 3. Container Image Handling

**Fetch Latest Image Module:** `infra/modules/fetch-container-image.bicep`

```bicep
params: {
  exists: agenticApiExists  // Does image exist?
  name: 'agentic-api'
}
output: {
  image: '...'  // Image URI or fallback
}
```

**Fallback Image:**
```bicep
image: agenticApiFetchLatestImage.outputs.?containers[?0].?image 
  ?? 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
```

**Purpose:** Allow deployment before first build (uses hello-world placeholder)

### Environment Configuration

#### Parameters File: `infra/main.parameters.json`

**Structure:**
```json
{
  "environmentName": { "value": "" },
  "location": { "value": "" },
  "aiDeploymentsLocation": { "value": "" },
  "principalId": { "value": "" },
  "principalType": { "value": "" }
}
```

**Parameter Sources:**
- Environment variables
- `.azure/<environment>/.env` file
- Interactive prompts during `azd provision`

#### Application Settings: `apphost.settings.json`

**Note:** File is `.gitignore`d (not committed)

**Expected Structure:**
```json
{
  "parameters": {
    "openAiEndpoint": "...",
    "openAiDeployment": "..."
  }
}
```

**Template:** `apphost.settings.template.json` (committed)

## Container Images

### Backend Dockerfile: `src/agentic-api/Dockerfile`

**Multi-Stage Build:**

**Stage 1: Base Runtime**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
USER app
```

**Stage 2: Build**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["agentic-api.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build
```

**Stage 3: Publish**
```dockerfile
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false
```

**Stage 4: Final**
```dockerfile
FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "agentic-api.dll"]
```

**Optimizations:**
- Multi-stage build (smaller final image)
- Non-root user (`app`)
- Layer caching with separate restore step

### Frontend Dockerfile: `src/agentic-ui/Dockerfile`

**Multi-Stage Build:**

**Stage 1: Dependencies**
```dockerfile
FROM node:20-slim AS deps
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm install
```

**Stage 2: Build**
```dockerfile
FROM base AS builder
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm install --include=optional
RUN npm install --no-save lightningcss-linux-x64-gnu @tailwindcss/oxide-linux-x64-gnu
COPY . .
RUN npm run build
```

**Stage 3: Production**
```dockerfile
FROM base AS runner
ENV NODE_ENV=production
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs
COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static
USER nextjs
EXPOSE 3000
CMD ["node", "server.js"]
```

**Optimizations:**
- Next.js standalone output (minimal runtime)
- Non-root user (`nextjs`)
- Native binaries for Linux x64 platform
- Static file optimization

**Next.js Configuration:** `src/agentic-ui/next.config.ts`
```typescript
const nextConfig: NextConfig = {
  output: 'standalone',  // Self-contained deployment
  serverExternalPackages: ['pino', 'thread-stream'],
};
```

## Local Development Setup

### .NET Aspire Orchestration

**Orchestrator:** `apphost.cs`

**C# Script with Package References:**
```csharp
#:sdk Aspire.AppHost.Sdk@13.0.0
#:package Aspire.Hosting.JavaScript@13.0.0
#:package Aspire.Hosting.Azure.CognitiveServices@13.0.0
#:package Aspire.Hosting.Azure.AIFoundry@13.0.0-preview.1.25560.3
```

**Service Registration:**

**Backend:**
```csharp
var api = builder.AddCSharpApp("agentic-api", "./src/agentic-api")
    .WithEnvironment("AZURE_OPENAI_ENDPOINT", openAiEndpoint)
    .WithEnvironment("AZURE_OPENAI_DEPLOYMENT_NAME", openAiDeployment);
```

**Frontend:**
```csharp
builder.AddJavaScriptApp("agentic-ui", "./src/agentic-ui")
    .WithRunScript("dev")
    .WithNpm(installCommand: "ci")
    .WithEnvironment("AGENT_API_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();
```

**Key Features:**
- **Service Discovery:** `api.GetEndpoint("http")` automatically resolved
- **Dependency Management:** `WaitFor(api)` ensures backend starts first
- **Configuration:** Environment variables injected
- **Observability:** Built-in Aspire dashboard

### Running Locally

**Option 1: Via Aspire**
```bash
# Run the app host
dotnet run --project apphost.cs
# or
./apphost.cs
```

**Option 2: Individual Services**
```bash
# Terminal 1: Backend
cd src/agentic-api
dotnet run

# Terminal 2: Frontend
cd src/agentic-ui
npm run dev
```

### Development Container

**Configuration:** `.devcontainer/devcontainer.json`

**Base Image:** `mcr.microsoft.com/devcontainers/python:1-3.12-bullseye`

**Features Installed:** (See Technology Stack documentation)

**Post-Create Command:**
```bash
pip install --user apm-cli && apm --version
```

**Purpose:**
- Consistent development environment
- All tools pre-installed
- Works locally or in GitHub Codespaces
- One-click environment setup

## Networking Architecture

### Public Endpoints

**Frontend URL:** `https://agentic-ui.<domain>.azurecontainerapps.io`
- Public ingress enabled
- HTTPS enforced
- Accessible from internet

**Backend URL:** `https://agentic-api.<domain>.azurecontainerapps.io`
- Public ingress enabled
- HTTPS enforced
- CORS configured for frontend

### Internal Communication

**Frontend to Backend:**
- Uses Container Apps environment internal DNS
- HTTP communication (HTTPS external)
- Service-to-service direct connection

**Backend to Azure Services:**
- Public endpoints (for now)
- HTTPS encrypted
- Authenticated via managed identity

### Future: Private Networking (Not Implemented)

**Potential Improvements:**
- VNet integration for Container Apps
- Private endpoints for Azure services
- No public internet access
- Network security groups (NSGs)

## Operational Considerations

### Health Checks: NOT IMPLEMENTED

**Current State:**
- No health endpoints defined
- No liveness/readiness probes
- Container Apps use default checks

**Recommendation:**
```csharp
// Add to Program.cs
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");
```

### Monitoring & Alerting: BASIC

**Current State:**
- Application Insights connected
- Automatic telemetry collection
- No custom metrics
- No dashboards configured
- No alerts defined

**Available Data:**
- Request telemetry
- Dependency telemetry
- Exception telemetry
- Performance counters

### Backup & Recovery: NOT CONFIGURED

**Current State:**
- No backup strategy
- No disaster recovery plan
- Single-region deployment
- No data replication

**Risk:**
- Cosmos DB data loss (if used)
- No regional failover
- Potential data unavailability

### Scaling Behavior

**Autoscaling Triggers:**
- HTTP request queue length
- CPU utilization
- Memory utilization (default Azure metrics)

**Scaling Limits:**
- Min: 1 replica (no scale-to-zero)
- Max: 10 replicas
- Scale-up: Relatively fast
- Scale-down: Gradual (Azure default)

**Cold Start:**
- Not applicable (min replicas = 1)
- Always at least one warm instance

### Cost Optimization

**Current Configuration:**

**Container Apps:**
- Min replicas: 1 (constant cost)
- 0.5 vCPU + 1 GB RAM per replica
- Always-on cost even with no traffic

**Cosmos DB:**
- Serverless (pay-per-request)
- $0 when unused
- Cost-effective for variable workloads

**Azure OpenAI:**
- Pay-per-token
- GlobalStandard SKU
- Capacity: 10 units

**Potential Optimizations:**
- Scale-to-zero for non-production environments
- Consumption-based pricing for dev/test
- Reserved capacity for predictable workloads
- Cost alerts and budgets

## Deployment Outputs

**Output Variables:** `infra/main.bicep`

```bicep
output AZURE_CONTAINER_REGISTRY_ENDPOINT string
output AZURE_RESOURCE_AGENTIC_API_ID string
output AZURE_RESOURCE_AGENTIC_UI_ID string
output AZURE_RESOURCE_AGENTIC_STORAGE_ID string
output AZURE_AI_PROJECT_ENDPOINT string
output AZURE_RESOURCE_AI_PROJECT_ID string
output AZURE_AI_SEARCH_ENDPOINT string
output AZURE_RESOURCE_SEARCH_ID string
output AZURE_OPENAI_ENDPOINT string
output AZURE_OPENAI_DEPLOYMENT_NAME string
```

**Usage:**
- Captured by azd in `.azure/<env>/.env`
- Used for subsequent deployments
- Available to application configuration
- Referenced in documentation

## CI/CD Integration: NOT IMPLEMENTED

**Current State:**
- No GitHub Actions workflows
- No Azure Pipelines
- Manual deployment via `azd deploy`
- No automated testing in pipeline

**Missing:**
- Build workflow
- Test workflow
- Deployment workflow
- Environment promotion strategy
- Rollback procedures
