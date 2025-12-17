# Technology Stack Analysis

## Overview

This document provides a comprehensive inventory of all technologies, frameworks, libraries, and tools used in the agentic-shell-dotnet application.

## Programming Languages

### Primary Languages

- **.NET 10.0** (C#)
  - Used for: Backend API service
  - Location: `src/agentic-api/`
  - Target framework: `net10.0`
  - Features: Nullable reference types enabled, implicit usings enabled

- **TypeScript 5.x**
  - Used for: Frontend application
  - Location: `src/agentic-ui/`
  - Target: ES2017
  - Configuration: Strict mode enabled, React JSX with `react-jsx`

- **C# Scripts**
  - Used for: Orchestration and app hosting
  - Location: `apphost.cs`
  - Purpose: .NET Aspire app host configuration

### Configuration Languages

- **Bicep**
  - Used for: Infrastructure as Code
  - Location: `infra/`
  - Purpose: Azure resource provisioning

- **YAML**
  - Used for: Azure Developer CLI configuration, APM configuration
  - Locations: `azure.yaml`, `apm.yml`

## Backend Framework & Libraries

### Core Frameworks

#### Microsoft Agent Framework (Microsoft.Agents.AI)
Primary AI agent framework for building and hosting agentic applications.

**Packages Used:**
- `Microsoft.Agents.AI.Workflows` - Version: `1.0.0-preview.251125.1`
  - Purpose: Workflow orchestration for agents
  - Usage: Building multi-step agent workflows with executors
  
- `Microsoft.Agents.AI.Hosting` - Version: `1.0.0-preview.251125.1`
  - Purpose: Core hosting infrastructure for agents
  - Usage: Agent lifecycle management, registration, hosting
  
- `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore` - Version: `1.0.0-preview.251125.1`
  - Purpose: AGUI (Agent GUI) integration for ASP.NET Core
  - Usage: Exposing agents through HTTP endpoints compatible with AGUI protocol
  
- `Microsoft.Agents.AI.Hosting.OpenAI` - Version: `1.0.0-alpha.251125.1`
  - Purpose: OpenAI integration for agent hosting
  - Usage: OpenAI response and conversation management
  
- `Microsoft.Agents.AI.DevUI` - Version: `1.0.0-preview.251125.1`
  - Purpose: Developer UI for testing agents
  - Usage: Development-time agent debugging interface

#### ASP.NET Core 10.0
- `Microsoft.AspNetCore.OpenApi` - Version: `10.0.0`
  - Purpose: OpenAPI/Swagger document generation
  - Usage: API documentation and schema generation

#### Azure AI Integration
- `Microsoft.Extensions.AI.OpenAI` - Version: `10.0.1-preview.1.25571.5`
  - Purpose: Microsoft.Extensions.AI integration with OpenAI
  - Usage: Unified AI client abstractions for OpenAI models

- `Azure.AI.OpenAI` - Version: `2.5.0-beta.1`
  - Purpose: Azure OpenAI Service client
  - Usage: Direct interaction with Azure OpenAI deployments

- `Azure.AI.Projects` - Version: `1.2.0-beta.4`
  - Purpose: Azure AI Foundry Project SDK
  - Usage: Integration with Azure AI Foundry projects

- `Aspire.Azure.AI.Inference` - Version: `13.0.0-preview.1.25560.3`
  - Purpose: .NET Aspire integration for Azure AI Inference
  - Usage: Service discovery and configuration for AI services

#### Authentication & Security
- `Azure.Identity` - Version: `1.18.0-beta.1`
  - Purpose: Azure authentication using DefaultAzureCredential
  - Usage: Managed identity and local development authentication

### Key Implementation Patterns

**Location:** `src/agentic-api/Program.cs`

- **Dependency Injection:** Uses built-in ASP.NET Core DI container
- **Configuration:** Environment variable-based configuration
  - `AZURE_OPENAI_ENDPOINT` - Required
  - `AZURE_OPENAI_DEPLOYMENT_NAME` - Required
- **Logging:** Console logging with Information level
- **Authentication:** DefaultAzureCredential for Azure services

**Location:** `src/agentic-api/AGUIWorkflowAgent.cs`

- **Agent Wrapper Pattern:** DelegatingAIAgent wrapper for AGUI protocol compatibility
- **Streaming Support:** Async streaming responses with IAsyncEnumerable
- **Event Processing:** Converts workflow events to AGUI-compatible text responses

**Location:** `src/agentic-api/Workflows/DummyWorkflow.cs`

- **Workflow Factory Pattern:** DI-based workflow construction
- **Executor Pattern:** Separate executors for different workflow steps
- **Two-handler Pattern:** Handlers for both ChatMessage and TurnToken inputs

## Frontend Framework & Libraries

### Core Framework

#### Next.js 16.0.3
- **Type:** React framework with server-side rendering
- **Configuration:** Standalone output mode, Turbopack enabled
- **Location:** `src/agentic-ui/`

#### React 19.2.0
- **Version:** Latest stable (React 19)
- **React DOM:** 19.2.0
- **JSX Transform:** Automatic (react-jsx)

### UI & Agent Integration

#### CopilotKit (`@copilotkit/*`)
Complete agent integration framework for building AI-powered UIs.

**Packages:**
- `@copilotkit/react-core` - Version: `^1.10.6`
  - Purpose: Core CopilotKit React integration
  - Usage: CopilotKit provider and runtime configuration
  
- `@copilotkit/react-ui` - Version: `^1.10.6`
  - Purpose: Pre-built UI components for AI interactions
  - Usage: CopilotSidebar component for chat interface
  
- `@copilotkit/runtime` - Version: `^1.10.6`
  - Purpose: Runtime for agent execution
  - Usage: CopilotRuntime with HTTP agent adapter

#### Agent GUI Integration (`@ag-ui/*`)
Microsoft Agent Framework frontend integration.

**Packages:**
- `@ag-ui/client` - Version: `^0.0.41`
  - Purpose: HTTP client for Microsoft Agent Framework
  - Usage: HttpAgent for connecting to backend AGUI endpoints
  
- `@ag-ui/langgraph` - Version: `^0.0.18`
  - Purpose: LangGraph integration for AG-UI
  - Usage: Graph-based agent orchestration (potential future use)

### Styling

#### Tailwind CSS 4.x
- **PostCSS Plugin:** `@tailwindcss/postcss` - Version: `^4`
- **Configuration:** Integrated via PostCSS
- **Usage:** Utility-first CSS styling throughout the application

### Development Tools

- **ESLint 9.x**
  - Configuration: `eslint-config-next` - Version: `16.0.3`
  - Purpose: Code linting and quality enforcement

- **TypeScript 5.x**
  - Type definitions: `@types/node`, `@types/react`, `@types/react-dom`
  - Strict mode enabled

## Orchestration & Infrastructure

### .NET Aspire

**Version:** 13.0.0

**SDK Package:**
- `Aspire.AppHost.Sdk` - Version: `13.0.0`
  - Purpose: App host orchestration framework
  - Location: `apphost.cs`

**Hosting Packages:**
- `Aspire.Hosting.JavaScript` - Version: `13.0.0`
  - Purpose: JavaScript/Node.js app hosting
  - Usage: Running Next.js frontend in Aspire
  
- `Aspire.Hosting.Azure.CognitiveServices` - Version: `13.0.0`
  - Purpose: Azure Cognitive Services integration
  - Usage: Azure OpenAI service references
  
- `Aspire.Hosting.Azure.AIFoundry` - Version: `13.0.0-preview.1.25560.3`
  - Purpose: Azure AI Foundry integration
  - Usage: AI Foundry project references

**Configuration:**
- Parameters: `openAiEndpoint`, `openAiDeployment`
- Service Discovery: Automatic endpoint resolution
- Environment Variables: Injected into services via Aspire

## Build & Development Tools

### .NET Tools

- **.NET SDK 10.0**
  - Build system: MSBuild
  - Package manager: NuGet
  - Runtime: .NET 10.0

- **Docker Support**
  - Multi-stage Dockerfiles for both services
  - Base images:
    - Backend: `mcr.microsoft.com/dotnet/aspnet:10.0`
    - SDK: `mcr.microsoft.com/dotnet/sdk:10.0`

### Node.js Tools

- **Node.js 20.x** (LTS)
  - Package manager: npm
  - Lock file: `package-lock.json` present

- **Next.js CLI**
  - Commands: `dev`, `build`, `start`, `lint`

### Azure Developer CLI (azd)

**Configuration:** `azure.yaml`

**Services:**
- `agentic-api` - .NET backend (Container App)
- `agentic-ui` - Next.js frontend (Container App)

**Resources:**
- Container Apps
- Azure OpenAI
- Azure AI Foundry

**Hooks:**
- `postprovision` - Scripts for post-deployment configuration

### APM CLI

**Version:** Installed via pip (latest)

**Configuration:** `apm.yml`

**Purpose:** Agent Package Manager for managing engineering standards

**Dependencies:**
- `EmeaAppGbb/spec2cloud-guidelines`
- `EmeaAppGbb/spec2cloud-guidelines-backend`
- `EmeaAppGbb/spec2cloud-guidelines-frontend`

**Scripts:**
- `prd` - Product Requirements Document generation
- `frd` - Feature Requirements Documents generation
- `plan` - Technical planning
- `implement` - Implementation
- `delegate` - Task delegation
- `deploy` - Azure deployment

## Development Environment

### Dev Container

**Base Image:** `mcr.microsoft.com/devcontainers/python:1-3.12-bullseye`

**Features Installed:**
- **Azure CLI** (latest) with Bicep (latest)
- **TypeScript** (latest)
- **Azure Developer CLI** (stable)
- **Docker-in-Docker** (latest with Buildx and Compose)
- **.NET SDK 9.0**
- **.NET Aspire** (latest)
- **MkDocs** (latest) with plugins:
  - mkdocs-material
  - pymdown-extensions
  - mkdocstrings[crystal,python]
  - mkdocs-monorepo-plugin
  - mkdocs-pdf-export-plugin
  - mkdocs-awesome-pages-plugin
  - mkdocs-minify-plugin
  - mkdocs-git-revision-date-localized-plugin

**VS Code Extensions:**
- GitHub Copilot Chat
- Azure Pack (`ms-vscode.vscode-node-azure-pack`)
- AI Toolkit (`ms-windows-ai-studio.windows-ai-studio`)
- C# Dev Kit (`ms-dotnettools.csdevkit`)
- .NET Aspire (`microsoft-aspire.aspire-vscode`)
- .NET Pack (`ms-dotnettools.vscode-dotnet-pack`)

**Post-Create Command:** APM CLI installation via pip

**Environment Variables:**
- `GITHUB_COPILOT_PAT` - Passed from host
- `GITHUB_APM_PAT` - Passed from host

### Model Context Protocol (MCP) Servers

**Configuration:** `.vscode/mcp.json`

**Servers Available:**
1. **context7** - `https://mcp.context7.com/mcp`
   - Type: HTTP
   - Purpose: Up-to-date library documentation

2. **github** - `https://api.githubcopilot.com/mcp/`
   - Type: HTTP
   - Purpose: Repository management and operations

3. **microsoft.docs.mcp** - `https://learn.microsoft.com/api/mcp`
   - Type: HTTP
   - Purpose: Official Microsoft/Azure documentation

4. **playwright** - `npx @playwright/mcp@latest`
   - Type: stdio
   - Purpose: Browser automation capabilities

5. **deepwiki** - `https://mcp.deepwiki.com/sse`
   - Type: HTTP
   - Purpose: Repository context and understanding

6. **copilotkit** - `https://mcp.copilotkit.ai/sse`
   - Type: HTTP
   - Purpose: CopilotKit-specific integrations

## Testing & Quality Assurance

### Current State: No Testing Framework Detected

**Observation:** No test files, test configurations, or testing frameworks were found in the codebase.

**Missing:**
- No unit test projects or test files
- No test runners (xUnit, NUnit, MSTest, Jest, Vitest, etc.)
- No test configuration files
- No code coverage tools
- No CI/CD pipelines with test execution

## External Dependencies & Services

### Required Azure Services

1. **Azure OpenAI Service**
   - Required configuration: Endpoint, Deployment Name
   - Authentication: DefaultAzureCredential

2. **Azure AI Foundry**
   - Project endpoint required
   - Integration via Azure AI Projects SDK

3. **Azure Cosmos DB** (Provisioned but not actively used in code)
   - Serverless mode
   - SQL API
   - Database: `agentic-storage`

4. **Azure AI Search** (Provisioned but not actively used in code)
   - Basic SKU
   - RBAC authentication

5. **Azure Container Registry**
   - For storing container images
   - Used by Azure Container Apps

6. **Azure Container Apps**
   - Hosting platform for both services
   - Environment with Log Analytics integration

7. **Azure Monitor / Application Insights**
   - Connection string injected into services
   - Telemetry and logging

## Version Summary

### Backend Core
- .NET: 10.0
- C#: 12 (implicit with .NET 10)
- ASP.NET Core: 10.0
- Microsoft Agent Framework: 1.0.0-preview.251125.1
- Azure OpenAI SDK: 2.5.0-beta.1
- Azure Identity: 1.18.0-beta.1

### Frontend Core
- Node.js: 20.x (LTS)
- TypeScript: 5.x
- Next.js: 16.0.3
- React: 19.2.0
- CopilotKit: 1.10.6
- AG-UI Client: 0.0.41

### Infrastructure
- .NET Aspire: 13.0.0
- Azure Developer CLI: Latest
- Bicep: Latest
- Docker: Latest

### Development
- Python: 3.12 (dev container)
- APM CLI: Latest
- MkDocs: Latest

## Notable Technology Choices

### Cutting-Edge Stack
- **.NET 10.0** - Latest .NET version (preview/early release)
- **React 19** - Latest React version with new features
- **Next.js 16** - Latest Next.js with Turbopack
- **Microsoft Agent Framework** - Preview release (1.0.0-preview)
- **Azure AI Foundry** - New Azure AI platform

### Preview/Beta Software
The application heavily uses preview and beta versions:
- All Microsoft Agent Framework packages (preview)
- Azure OpenAI SDK (beta)
- Azure Identity (beta)
- Aspire Azure AI Foundry integration (preview)
- Microsoft.Extensions.AI.OpenAI (preview)

### Modern Patterns
- **Managed Identity** for authentication (no secrets)
- **Standalone Next.js** output for containerization
- **Server-side rendering** with React Server Components
- **Workflow-based agent architecture**
- **Infrastructure as Code** with Bicep
