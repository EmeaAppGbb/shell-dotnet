# Dependencies Analysis

## Overview

This document provides a comprehensive analysis of all external dependencies, their versions, purposes, and implications for the application.

## Backend Dependencies (.NET/C#)

### Source: `src/agentic-api/agentic-api.csproj`

#### Microsoft Agent Framework Ecosystem

These are the core dependencies for building and hosting AI agents using the Microsoft Agent Framework.

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `Microsoft.Agents.AI.Workflows` | `1.0.0-preview.251125.1` | Preview | Workflow orchestration, executor pattern, multi-step agent flows |
| `Microsoft.Agents.AI.Hosting` | `1.0.0-preview.251125.1` | Preview | Core hosting infrastructure, agent lifecycle management |
| `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore` | `1.0.0-preview.251125.1` | Preview | AGUI protocol integration for ASP.NET Core |
| `Microsoft.Agents.AI.Hosting.OpenAI` | `1.0.0-alpha.251125.1` | **Alpha** | OpenAI-specific hosting extensions |
| `Microsoft.Agents.AI.DevUI` | `1.0.0-preview.251125.1` | Preview | Developer testing UI |

**Dependency Chain:**
```
Microsoft.Agents.AI.Hosting.AGUI.AspNetCore
  └── Microsoft.Agents.AI.Hosting
      └── Microsoft.Agents.AI.Workflows
```

#### Azure AI & OpenAI Integration

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `Azure.AI.OpenAI` | `2.5.0-beta.1` | Beta | Azure OpenAI Service client SDK |
| `Azure.AI.Projects` | `1.2.0-beta.4` | Beta | Azure AI Foundry project integration |
| `Microsoft.Extensions.AI.OpenAI` | `10.0.1-preview.1.25571.5` | Preview | Unified AI abstractions for OpenAI |
| `Aspire.Azure.AI.Inference` | `13.0.0-preview.1.25560.3` | Preview | .NET Aspire AI service integration |

**Dependency Chain:**
```
Aspire.Azure.AI.Inference
  └── Azure.AI.Projects
      └── Azure.AI.OpenAI
```

#### Authentication & Security

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `Azure.Identity` | `1.18.0-beta.1` | Beta | Azure authentication (DefaultAzureCredential) |

**Authentication Flow:**
```
DefaultAzureCredential attempts:
1. Environment credentials
2. Managed Identity (Azure resources)
3. Visual Studio/VS Code
4. Azure CLI
5. Azure PowerShell
```

#### ASP.NET Core

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `Microsoft.AspNetCore.OpenApi` | `10.0.0` | Release | OpenAPI/Swagger documentation |

### Implicit Dependencies

These are automatically included with .NET 10.0 SDK:

- `Microsoft.AspNetCore.App` framework reference
- `Microsoft.NETCore.App` framework reference
- System libraries (HTTP, JSON, logging, DI)

## Frontend Dependencies (Node.js/TypeScript)

### Source: `src/agentic-ui/package.json`

#### Core Framework Dependencies

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `next` | `16.0.3` | Release | React framework with SSR/SSG |
| `react` | `19.2.0` | Release | Core React library |
| `react-dom` | `19.2.0` | Release | React DOM rendering |

**React 19 New Features Used:**
- Automatic JSX transform (`react-jsx`)
- Modern hooks and concurrent rendering
- Server Components support in Next.js

#### AI Agent Integration (CopilotKit)

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `@copilotkit/react-core` | `^1.10.6` | Release | Core CopilotKit React integration |
| `@copilotkit/react-ui` | `^1.10.6` | Release | Pre-built UI components |
| `@copilotkit/runtime` | `^1.10.6` | Release | Agent runtime and execution |

**CopilotKit Component Usage:**
- `CopilotKit` - Provider component wrapping the app
- `CopilotSidebar` - Chat interface component
- `CopilotRuntime` - Runtime configuration
- `copilotRuntimeNextJSAppRouterEndpoint` - Next.js App Router integration

#### Microsoft Agent Framework Integration (AG-UI)

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `@ag-ui/client` | `^0.0.41` | **Pre-1.0** | HTTP client for Microsoft Agent Framework |
| `@ag-ui/langgraph` | `^0.0.18` | **Pre-1.0** | LangGraph integration |

**Usage Pattern:**
```typescript
new HttpAgent({ 
  url: process.env.AGENT_API_URL || "http://localhost:5149" 
})
```

#### Styling Framework

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `@tailwindcss/postcss` | `^4` | Release | Tailwind CSS v4 PostCSS plugin |
| `tailwindcss` | `^4` | Release | Utility-first CSS framework |

### Development Dependencies

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `typescript` | `^5` | Release | TypeScript compiler |
| `eslint` | `^9` | Release | Code linting |
| `eslint-config-next` | `16.0.3` | Release | Next.js ESLint configuration |
| `@types/node` | `^20` | Release | Node.js type definitions |
| `@types/react` | `^19` | Release | React type definitions |
| `@types/react-dom` | `^19` | Release | React DOM type definitions |

### Transitive Dependencies

**Key transitive dependencies detected in `package-lock.json`:**
- `@playwright/test` - Browser automation (unused in current code)
- Multiple polyfills and utilities for React/Next.js
- PostCSS plugins for CSS processing
- ESLint plugins for code quality

## Orchestration Dependencies

### Source: `apphost.cs` (C# script with package references)

| Package | Version | Status | Purpose |
|---------|---------|--------|---------|
| `Aspire.AppHost.Sdk` | `13.0.0` | Release | .NET Aspire app host SDK |
| `Aspire.Hosting.JavaScript` | `13.0.0` | Release | JavaScript/Node.js app hosting |
| `Aspire.Hosting.Azure.CognitiveServices` | `13.0.0` | Release | Azure Cognitive Services integration |
| `Aspire.Hosting.Azure.AIFoundry` | `13.0.0-preview.1.25560.3` | Preview | Azure AI Foundry integration |

## Infrastructure Dependencies

### Azure Bicep Modules

**Source:** `infra/main.bicep`, `infra/resources.bicep`

**Microsoft Public Registry Modules (br/public:avm/...):**

| Module | Version | Purpose |
|--------|---------|---------|
| `avm/ptn/azd/monitoring` | `0.1.0` | Monitoring (Log Analytics, App Insights) |
| `avm/res/container-registry/registry` | `0.1.1` | Azure Container Registry |
| `avm/res/app/managed-environment` | `0.4.5` | Container Apps environment |
| `avm/res/document-db/database-account` | `0.8.1` | Cosmos DB account |
| `avm/res/search/search-service` | `0.10.0` | Azure AI Search |
| `avm/res/managed-identity/user-assigned-identity` | `0.2.1` | Managed identities |
| `avm/res/app/container-app` | `0.8.0` | Container Apps |

## Development Environment Dependencies

### APM Dependencies

**Source:** `apm.yml`

| Package | Type | Purpose |
|---------|------|---------|
| `EmeaAppGbb/spec2cloud-guidelines` | APM | General engineering standards |
| `EmeaAppGbb/spec2cloud-guidelines-backend` | APM | Backend-specific standards |
| `EmeaAppGbb/spec2cloud-guidelines-frontend` | APM | Frontend-specific standards |

### Python Dependencies (Dev Container)

**Installed via pip:**
- `apm-cli` - Latest version
- MkDocs ecosystem (via devcontainer features)

## Dependency Update Strategy

### Current State: No Automated Updates

**Observations:**
- No Dependabot configuration found
- No automated dependency update workflows
- No lock file update strategy documented
- No security scanning configuration

**Missing:**
- `dependabot.yml` or `renovate.json`
- Security vulnerability scanning
- Automated PR creation for updates

### Version Pinning Strategy

**Backend (.NET):**
- Exact versions specified in `.csproj`
- No floating versions or wildcards
- NuGet lock file not present

**Frontend (Node.js):**
- Caret ranges (`^`) used for most packages
- `package-lock.json` provides deterministic installs
- Allows minor and patch updates automatically

**Infrastructure (Bicep):**
- Explicit module versions
- No automatic module updates

## Dependency Graph

### Backend (Simplified)

```
agentic-api.csproj
├── Microsoft.Agents.AI.Hosting.AGUI.AspNetCore@1.0.0-preview.251125.1
│   ├── Microsoft.Agents.AI.Hosting@1.0.0-preview.251125.1
│   │   └── Microsoft.Agents.AI.Workflows@1.0.0-preview.251125.1
│   └── Microsoft.AspNetCore.App@10.0.0 (framework)
├── Microsoft.Agents.AI.Hosting.OpenAI@1.0.0-alpha.251125.1
├── Microsoft.Extensions.AI.OpenAI@10.0.1-preview.1.25571.5
│   └── Azure.AI.OpenAI@2.5.0-beta.1
├── Aspire.Azure.AI.Inference@13.0.0-preview.1.25560.3
├── Azure.AI.Projects@1.2.0-beta.4
├── Azure.Identity@1.18.0-beta.1
└── Microsoft.AspNetCore.OpenApi@10.0.0
```

### Frontend (Simplified)

```
package.json
├── next@16.0.3
│   ├── react@19.2.0
│   └── react-dom@19.2.0
├── @copilotkit/react-core@^1.10.6
│   ├── @copilotkit/runtime@^1.10.6
│   └── @copilotkit/react-ui@^1.10.6
├── @ag-ui/client@^0.0.41
├── @ag-ui/langgraph@^0.0.18
└── tailwindcss@^4
    └── @tailwindcss/postcss@^4
```

## Summary

### Total Dependencies

- **Backend:** 10 direct dependencies
- **Frontend:** 9 direct dependencies (production)
- **Frontend Dev:** 6 direct dependencies (development)
- **Infrastructure:** 8 Azure modules
- **APM:** 3 guideline packages
