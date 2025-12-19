# Environment Configuration

## Environment Overview

The application supports multiple deployment environments through Azure Developer CLI (azd) environment management.

**Environment Concept**: Each environment is a completely isolated deployment with its own Azure resources.

## Supported Environments

### Local Development
**Purpose**: Developer workstation development and testing

**Orchestration**: .NET Aspire

**Configuration File**: [apphost.cs](../../../apphost.cs)

**Services**:
- Backend: `http://localhost:5000`
- Frontend: Dynamic port (shown in Aspire Dashboard)
- Aspire Dashboard: `http://localhost:15888`

**Database**: Azure Cosmos DB (cloud) or Cosmos DB Emulator (local)

**Start Command**:
```bash
dotnet run --project apphost.cs
```

---

### Azure Environments

**Pattern**: One resource group per environment

**Common Environments**:
- `dev` - Development
- `staging` - Pre-production
- `prod` - Production

**Environment Creation**:
```bash
# Create new environment
azd env new {environmentName}

# Switch environment
azd env select {environmentName}

# List environments
azd env list
```

**Environment Storage**:
- Location: `.azure/{environmentName}/.env`
- Not committed to source control

---

## Environment Variables

### Backend Environment Variables

**Required Variables** (Container App):

| Variable | Purpose | Example | Source |
|----------|---------|---------|--------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Telemetry | `InstrumentationKey=...` | Bicep output |
| `AZURE_CLIENT_ID` | Managed Identity | `12345678-...` | Bicep output |
| `ConnectionStrings__cosmos-db` | Cosmos DB endpoint | `https://cosmos-abc.documents.azure.com:443/` | Bicep output |
| `PORT` | HTTP port | `8080` | Hardcoded |

**Configuration**: [infra/resources.bicep](../../../infra/resources.bicep#L168-L182)

**Local Development** (.NET Aspire):
```csharp
var cosmos = builder.AddAzureCosmosDB("cosmos-db")
    .AsExisting(cosmosName, cosmosResourceGroup);

var api = builder.AddCSharpApp("backend", "./src/backend")
          .WithReference(cosmos);
```

**File**: [apphost.cs](../../../apphost.cs)

---

### Frontend Environment Variables

**Build-Time Variables** (Vite):

| Variable | Purpose | Example | Source |
|----------|---------|---------|--------|
| `VITE_BACKEND_URL` | Backend API base URL | `https://backend.{env}.azurecontainerapps.io` | predeploy hook |

**Runtime Variables** (Container App):

| Variable | Purpose | Example | Source |
|----------|---------|---------|--------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Telemetry | `InstrumentationKey=...` | Bicep output |
| `AZURE_CLIENT_ID` | Managed Identity | `12345678-...` | Bicep output |
| `BACKEND_URL` | Backend URL (informational) | `https://backend...` | Bicep output |
| `PORT` | HTTP port | `3000` | Hardcoded |

**Configuration**: [infra/resources.bicep](../../../infra/resources.bicep#L257-L270)

**Build-Time Configuration** (.env.production):
```bash
# Created by predeploy.sh
VITE_BACKEND_URL=https://backend.{env}.azurecontainerapps.io
```

**File**: [infra/scripts/predeploy.sh](../../../infra/scripts/predeploy.sh)

---

### Azure Environment Variables (azd)

**Managed by azd** (.azure/{env}/.env):

| Variable | Purpose | Example |
|----------|---------|---------|
| `AZURE_ENV_NAME` | Environment name | `dev`, `prod` |
| `AZURE_LOCATION` | Azure region | `eastus`, `westeurope` |
| `AZURE_SUBSCRIPTION_ID` | Target subscription | `12345678-1234-...` |
| `AZURE_RESOURCE_GROUP` | Resource group name | `rg-dev` |
| `AZURE_CONTAINER_REGISTRY_ENDPOINT` | ACR login server | `crabcxyz.azurecr.io` |
| `AZURE_COSMOS_NAME` | Cosmos DB account name | `cosmos-abc123` |
| `AZURE_COSMOS_RESOURCE_GROUP` | Cosmos DB RG | `rg-dev` |
| `BACKEND_URL` | Backend service URL | `https://backend...` |

**Access**:
```bash
# View all
azd env get-values

# Get specific
azd env get-value BACKEND_URL

# Set value
azd env set MY_VAR value
```

---

### Aspire Settings (Local Development)

**File**: `apphost.settings.json` (generated from template)

**Template**: [apphost.settings.template.json](../../../apphost.settings.template.json)

**Parameters** (example):
```json
{
  "Parameters": {
    "cosmosName": "cosmos-abc123",
    "cosmosResourceGroup": "rg-dev"
  }
}
```

**Generation**: Created by `postprovision` hook

**Purpose**: Connect local development to Azure resources

---

## Configuration Files

### Backend Configuration

#### appsettings.json
**File**: [src/backend/appsettings.json](../../../src/backend/appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Purpose**: Application-wide settings, logging levels

#### appsettings.Development.json
**File**: [src/backend/appsettings.Development.json](../../../src/backend/appsettings.Development.json)

**Purpose**: Development-specific overrides (content not examined)

**Loading**: Automatically loaded when `ASPNETCORE_ENVIRONMENT=Development`

#### User Secrets
**Enabled**: Yes (User Secrets ID: `25843fc3-40ad-442f-9cf1-c1c6f3595cdd`)

**File**: [src/backend/backend.csproj](../../../src/backend/backend.csproj#L7)

**Purpose**: Store sensitive data locally (not in source control)

**Usage**:
```bash
dotnet user-secrets set "ConnectionStrings:cosmos-db" "https://..."
dotnet user-secrets list
```

---

### Frontend Configuration

#### Vite Configuration
**File**: [src/frontend/vite.config.ts](../../../src/frontend/vite.config.ts)

**Key Settings**:
- **Dev server proxy**: Forwards `/api` and `/weatherforecast` to backend
- **Backend URL**: From `process.env.BACKEND_URL` or `http://localhost:5000`
- **Plugins**: Vue, JSX, DevTools
- **Alias**: `@` → `./src`

#### Environment Files

**Development** (`.env.development`):
- ❌ **Not present** - Uses defaults

**Production** (`.env.production`):
- **Generated** by predeploy hook
- Contains `VITE_BACKEND_URL`
- Not committed to source control

**Environment Variable Prefix**: `VITE_`
- Only variables starting with `VITE_` are exposed to client

**Access in Code**:
```typescript
const API_BASE = import.meta.env.VITE_BACKEND_URL || ''
```

**File**: [src/frontend/src/services/api.ts](../../../src/frontend/src/services/api.ts)

---

## Resource Naming Convention

**Pattern**: `{abbreviation}{resourceToken}`

**Resource Token**: `uniqueString(subscription().id, resourceGroup().id, location)`
- Example: `abc123xyz`

**Abbreviations**: [infra/abbreviations.json](../../../infra/abbreviations.json)

**Examples**:
| Resource Type | Abbreviation | Example Name |
|---------------|--------------|--------------|
| Resource Group | `rg-` | `rg-dev` |
| Container Apps Environment | `cae-` | `cae-abc123` |
| Container Registry | `crabcd` | `crabcabc123` |
| Cosmos DB | `cosmos-` | `cosmos-abc123` |
| Log Analytics | `log-` | `log-abc123` |
| Application Insights | `appi-` | `appi-abc123` |
| Managed Identity | `id-` | `id-backend-abc123` |

**File**: [infra/resources.bicep](../../../infra/resources.bicep#L10-L11)

---

## Environment-Specific Configuration

### Development Environment

**Characteristics**:
- Single replica (cost savings)
- Detailed logging (Information level)
- OpenAPI documentation enabled
- Developer-friendly error messages
- Public Cosmos DB access
- Direct HTTP (no ingress controller)

**Settings**:
```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // Only in development
}
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L25-L28)

### Production Environment

**Characteristics**:
- Multiple replicas (high availability)
- Reduced logging (Warning level)
- OpenAPI documentation disabled
- Generic error messages
- Should use private endpoints (not configured)
- HTTPS everywhere

**Differences from Dev**:
- `ASPNETCORE_ENVIRONMENT=Production` (not explicitly set, defaults to Production)
- OpenAPI disabled
- Error details hidden (not implemented - still shows details)

### Missing Environment Configurations

❌ **No staging environment** - Should exist between dev and prod
❌ **No environment-specific scaling** - Same scaling for all environments
❌ **No environment-specific security** - Same CORS policy everywhere
❌ **No feature flags** - Cannot toggle features per environment
❌ **No environment-specific monitoring** - Same Application Insights config

---

## Configuration Precedence

### Backend Configuration Order (Highest to Lowest)

1. **Command-line arguments** (not used)
2. **Environment variables**
3. **User Secrets** (Development only)
4. **appsettings.{Environment}.json**
5. **appsettings.json**

### Frontend Configuration Order

1. **Build-time environment variables** (`.env.production`, `.env.development`)
2. **Default values in code**

---

## Secrets Management

### Current Approach

**Backend**:
- ✅ Managed Identity for Cosmos DB (no password)
- ⚠️ Connection string as environment variable (endpoint URL, not sensitive)
- ✅ User Secrets for local development

**Frontend**:
- ❌ No secrets (only public URLs)

### Recommended Approach

**Azure Key Vault** (not implemented):
```bicep
// Reference Key Vault secret
env: [
  {
    name: 'ConnectionStrings__cosmos-db'
    secretRef: 'cosmos-connection'  // From Key Vault
  }
]
```

**Benefits**:
- Centralized secret management
- Secret versioning
- Access auditing
- Automatic rotation

---

## Multi-Environment Strategy

### Environment Isolation

**Current**: Complete isolation per environment
- Separate resource group
- Separate Cosmos DB account
- Separate Container Apps
- No shared resources

**Benefits**:
- ✅ No cross-environment contamination
- ✅ Can delete entire environment easily (`azd down`)
- ✅ Independent scaling and configuration

**Drawbacks**:
- ⚠️ Higher cost (duplicate resources)
- ⚠️ No shared data or state
- ⚠️ Duplicate configuration maintenance

### Shared Resources (Not Implemented)

**Could Share**:
- Container Registry (images are the same)
- Log Analytics Workspace (centralized logging)
- Application Insights (separate resources but shared workspace)

**Should Not Share**:
- Cosmos DB (data isolation required)
- Container Apps (scaling and availability independence)

---

## Environment Promotion

### Current Process

❌ **No automated promotion** - Manual deployment to each environment

**Manual Promotion**:
```bash
# Deploy to dev
azd env select dev
azd deploy

# Test in dev...

# Deploy to prod
azd env select prod
azd deploy
```

### Recommended CI/CD Pipeline

```
Code Commit
    ↓
Build & Test
    ↓
Deploy to Dev (automatic)
    ↓
Integration Tests
    ↓
Deploy to Staging (automatic)
    ↓
Smoke Tests
    ↓
Deploy to Prod (manual approval)
    ↓
Smoke Tests
```

---

## Configuration Best Practices

### Followed

✅ Environment variables for configuration
✅ Separate appsettings per environment
✅ User secrets for local development
✅ Managed identities (no passwords)

### Not Followed

❌ **No Key Vault** for secrets
❌ **No configuration validation** on startup
❌ **No environment-specific feature flags**
❌ **No centralized configuration** (Azure App Configuration)
❌ **No configuration as code** (all environment variables in Bicep)

---

## Troubleshooting Configuration

### View Current Configuration

**Backend**:
```bash
# In container
az containerapp exec \
  --name backend \
  --resource-group {rg} \
  --command env
```

**Frontend**:
```bash
# View build-time variables
cat src/frontend/.env.production

# In container
az containerapp exec \
  --name frontend \
  --resource-group {rg} \
  --command env
```

### Common Issues

**Issue**: Backend can't connect to Cosmos DB
```bash
# Check environment variable
azd env get-value AZURE_COSMOS_NAME

# Verify managed identity has access
az cosmosdb sql role assignment list \
  --account-name {cosmosName} \
  --resource-group {rg}
```

**Issue**: Frontend can't reach backend
```bash
# Check backend URL
azd env get-value BACKEND_URL

# Verify predeploy ran
cat src/frontend/.env.production
```

**Issue**: Aspire can't find Cosmos DB
```bash
# Check settings file
cat apphost.settings.json

# Re-run postprovision
bash infra/scripts/postprovision.sh
```

---

## Environment Variables Reference

### Complete List (Production)

#### Backend
```bash
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...
AZURE_CLIENT_ID=12345678-1234-...
ConnectionStrings__cosmos-db=https://cosmos-abc.documents.azure.com:443/
PORT=8080
ASPNETCORE_ENVIRONMENT=Production  # Implicit default
```

#### Frontend
```bash
VITE_BACKEND_URL=https://backend.{env}.azurecontainerapps.io
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...
AZURE_CLIENT_ID=12345678-1234-...
BACKEND_URL=https://backend.{env}.azurecontainerapps.io
PORT=3000
```

#### azd Environment
```bash
AZURE_ENV_NAME=prod
AZURE_LOCATION=eastus
AZURE_SUBSCRIPTION_ID=12345678-1234-...
AZURE_RESOURCE_GROUP=rg-prod
AZURE_CONTAINER_REGISTRY_ENDPOINT=crabcabc123.azurecr.io
AZURE_COSMOS_NAME=cosmos-abc123
AZURE_COSMOS_RESOURCE_GROUP=rg-prod
AZURE_RESOURCE_BACKEND_ID=/subscriptions/.../backend
AZURE_RESOURCE_FRONTEND_ID=/subscriptions/.../frontend
BACKEND_URL=https://backend.{env}.azurecontainerapps.io
```
