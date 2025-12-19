# Dependencies and Code Organization

## Dependency Analysis

### Backend Dependencies

#### NuGet Packages
From [src/backend/backend.csproj](../../../src/backend/backend.csproj):

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.OpenApi | 10.0.0 | OpenAPI/Swagger documentation generation |
| Aspire.Microsoft.Azure.Cosmos | 13.1.0 | Azure Cosmos DB client with .NET Aspire integration |

**Dependency Notes:**
- Very minimal dependency footprint
- Relies on .NET 10.0 SDK built-in packages (ASP.NET Core, etc.)
- No additional logging, serialization, or utility packages
- User secrets configured with ID: `25843fc3-40ad-442f-9cf1-c1c6f3595cdd`

### Frontend Dependencies

#### Production Dependencies
From [src/frontend/package.json](../../../src/frontend/package.json):

| Package | Version | Purpose |
|---------|---------|---------|
| pinia | ^3.0.4 | Vue state management library |
| vue | ^3.5.25 | Vue.js framework |
| vue-router | ^4.6.3 | Official Vue.js routing |

#### Development Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| @playwright/test | ^1.57.0 | E2E testing framework |
| @tsconfig/node24 | ^24.0.3 | TypeScript config for Node 24 |
| @types/jsdom | ^27.0.0 | Type definitions for jsdom |
| @types/node | ^24.10.1 | Type definitions for Node.js |
| @vitejs/plugin-vue | ^6.0.2 | Vite plugin for Vue SFC |
| @vitejs/plugin-vue-jsx | ^5.1.2 | Vite plugin for Vue JSX |
| @vitest/eslint-plugin | ^1.5.0 | ESLint plugin for Vitest |
| @vue/eslint-config-prettier | ^10.2.0 | Prettier config for Vue |
| @vue/eslint-config-typescript | ^14.6.0 | TypeScript config for Vue |
| @vue/test-utils | ^2.4.6 | Vue component testing utilities |
| @vue/tsconfig | ^0.8.1 | Vue TypeScript configurations |
| eslint | ^9.39.1 | JavaScript/TypeScript linter |
| eslint-plugin-playwright | ^2.3.0 | ESLint rules for Playwright |
| eslint-plugin-vue | ~10.5.1 | ESLint rules for Vue |
| jiti | ^2.6.1 | Runtime TypeScript and ESM support |
| jsdom | ^27.2.0 | JavaScript DOM implementation |
| npm-run-all2 | ^8.0.4 | Run multiple npm scripts |
| prettier | 3.6.2 | Code formatter |
| typescript | ~5.9.0 | TypeScript compiler |
| vite | ^7.2.4 | Build tool and dev server |
| vite-plugin-vue-devtools | ^8.0.5 | Vue DevTools integration |
| vitest | ^4.0.14 | Unit testing framework |
| vue-tsc | ^3.1.5 | Vue TypeScript compiler |

### Infrastructure Dependencies

#### Azure Bicep Modules
From [infra/resources.bicep](../../../infra/resources.bicep):

All infrastructure uses **Azure Verified Modules (AVM)** from the public Bicep registry:

| Module | Version | Purpose |
|--------|---------|---------|
| avm/ptn/azd/monitoring | 0.1.0 | Log Analytics & Application Insights |
| avm/res/container-registry/registry | 0.1.1 | Azure Container Registry |
| avm/res/app/managed-environment | 0.4.5 | Container Apps Environment |
| avm/res/document-db/database-account | 0.8.1 | Azure Cosmos DB |
| avm/res/managed-identity/user-assigned-identity | 0.2.1 | User-assigned managed identities |
| avm/res/app/container-app | 0.8.0 | Container Apps (backend & frontend) |

Custom module:
- [infra/modules/fetch-container-image.bicep](../../../infra/modules/fetch-container-image.bicep) - Fetches latest container image for deployment

## Code Organization

### Backend Structure

```
src/backend/
├── appsettings.json              # Application configuration
├── appsettings.Development.json  # Development-specific config
├── backend.csproj                # Project file with dependencies
├── backend.http                  # HTTP request examples
├── Dockerfile                    # Container build instructions
├── Program.cs                    # Main application file (325 lines)
│   ├── Configuration & DI setup
│   ├── CORS configuration
│   ├── Weather forecast endpoint
│   ├── Temperature CRUD endpoints
│   ├── Data models (records)
│   └── TemperatureMeasurementStore class
├── Properties/
│   └── launchSettings.json       # Launch profiles
├── bin/                          # Build output (not in source control)
└── obj/                          # Build artifacts (not in source control)
```

**Code Organization Notes:**
- **Single-file application**: All code in [Program.cs](../../../src/backend/Program.cs)
- **Minimal API pattern**: No controllers, uses `app.Map*` methods
- **Top-level statements**: Modern C# 10+ style
- **Record types**: Immutable data models
- **Inline class definitions**: `TemperatureMeasurementStore` defined in same file
- **No separate layer separation**: Business logic, data access, and API endpoints all in one file

### Frontend Structure

```
src/frontend/
├── public/                       # Static assets
├── src/
│   ├── App.vue                   # Root component
│   ├── main.ts                   # Application entry point
│   ├── assets/
│   │   ├── base.css              # Base styles
│   │   └── main.css              # Main styles
│   ├── components/               # Vue components
│   │   ├── HelloWorld.vue
│   │   ├── TemperatureManager.vue  # Temperature CRUD UI (250+ lines)
│   │   ├── TheWelcome.vue
│   │   ├── WeatherForecast.vue     # Weather display (160+ lines)
│   │   ├── WelcomeItem.vue
│   │   ├── __tests__/            # Component tests
│   │   │   └── HelloWorld.spec.ts
│   │   └── icons/                # SVG icon components
│   ├── router/
│   │   └── index.ts              # Route definitions
│   ├── services/
│   │   └── api.ts                # Backend API client (80 lines)
│   ├── stores/
│   │   └── counter.ts            # Pinia store example
│   ├── types/
│   │   └── weather.ts            # TypeScript type definitions
│   └── views/                    # Route view components
│       ├── AboutView.vue
│       └── HomeView.vue          # Main dashboard (200+ lines)
├── e2e/                          # End-to-end tests
│   ├── tsconfig.json
│   └── vue.spec.ts
├── index.html                    # HTML entry point
├── package.json                  # Dependencies and scripts
├── tsconfig.json                 # TypeScript config
├── vite.config.ts                # Vite configuration
├── vitest.config.ts              # Vitest config
├── playwright.config.ts          # Playwright config
├── eslint.config.ts              # ESLint config
└── Dockerfile                    # Container build

```

**Code Organization Notes:**
- **Component-based architecture**: Reusable Vue SFC components
- **Composition API**: Modern Vue 3 `<script setup>` syntax
- **Centralized API client**: Single [api.ts](../../../src/frontend/src/services/api.ts) file for all backend calls
- **Type safety**: TypeScript interfaces in [types/weather.ts](../../../src/frontend/src/types/weather.ts)
- **Scoped styles**: Each component has scoped CSS
- **Route-based code splitting**: Lazy-loaded routes

### Infrastructure Structure

```
infra/
├── main.bicep                    # Subscription-scope entry point
├── resources.bicep               # Resource group resources (300+ lines)
├── main.parameters.json          # Bicep parameters
├── abbreviations.json            # Azure resource name abbreviations
├── modules/
│   └── fetch-container-image.bicep  # Custom module
└── scripts/
    ├── postprovision.sh          # Post-provision hook (bash)
    ├── postprovision.ps1         # Post-provision hook (PowerShell)
    ├── predeploy.sh              # Pre-deploy hook (bash)
    └── predeploy.ps1             # Pre-deploy hook (PowerShell)
```

### Root Configuration Files

```
/
├── apphost.cs                    # .NET Aspire orchestration
├── apphost.settings.json         # Aspire settings (generated)
├── apphost.settings.template.json # Settings template
├── apphost.run.json              # Aspire run configuration
├── azure.yaml                    # Azure Developer CLI config
├── apm.yml                       # Application metadata (likely)
├── build.sh                      # Build script
├── README.md                     # Documentation
└── SPEC2CLOUD.md                 # Specification document
```

## Testing Coverage

### Frontend Tests

#### Unit Tests (Vitest)
- **Location**: [src/frontend/src/components/__tests__/](../../../src/frontend/src/components/__tests__/)
- **Framework**: Vitest with Vue Test Utils
- **Current Coverage**: 
  - ✅ 1 test file found: `HelloWorld.spec.ts`
  - ❌ No tests for critical components:
    - `TemperatureManager.vue` - **NOT TESTED**
    - `WeatherForecast.vue` - **NOT TESTED**
    - `HomeView.vue` - **NOT TESTED**
- **Test Command**: `npm run test:unit`

#### E2E Tests (Playwright)
- **Location**: [src/frontend/e2e/](../../../src/frontend/e2e/)
- **Framework**: Playwright
- **Current Coverage**: 
  - ✅ 1 test file: `vue.spec.ts`
  - Coverage details unknown (file content not examined)
- **Test Command**: `npm run test:e2e`

### Backend Tests
- **❌ NO TESTS CONFIGURED**
- No test project in solution
- No test files found
- No test framework dependencies
- No test task in tasks.json

**Testing Gap Analysis:**
- Backend has **0% test coverage**
- Frontend unit tests cover only basic component
- No integration tests
- No API contract tests
- No load/performance tests

## Code Quality Tools

### Linting (Frontend Only)
- **ESLint**: Configured with Vue, TypeScript, Prettier rules
- **Config**: [src/frontend/eslint.config.ts](../../../src/frontend/eslint.config.ts)
- **Command**: `npm run lint`

### Formatting (Frontend Only)
- **Prettier**: Integrated with ESLint
- **Command**: `npm run format`

### Backend Code Quality
- **❌ No linter configured** (no .editorconfig, no analyzer packages)
- **.NET built-in analyzers** likely active via SDK
- No explicit code coverage tools

## Documentation Quality

### Backend Documentation
- ✅ README.md - Good overview and getting started guide
- ✅ OpenAPI - Automatically generated via ASP.NET Core OpenApi
- ✅ Inline comments - Minimal but adequate in Program.cs
- ❌ No XML documentation comments
- ❌ No separate API documentation
- ✅ backend.http - Example HTTP requests for manual testing

### Frontend Documentation
- ✅ README.md in frontend directory (basic Vue info)
- ✅ TypeScript types document data structures
- ❌ No component documentation (no Storybook, etc.)
- ❌ Minimal inline comments in components

### Infrastructure Documentation
- ✅ README.md covers deployment
- ✅ Bicep files are self-documenting with parameters
- ❌ No architecture diagrams
- ❌ No runbook or operations guide

## Package Management Best Practices

### Backend (.NET)
- **Package Manager**: NuGet (implicit via .NET SDK)
- **Lock File**: None (obj/project.assets.json is generated)
- **Vulnerability Scanning**: None configured
- **Version Strategy**: Explicit versions (10.0.0, 13.1.0)

### Frontend (npm)
- **Package Manager**: npm
- **Lock File**: ❌ **NOT FOUND** - package-lock.json missing (should exist)
- **Vulnerability Scanning**: None configured
- **Version Strategy**: Caret ranges (^) for flexibility

### Infrastructure (Bicep)
- **Module Registry**: Azure public Bicep registry
- **Version Strategy**: Explicit versions for AVM modules
- **No lock file** - Bicep doesn't support lock files yet

## Build Performance

### Backend Build
- **Build time**: Fast (single project, minimal dependencies)
- **Output**: Native .NET assemblies
- **Docker build**: Multi-stage for optimization

### Frontend Build
- **Build tool**: Vite (extremely fast)
- **Output**: Optimized JavaScript bundles
- **Docker build**: Multi-stage (Node build → Nginx serve)

## Summary

### Strengths
✅ Minimal, focused dependencies
✅ Modern frameworks and tooling
✅ Type safety with TypeScript and C#
✅ Good infrastructure as code practices

### Gaps and Technical Debt
❌ **No backend testing whatsoever**
❌ **Minimal frontend test coverage**
❌ **Missing package-lock.json** for frontend
❌ **No CI/CD pipeline**
❌ **No linting for backend**
❌ **Single-file backend** may become unmaintainable
❌ **No API versioning strategy**
❌ **No explicit error handling strategy**
