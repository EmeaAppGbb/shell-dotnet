# Development and Build Tools

## Local Development Tools

### .NET Aspire
- **Version**: 13.1.0
- **Purpose**: Local development orchestration and service discovery
- **Configuration**: [apphost.cs](../../../apphost.cs)
- **Dashboard**: http://localhost:15888
- **Features**:
  - Service orchestration (backend + frontend)
  - Service discovery and communication
  - Built-in observability dashboard
  - Azure service emulation/connections
  - Environment variable injection

**Usage**:
```bash
dotnet run --project apphost.cs
```

**Services Defined**:
- `cosmos-db` - Azure Cosmos DB (existing resource reference)
- `backend` - C# application at `./src/backend`
- `frontend` - Vite application at `./src/frontend`

### Vite Development Server
- **Version**: 7.2.4
- **Purpose**: Frontend development server with HMR
- **Configuration**: [src/frontend/vite.config.ts](../../../src/frontend/vite.config.ts)
- **Features**:
  - Fast Hot Module Replacement (HMR)
  - Proxy configuration for backend API
  - Vue DevTools integration
  - TypeScript support

**Proxy Configuration**:
```typescript
proxy: {
  '/api': {
    target: process.env.BACKEND_URL || 'http://localhost:5000',
    changeOrigin: true,
  },
  '/weatherforecast': {
    target: process.env.BACKEND_URL || 'http://localhost:5000',
    changeOrigin: true,
  },
}
```

**Usage**:
```bash
cd src/frontend
npm run dev
```

## Build Tools

### Backend Build (.NET SDK)
- **Version**: .NET 10.0
- **Build Command**: `dotnet build`
- **Project File**: [src/backend/backend.csproj](../../../src/backend/backend.csproj)
- **Output**: `bin/Debug/net10.0/` or `bin/Release/net10.0/`

**Build Configuration**:
- Target Framework: `net10.0`
- Nullable Reference Types: Enabled
- Implicit Usings: Enabled
- User Secrets: Configured

**VS Code Task**: `build-backend`
```bash
dotnet build ${workspaceFolder}/src/backend/backend.csproj
```

### Frontend Build (Vite)
- **Version**: 7.2.4
- **Build Command**: `npm run build`
- **Output Directory**: `dist/`
- **Build Pipeline**:
  1. Type checking: `vue-tsc --build`
  2. Vite build: Bundles and optimizes

**VS Code Task**: `build-frontend`
```bash
cd src/frontend && npm run build
```

**Build Script** (from package.json):
```json
"build": "run-p type-check \"build-only {@}\" --"
```

### Combined Build
**VS Code Task**: `build-all` (default)
- Runs both `build-backend` and `build-frontend` in parallel
- Defined in [.vscode/tasks.json](../../.vscode/tasks.json)

**Shell Script**: [build.sh](../../../build.sh)
- Likely builds both services (content not examined)

## Package Managers

### npm (Frontend)
- **Version Requirement**: Node.js ^20.19.0 || >=22.12.0
- **Package File**: [src/frontend/package.json](../../../src/frontend/package.json)
- **Lock File**: ❌ **NOT PRESENT** (package-lock.json missing)

**Key Scripts**:
```json
{
  "dev": "vite",                                    // Start dev server
  "build": "run-p type-check \"build-only {@}\" --", // Type check + build
  "preview": "vite preview",                        // Preview prod build
  "test:unit": "vitest",                            // Run unit tests
  "test:e2e": "playwright test",                    // Run E2E tests
  "lint": "eslint . --fix --cache",                 // Lint and fix
  "format": "prettier --write --experimental-cli src/" // Format code
}
```

### NuGet (Backend)
- **Implicit via .NET SDK**
- No explicit package manager commands
- Dependencies managed in [backend.csproj](../../../src/backend/backend.csproj)
- Restore happens automatically during build

## Containerization Tools

### Docker
**Backend Dockerfile**: [src/backend/Dockerfile](../../../src/backend/Dockerfile)
- Multi-stage build
- Base: `mcr.microsoft.com/dotnet/aspnet:10.0`
- Build: `mcr.microsoft.com/dotnet/sdk:10.0`
- Exposes port: 8080
- Non-root user: `app`

**Build stages**:
1. `base` - Runtime environment
2. `build` - Compile application
3. `publish` - Create release artifacts
4. `final` - Copy artifacts to runtime image

**Frontend Dockerfile**: [src/frontend/Dockerfile](../../../src/frontend/Dockerfile)
- Multi-stage build
- Build: `node:lts-alpine`
- Production: `nginx:stable-alpine`
- Exposes port: 80

**Build stages**:
1. `build-stage` - Install deps and build with Vite
2. `production-stage` - Serve static files with Nginx

## Testing Tools

### Frontend Unit Testing
- **Framework**: Vitest 4.0.14
- **Configuration**: [src/frontend/vitest.config.ts](../../../src/frontend/vitest.config.ts)
- **Test Utilities**: @vue/test-utils 2.4.6
- **Environment**: jsdom 27.2.0
- **Command**: `npm run test:unit`

### Frontend E2E Testing
- **Framework**: Playwright 1.57.0
- **Configuration**: [src/frontend/playwright.config.ts](../../../src/frontend/playwright.config.ts)
- **Command**: `npm run test:e2e`

### Backend Testing
- ❌ **No testing tools configured**

## Code Quality Tools

### ESLint (Frontend)
- **Version**: 9.39.1
- **Configuration**: [src/frontend/eslint.config.ts](../../../src/frontend/eslint.config.ts)
- **Plugins**:
  - `eslint-plugin-vue` ~10.5.1
  - `eslint-plugin-playwright` ^2.3.0
  - `@vitest/eslint-plugin` ^1.5.0
- **Configs**:
  - `@vue/eslint-config-typescript` ^14.6.0
  - `@vue/eslint-config-prettier` ^10.2.0
- **Command**: `npm run lint`
- **Features**: Auto-fix, caching enabled

### Prettier (Frontend)
- **Version**: 3.6.2
- **Integrated with ESLint**
- **Command**: `npm run format`
- **Target**: `src/` directory

### Backend Code Quality
- ❌ No explicit linting tool
- .NET SDK built-in analyzers may be active
- No .editorconfig found
- No StyleCop or other analyzer packages

## Deployment Tools

### Azure Developer CLI (azd)
- **Configuration**: [azure.yaml](../../../azure.yaml)
- **Purpose**: Streamlined Azure deployment and management

**Key Commands**:
```bash
azd auth login          # Authenticate
azd up                  # Provision + deploy
azd deploy              # Deploy only
azd down                # Delete all resources
azd show                # View deployed resources
azd env get-values      # Get environment variables
```

**Services Defined**:
- `backend` - .NET project, hosted on Container Apps
- `frontend` - TypeScript/Node project, hosted on Container Apps

**Hooks**:
- **predeploy**: Runs before deployment
  - Bash: [infra/scripts/predeploy.sh](../../../infra/scripts/predeploy.sh)
  - PowerShell: [infra/scripts/predeploy.ps1](../../../infra/scripts/predeploy.ps1)
- **postprovision**: Runs after infrastructure provisioning
  - Bash: [infra/scripts/postprovision.sh](../../../infra/scripts/postprovision.sh)
  - PowerShell: [infra/scripts/postprovision.ps1](../../../infra/scripts/postprovision.ps1)

### Azure Bicep
- **Version**: Latest (installed with Azure CLI)
- **Purpose**: Infrastructure as Code
- **Entry Point**: [infra/main.bicep](../../../infra/main.bicep)
- **Deployment**: Automated via `azd`

**Build/Validation**:
```bash
az bicep build -f infra/main.bicep
az deployment sub what-if -f infra/main.bicep -l <location>
```

## IDE and Editor Support

### VS Code Configuration
**Tasks Defined** (in [.vscode/tasks.json](../../.vscode/tasks.json)):
- `build-backend` - Build .NET backend
- `build-frontend` - Build Vue frontend
- `build-all` - Build both (default build task)

**Extensions Likely Used** (not explicitly configured):
- C# / C# Dev Kit
- Vue Language Features (Volar)
- ESLint
- Prettier
- Azure Tools

## Utility Tools

### npm-run-all2
- **Version**: 8.0.4
- **Purpose**: Run multiple npm scripts in parallel or sequentially
- **Usage**: Build script runs type-check and build in parallel

### jiti
- **Version**: 2.6.1
- **Purpose**: Runtime TypeScript and ESM support for configuration files
- **Usage**: Config files (Vite, ESLint, etc.) can be written in TypeScript

### vue-tsc
- **Version**: 3.1.5
- **Purpose**: Vue TypeScript compiler
- **Usage**: Type-checking Vue SFC files
- **Command**: `npm run type-check`

## Development Workflow Summary

### Local Development
1. Start Aspire: `dotnet run --project apphost.cs`
2. Access Dashboard: http://localhost:15888
3. Services start automatically:
   - Backend on port 5000
   - Frontend on dynamic port (shown in dashboard)

**Alternative (without Aspire)**:
1. Backend: `cd src/backend && dotnet run`
2. Frontend: `cd src/frontend && npm run dev`

### Testing
```bash
# Frontend unit tests
cd src/frontend && npm run test:unit

# Frontend E2E tests
cd src/frontend && npm run test:e2e

# Backend tests - NONE CONFIGURED
```

### Code Quality
```bash
# Frontend linting
cd src/frontend && npm run lint

# Frontend formatting
cd src/frontend && npm run format

# Backend - NO TOOLS CONFIGURED
```

### Building
```bash
# Via VS Code: Run "build-all" task
# Or manually:
dotnet build src/backend/backend.csproj
cd src/frontend && npm run build
```

### Deployment
```bash
# First time
azd auth login
azd up

# Subsequent deployments
azd deploy

# Cleanup
azd down
```

## Tool Gaps and Recommendations

### Missing Tools
❌ **Backend testing framework** (xUnit, NUnit, or MSTest)
❌ **Backend linting/formatting** (StyleCop, EditorConfig)
❌ **API testing tool** (Postman collections, REST Client)
❌ **Frontend package-lock.json** (security and reproducibility)
❌ **CI/CD pipeline** (GitHub Actions, Azure DevOps)
❌ **Container scanning** (Trivy, Snyk)
❌ **Dependency scanning** (Dependabot, Renovate)
❌ **Load testing tool** (k6, JMeter, Azure Load Testing)

### Tool Version Currency
✅ All tools are **modern and current**:
- .NET 10.0 (latest)
- Vue 3.5 (latest major)
- Vite 7 (latest major)
- TypeScript 5.9 (current)
- All npm packages are recent versions
