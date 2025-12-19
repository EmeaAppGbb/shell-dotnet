# Technology Stack

## Overview

This application is built as a full-stack web application with a .NET backend and Vue.js frontend, designed for deployment to Azure Container Apps with Cosmos DB as the data store.

## Programming Languages

### Backend
- **C# / .NET 10.0** - Primary backend language
  - Target Framework: `net10.0`
  - Uses implicit usings and nullable reference types enabled
  - File: [src/backend/backend.csproj](../../../src/backend/backend.csproj)

### Frontend
- **TypeScript** - Primary frontend language
  - TypeScript version: `~5.9.0`
  - Strict type checking enabled
  - Files: [src/frontend/tsconfig.json](../../../src/frontend/tsconfig.json), [src/frontend/package.json](../../../src/frontend/package.json)

### Infrastructure
- **Bicep** - Azure infrastructure as code
  - Files: [infra/main.bicep](../../../infra/main.bicep), [infra/resources.bicep](../../../infra/resources.bicep)

### Shell Scripts
- **Bash/PowerShell** - Deployment hooks and automation
  - Files: [infra/scripts/*.sh](../../../infra/scripts/), [infra/scripts/*.ps1](../../../infra/scripts/)

## Frameworks and Libraries

### Backend Framework
- **ASP.NET Core 10.0** - Web API framework
  - Minimal API pattern (no controllers)
  - Built-in dependency injection
  - OpenAPI/Swagger support
  - File: [src/backend/Program.cs](../../../src/backend/Program.cs)

### Frontend Framework
- **Vue.js 3.5.25** - Progressive JavaScript framework
  - Composition API with `<script setup>`
  - Single File Components (.vue)
  - File: [src/frontend/package.json](../../../src/frontend/package.json)

### Frontend State Management
- **Pinia 3.0.4** - Vue state management library
  - Used for application state management
  - File: [src/frontend/src/stores/counter.ts](../../../src/frontend/src/stores/counter.ts)

### Frontend Routing
- **Vue Router 4.6.3** - Official Vue.js routing library
  - File: [src/frontend/src/router/index.ts](../../../src/frontend/src/router/index.ts)

## Database and Data Access

### Database
- **Azure Cosmos DB** - NoSQL database with serverless capability
  - Database: `TemperatureDb`
  - Container: `Temperatures`
  - Partition Key: `/location`
  - File: [src/backend/Program.cs](../../../src/backend/Program.cs#L146-L165)

### Data Access Library
- **Azure Cosmos DB SDK**
  - Package: `Aspire.Microsoft.Azure.Cosmos` version 13.1.0
  - Provides CosmosClient for database operations
  - File: [src/backend/backend.csproj](../../../src/backend/backend.csproj)

## Build Systems and Tools

### Backend Build
- **.NET SDK 10.0** - Build and runtime environment
  - Command: `dotnet build`
  - Project file: [src/backend/backend.csproj](../../../src/backend/backend.csproj)
  - Task defined: [.vscode/tasks.json](../../.vscode/tasks.json)

### Frontend Build
- **Vite 7.2.4** - Frontend build tool and dev server
  - Fast HMR (Hot Module Replacement)
  - Optimized production builds
  - Config: [src/frontend/vite.config.ts](../../../src/frontend/vite.config.ts)

- **npm** - Package manager
  - Node.js requirement: `^20.19.0 || >=22.12.0`
  - Build script: `npm run build`
  - File: [src/frontend/package.json](../../../src/frontend/package.json)

### Local Orchestration
- **.NET Aspire 13.1.0** - Local development orchestration
  - Orchestrates backend and frontend services
  - Provides dashboard at http://localhost:15888
  - File: [apphost.cs](../../../apphost.cs)
  - Packages:
    - `Aspire.AppHost.Sdk@13.1.0`
    - `Aspire.Hosting.JavaScript@13.1.0`
    - `Aspire.Hosting.Azure.CosmosDB@13.1.0`

## Development Tools

### Code Quality - Frontend
- **ESLint 9.39.1** - JavaScript/TypeScript linting
  - Config: [src/frontend/eslint.config.ts](../../../src/frontend/eslint.config.ts)
  - Plugins: Vue, Playwright, Prettier integration

- **Prettier 3.6.2** - Code formatting
  - Integrated with ESLint

### Testing Frameworks

#### Frontend Unit Testing
- **Vitest 4.0.14** - Unit test framework
  - Config: [src/frontend/vitest.config.ts](../../../src/frontend/vitest.config.ts)
  - Test utilities: `@vue/test-utils` 2.4.6
  - Environment: jsdom 27.2.0
  - Example test: [src/frontend/src/components/__tests__/HelloWorld.spec.ts](../../../src/frontend/src/components/__tests__/HelloWorld.spec.ts)

#### Frontend E2E Testing
- **Playwright 1.57.0** - End-to-end testing
  - Config: [src/frontend/playwright.config.ts](../../../src/frontend/playwright.config.ts)
  - Test file: [src/frontend/e2e/vue.spec.ts](../../../src/frontend/e2e/vue.spec.ts)

#### Backend Testing
- **No testing framework currently configured** - Backend has no test project or test dependencies in the solution

## Infrastructure and Cloud

### Cloud Platform
- **Microsoft Azure** - Primary cloud provider
  - Services used:
    - Azure Container Apps (compute)
    - Azure Cosmos DB (database)
    - Azure Container Registry (container images)
    - Azure Monitor / Application Insights (observability)
    - Azure Log Analytics (logging)

### Infrastructure as Code
- **Azure Bicep** - Azure resource definitions
  - Main template: [infra/main.bicep](../../../infra/main.bicep)
  - Resources template: [infra/resources.bicep](../../../infra/resources.bicep)
  - Modules: [infra/modules/fetch-container-image.bicep](../../../infra/modules/fetch-container-image.bicep)

### Deployment Tool
- **Azure Developer CLI (azd)** - Deployment orchestration
  - Config: [azure.yaml](../../../azure.yaml)
  - Commands:
    - `azd up` - Provision and deploy
    - `azd deploy` - Deploy only
    - `azd down` - Delete resources

### Containerization
- **Docker** - Container runtime
  - Backend Dockerfile: [src/backend/Dockerfile](../../../src/backend/Dockerfile)
    - Base: `mcr.microsoft.com/dotnet/aspnet:10.0`
    - Build: `mcr.microsoft.com/dotnet/sdk:10.0`
  - Frontend Dockerfile: [src/frontend/Dockerfile](../../../src/frontend/Dockerfile)
    - Build: `node:lts-alpine`
    - Production: `nginx:stable-alpine`

## Development Dependencies

### Frontend Development Dependencies
Complete list from [src/frontend/package.json](../../../src/frontend/package.json):

- **TypeScript Configuration**: `@tsconfig/node24`, `@vue/tsconfig`
- **Build Plugins**: `@vitejs/plugin-vue`, `@vitejs/plugin-vue-jsx`
- **Dev Tools**: `vite-plugin-vue-devtools`
- **Testing**: `@playwright/test`, `vitest`, `@vitest/eslint-plugin`
- **Type Definitions**: `@types/node`, `@types/jsdom`
- **Linting**: ESLint plugins for Vue, TypeScript, Prettier, Playwright
- **Utilities**: `npm-run-all2`, `jiti`
- **Compiler**: `vue-tsc`

### Backend Dependencies
From [src/backend/backend.csproj](../../../src/backend/backend.csproj):

- **Microsoft.AspNetCore.OpenApi** 10.0.0 - OpenAPI documentation generation
- **Aspire.Microsoft.Azure.Cosmos** 13.1.0 - Cosmos DB client with Aspire integration

## External Services and APIs

### Azure Services
- **Azure Cosmos DB** - NoSQL database
  - Connection configured via connection string
  - Managed identity authentication in production
  - File: [src/backend/Program.cs](../../../src/backend/Program.cs#L19)

### Observability
- **Application Insights** - Application monitoring
  - Configured via connection string environment variable
  - File: [infra/resources.bicep](../../../infra/resources.bicep#L179)

## Version Control and CI/CD

### Version Control
- **Git** - Source control
  - Repository: EmeaAppGbb/shell-dotnet
  - Branch: main

### No CI/CD Pipeline Currently Configured
- No GitHub Actions workflows found
- No Azure DevOps pipeline files found
- Deployment is manual via `azd` commands

## Summary

This is a **modern cloud-native application** built with:
- **.NET 10.0** backend with minimal APIs
- **Vue 3 + TypeScript** frontend with Composition API
- **Azure Container Apps** for serverless container hosting
- **Cosmos DB** for NoSQL data persistence
- **.NET Aspire** for local development orchestration
- **Vite** for fast frontend builds
- **Bicep** for infrastructure as code
- **Azure Developer CLI** for streamlined deployments
