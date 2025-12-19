# System Architecture Overview

## Architecture Pattern

This application follows a **three-tier architecture** with clear separation of concerns:

1. **Presentation Layer** - Vue.js SPA frontend
2. **Application Layer** - ASP.NET Core Web API backend
3. **Data Layer** - Azure Cosmos DB

The deployment model is **cloud-native microservices** on Azure Container Apps, with each tier deployed as an independent container.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Internet                             │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              Azure Container Apps Environment                │
│                                                              │
│  ┌──────────────────┐         ┌─────────────────────────┐  │
│  │   Frontend App   │         │     Backend API         │  │
│  │   (Container)    │────────▶│     (Container)         │  │
│  │                  │  HTTP   │                         │  │
│  │  Vue.js + Nginx  │         │  ASP.NET Core Minimal   │  │
│  │  Port: 80        │         │  API Port: 8080         │  │
│  └──────────────────┘         └──────────┬──────────────┘  │
│                                           │                  │
└───────────────────────────────────────────┼──────────────────┘
                                            │ Cosmos SDK
                                            ▼
                              ┌──────────────────────────┐
                              │   Azure Cosmos DB        │
                              │   (Serverless)           │
                              │                          │
                              │   Database: TemperatureDb│
                              │   Container: Temperatures│
                              └──────────────────────────┘
```

## Component Architecture

### Frontend Component (Container App)
- **Technology**: Vue.js 3 + TypeScript, served by Nginx
- **Port**: 80 (HTTP)
- **Responsibilities**:
  - Render user interface
  - Handle user interactions
  - Make HTTP calls to backend API
  - Client-side routing
  - State management (Pinia)
- **Scaling**: 1-10 replicas (auto-scale)
- **Container Image**: Built from [src/frontend/Dockerfile](../../../src/frontend/Dockerfile)

**Key Files**:
- Entry: [src/frontend/src/main.ts](../../../src/frontend/src/main.ts)
- API Client: [src/frontend/src/services/api.ts](../../../src/frontend/src/services/api.ts)
- Router: [src/frontend/src/router/index.ts](../../../src/frontend/src/router/index.ts)

### Backend Component (Container App)
- **Technology**: ASP.NET Core 10 Minimal API
- **Port**: 8080 (HTTP)
- **Responsibilities**:
  - Expose REST API endpoints
  - Business logic execution
  - Data validation
  - Cosmos DB data access
  - CORS handling
- **Scaling**: 1-10 replicas (auto-scale)
- **Container Image**: Built from [src/backend/Dockerfile](../../../src/backend/Dockerfile)

**Key Files**:
- Main: [src/backend/Program.cs](../../../src/backend/Program.cs)

**API Endpoints**:
- `GET /weatherforecast` - Weather forecast data
- `GET /api/temperatures` - List temperature measurements
- `GET /api/temperatures/{id}` - Get specific measurement
- `POST /api/temperatures` - Create measurement
- `PUT /api/temperatures/{id}` - Update measurement
- `DELETE /api/temperatures/{id}` - Delete measurement

### Data Component (Azure Cosmos DB)
- **Type**: NoSQL document database
- **Mode**: Serverless (pay-per-request)
- **Database**: `TemperatureDb`
- **Container**: `Temperatures`
- **Partition Key**: `/location`
- **Authentication**: Managed identity (Azure AD)
- **Connection**: Cosmos SDK via connection string

**Data Model**:
```typescript
{
  id: string,              // GUID
  location: string,        // Partition key
  temperatureC: number,    // Temperature in Celsius
  recordedAt: string       // ISO 8601 timestamp
}
```

## Data Flow

### Read Temperature Measurements
```
User → Frontend (Vue) → HTTP GET /api/temperatures
                    ↓
    Backend (ASP.NET) → TemperatureMeasurementStore.GetAllAsync()
                    ↓
         Cosmos DB → Query: "SELECT * FROM c ORDER BY c.recordedAt DESC"
                    ↓
         JSON Response → Frontend → UI Rendering
```

### Create Temperature Measurement
```
User → Frontend Form → HTTP POST /api/temperatures
                    ↓  { location, temperatureC, recordedAt }
    Backend Validation → Generate GUID
                    ↓
    TemperatureMeasurementStore.AddAsync()
                    ↓
         Cosmos DB → CreateItemAsync(document, partitionKey)
                    ↓
         Created Document → Frontend → UI Update
```

### Get Weather Forecast
```
User → Frontend (Vue) → HTTP GET /weatherforecast
                    ↓
    Backend (ASP.NET) → Generate Random Forecast
                    ↓
         JSON Response → Frontend → UI Rendering
```

**Note**: Weather forecast is **randomly generated**, not from an external API.

## Communication Patterns

### Frontend to Backend
- **Protocol**: HTTP/HTTPS
- **Format**: JSON
- **Method**: RESTful API calls
- **Authentication**: None currently implemented
- **CORS**: Configured to allow any origin in backend

**Local Development**:
- Vite proxy forwards `/api/*` and `/weatherforecast` to backend
- Target: `process.env.BACKEND_URL || 'http://localhost:5000'`
- Config: [src/frontend/vite.config.ts](../../../src/frontend/vite.config.ts)

**Production**:
- Direct HTTPS calls to backend Container App URL
- Backend URL injected via `VITE_BACKEND_URL` environment variable
- Config: [infra/resources.bicep](../../../infra/resources.bicep#L221)

### Backend to Database
- **Protocol**: HTTPS (Cosmos DB REST API wrapped by SDK)
- **SDK**: Azure.Cosmos NuGet package
- **Authentication**: Managed Identity (production) or connection string
- **Connection Config**: `ConnectionStrings__cosmos-db` environment variable

## Deployment Architecture

### Local Development (.NET Aspire)
```
Developer Machine
├── .NET Aspire AppHost (apphost.cs)
│   ├── Orchestrates services
│   ├── Provides dashboard (localhost:15888)
│   └── Injects environment variables
│
├── Backend Process (dotnet run)
│   └── Port: 5000
│
├── Frontend Process (vite dev)
│   └── Port: Dynamic (from Aspire)
│
└── Azure Cosmos DB (cloud or emulator)
```

### Production Deployment (Azure)
```
Azure Subscription
└── Resource Group: rg-{environmentName}
    ├── Container Apps Environment
    │   ├── Log Analytics Workspace
    │   ├── Application Insights
    │   └── Shared networking/infrastructure
    │
    ├── Container Registry
    │   ├── Backend container image
    │   └── Frontend container image
    │
    ├── Container App: backend
    │   ├── Managed Identity (backend)
    │   ├── Environment variables
    │   ├── Ingress: External, Port 8080
    │   └── CORS configured
    │
    ├── Container App: frontend
    │   ├── Managed Identity (frontend)
    │   ├── Environment variables (BACKEND_URL)
    │   ├── Ingress: External, Port 80
    │   └── No CORS needed (serves static)
    │
    └── Cosmos DB Account
        ├── Serverless capacity
        ├── Database: TemperatureDb
        ├── Container: Temperatures
        ├── RBAC roles assigned to managed identities
        └── Public network access enabled
```

## Integration Points

### External Integrations
**Currently: NONE**

The application is **self-contained** with no external API integrations:
- ❌ No weather API (data is randomly generated)
- ❌ No authentication provider (Azure AD, Auth0, etc.)
- ❌ No third-party services
- ❌ No message queues or event systems
- ❌ No CDN

### Azure Service Integrations
1. **Application Insights** - Telemetry and monitoring
   - Connection string injected via environment variable
   - Automatic instrumentation

2. **Azure Cosmos DB** - Data persistence
   - Managed identity authentication
   - SDK-based integration

3. **Azure Container Registry** - Container image storage
   - Managed identity pull access

## Design Patterns

### Backend Patterns
1. **Minimal API Pattern** - Endpoint definitions without controllers
   - File: [src/backend/Program.cs](../../../src/backend/Program.cs#L44-L95)

2. **Repository Pattern** - `TemperatureMeasurementStore` class
   - File: [src/backend/Program.cs](../../../src/backend/Program.cs#L146-L238)

3. **Record Types** - Immutable DTOs
   - File: [src/backend/Program.cs](../../../src/backend/Program.cs#L98-L145)

4. **Dependency Injection** - Built-in ASP.NET Core DI
   - File: [src/backend/Program.cs](../../../src/backend/Program.cs#L6-L20)

### Frontend Patterns
1. **Composition API** - Vue 3 `<script setup>` syntax
   - Examples: [WeatherForecast.vue](../../../src/frontend/src/components/WeatherForecast.vue), [TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue)

2. **Service Layer Pattern** - Centralized API client
   - File: [src/frontend/src/services/api.ts](../../../src/frontend/src/services/api.ts)

3. **Component-Based Architecture** - Reusable Vue components
   - Directory: [src/frontend/src/components/](../../../src/frontend/src/components/)

4. **Route-Based Code Splitting** - Lazy-loaded views
   - File: [src/frontend/src/router/index.ts](../../../src/frontend/src/router/index.ts#L11-L18)

## Scalability Considerations

### Current Scaling Capabilities
✅ **Horizontal Scaling**:
- Both frontend and backend can scale 1-10 replicas
- Stateless design enables scaling
- Azure Container Apps handles auto-scaling

✅ **Database Scaling**:
- Cosmos DB serverless auto-scales
- Request Units (RUs) allocated on-demand
- Partition key (`/location`) enables distribution

### Scaling Limitations
❌ **No Caching** - Every request hits the database
❌ **No Load Balancing Strategy** - Default Container Apps LB only
❌ **No Rate Limiting** - Unlimited requests possible
❌ **No CDN** - Static frontend assets not cached globally
❌ **Partition Key Design** - `/location` may create hot partitions if locations are uneven

## Performance Characteristics

### Expected Performance
- **Frontend**: Fast (static assets from Nginx)
- **Backend API**: Sub-100ms for simple operations
- **Database Queries**: 10-50ms (serverless, varies with scale)

### Performance Bottlenecks
1. **No connection pooling explicit config** - May affect high throughput
2. **Cosmos DB serverless** - Cold start latency possible
3. **No caching** - Repeated queries fetch from DB every time
4. **Query without partition key** - GetByIdAsync scans all partitions

## Reliability and Resilience

### Current Resilience Features
✅ Container Apps automatic restart on failure
✅ Cosmos DB 99.99% SLA
✅ Multi-replica capability (1-10)

### Missing Resilience Features
❌ **No retry policies** - Failed requests don't retry
❌ **No circuit breakers** - Cascading failures possible
❌ **No health checks** - Container Apps defaults only
❌ **No graceful degradation** - Frontend breaks if backend unavailable
❌ **No request timeouts** - Long-running queries may hang

## Monitoring and Observability

### Configured Monitoring
✅ **Application Insights** - Both services instrumented
✅ **Log Analytics** - Centralized logging
✅ **.NET Aspire Dashboard** - Local development observability

### Monitoring Gaps
❌ **No custom metrics** - Only default telemetry
❌ **No distributed tracing** - No correlation IDs
❌ **No alerts configured** - No proactive monitoring
❌ **No dashboards** - Application Insights portal only

## Security Architecture

See [security.md](./security.md) for detailed security analysis.

**Summary**:
- ❌ No authentication implemented
- ❌ No authorization implemented
- ✅ Managed identities for Azure resources
- ✅ HTTPS in production (Container Apps default)
- ⚠️ CORS allows any origin (insecure)

## Technology Choices Rationale

### Why .NET 10?
- Latest LTS version (modern, performant)
- Excellent Azure integration
- Minimal API pattern simplifies development

### Why Vue 3?
- Progressive framework, easy to learn
- Composition API provides better TypeScript support
- Excellent tooling (Vite, Vue DevTools)

### Why Cosmos DB?
- Serverless pricing (cost-effective for low traffic)
- Global distribution capability
- Flexible schema (NoSQL)
- Native Azure integration

### Why Container Apps?
- Serverless container hosting
- Auto-scaling without VM management
- Integrated with Azure Monitor
- Simpler than AKS for this scale

## Future Architecture Considerations

### Recommended Enhancements
1. **Add API Gateway** - Azure API Management or Application Gateway
2. **Implement Caching** - Redis or Cosmos DB integrated cache
3. **Add CDN** - Azure Front Door or CDN for static assets
4. **Event-Driven Architecture** - Azure Service Bus or Event Grid
5. **Multi-Region Deployment** - Cosmos DB multi-region writes
6. **Authentication** - Azure AD B2C or Entra ID
7. **API Versioning** - Support backward compatibility
