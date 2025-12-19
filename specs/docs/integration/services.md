# External Service Dependencies

## Azure Services

The application depends on several Azure services for operation and observability.

### 1. Azure Cosmos DB

**Purpose**: NoSQL database for temperature measurements

**Service Type**: Azure Cosmos DB (SQL API, Serverless)

**Dependency Level**: **Critical** - Application cannot function without it

**Configuration**:
- **Account Name**: Generated (e.g., `cosmos-abc123`)
- **Endpoint**: `https://{accountName}.documents.azure.com:443/`
- **Database**: `TemperatureDb`
- **Container**: `Temperatures`
- **Authentication**: Managed Identity (production), Connection string (development)

**Integration Point**: Backend only

**Files**:
- Infrastructure: [infra/resources.bicep](../../../infra/resources.bicep#L66-L100)
- Backend: [src/backend/Program.cs](../../../src/backend/Program.cs#L19)

**SLA**: 99.99% uptime

**Cost**: Serverless (pay-per-request)
- **Storage**: $0.25/GB/month
- **Requests**: Variable RU charges

**Failure Impact**: Application cannot read or write temperature data

---

### 2. Azure Application Insights

**Purpose**: Application performance monitoring and telemetry

**Service Type**: Azure Monitor / Application Insights

**Dependency Level**: **Optional** - Application works without it, but no observability

**Configuration**:
- **Name**: Generated (e.g., `appi-abc123`)
- **Connection String**: Injected via environment variable
- **Instrumentation**: Automatic via ASP.NET Core and Vite

**Integration Point**: Both backend and frontend

**Files**:
- Infrastructure: [infra/resources.bicep](../../../infra/resources.bicep#L26-L35)
- Backend env: [infra/resources.bicep](../../../infra/resources.bicep#L177)
- Frontend env: [infra/resources.bicep](../../../infra/resources.bicep#L262)

**Data Collected**:
- HTTP requests and responses
- Exceptions and errors
- Dependencies (Cosmos DB calls)
- Custom events (if implemented)
- Browser page views (frontend)

**Retention**: 90 days (default)

**Cost**: Pay-per-GB ingested
- First 5GB/month: Free
- Additional: $2.30/GB

**Failure Impact**: No monitoring, but application continues to function

---

### 3. Azure Log Analytics

**Purpose**: Centralized log storage and analysis

**Service Type**: Azure Monitor / Log Analytics Workspace

**Dependency Level**: **Optional** - Used for Container Apps logs

**Configuration**:
- **Name**: Generated (e.g., `log-abc123`)
- **Integration**: Container Apps Environment linked

**Integration Point**: Infrastructure (Container Apps)

**Files**:
- Infrastructure: [infra/resources.bicep](../../../infra/resources.bicep#L26-L35)

**Data Collected**:
- Container stdout/stderr logs
- Container Apps system logs
- Application Insights data (linked)

**Retention**: 30 days (default)

**Cost**: Pay-per-GB ingested
- First 5GB/month: Free
- Additional: $2.30/GB

**Failure Impact**: No log collection, but application continues to function

---

### 4. Azure Container Registry

**Purpose**: Store Docker container images

**Service Type**: Azure Container Registry

**Dependency Level**: **Deployment-time only** - Required for deploying new versions

**Configuration**:
- **Name**: Generated (e.g., `crabcabc123`)
- **SKU**: Basic
- **Public Network Access**: Enabled
- **Authentication**: Managed Identity pull access

**Integration Point**: Deployment pipeline (azd)

**Files**:
- Infrastructure: [infra/resources.bicep](../../../infra/resources.bicep#L37-L54)

**Images Stored**:
- `backend:latest` - Backend .NET application
- `frontend:latest` - Frontend Vue.js application

**Pull Access**:
- Backend managed identity
- Frontend managed identity

**Cost**: $5/month + storage costs

**Failure Impact**: Cannot deploy new versions, but existing deployment unaffected

---

### 5. Azure Container Apps Environment

**Purpose**: Hosting environment for containerized applications

**Service Type**: Azure Container Apps (Managed Environment)

**Dependency Level**: **Critical** - Hosts both frontend and backend

**Configuration**:
- **Name**: Generated (e.g., `cae-abc123`)
- **Ingress**: External (HTTPS enabled)
- **Zone Redundant**: No
- **Monitoring**: Linked to Log Analytics

**Integration Point**: Both backend and frontend

**Files**:
- Infrastructure: [infra/resources.bicep](../../../infra/resources.bicep#L56-L64)

**Features**:
- Auto-scaling (1-10 replicas)
- HTTPS ingress
- Internal DNS
- Managed certificates

**Cost**: $0.000012/second per vCPU + $0.000002/second per GB RAM

**Failure Impact**: Complete application outage

---

## External APIs

### Weather API

**Status**: ❌ **NOT USED**

**Current Implementation**: Weather data is **randomly generated** in the backend

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L44-L60)

**Why No External API**:
- Demo/starter application
- Avoids external API keys and costs
- Simplifies deployment

**If Implementing**:
- Options: OpenWeatherMap, WeatherAPI, Azure Maps Weather
- Requires: API key management, error handling, caching

---

## Authentication Providers

**Status**: ❌ **NOT IMPLEMENTED**

**No authentication or identity provider** is currently integrated.

**Potential Providers**:
- Azure AD B2C
- Azure Entra ID
- Auth0
- OAuth providers (Google, GitHub, etc.)

---

## Message Queues / Event Systems

**Status**: ❌ **NOT IMPLEMENTED**

**No message queues or event systems** are used.

**Potential Use Cases**:
- Azure Service Bus - For async processing
- Azure Event Grid - For event-driven architecture
- Azure Event Hubs - For streaming data

---

## CDN / Static Asset Services

**Status**: ❌ **NOT IMPLEMENTED**

**Frontend assets served directly** from Nginx in Container Apps.

**Potential Services**:
- Azure Front Door - Global CDN with WAF
- Azure CDN - Content delivery network
- Cloudflare - Third-party CDN

---

## Caching Services

**Status**: ❌ **NOT IMPLEMENTED**

**No caching layer** is used. All requests hit the database.

**Potential Services**:
- Azure Cache for Redis - Distributed cache
- Cosmos DB integrated cache - Built-in caching
- In-memory cache - ASP.NET Core memory cache

---

## Email / Notification Services

**Status**: ❌ **NOT IMPLEMENTED**

**No email or notification services** are integrated.

**Potential Services**:
- Azure Communication Services - Email, SMS, etc.
- SendGrid - Email service
- Twilio - SMS service

---

## Storage Services

**Status**: ❌ **NOT IMPLEMENTED**

**No blob storage or file storage** is used.

**Potential Services**:
- Azure Blob Storage - Object storage
- Azure Files - File shares
- Azure Data Lake - Big data storage

---

## AI / ML Services

**Status**: ❌ **NOT IMPLEMENTED**

**No AI or machine learning services** are integrated.

**Potential Services**:
- Azure OpenAI - GPT models
- Azure Cognitive Services - Vision, speech, etc.
- Azure Machine Learning - Custom ML models

---

## Service Dependency Map

```
Application
├── [CRITICAL] Azure Cosmos DB
│   └── Backend → Cosmos DB SDK → HTTPS
│
├── [OPTIONAL] Application Insights
│   ├── Backend → App Insights SDK → HTTPS
│   └── Frontend → App Insights JS → HTTPS
│
├── [OPTIONAL] Log Analytics
│   └── Container Apps → System Logs → HTTPS
│
├── [DEPLOYMENT] Container Registry
│   ├── Backend Pull → Managed Identity → HTTPS
│   └── Frontend Pull → Managed Identity → HTTPS
│
└── [CRITICAL] Container Apps Environment
    ├── Backend Container (port 8080)
    └── Frontend Container (port 80)
```

---

## Service Health Monitoring

### Current Monitoring
✅ **Application Insights** - Automatic dependency tracking
✅ **Azure Monitor** - Service health alerts (portal only)

### Missing Monitoring
❌ **No health check endpoints** - Cannot detect partial failures
❌ **No custom alerts** - No proactive notifications
❌ **No status page** - No public service status
❌ **No dependency health checks** - Don't validate Cosmos DB connectivity

---

## Service Failover and Resilience

### Current Resilience
✅ **Container Apps auto-restart** - Failed containers restart automatically
✅ **Multi-replica capability** - Can scale to 10 replicas
✅ **Cosmos DB multi-region** - Capability (not configured)

### Missing Resilience
❌ **No retry policies** - Failed requests don't retry
❌ **No circuit breakers** - No protection against cascading failures
❌ **No fallback logic** - No graceful degradation
❌ **No multi-region deployment** - Single region only

---

## Service Costs (Estimated)

### Monthly Cost Breakdown (Low Traffic)

| Service | Cost | Notes |
|---------|------|-------|
| Cosmos DB | $5-10 | Serverless, depends on usage |
| Container Apps | $10-20 | 2 apps, 0.5 vCPU, 1GB RAM each |
| Container Registry | $5 | Basic SKU |
| Application Insights | $0-5 | First 5GB free |
| Log Analytics | $0-5 | First 5GB free |
| **Total** | **$20-45/month** | For low-traffic development |

### High Traffic Scenario

| Service | Cost | Notes |
|---------|------|-------|
| Cosmos DB | $50-100 | Higher RU consumption |
| Container Apps | $100-200 | Scaled to max replicas |
| Container Registry | $5 | No change |
| Application Insights | $20-50 | More telemetry data |
| Log Analytics | $20-50 | More log data |
| **Total** | **$195-405/month** | For production workload |

---

## Service Alternatives

### Database Alternatives
- **Azure SQL Database** - Relational database (if structured data needed)
- **MongoDB Atlas** - Managed MongoDB
- **Azure Table Storage** - Key-value store (simpler, cheaper)

### Hosting Alternatives
- **Azure App Service** - PaaS for web apps (simpler than containers)
- **Azure Kubernetes Service** - Full Kubernetes (more complex, more control)
- **Azure Static Web Apps** - Frontend only (with API routes)

### Monitoring Alternatives
- **Datadog** - Third-party APM
- **New Relic** - Third-party APM
- **Grafana Cloud** - Open-source monitoring

---

## Service Configuration Management

### Connection Strings
**Stored In**: Environment variables (Container Apps)

**Configuration Files**:
- Backend: `appsettings.json` (minimal, mostly env vars)
- Frontend: `import.meta.env.VITE_BACKEND_URL`

**Secret Management**: ❌ None (should use Azure Key Vault)

### Managed Identities
**Used For**:
- Cosmos DB access
- Container Registry pull

**Benefits**:
- No passwords in code
- Automatic rotation
- Azure AD-based access

**File**: [infra/resources.bicep](../../../infra/resources.bicep)

---

## Service Limits and Quotas

### Cosmos DB (Serverless)
- **Max RU/s**: 5,000 RU/s per partition
- **Max Storage**: 50 GB per container
- **Max Item Size**: 2 MB

### Container Apps
- **Max Replicas**: 10 (configured)
- **Max vCPU**: 2.0 per container
- **Max Memory**: 4 GB per container

### Container Registry
- **Max Storage**: 5 TB (Basic SKU)
- **Max Repositories**: Unlimited
- **Webhooks**: 2 (Basic SKU)

---

## Service Support and SLAs

| Service | SLA | Support Level |
|---------|-----|---------------|
| Cosmos DB | 99.99% | Standard Azure Support |
| Container Apps | 99.95% | Standard Azure Support |
| Container Registry | 99.9% | Standard Azure Support |
| Application Insights | 99.9% | Standard Azure Support |

**Support Plan**: Depends on Azure subscription (Basic, Standard, Professional Direct)
