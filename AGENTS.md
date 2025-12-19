# AGENTS.md

A guide for AI coding agents working with this .NET + Vue.js full-stack application deployed on Azure Container Apps.

---

## Project Overview

**Project Name**: shell-dotnet  
**Type**: Full-stack web application (Demo/Starter)  
**Architecture**: Three-tier (Presentation → Application → Data)  
**Status**: Development/Demo - NOT production-ready

### Technology Stack

**Backend**:
- .NET 10.0 with ASP.NET Core Minimal API
- Single-file backend (`src/backend/Program.cs`, ~325 lines)
- Azure Cosmos DB SDK (`Microsoft.Azure.Cosmos` 3.46.0)
- .NET Aspire 13.1.0 for local orchestration

**Frontend**:
- Vue.js 3.5.25 with Composition API + TypeScript 5.7.3
- Vite 7.2.4 (build tool and dev server)
- Vue Router 4.5.0 for SPA routing
- No state management library (uses Vue refs/reactive)

**Infrastructure**:
- Azure Container Apps (serverless containers)
- Azure Cosmos DB (NoSQL, serverless, SQL API)
- Azure Container Registry
- Application Insights for monitoring
- Deployed via Azure Developer CLI (azd)
- Infrastructure as Code: Bicep with Azure Verified Modules

**Key Features**:
1. Weather forecast display (randomly generated, demo feature)
2. Temperature measurement CRUD operations (Cosmos DB persistence)
3. Responsive Vue.js frontend with modal forms

### Repository Structure

```
/
├── src/
│   ├── backend/          # .NET 10.0 ASP.NET Core Minimal API
│   │   ├── Program.cs    # Single-file backend (~325 lines)
│   │   ├── backend.csproj
│   │   ├── Dockerfile    # Multi-stage .NET build
│   │   └── appsettings.json
│   └── frontend/         # Vue.js 3 + TypeScript
│       ├── src/
│       │   ├── components/  # Vue components
│       │   ├── views/       # Page components
│       │   ├── services/    # API client (api.ts)
│       │   ├── types/       # TypeScript interfaces
│       │   └── router/      # Vue Router config
│       ├── package.json
│       ├── vite.config.ts
│       └── Dockerfile    # Multi-stage Node + Nginx build
├── infra/               # Bicep infrastructure
│   ├── main.bicep       # Entry point
│   ├── resources.bicep  # Azure resources (300+ lines)
│   └── scripts/         # Pre/post deployment scripts
├── specs/               # Technical documentation (YOU CREATED THIS)
│   ├── docs/            # Architecture, APIs, infrastructure
│   └── features/        # Feature requirements
├── apphost.cs           # .NET Aspire orchestration
├── azure.yaml           # Azure Developer CLI config
└── build.sh             # Build script
```

---

## Build and Run Commands

### Prerequisites

Install these tools before working with the project:

```bash
# Required
dotnet --version    # Must be 10.0+
node --version      # Must be 22.x
npm --version       # Must be 10.x
azd version         # Azure Developer CLI

# Optional (for local dev)
docker --version    # For containerized dev
```

### Local Development

**Option 1: .NET Aspire (Recommended)**

Start both backend and frontend with orchestration dashboard:

```bash
dotnet run --project apphost.cs
```

- Opens Aspire Dashboard at http://localhost:15888
- Backend runs at http://localhost:5000
- Frontend runs at http://localhost:5173
- Automatically sets up service discovery

**Option 2: Manual (Backend + Frontend separately)**

Backend:
```bash
cd src/backend
dotnet restore
dotnet build
dotnet run
# Runs at http://localhost:5000
```

Frontend:
```bash
cd src/frontend
npm install
npm run dev
# Runs at http://localhost:5173
```

**Environment Variables (Backend)**:
```bash
export AZURE_COSMOS_DB_ENDPOINT="https://[account].documents.azure.com:443/"
# OR use Aspire which sets this automatically
```

### Build Commands

**Backend**:
```bash
cd src/backend
dotnet build backend.csproj
# Output: bin/Debug/net10.0/backend.dll
```

**Frontend**:
```bash
cd src/frontend
npm install
npm run build
# Output: dist/ folder with static files
```

**Build All** (via VS Code task):
```bash
# From workspace root
# Uses .vscode/tasks.json
code --task build-all
```

**Docker Build**:
```bash
# Backend
cd src/backend
docker build -t backend:latest .

# Frontend
cd src/frontend
docker build -t frontend:latest .
```

### Test Commands

**Backend**:
```bash
cd src/backend
dotnet test
# NOTE: No tests currently exist (0% coverage)
```

**Frontend**:
```bash
cd src/frontend
npm run test:unit      # Vitest unit tests
npm run test:e2e       # Playwright E2E tests (headless)
npm run test:e2e:ui    # Playwright with UI
# NOTE: Only default scaffold tests exist
```

**Linting**:
```bash
cd src/frontend
npm run lint           # ESLint check
npm run format         # Check formatting
```

### Deployment

**Deploy to Azure** (creates all resources):
```bash
azd up
# Provisions Azure resources via Bicep
# Builds and deploys backend + frontend containers
# Takes ~10-15 minutes first time
```

**Re-deploy after code changes**:
```bash
azd deploy
# Rebuilds containers and deploys
# Takes ~5 minutes
```

**Infrastructure only** (no deployment):
```bash
azd provision
```

**Destroy all Azure resources**:
```bash
azd down --purge
```

---

## Development Workflows

### Adding a New API Endpoint

**Backend** (`src/backend/Program.cs`):

1. Add endpoint registration after existing endpoints (around line 95):
```csharp
app.MapGet("/api/myendpoint", async (MyService service) => 
{
    var result = await service.GetDataAsync();
    return Results.Ok(result);
})
.WithName("GetMyData")
.WithOpenApi();
```

2. Add data models as records (after line 144):
```csharp
public record MyDataModel(string Id, string Name);
```

3. If needed, add repository methods to `TemperatureMeasurementStore` or create new store class

**Frontend** (`src/frontend/src/services/api.ts`):

1. Add TypeScript function:
```typescript
export async function getMyData(): Promise<MyDataModel[]> {
  const response = await fetch(`${API_BASE}/api/myendpoint`)
  if (!response.ok) throw new Error('Failed to fetch data')
  return response.json()
}
```

2. Add TypeScript interface (`src/frontend/src/types/`):
```typescript
export interface MyDataModel {
  id: string
  name: string
}
```

**Testing**:
- Backend: Add unit test in new `tests/` directory
- Frontend: Add test in `src/components/__tests__/`
- E2E: Add test in `e2e/` directory

### Adding a New Vue Component

1. Create component file: `src/frontend/src/components/MyComponent.vue`

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getMyData } from '@/services/api'

const data = ref([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    data.value = await getMyData()
  } catch (e: any) {
    error.value = e.message
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div>
    <p v-if="loading">Loading...</p>
    <p v-if="error" class="error">{{ error }}</p>
    <div v-if="!loading && !error">
      <!-- Component content -->
    </div>
  </div>
</template>

<style scoped>
/* Component styles */
</style>
```

2. Import in parent component:
```vue
<script setup lang="ts">
import MyComponent from '@/components/MyComponent.vue'
</script>

<template>
  <MyComponent />
</template>
```

3. Add test: `src/frontend/src/components/__tests__/MyComponent.spec.ts`

### Adding a New Page/Route

1. Create view: `src/frontend/src/views/MyView.vue`

2. Register route in `src/frontend/src/router/index.ts`:
```typescript
{
  path: '/mypage',
  name: 'mypage',
  component: () => import('../views/MyView.vue')
}
```

3. Add navigation link in `src/frontend/src/App.vue`:
```vue
<RouterLink to="/mypage">My Page</RouterLink>
```

### Modifying Cosmos DB Schema

**WARNING**: Cosmos DB is schema-less, but partition key cannot be changed after container creation.

**Adding a new field**:
1. Update C# record in `Program.cs`:
```csharp
public record TemperatureMeasurement(
    string Id,
    string Location,
    double TemperatureC,
    DateTime Timestamp,
    string? Description,
    string? NewField  // Add here
);
```

2. Update document class:
```csharp
public class TemperatureMeasurementDocument
{
    // ... existing properties
    
    [JsonPropertyName("newField")]
    public string? NewField { get; set; }
}
```

3. Update TypeScript interface in `src/frontend/src/types/weather.ts`:
```typescript
export interface TemperatureMeasurement {
  // ... existing properties
  newField?: string
}
```

4. Update UI component to display new field

**Creating a new container**:
1. Add to `src/backend/Program.cs` after line 20:
```csharp
var newContainer = cosmosDatabase.GetContainer("NewContainerName");
builder.Services.AddSingleton(newContainer);
```

2. Add to `infra/resources.bicep` in Cosmos DB account resource (around line 100):
```bicep
{
  name: 'NewContainerName'
  properties: {
    resource: {
      id: 'NewContainerName'
      partitionKey: {
        paths: ['/partitionKeyField']
        kind: 'Hash'
      }
    }
  }
}
```

3. Deploy infrastructure: `azd provision`

### Adding Azure Resources

1. Edit `infra/resources.bicep`
2. Add new module or resource definition
3. Wire up dependencies (container apps need connection strings)
4. Run `azd provision` to apply changes

**Example: Add Azure Storage**:
```bicep
module storage 'br/public:avm/res/storage/storage-account:0.14.3' = {
  name: 'storage'
  params: {
    name: 'st${abbrs.storageStorageAccounts}${resourceToken}'
    location: location
    kind: 'StorageV2'
  }
}
```

---

## Code Style and Conventions

### Backend (.NET)

**File Organization**:
- Single-file backend approach (Program.cs)
- All code in one file: endpoints, models, data access
- Top-down structure: configuration → endpoints → models → data access

**Naming Conventions**:
- PascalCase for types, methods, properties
- camelCase for local variables, parameters
- Records for DTOs: `public record WeatherForecast(...)`
- Classes for Cosmos DB documents with JsonPropertyName attributes

**API Conventions**:
- RESTful endpoints: `/api/resource` pattern
- Use `Results` static class: `Results.Ok()`, `Results.NotFound()`
- Always call `.WithName()` and `.WithOpenApi()` for Swagger
- Async/await for all I/O operations
- Return status codes: 200 (OK), 201 (Created), 204 (No Content), 404 (Not Found)

**Example Endpoint Pattern**:
```csharp
app.MapGet("/api/resource/{id}", async (string id, MyService service) =>
{
    var item = await service.GetByIdAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
})
.WithName("GetResourceById")
.WithOpenApi();
```

**Cosmos DB Patterns**:
- Use repository pattern (see `TemperatureMeasurementStore` class)
- Always include partition key in queries for efficiency
- Use `ReadItemAsync` (point read) when you know ID + partition key (1 RU)
- Avoid cross-partition queries when possible (expensive)
- Use `FeedIterator` for pagination (not currently implemented)

**Error Handling**:
- Try-catch in repository methods
- Return null for not found (404)
- Throw exceptions for unexpected errors
- Let minimal API middleware handle exceptions

### Frontend (Vue.js + TypeScript)

**File Organization**:
- Components: `src/components/[ComponentName].vue`
- Views: `src/views/[ViewName].vue`
- Services: `src/services/[serviceName].ts`
- Types: `src/types/[domain].ts`
- Use index exports for cleaner imports

**Component Structure**:
```vue
<script setup lang="ts">
// 1. Imports
import { ref, computed, onMounted } from 'vue'
import type { MyType } from '@/types/mytype'

// 2. Props/Emits (if any)
const props = defineProps<{
  myProp: string
}>()

// 3. State
const data = ref<MyType[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

// 4. Computed
const filteredData = computed(() => {
  // ...
})

// 5. Methods
const loadData = async () => {
  // ...
}

// 6. Lifecycle hooks
onMounted(async () => {
  await loadData()
})
</script>

<template>
  <!-- Template with conditional rendering -->
  <div>
    <p v-if="loading">Loading...</p>
    <p v-if="error" class="error">{{ error }}</p>
    <div v-if="!loading && !error">
      <!-- Content -->
    </div>
  </div>
</template>

<style scoped>
/* Scoped styles - avoid global styles */
</style>
```

**Naming Conventions**:
- PascalCase for components: `MyComponent.vue`
- camelCase for functions, variables: `loadData`, `myVariable`
- kebab-case in templates: `<my-component />`
- Prefix custom events: `emit('update:modelValue')`

**TypeScript**:
- Always use TypeScript, never plain JavaScript
- Prefer `interface` over `type` for object shapes
- Use `Type[]` instead of `Array<Type>`
- Mark optional properties with `?`
- Use `as const` for literal types

**Async Patterns**:
```typescript
// Use try-catch-finally for loading states
const loadData = async () => {
  try {
    loading.value = true
    error.value = null
    data.value = await apiCall()
  } catch (e: any) {
    error.value = e.message || 'An error occurred'
  } finally {
    loading.value = false
  }
}
```

**API Client Pattern** (`src/services/api.ts`):
```typescript
const API_BASE = import.meta.env.VITE_API_BASE_URL || ''

export async function getResource(): Promise<ResourceType[]> {
  const response = await fetch(`${API_BASE}/api/resource`)
  if (!response.ok) {
    throw new Error(`Failed to fetch: ${response.statusText}`)
  }
  return response.json()
}
```

**State Management**:
- Use `ref()` for primitives: `const count = ref(0)`
- Use `reactive()` for objects: `const form = reactive({ name: '', email: '' })`
- No Pinia/Vuex in this project (consider adding for complex state)

**CSS/Styling**:
- Always use `<style scoped>` to avoid global pollution
- Use CSS custom properties for theming (defined in `base.css`)
- Mobile-first responsive design
- Use Flexbox or CSS Grid for layouts

### Bicep (Infrastructure)

**File Organization**:
- `main.bicep` - Entry point, parameters, resource group scope
- `resources.bicep` - All resource definitions
- `modules/` - Reusable modules (currently only fetch-container-image.bicep)

**Conventions**:
- Use Azure Verified Modules from `br/public:avm/...` registry
- Managed identities for authentication (no secrets in code)
- Parameter naming: camelCase
- Resource naming: Use abbreviations.json + resourceToken
- Always tag resources: `tags: tags` parameter

**Module Pattern**:
```bicep
module myResource 'br/public:avm/res/[provider]/[resource]:version' = {
  name: 'myResource'
  params: {
    name: '${abbrs.resourceType}${resourceToken}'
    location: location
    tags: tags
    // ... other params
  }
}
```

**Outputs**:
- Always output connection strings, endpoints as secure strings
- Use KeyVault for secrets (not currently implemented)

---

## Testing Instructions

### Backend Testing

**Current State**: ❌ NO TESTS EXIST (0% coverage)

**Create test project**:
```bash
cd tests
dotnet new xunit -n backend.Tests
cd backend.Tests
dotnet add reference ../../src/backend/backend.csproj
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Moq
```

**Test Structure**:
```
tests/
└── backend.Tests/
    ├── Controllers/
    │   └── TemperatureControllerTests.cs
    ├── Services/
    │   └── TemperatureMeasurementStoreTests.cs
    └── Integration/
        └── ApiIntegrationTests.cs
```

**Example Unit Test**:
```csharp
using Xunit;
using Moq;

public class TemperatureMeasurementStoreTests
{
    [Fact]
    public async Task CreateAsync_Should_GenerateId_And_SetTimestamp()
    {
        // Arrange
        var mockContainer = new Mock<Container>();
        var store = new TemperatureMeasurementStore(mockContainer.Object);
        var input = new TemperatureMeasurementInput("Office", 22.5, "Test");

        // Act
        var result = await store.CreateAsync(input);

        // Assert
        Assert.NotNull(result.Id);
        Assert.NotEmpty(result.Id);
        Assert.True(result.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
    {
        // Arrange
        var mockContainer = new Mock<Container>();
        mockContainer
            .Setup(c => c.ReadItemAsync<TemperatureMeasurementDocument>(
                It.IsAny<string>(), 
                It.IsAny<PartitionKey>(), 
                null, 
                default))
            .ThrowsAsync(new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0));
        
        var store = new TemperatureMeasurementStore(mockContainer.Object);

        // Act
        var result = await store.GetByIdAsync("nonexistent", "Office");

        // Assert
        Assert.Null(result);
    }
}
```

**Run Tests**:
```bash
cd tests/backend.Tests
dotnet test
dotnet test --logger "console;verbosity=detailed"  # Verbose output
dotnet test --collect:"XPlat Code Coverage"        # Coverage report
```

### Frontend Testing

**Current State**: ⚠️ Only default scaffold tests exist

**Test Structure**:
```
src/frontend/
├── src/
│   └── components/
│       └── __tests__/
│           ├── WeatherForecast.spec.ts
│           ├── TemperatureManager.spec.ts
│           └── HelloWorld.spec.ts (exists, but HelloWorld.vue not used)
└── e2e/
    ├── weather.spec.ts
    └── temperature.spec.ts
```

**Unit Tests (Vitest)**:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import TemperatureManager from '@/components/TemperatureManager.vue'
import * as api from '@/services/api'

// Mock API
vi.mock('@/services/api')

describe('TemperatureManager', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('loads measurements on mount', async () => {
    const mockData = [
      { id: '1', location: 'Office', temperatureC: 22, timestamp: '2025-12-19T10:00:00Z' }
    ]
    vi.mocked(api.getTemperatures).mockResolvedValue(mockData)

    const wrapper = mount(TemperatureManager)
    await flushPromises()

    expect(wrapper.text()).toContain('Office')
    expect(wrapper.text()).toContain('22')
  })

  it('displays error when API fails', async () => {
    vi.mocked(api.getTemperatures).mockRejectedValue(new Error('Network error'))

    const wrapper = mount(TemperatureManager)
    await flushPromises()

    expect(wrapper.text()).toContain('Network error')
  })

  it('opens modal when Add button clicked', async () => {
    vi.mocked(api.getTemperatures).mockResolvedValue([])
    
    const wrapper = mount(TemperatureManager)
    await flushPromises()
    
    await wrapper.find('button.add-button').trigger('click')
    
    expect(wrapper.find('.modal').isVisible()).toBe(true)
  })
})
```

**Run Unit Tests**:
```bash
cd src/frontend
npm run test:unit                    # Run once
npm run test:unit -- --watch         # Watch mode
npm run test:unit -- --coverage      # With coverage
npm run test:unit -- --ui            # Visual UI
```

**E2E Tests (Playwright)**:

```typescript
import { test, expect } from '@playwright/test'

test.describe('Temperature Management', () => {
  test('complete CRUD workflow', async ({ page }) => {
    await page.goto('/about')

    // Create
    await page.click('text=Add Temperature')
    await page.fill('input[name="location"]', 'Test Location')
    await page.fill('input[name="temperatureC"]', '25')
    await page.fill('textarea[name="description"]', 'Test description')
    await page.click('button:has-text("Save")')

    // Verify created
    await expect(page.locator('text=Test Location')).toBeVisible()
    await expect(page.locator('text=25')).toBeVisible()

    // Edit
    await page.click('button:has-text("Edit")')
    await page.fill('input[name="temperatureC"]', '26')
    await page.click('button:has-text("Save")')

    // Verify updated
    await expect(page.locator('text=26')).toBeVisible()

    // Delete
    await page.click('button:has-text("Delete")')
    await expect(page.locator('text=Test Location')).not.toBeVisible()
  })

  test('displays validation errors', async ({ page }) => {
    await page.goto('/about')
    await page.click('text=Add Temperature')
    
    // Try to save without required fields
    await page.click('button:has-text("Save")')
    
    await expect(page.locator('text=required')).toBeVisible()
  })
})
```

**Run E2E Tests**:
```bash
cd src/frontend
npm run test:e2e              # Headless
npm run test:e2e:ui           # With UI
npx playwright test --headed  # See browser
npx playwright test --debug   # Debug mode
npx playwright show-report    # View HTML report
```

**Test Coverage Requirements**:
- Aim for 80%+ coverage on critical paths
- All API endpoints should have integration tests
- All UI components with logic should have unit tests
- Key user workflows should have E2E tests

**Before Committing**:
```bash
# Backend
cd src/backend
dotnet test

# Frontend
cd src/frontend
npm run lint
npm run test:unit
npm run test:e2e

# If all pass, commit
```

---

## Security Considerations

### ⚠️ CRITICAL GAPS IN THIS APPLICATION

**NO AUTHENTICATION** - Anyone can access all endpoints

**NO AUTHORIZATION** - No access control or permissions

**INSECURE CORS** - Allows all origins: `builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()))`

**NO INPUT VALIDATION** - Temperature, location, description not validated

**NO RATE LIMITING** - API can be abused

**NO SECRETS MANAGEMENT** - No Azure KeyVault integration

**NO AUDIT LOGGING** - No tracking of who did what

### What IS Secure

✅ **Managed Identities** - Backend uses managed identity for Cosmos DB (no connection strings)

✅ **HTTPS** - Container Apps enforce HTTPS

✅ **Cosmos DB RBAC** - Uses role-based access control (not master key)

### Recommendations for Production

1. **Add Authentication**:
```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = "https://login.microsoftonline.com/{tenant}";
        options.Audience = "{client-id}";
    });

app.UseAuthentication();
app.UseAuthorization();

// Protect endpoints
app.MapGet("/api/temperatures", async () => { })
    .RequireAuthorization();
```

2. **Add Input Validation**:
```csharp
public record TemperatureMeasurementInput(
    [Required, StringLength(100)] string Location,
    [Range(-50, 100)] double TemperatureC,
    [StringLength(500)] string? Description
);
```

3. **Fix CORS**:
```csharp
builder.Services.AddCors(options => 
    options.AddDefaultPolicy(policy => 
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()));
```

4. **Add Rate Limiting**:
```csharp
builder.Services.AddRateLimiter(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            partition => new FixedWindowRateLimiterOptions {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

5. **Add Azure KeyVault**:
```bicep
// infra/resources.bicep
module keyVault 'br/public:avm/res/key-vault/vault:0.10.2' = {
  name: 'keyVault'
  params: {
    name: '${abbrs.keyVaultVaults}${resourceToken}'
    location: location
    enableRbacAuthorization: true
  }
}
```

6. **Add Audit Logging**:
```csharp
// Middleware to log all requests
app.Use(async (context, next) => {
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path} by {User}", 
        context.Request.Method, 
        context.Request.Path,
        context.User.Identity?.Name ?? "Anonymous");
    
    await next();
});
```

### When Modifying Security-Sensitive Code

- **Always validate user input** before database operations
- **Never trust client-side validation** alone
- **Use parameterized queries** (Cosmos DB SDK does this by default)
- **Sanitize output** if displaying user-generated content
- **Log security events** (failed auth, suspicious activity)
- **Test error handling** to avoid leaking information
- **Review OWASP Top 10** before deploying

---

## Database Operations

### Cosmos DB Container: `Temperatures`

**Partition Key**: `/location`

**Implications**:
- All queries must specify location or be cross-partition
- Point reads (by ID + location) cost 1 RU (very efficient)
- Cross-partition queries cost more RUs and are slower

### Efficient Query Patterns

**DO: Point Read** (when you know ID and location):
```csharp
var item = await container.ReadItemAsync<TemperatureMeasurementDocument>(
    id, 
    new PartitionKey(location));
// Cost: 1 RU
```

**DO: Query within a partition**:
```csharp
var query = container.GetItemQueryIterator<TemperatureMeasurementDocument>(
    new QueryDefinition("SELECT * FROM c WHERE c.location = @location")
        .WithParameter("@location", "Office"));
// Cost: Few RUs, depends on data size
```

**AVOID: Cross-partition query** (current list all implementation):
```csharp
var query = container.GetItemQueryIterator<TemperatureMeasurementDocument>(
    "SELECT * FROM c ORDER BY c.timestamp DESC");
// Cost: High RUs, slow with large datasets
```

### Pagination (Not Implemented - Should Be)

```csharp
// Recommended pattern
public async Task<(List<TemperatureMeasurement> Items, string? ContinuationToken)> 
    GetAllAsync(int pageSize = 20, string? continuationToken = null)
{
    var queryOptions = new QueryRequestOptions { MaxItemCount = pageSize };
    var query = container.GetItemQueryIterator<TemperatureMeasurementDocument>(
        "SELECT * FROM c ORDER BY c.timestamp DESC",
        continuationToken,
        queryOptions);
    
    var response = await query.ReadNextAsync();
    
    return (
        response.Select(MapToModel).ToList(),
        response.ContinuationToken
    );
}
```

### Data Consistency

- Cosmos DB uses eventual consistency by default
- For this app, consistency level is set in Bicep (resources.bicep)
- Strong consistency = higher RU cost, lower latency tolerance
- Session consistency = default, good for most apps

### Backup and Recovery

**Current Setup**: Automatic backups enabled (default)

**Restore Process** (manual):
```bash
az cosmosdb sql container restore \
  --account-name <account> \
  --resource-group <rg> \
  --database-name TemperatureDB \
  --name Temperatures \
  --restore-timestamp "2025-12-19T10:00:00Z"
```

### Performance Tips

1. **Avoid cross-partition queries** - Add location filter when possible
2. **Use point reads** - Most efficient (1 RU)
3. **Implement pagination** - Don't fetch all items at once
4. **Index only what you query** - Default indexing is usually fine
5. **Batch operations** - Use transactions for multiple writes to same partition
6. **Monitor RU consumption** - Use Application Insights metrics

---

## Deployment and CI/CD

### Manual Deployment (Current)

**First deployment**:
```bash
azd up
# Prompts for:
# - Environment name (e.g., "dev", "prod")
# - Azure subscription
# - Azure region (e.g., "eastus")
```

**Subsequent deployments**:
```bash
azd deploy
# Uses existing environment
```

**Deploy specific service**:
```bash
azd deploy backend   # Backend only
azd deploy frontend  # Frontend only
```

### Deployment Process

1. **Build**: Containers built locally or in cloud
2. **Push**: Images pushed to Azure Container Registry
3. **Update**: Container Apps updated with new images
4. **Health Check**: Basic HTTP check (not implemented in code)

**Pre-deployment Script**: `infra/scripts/predeploy.sh`
- Exports configuration values
- Sets environment variables

**Post-deployment Script**: `infra/scripts/postprovision.sh`
- Currently empty
- Could be used for database seeding, smoke tests

### CI/CD (Not Implemented)

**Recommended GitHub Actions workflow**:

```yaml
# .github/workflows/deploy.yml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Test Backend
        run: |
          cd src/backend
          dotnet test
      
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '22'
      
      - name: Test Frontend
        run: |
          cd src/frontend
          npm install
          npm run lint
          npm run test:unit
  
  deploy:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Install azd
        run: curl -fsSL https://aka.ms/install-azd.sh | bash
      
      - name: Azure Login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy
        run: azd deploy --no-prompt
        env:
          AZURE_ENV_NAME: ${{ secrets.AZURE_ENV_NAME }}
          AZURE_LOCATION: ${{ secrets.AZURE_LOCATION }}
          AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

### Environment Management

**Local**: Uses .env files and Aspire configuration
**Azure**: Uses Container Apps environment variables (set in Bicep)

**View deployed configuration**:
```bash
azd env get-values
```

**Set environment variable**:
```bash
azd env set MY_VARIABLE "value"
azd deploy  # Re-deploy to apply
```

### Rollback

**Container Apps** support revision management:
```bash
# List revisions
az containerapp revision list \
  --name <app-name> \
  --resource-group <rg>

# Activate previous revision
az containerapp revision activate \
  --revision <revision-name> \
  --resource-group <rg>
```

**Azd doesn't support rollback** - must redeploy previous version manually

---

## Monitoring and Debugging

### Application Insights

**Telemetry Automatically Collected**:
- HTTP requests (status codes, durations)
- Dependencies (Cosmos DB calls)
- Exceptions
- Custom events (if added)

**View Logs**:
```bash
# Via Azure Portal
# Application Insights > Logs (KQL queries)

# Example KQL query
requests
| where timestamp > ago(1h)
| where resultCode != 200
| project timestamp, url, resultCode, duration
```

**Custom Telemetry** (add to backend):
```csharp
var telemetryClient = app.Services.GetRequiredService<TelemetryClient>();

app.MapGet("/api/temperatures", async (TemperatureMeasurementStore store) => {
    using var operation = telemetryClient.StartOperation<RequestTelemetry>("GetTemperatures");
    try {
        var result = await store.GetAllAsync();
        telemetryClient.TrackMetric("TemperatureCount", result.Count);
        return Results.Ok(result);
    } catch (Exception ex) {
        telemetryClient.TrackException(ex);
        throw;
    }
});
```

### Container Apps Logs

**Stream logs in real-time**:
```bash
# Backend
az containerapp logs show \
  --name backend \
  --resource-group <rg> \
  --follow

# Frontend
az containerapp logs show \
  --name frontend \
  --resource-group <rg> \
  --follow
```

**View in Azure Portal**:
Container Apps > [App Name] > Log Stream

### Local Debugging

**Backend (.NET)**:
1. Open `src/backend/backend.csproj` in VS Code
2. F5 or Run → Start Debugging
3. Attach to process if needed

**Frontend (Vue.js)**:
1. `npm run dev` in `src/frontend`
2. Open browser DevTools
3. Use Vue DevTools browser extension for component inspection

**Aspire Dashboard**:
1. `dotnet run --project apphost.cs`
2. Open http://localhost:15888
3. View: Logs, traces, metrics, environment variables

### Common Issues

**Issue**: Backend can't connect to Cosmos DB

**Solution**:
```bash
# Check endpoint is set
echo $AZURE_COSMOS_DB_ENDPOINT

# Use Aspire to auto-configure
dotnet run --project apphost.cs

# Or set manually
export AZURE_COSMOS_DB_ENDPOINT="https://[account].documents.azure.com:443/"
```

**Issue**: Frontend API calls fail with CORS error

**Solution**: Ensure backend CORS allows frontend origin (currently allows all)

**Issue**: Container App deployment fails

**Solution**:
```bash
# Check logs
azd deploy --debug

# View Azure deployment
az deployment group show \
  --resource-group <rg> \
  --name <deployment-name>

# Check Container Apps status
az containerapp show \
  --name backend \
  --resource-group <rg> \
  --query "properties.provisioningState"
```

**Issue**: Frontend shows 404 for backend API

**Solution**: Check `VITE_API_BASE_URL` environment variable. Should be empty or point to backend URL.

---

## Common Tasks

### Add a New NuGet Package

```bash
cd src/backend
dotnet add package PackageName --version X.Y.Z
dotnet restore
dotnet build
```

### Add a New npm Package

```bash
cd src/frontend
npm install package-name
npm install --save-dev package-name  # Dev dependency
```

### Update Dependencies

**Backend**:
```bash
cd src/backend
dotnet list package --outdated
dotnet add package Microsoft.Azure.Cosmos --version <new-version>
```

**Frontend**:
```bash
cd src/frontend
npm outdated
npm update
npm install package-name@latest  # Specific package
```

### Clean Build

**Backend**:
```bash
cd src/backend
rm -rf bin obj
dotnet clean
dotnet restore
dotnet build
```

**Frontend**:
```bash
cd src/frontend
rm -rf node_modules dist
npm install
npm run build
```

### View Cosmos DB Data Locally

**Option 1: VS Code Extension**
1. Install "Azure Databases" extension
2. Connect to Azure account
3. Browse Cosmos DB > [Account] > [Database] > [Container]

**Option 2: Azure Portal**
1. Navigate to Cosmos DB account
2. Data Explorer
3. Browse containers and documents

**Option 3: Azure CLI**
```bash
az cosmosdb sql container query \
  --account-name <account> \
  --database-name TemperatureDB \
  --name Temperatures \
  --query-text "SELECT * FROM c"
```

### Database Seeding (Not Implemented)

**Create seed script** (`scripts/seed-data.sh`):
```bash
#!/bin/bash
ENDPOINT=$(azd env get-values | grep AZURE_COSMOS_DB_ENDPOINT | cut -d'=' -f2 | tr -d '"')

curl -X POST $BACKEND_URL/api/temperatures \
  -H "Content-Type: application/json" \
  -d '{"location":"Office","temperatureC":22,"description":"Seed data"}'

curl -X POST $BACKEND_URL/api/temperatures \
  -H "Content-Type: application/json" \
  -d '{"location":"Warehouse","temperatureC":18,"description":"Seed data"}'
```

Run: `bash scripts/seed-data.sh`

### Performance Testing (Not Implemented)

**Load test backend**:
```bash
# Install k6
brew install k6

# Create test script (k6-test.js)
# Run test
k6 run k6-test.js
```

**Frontend performance**:
```bash
cd src/frontend
npm run build
npx lighthouse http://localhost:5173 --view
```

---

## Project Quirks and Gotchas

### Backend

1. **Single-file backend**: All code in Program.cs (~325 lines). This is intentional for simplicity but doesn't scale. Consider splitting into Controllers, Services, Models for larger projects.

2. **No health check endpoint**: Container Apps expect `/health` or `/healthz`. Consider adding:
```csharp
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("HealthCheck")
   .ExcludeFromDescription();
```

3. **Cross-partition queries**: `GetAllAsync()` queries all partitions. Inefficient at scale. Add pagination and location filtering.

4. **Timestamp not updated on edit**: Edit operations preserve original timestamp. This may or may not be desired.

5. **Location is partition key**: Changing location in edit requires delete + create. Current implementation doesn't handle this correctly.

### Frontend

1. **No delete confirmation**: Delete button immediately deletes without asking. Users can't undo.

2. **API base URL**: Uses `VITE_API_BASE_URL` env variable. Empty string = same origin. In production, set to backend URL.

3. **Error handling**: Basic error messages. No retry logic or detailed error info.

4. **Loading states**: Each component manages its own loading state. No global loading indicator.

5. **Weather forecast is fake**: Data is randomly generated. Not from real weather API.

6. **No pagination**: Frontend fetches all measurements at once. Will be slow with many records.

### Infrastructure

1. **Serverless Cosmos DB**: Cost-effective for dev but may not be cheapest for production. Consider provisioned throughput for consistent workloads.

2. **Container Apps autoscaling**: Min 1, max 10 replicas. Adjust in `resources.bicep` for your needs.

3. **No custom domain**: Apps use default `.azurecontainerapps.io` domains. Add custom domain in Bicep or portal.

4. **No CDN**: Frontend served directly from Container App. Consider Azure CDN for global distribution.

5. **Logs retention**: Default retention is 30 days. Adjust in Log Analytics workspace settings.

### Development

1. **Aspire requires .NET 10**: Must have latest .NET SDK installed.

2. **Node version**: Frontend tested with Node 22. May work with Node 20+ but not guaranteed.

3. **Docker on Apple Silicon**: Multi-platform builds may be slow. Consider using GitHub Actions for production builds.

4. **Environment variables**: Aspire auto-configures many things. If running services independently, must set manually.

---

## Documentation References

**Complete technical documentation** is available in `specs/`:

### Architecture
- [specs/docs/architecture/overview.md](specs/docs/architecture/overview.md) - System architecture, data flow, deployment view
- [specs/docs/architecture/components.md](specs/docs/architecture/components.md) - Component relationships, responsibilities
- [specs/docs/architecture/patterns.md](specs/docs/architecture/patterns.md) - Design patterns, coding conventions
- [specs/docs/architecture/security.md](specs/docs/architecture/security.md) - Security analysis and recommendations

### APIs and Integration
- [specs/docs/integration/apis.md](specs/docs/integration/apis.md) - Complete API specifications with examples
- [specs/docs/integration/databases.md](specs/docs/integration/databases.md) - Database schemas, data models, query patterns
- [specs/docs/integration/services.md](specs/docs/integration/services.md) - External service dependencies

### Technology
- [specs/docs/technology/stack.md](specs/docs/technology/stack.md) - Complete technology inventory
- [specs/docs/technology/dependencies.md](specs/docs/technology/dependencies.md) - All dependencies with versions and purposes
- [specs/docs/technology/tools.md](specs/docs/technology/tools.md) - Build tools, dev tools, deployment tools

### Infrastructure
- [specs/docs/infrastructure/deployment.md](specs/docs/infrastructure/deployment.md) - Deployment architecture and procedures
- [specs/docs/infrastructure/environments.md](specs/docs/infrastructure/environments.md) - Environment configuration and variables
- [specs/docs/infrastructure/operations.md](specs/docs/infrastructure/operations.md) - Monitoring, alerting, maintenance procedures

### Features
- [specs/features/weather-forecast-display.md](specs/features/weather-forecast-display.md) - Weather forecast feature documentation
- [specs/features/temperature-management.md](specs/features/temperature-management.md) - Temperature CRUD feature documentation
- [specs/features/dashboard-navigation.md](specs/features/dashboard-navigation.md) - Navigation and dashboard documentation

---

## Contributing Guidelines

### Before Starting Work

1. **Read the documentation** in `specs/docs/` to understand the system
2. **Check existing issues** to avoid duplicate work
3. **Run the app locally** to understand current behavior
4. **Review code style** conventions in this file

### Development Process

1. **Create a branch** from `main`: `git checkout -b feature/my-feature`
2. **Make changes** following code style guidelines
3. **Add tests** for new functionality
4. **Run tests** before committing:
   ```bash
   # Backend
   cd src/backend && dotnet test
   
   # Frontend
   cd src/frontend && npm run lint && npm run test:unit
   ```
5. **Commit with clear messages**:
   ```
   feat: Add temperature filtering by location
   fix: Prevent deletion without confirmation
   docs: Update API documentation
   test: Add unit tests for TemperatureStore
   ```
6. **Push and create PR** to `main`

### PR Checklist

- [ ] Code follows style guidelines
- [ ] Tests added for new functionality
- [ ] All tests pass locally
- [ ] Documentation updated (if applicable)
- [ ] No secrets or sensitive data in code
- [ ] Commit messages are clear and descriptive
- [ ] PR description explains what and why

### Commit Message Format

Use conventional commits:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `test:` Test additions or changes
- `refactor:` Code refactoring
- `perf:` Performance improvements
- `chore:` Maintenance tasks

---

## Support and Resources

### External Documentation

- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core/)
- [Vue.js Documentation](https://vuejs.org/guide/)
- [Vite Documentation](https://vitejs.dev/guide/)
- [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/)
- [Azure Cosmos DB](https://learn.microsoft.com/azure/cosmos-db/)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)

### Useful Tools

- **VS Code Extensions**:
  - C# Dev Kit
  - Azure Tools
  - Vue - Official
  - ESLint
  - Playwright Test for VSCode

- **Browser Extensions**:
  - Vue DevTools
  - React Developer Tools (if adding React)

- **CLI Tools**:
  - Azure CLI (`az`)
  - Azure Developer CLI (`azd`)
  - Docker CLI
  - kubectl (if using Kubernetes in future)

### Getting Help

- Check `specs/docs/` for detailed documentation
- Review code comments in source files
- Search existing issues on GitHub
- Ask in team channels

---

## Quick Reference

### Most Common Commands

```bash
# Local development
dotnet run --project apphost.cs           # Start with Aspire
cd src/frontend && npm run dev            # Frontend only

# Build
cd src/backend && dotnet build            # Backend
cd src/frontend && npm run build          # Frontend

# Test
cd src/backend && dotnet test             # Backend tests
cd src/frontend && npm run test:unit      # Frontend tests

# Deploy
azd up                                    # First time
azd deploy                                # Updates

# Logs
az containerapp logs show --name backend --resource-group <rg> --follow
az containerapp logs show --name frontend --resource-group <rg> --follow

# Database query
az cosmosdb sql container query \
  --account-name <account> \
  --database-name TemperatureDB \
  --name Temperatures \
  --query-text "SELECT * FROM c"
```

### Environment Variables

**Backend**:
- `AZURE_COSMOS_DB_ENDPOINT` - Cosmos DB endpoint URL
- `DatabaseName` - Database name (default: "TemperatureDB")
- `ContainerName` - Container name (default: "Temperatures")

**Frontend**:
- `VITE_API_BASE_URL` - Backend API base URL (empty = same origin)

### File Locations

- Backend code: `src/backend/Program.cs`
- Frontend components: `src/frontend/src/components/`
- API client: `src/frontend/src/services/api.ts`
- Infrastructure: `infra/resources.bicep`
- Configuration: `azure.yaml`, `apphost.cs`

---

## Change Log

- **2025-12-19**: Initial AGENTS.md created from comprehensive codebase analysis
  - Documented current state (dev/demo, not production-ready)
  - Identified critical security gaps
  - Provided testing recommendations
  - Added deployment and monitoring guidance

---

**Last Updated**: 2025-12-19  
**Maintained By**: Development Team  
**Status**: ⚠️ Demo/Starter Application - Not Production-Ready
