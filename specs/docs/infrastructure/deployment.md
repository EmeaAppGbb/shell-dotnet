# Deployment Architecture and Methods

## Deployment Overview

The application uses **Azure Developer CLI (azd)** for streamlined infrastructure provisioning and application deployment to Azure Container Apps.

**Deployment Pattern**: Infrastructure as Code (Bicep) + Container-based deployment

## Deployment Architecture

### Production Architecture

```
Azure Subscription
└── Resource Group: rg-{environmentName}
    │
    ├── Log Analytics Workspace
    │   └── Linked to Application Insights
    │
    ├── Application Insights
    │   └── Monitors both containers
    │
    ├── Container Registry
    │   ├── Image: backend:latest
    │   └── Image: frontend:latest
    │
    ├── Container Apps Environment
    │   ├── Shared networking
    │   ├── Log Analytics integration
    │   └── Contains 2 Container Apps
    │
    ├── Container App: backend
    │   ├── User-Assigned Managed Identity
    │   ├── Image: {registry}/backend:latest
    │   ├── Ingress: External HTTPS (port 8080)
    │   ├── Scaling: 1-10 replicas
    │   ├── Resources: 0.5 vCPU, 1GB RAM
    │   └── Environment variables (Cosmos DB, App Insights)
    │
    ├── Container App: frontend
    │   ├── User-Assigned Managed Identity
    │   ├── Image: {registry}/frontend:latest
    │   ├── Ingress: External HTTPS (port 80)
    │   ├── Scaling: 1-10 replicas
    │   ├── Resources: 0.5 vCPU, 1GB RAM
    │   └── Environment variables (Backend URL, App Insights)
    │
    └── Cosmos DB Account
        ├── Serverless mode
        ├── Database: TemperatureDb
        ├── Container: Temperatures
        └── RBAC: Managed identities granted access
```

### Deployment Flow

```
Developer Machine
│
├── 1. azd auth login
│   └── Authenticates with Azure
│
├── 2. azd up (or azd provision + azd deploy)
│   │
│   ├── Provision Phase
│   │   ├── Deploys Bicep templates
│   │   ├── Creates all Azure resources
│   │   ├── Configures RBAC roles
│   │   └── Runs postprovision hooks
│   │
│   └── Deploy Phase
│       ├── Runs predeploy hooks
│       ├── Builds Docker images
│       ├── Pushes to Container Registry
│       └── Updates Container Apps
│
└── Application Running
    ├── Frontend: https://frontend.{env}.azurecontainerapps.io
    └── Backend: https://backend.{env}.azurecontainerapps.io
```

## Deployment Methods

### 1. Full Deployment (azd up)

**Command**:
```bash
azd up
```

**What it does**:
1. Prompts for environment name (if first time)
2. Prompts for Azure location
3. Provisions all infrastructure (Bicep)
4. Runs postprovision hooks
5. Builds and deploys applications
6. Outputs service URLs

**When to use**: First deployment or infrastructure changes

**Duration**: ~5-10 minutes

---

### 2. Infrastructure Only (azd provision)

**Command**:
```bash
azd provision
```

**What it does**:
1. Deploys Bicep templates
2. Creates/updates Azure resources
3. Runs postprovision hooks
4. Does NOT deploy application code

**When to use**: Testing infrastructure changes without deploying code

**Duration**: ~3-5 minutes

---

### 3. Application Only (azd deploy)

**Command**:
```bash
azd deploy
```

**What it does**:
1. Runs predeploy hooks
2. Builds Docker images
3. Pushes to Container Registry
4. Updates Container Apps with new images
5. Does NOT modify infrastructure

**When to use**: Code changes without infrastructure changes (most common)

**Duration**: ~2-4 minutes

**Example**:
```bash
# After making code changes
azd deploy

# Deploy specific service only
azd deploy backend
azd deploy frontend
```

---

### 4. Teardown (azd down)

**Command**:
```bash
azd down
```

**What it does**:
1. Deletes the entire resource group
2. Removes all deployed resources
3. Keeps local configuration

**When to use**: Cleaning up development environments

⚠️ **Warning**: This is **destructive** and **irreversible**

**Options**:
```bash
# Delete without confirmation
azd down --force

# Purge (delete permanently, no soft delete)
azd down --purge
```

---

## Deployment Hooks

### Predeploy Hook

**Purpose**: Configure environment-specific settings before deployment

**Files**:
- Bash: [infra/scripts/predeploy.sh](../../../infra/scripts/predeploy.sh)
- PowerShell: [infra/scripts/predeploy.ps1](../../../infra/scripts/predeploy.ps1)

**What it does**:
1. Reads `BACKEND_URL` from azd environment
2. Writes `.env.production` file for Vite
3. Sets `VITE_BACKEND_URL={backendUrl}`

**Example** (predeploy.sh):
```bash
eval "$(azd env get-values)"
echo "VITE_BACKEND_URL=$BACKEND_URL" > "$FRONTEND_DIR/.env.production"
```

**When executed**: Before `azd deploy` builds containers

---

### Postprovision Hook

**Purpose**: Configure application settings after infrastructure is provisioned

**Files**:
- Bash: [infra/scripts/postprovision.sh](../../../infra/scripts/postprovision.sh)
- PowerShell: [infra/scripts/postprovision.ps1](../../../infra/scripts/postprovision.ps1)

**What it does**:
1. Reads Azure resource names from azd environment
2. Updates `apphost.settings.json` with Cosmos DB details
3. Enables local development with provisioned resources

**Example** (postprovision.sh):
```bash
eval "$(azd env get-values)"
COSMOS_NAME="${AZURE_COSMOS_NAME:-}"
COSMOS_RESOURCE_GROUP="${AZURE_COSMOS_RESOURCE_GROUP:-}"

# Update apphost.settings.json using jq
jq '.Parameters.cosmosName = $cosmosName' \
   "$SETTINGS_FILE" > "$SETTINGS_FILE.tmp"
```

**When executed**: After `azd provision` creates resources

---

## Build Process

### Backend Build

**Dockerfile**: [src/backend/Dockerfile](../../../src/backend/Dockerfile)

**Build Stages**:
```dockerfile
# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN dotnet restore
RUN dotnet build
RUN dotnet publish

# Stage 3: Final runtime
FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]
```

**Build Command** (executed by azd):
```bash
docker build -t {registry}.azurecr.io/backend:latest src/backend
docker push {registry}.azurecr.io/backend:latest
```

**Build Time**: ~1-2 minutes

---

### Frontend Build

**Dockerfile**: [src/frontend/Dockerfile](../../../src/frontend/Dockerfile)

**Build Stages**:
```dockerfile
# Stage 1: Build with Node.js
FROM node:lts-alpine as build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Stage 2: Serve with Nginx
FROM nginx:stable-alpine as production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**Build Command** (executed by azd):
```bash
docker build -t {registry}.azurecr.io/frontend:latest src/frontend
docker push {registry}.azurecr.io/frontend:latest
```

**Build Time**: ~2-3 minutes (npm install is slowest)

---

## Infrastructure as Code (Bicep)

### Template Structure

```
infra/
├── main.bicep                   # Subscription-scope entry point
├── resources.bicep              # Resource group resources
├── main.parameters.json         # Parameter overrides
├── abbreviations.json           # Azure resource naming
└── modules/
    └── fetch-container-image.bicep  # Fetch existing images
```

### Main Template (main.bicep)

**Scope**: Subscription

**Resources Created**:
- Resource Group

**Parameters**:
- `environmentName` - Environment identifier
- `location` - Azure region
- `principalId` - Deployment user/service principal ID
- `principalType` - User or ServicePrincipal

**File**: [infra/main.bicep](../../../infra/main.bicep)

---

### Resources Template (resources.bicep)

**Scope**: Resource Group

**Modules Used** (Azure Verified Modules):
- `avm/ptn/azd/monitoring:0.1.0` - Log Analytics + App Insights
- `avm/res/container-registry/registry:0.1.1` - Container Registry
- `avm/res/app/managed-environment:0.4.5` - Container Apps Environment
- `avm/res/document-db/database-account:0.8.1` - Cosmos DB
- `avm/res/managed-identity/user-assigned-identity:0.2.1` - Managed Identities
- `avm/res/app/container-app:0.8.0` - Container Apps (2x)

**Custom Modules**:
- `modules/fetch-container-image.bicep` - Fetches existing container images

**Outputs**:
- `AZURE_CONTAINER_REGISTRY_ENDPOINT` - Registry login server
- `AZURE_RESOURCE_BACKEND_ID` - Backend resource ID
- `AZURE_RESOURCE_FRONTEND_ID` - Frontend resource ID
- `AZURE_COSMOS_NAME` - Cosmos DB account name
- `AZURE_COSMOS_RESOURCE_GROUP` - Cosmos DB resource group
- `BACKEND_URL` - Backend public URL

**File**: [infra/resources.bicep](../../../infra/resources.bicep)

---

## Deployment Configuration

### Azure Developer CLI (azure.yaml)

**File**: [azure.yaml](../../../azure.yaml)

**Services Defined**:
```yaml
services:
  backend:
    project: ./src/backend
    host: containerapp
    language: dotnet
  frontend:
    project: ./src/frontend
    host: containerapp
    language: ts
```

**Resources Configuration**:
```yaml
resources:
  backend:
    type: host.containerapp
    port: 8080
  frontend:
    type: host.containerapp
    uses:
      - backend
    port: 80
```

**Hooks**:
- `predeploy`: Runs before deployment (configures .env.production)
- `postprovision`: Runs after provisioning (configures apphost.settings.json)

---

## Environment Configuration

### Azure Environment Variables

**Stored in**: `.azure/{environmentName}/.env`

**Key Variables**:
- `AZURE_SUBSCRIPTION_ID` - Target subscription
- `AZURE_LOCATION` - Deployment region
- `AZURE_RESOURCE_GROUP` - Resource group name
- `BACKEND_URL` - Backend service URL
- `AZURE_COSMOS_NAME` - Cosmos DB account name
- `AZURE_COSMOS_RESOURCE_GROUP` - Cosmos DB resource group

**Access**:
```bash
# View all environment variables
azd env get-values

# Get specific value
azd env get-value BACKEND_URL

# Set value
azd env set KEY value
```

### Container Environment Variables

**Backend Container**:
```yaml
env:
  - name: APPLICATIONINSIGHTS_CONNECTION_STRING
    value: {appInsightsConnectionString}
  - name: AZURE_CLIENT_ID
    value: {backendManagedIdentityClientId}
  - name: ConnectionStrings__cosmos-db
    value: {cosmosEndpoint}
  - name: PORT
    value: '8080'
```

**Frontend Container**:
```yaml
env:
  - name: APPLICATIONINSIGHTS_CONNECTION_STRING
    value: {appInsightsConnectionString}
  - name: AZURE_CLIENT_ID
    value: {frontendManagedIdentityClientId}
  - name: BACKEND_URL
    value: {backendPublicUrl}
  - name: PORT
    value: '3000'
```

**File**: [infra/resources.bicep](../../../infra/resources.bicep)

---

## Deployment Troubleshooting

### Common Issues

#### 1. Authentication Failures
```bash
# Issue: Not authenticated with Azure
azd auth login

# Issue: Wrong subscription
az account set --subscription {subscriptionId}
```

#### 2. Build Failures
```bash
# Issue: Frontend build fails
cd src/frontend
npm install
npm run build

# Issue: Backend build fails
cd src/backend
dotnet build
```

#### 3. Deployment Failures
```bash
# Check deployment logs
azd deploy --debug

# Check specific service
azd deploy backend --debug
```

#### 4. Hook Failures
```bash
# Check predeploy script
bash infra/scripts/predeploy.sh

# Check postprovision script
bash infra/scripts/postprovision.sh
```

---

## Rollback Strategy

### Current Rollback Approach

❌ **No automated rollback** implemented

**Manual Rollback**:
1. Identify previous working container image tag
2. Update Container App with previous image
3. Restart Container App

**Using Azure Portal**:
1. Navigate to Container App
2. Go to "Revisions"
3. Activate previous revision

**Using Azure CLI**:
```bash
az containerapp revision activate \
  --name {containerAppName} \
  --resource-group {resourceGroup} \
  --revision {previousRevisionName}
```

---

## Blue-Green Deployment

❌ **Not Implemented**

**Container Apps Capability**: Supports traffic splitting between revisions

**Potential Implementation**:
1. Deploy new revision (blue)
2. Split traffic 10% to new revision
3. Monitor for errors
4. Gradually increase traffic
5. Deactivate old revision (green)

---

## Deployment Monitoring

### Deployment Logs

**View via azd**:
```bash
azd deploy --debug
```

**View via Azure Portal**:
1. Navigate to Container App
2. Go to "Revision Management"
3. View revision logs

### Application Logs

**Stream logs**:
```bash
# Backend logs
az containerapp logs show \
  --name backend \
  --resource-group {resourceGroup} \
  --follow

# Frontend logs
az containerapp logs show \
  --name frontend \
  --resource-group {resourceGroup} \
  --follow
```

---

## Deployment Best Practices

### Recommended Practices

✅ **Test locally first**: Use .NET Aspire before deploying
✅ **Use azd deploy**: For code-only changes (faster)
✅ **Tag images**: Use semantic versioning for container images
✅ **Monitor deployments**: Check logs after deployment
✅ **Use environments**: Separate dev, staging, production

### Missing Practices

❌ **No CI/CD pipeline**: Manual deployments only
❌ **No automated testing**: No tests run before deployment
❌ **No smoke tests**: No post-deployment validation
❌ **No deployment notifications**: No alerts on deployment status
❌ **No approval gates**: No manual approval required

---

## Deployment Performance

### Deployment Times (Typical)

| Phase | Duration | Notes |
|-------|----------|-------|
| `azd provision` | 3-5 min | Infrastructure only |
| Backend build | 1-2 min | Docker build |
| Frontend build | 2-3 min | npm install + Vite build |
| Container push | 30s-1 min | Upload to ACR |
| Container App update | 30s-1 min | Pull and start |
| **Total (azd up)** | **7-12 min** | Full deployment |
| **Total (azd deploy)** | **4-7 min** | Code-only deployment |

---

## Cost Optimization

### Deployment Cost Savings

1. **Use azd deploy**: Skips infrastructure provisioning
2. **Scale down when idle**: Reduce min replicas in dev
3. **Use azd down**: Delete resources when not in use
4. **Optimize container images**: Smaller images = faster deploys
5. **Cache Docker layers**: Speeds up subsequent builds

---

## Deployment Security

### Secure Deployment Practices

✅ **Managed Identities**: No passwords in deployment
✅ **RBAC**: Least-privilege access for identities
✅ **Private registries**: ACR access via managed identity

### Missing Security

❌ **No secret scanning**: Secrets could be committed
❌ **No image scanning**: Vulnerabilities not checked
❌ **No deployment approvals**: Anyone with access can deploy
❌ **No audit logs**: Deployment actions not logged
