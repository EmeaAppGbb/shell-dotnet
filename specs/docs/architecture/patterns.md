# Design Patterns and Conventions

## Backend Design Patterns

### 1. Minimal API Pattern
**Location**: [src/backend/Program.cs](../../../src/backend/Program.cs#L44-L95)

**Description**: ASP.NET Core Minimal APIs define endpoints using lambda expressions without controllers.

**Implementation**:
```csharp
// Single endpoint definition
app.MapGet("/weatherforecast", () =>
{
    // Handler logic inline
    return forecast;
})
.WithName("GetWeatherForecast");

// Grouped endpoints
var temperatureGroup = app.MapGroup("/api/temperatures");
temperatureGroup.MapGet("/", async (TemperatureMeasurementStore store) => { });
temperatureGroup.MapPost("/", async (CreateTemperatureMeasurement request, TemperatureMeasurementStore store) => { });
```

**Benefits**:
- Less boilerplate than controller-based APIs
- Clear, linear code flow
- Easy to understand for simple APIs
- Built-in dependency injection in parameters

**Drawbacks**:
- Can become unwieldy with many endpoints
- Less structure than controller classes
- Harder to unit test

### 2. Repository Pattern
**Location**: [src/backend/Program.cs](../../../src/backend/Program.cs#L146-L238)

**Description**: `TemperatureMeasurementStore` class encapsulates data access logic.

**Implementation**:
```csharp
class TemperatureMeasurementStore
{
    private readonly Container _container;
    
    public async Task<IEnumerable<TemperatureMeasurement>> GetAllAsync() { }
    public async Task<TemperatureMeasurement?> GetByIdAsync(Guid id) { }
    public async Task<TemperatureMeasurement> AddAsync(TemperatureMeasurement measurement) { }
    public async Task<TemperatureMeasurement?> UpdateAsync(TemperatureMeasurement measurement) { }
    public async Task<bool> DeleteAsync(Guid id, string location) { }
}
```

**Benefits**:
- Abstracts Cosmos DB operations
- Single place for data access logic
- Testable (can mock)
- Consistent interface

**Limitations**:
- **Not a true repository** - Returns domain models directly
- **No interface** - Cannot easily swap implementations
- **No unit of work** - No transaction support

### 3. Record Types (Immutable DTOs)
**Location**: [src/backend/Program.cs](../../../src/backend/Program.cs#L98-L145)

**Description**: C# records provide immutable data structures with value semantics.

**Implementation**:
```csharp
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record TemperatureMeasurement(Guid Id, string Location, double TemperatureC, DateTime RecordedAt)
{
    public double TemperatureF => 32 + (TemperatureC * 9 / 5);
}
```

**Benefits**:
- Immutability prevents accidental changes
- Value-based equality
- Concise syntax
- Computed properties

**Use Cases**:
- API request/response models
- Domain models
- DTOs between layers

### 4. Dependency Injection
**Location**: [src/backend/Program.cs](../../../src/backend/Program.cs#L6-L20)

**Description**: Built-in ASP.NET Core DI container manages service lifetimes.

**Implementation**:
```csharp
// Service registration
builder.AddAzureCosmosClient("cosmos-db");
builder.Services.AddSingleton<TemperatureMeasurementStore>();

// Usage in endpoints
temperatureGroup.MapGet("/", async (TemperatureMeasurementStore store) =>
{
    // store injected automatically
});
```

**Registered Services**:
- `CosmosClient` - Scoped (via Aspire extension)
- `TemperatureMeasurementStore` - Singleton
- `ILogger<T>` - Built-in

**Lifetime Strategies**:
- **Singleton**: `TemperatureMeasurementStore` (holds `Container` reference)
- **Scoped**: `CosmosClient` (connection per request)
- **Transient**: Not used

### 5. Results Pattern (HTTP Results)
**Location**: [src/backend/Program.cs](../../../src/backend/Program.cs#L62-L95)

**Description**: Methods return `IResult` types for explicit HTTP responses.

**Implementation**:
```csharp
temperatureGroup.MapGet("/{id:guid}", async (Guid id, TemperatureMeasurementStore store) =>
{
    var measurement = await store.GetByIdAsync(id);
    return measurement is not null ? Results.Ok(measurement) : Results.NotFound();
});

temperatureGroup.MapPost("/", async (CreateTemperatureMeasurement request, TemperatureMeasurementStore store) =>
{
    var created = await store.AddAsync(measurement);
    return Results.Created($"/api/temperatures/{created.Id}", created);
});
```

**HTTP Status Codes Used**:
- `200 OK` - Successful GET
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE
- `404 Not Found` - Resource not found

### 6. Domain-to-Document Mapping
**Location**: [src/backend/Program.cs](../../../src/backend/Program.cs#L98-L145)

**Description**: Separate document model for Cosmos DB with mapping functions.

**Implementation**:
```csharp
// Cosmos DB document (lowercase for compatibility)
class TemperatureMeasurementDocument
{
    public string id { get; set; }
    public string location { get; set; }
    public double temperatureC { get; set; }
    public DateTime recordedAt { get; set; }
    
    public TemperatureMeasurement ToRecord() => new(Guid.Parse(id), location, temperatureC, recordedAt);
    
    public static TemperatureMeasurementDocument FromRecord(TemperatureMeasurement record) => new()
    {
        id = record.Id.ToString(),
        location = record.Location,
        temperatureC = record.TemperatureC,
        recordedAt = record.RecordedAt
    };
}
```

**Benefits**:
- Separates domain model from persistence
- Handles Cosmos DB lowercase convention
- Clear transformation points

## Frontend Design Patterns

### 1. Composition API Pattern
**Location**: All Vue components ([WeatherForecast.vue](../../../src/frontend/src/components/WeatherForecast.vue), [TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue))

**Description**: Vue 3 Composition API with `<script setup>` for reactive logic.

**Implementation**:
```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'

const forecasts = ref<WeatherForecast[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    forecasts.value = await getWeatherForecast()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load forecast'
  } finally {
    loading.value = false
  }
})
</script>
```

**Benefits**:
- Better TypeScript support than Options API
- More flexible code organization
- Easier to extract and reuse logic
- Less boilerplate

### 2. Service Layer Pattern
**Location**: [src/frontend/src/services/api.ts](../../../src/frontend/src/services/api.ts)

**Description**: Centralized API client abstracts HTTP calls.

**Implementation**:
```typescript
const API_BASE = (import.meta.env.VITE_BACKEND_URL || '').replace(/\/$/, '')

export async function getTemperatures(): Promise<TemperatureMeasurement[]> {
  const response = await fetch(`${API_BASE}/api/temperatures`)
  if (!response.ok) {
    throw new Error('Failed to fetch temperatures')
  }
  return response.json()
}
```

**Benefits**:
- Single source of truth for API URLs
- Consistent error handling
- Easy to mock for testing
- Type-safe responses

**Usage in Components**:
```typescript
import { getTemperatures } from '@/services/api'

const measurements = ref<TemperatureMeasurement[]>([])
measurements.value = await getTemperatures()
```

### 3. Single File Component Pattern
**Location**: All `.vue` files

**Description**: Vue SFC combines template, script, and styles in one file.

**Structure**:
```vue
<script setup lang="ts">
// Component logic
</script>

<template>
  <!-- Component markup -->
</template>

<style scoped>
/* Component-specific styles */
</style>
```

**Benefits**:
- Colocation of related concerns
- Scoped styles prevent leakage
- Easy to navigate
- Self-contained components

### 4. Reactive State Pattern
**Location**: All Vue components

**Description**: Use `ref()` and `reactive()` for reactive state.

**Implementation**:
```typescript
// Reactive primitive
const loading = ref(true)
loading.value = false  // Update triggers re-render

// Reactive array
const measurements = ref<TemperatureMeasurement[]>([])
measurements.value = await getTemperatures()  // Re-renders component
```

**Conventions**:
- Use `ref()` for primitives and arrays
- Use `reactive()` for complex objects (rarely used here)
- Access ref values with `.value` in script

### 5. Loading/Error State Pattern
**Location**: [WeatherForecast.vue](../../../src/frontend/src/components/WeatherForecast.vue), [TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue)

**Description**: Consistent handling of async operation states.

**Implementation**:
```vue
<script setup lang="ts">
const data = ref<T[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    data.value = await fetchData()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div v-if="loading">Loading...</div>
  <div v-else-if="error">{{ error }}</div>
  <div v-else><!-- Display data --></div>
</template>
```

**Benefits**:
- Clear user feedback
- Consistent UX across components
- Prevents rendering stale data

### 6. Modal/Overlay Pattern
**Location**: [TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue#L155-L170)

**Description**: Modal forms for create/edit operations.

**Implementation**:
```vue
<script setup lang="ts">
const showForm = ref(false)
const editingId = ref<string | null>(null)
const formData = ref<CreateTemperatureMeasurement>({ location: '', temperatureC: 0 })

function openCreateForm() {
  resetForm()
  showForm.value = true
}

function resetForm() {
  formData.value = { location: '', temperatureC: 0 }
  editingId.value = null
  showForm.value = false
}
</script>

<template>
  <div v-if="showForm" class="modal-overlay" @click.self="resetForm">
    <div class="modal">
      <form @submit.prevent="handleSubmit">
        <!-- Form fields -->
      </form>
    </div>
  </div>
</template>
```

**Features**:
- Click overlay to close
- Reused for create and edit
- Form state isolated

### 7. Route-Based Code Splitting
**Location**: [src/frontend/src/router/index.ts](../../../src/frontend/src/router/index.ts)

**Description**: Lazy-load routes to reduce initial bundle size.

**Implementation**:
```typescript
const router = createRouter({
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,  // Eager loaded
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('../views/AboutView.vue'),  // Lazy loaded
    },
  ],
})
```

**Benefits**:
- Faster initial page load
- Smaller main bundle
- On-demand loading

## Infrastructure Patterns

### 1. Infrastructure as Code (IaC)
**Location**: [infra/main.bicep](../../../infra/main.bicep), [infra/resources.bicep](../../../infra/resources.bicep)

**Description**: Declarative Azure resource definitions using Bicep.

**Pattern**:
```bicep
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module resources 'resources.bicep' = {
  scope: rg
  name: 'resources'
  params: {
    location: location
    tags: tags
  }
}
```

### 2. Module Pattern (Bicep)
**Location**: [infra/resources.bicep](../../../infra/resources.bicep)

**Description**: Reusable Azure Verified Modules (AVM) from public registry.

**Implementation**:
```bicep
module monitoring 'br/public:avm/ptn/azd/monitoring:0.1.0' = {
  name: 'monitoring'
  params: {
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
    location: location
    tags: tags
  }
}
```

**Benefits**:
- Tested, maintained modules
- Consistent naming
- Best practices built-in

### 3. Managed Identity Pattern
**Location**: [infra/resources.bicep](../../../infra/resources.bicep#L104-L250)

**Description**: Azure AD identities for passwordless authentication.

**Implementation**:
```bicep
module backendIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.2.1' = {
  name: 'backendIdentity'
  params: {
    name: '${abbrs.managedIdentityUserAssignedIdentities}backend-${resourceToken}'
  }
}

// Assign to Container App
module backend 'br/public:avm/res/app/container-app:0.8.0' = {
  params: {
    managedIdentities: {
      userAssignedResourceIds: [backendIdentity.outputs.resourceId]
    }
  }
}

// Grant Cosmos DB access
cosmos.params.sqlRoleAssignmentsPrincipalIds = [
  backendIdentity.outputs.principalId
]
```

**Benefits**:
- No passwords in code
- Automatic credential rotation
- Azure AD-based access control

### 4. Multi-Stage Docker Build
**Location**: [src/backend/Dockerfile](../../../src/backend/Dockerfile), [src/frontend/Dockerfile](../../../src/frontend/Dockerfile)

**Description**: Optimize image size and security with multi-stage builds.

**Backend Pattern**:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Build application

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# Copy only runtime files
```

**Frontend Pattern**:
```dockerfile
FROM node:lts-alpine as build-stage
# Build with Vite

FROM nginx:stable-alpine as production-stage
# Serve static files only
```

**Benefits**:
- Smaller production images
- Build tools not in final image
- Faster deployments

## Naming Conventions

### Backend Naming
- **Classes**: PascalCase (`TemperatureMeasurementStore`)
- **Methods**: PascalCase (`GetAllAsync`)
- **Parameters**: camelCase (`temperatureC`)
- **Records**: PascalCase (`WeatherForecast`)
- **Private fields**: `_camelCase` (`_container`, `_logger`)
- **Async methods**: Suffix with `Async`

### Frontend Naming
- **Components**: PascalCase (`.vue` files: `WeatherForecast.vue`)
- **Functions**: camelCase (`getWeatherForecast`)
- **Variables**: camelCase (`measurements`, `loading`)
- **Types**: PascalCase (`TemperatureMeasurement`)
- **CSS classes**: kebab-case (`.forecast-card`)
- **API functions**: Descriptive verbs (`getTemperatures`, `createTemperature`)

### Infrastructure Naming
- **Resources**: `${abbreviation}${resourceToken}`
  - Example: `cae-abc123` (Container Apps Environment)
  - Abbreviations from [infra/abbreviations.json](../../../infra/abbreviations.json)
- **Modules**: Descriptive names (`monitoring`, `cosmos`, `backend`)
- **Parameters**: camelCase (`environmentName`, `principalId`)

## Code Organization Conventions

### Backend Organization
**Single-file structure** (unconventional but used here):
1. Using statements
2. Configuration and service registration
3. Middleware pipeline
4. Endpoint definitions
5. Data models
6. Data access classes

**No layering** - all in [Program.cs](../../../src/backend/Program.cs)

### Frontend Organization
**Directory structure**:
```
src/
├── components/     # Reusable UI components
├── views/          # Route-level components
├── services/       # API clients and utilities
├── stores/         # Pinia stores
├── types/          # TypeScript type definitions
├── router/         # Route configuration
└── assets/         # Styles and images
```

### Configuration Conventions

**Environment Variables**:
- Backend: `ConnectionStrings__cosmos-db`, `APPLICATIONINSIGHTS_CONNECTION_STRING`
- Frontend: `VITE_BACKEND_URL`
- Aspire: `BACKEND_URL`

**Configuration Files**:
- Backend: `appsettings.json` (minimal - mostly environment variables)
- Frontend: Environment variables via Vite
- Aspire: `apphost.settings.json` (generated from template)

## Error Handling Patterns

### Backend Error Handling
**Pattern**: Try-catch in repository, HTTP status codes in endpoints

```csharp
public async Task<TemperatureMeasurement?> UpdateAsync(TemperatureMeasurement measurement)
{
    try
    {
        var response = await _container.ReplaceItemAsync(document, id, partitionKey);
        return response.Resource.ToRecord();
    }
    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        return null;
    }
}
```

**No global error handler** - errors propagate as 500s

### Frontend Error Handling
**Pattern**: Try-catch in async functions, display error state

```typescript
const error = ref<string | null>(null)

try {
  measurements.value = await getTemperatures()
} catch (e) {
  error.value = e instanceof Error ? e.message : 'Failed to load measurements'
}
```

**No error boundary** - errors handled per-component

## Patterns Not Used (But Could Be)

### Backend
❌ **CQRS** - No separation of read/write models
❌ **Mediator Pattern** - No MediatR or similar
❌ **Unit of Work** - No transaction coordination
❌ **Factory Pattern** - Direct instantiation
❌ **Strategy Pattern** - No pluggable algorithms
❌ **Decorator Pattern** - No middleware customization

### Frontend
❌ **Render Props** - Composition API used instead
❌ **Higher-Order Components** - Not idiomatic in Vue 3
❌ **Composables** - Could extract reusable logic
❌ **Error Boundaries** - Vue 3 has `onErrorCaptured` but not used
❌ **Global State** - Pinia store exists but unused

## Anti-Patterns to Avoid

### Current Anti-Patterns
⚠️ **God Object** - `Program.cs` does everything (325 lines)
⚠️ **No Separation of Concerns** - API + business logic + data access in one file
⚠️ **Repository Leaking Cosmos Types** - `CosmosException` caught in repository
⚠️ **CORS Allow All** - Security risk
⚠️ **Query Without Partition Key** - `GetByIdAsync` scans all partitions
⚠️ **No Validation** - Direct deserialization without validation
⚠️ **Generic Error Messages** - Frontend loses specific error context
