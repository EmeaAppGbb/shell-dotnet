# Component Architecture and Relationships

## Component Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                        Frontend Container                         │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │                      Nginx (Port 80)                        │  │
│  │  Serves: /index.html, /assets/*, /favicon.ico              │  │
│  └────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │                Vue.js Application                           │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │  │
│  │  │   Router     │  │    Pinia     │  │  API Client  │    │  │
│  │  │   (Views)    │  │   (State)    │  │  (Services)  │    │  │
│  │  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘    │  │
│  │         │                  │                  │             │  │
│  │         └──────────────────┴──────────────────┘             │  │
│  │                            │                                 │  │
│  │  ┌─────────────────────────▼──────────────────────────┐    │  │
│  │  │              Vue Components                         │    │  │
│  │  │  • HomeView                                         │    │  │
│  │  │  • WeatherForecast (displays forecast data)        │    │  │
│  │  │  • TemperatureManager (CRUD UI for measurements)   │    │  │
│  │  │  • HelloWorld, TheWelcome (info/demo)              │    │  │
│  │  └─────────────────────────────────────────────────────┘    │  │
│  └────────────────────────────────────────────────────────────┘  │
└───────────────────────────────┬──────────────────────────────────┘
                                │ HTTPS JSON
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│                        Backend Container                          │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │              ASP.NET Core (Port 8080)                       │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │                  Middleware Pipeline                  │  │  │
│  │  │  • CORS (allow all origins)                          │  │  │
│  │  │  • HTTPS Redirection                                 │  │  │
│  │  │  • OpenAPI (dev only)                                │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │                   API Endpoints                       │  │  │
│  │  │  • GET /weatherforecast (random data generator)     │  │  │
│  │  │  • GET /api/temperatures (list all)                 │  │  │
│  │  │  • GET /api/temperatures/{id} (get by id)           │  │  │
│  │  │  • POST /api/temperatures (create)                  │  │  │
│  │  │  • PUT /api/temperatures/{id} (update)              │  │  │
│  │  │  • DELETE /api/temperatures/{id} (delete)           │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │           TemperatureMeasurementStore                 │  │  │
│  │  │  • GetAllAsync()                                      │  │  │
│  │  │  • GetByIdAsync(id)                                   │  │  │
│  │  │  • AddAsync(measurement)                              │  │  │
│  │  │  • UpdateAsync(measurement)                           │  │  │
│  │  │  • DeleteAsync(id, location)                          │  │  │
│  │  └──────────────────┬────────────────────────────────────┘  │  │
│  │                     │ Cosmos SDK                             │  │
│  └─────────────────────┼────────────────────────────────────────┘  │
└─────────────────────────┼────────────────────────────────────────┘
                          │
                          ▼
        ┌──────────────────────────────────────┐
        │      Azure Cosmos DB                 │
        │  Database: TemperatureDb             │
        │  Container: Temperatures             │
        │  Partition Key: /location            │
        └──────────────────────────────────────┘
```

## Frontend Components

### Core Application Components

#### 1. Main Entry Point
**File**: [src/frontend/src/main.ts](../../../src/frontend/src/main.ts)
- Creates Vue app instance
- Registers router and Pinia store
- Mounts app to DOM

**Dependencies**:
- `vue`
- `vue-router`
- `pinia`
- [App.vue](../../../src/frontend/src/App.vue)

#### 2. Root Component
**File**: [src/frontend/src/App.vue](../../../src/frontend/src/App.vue)
- Top-level component
- Contains `<router-view>` for page navigation
- Provides global layout structure

### Routing Layer

#### Router Configuration
**File**: [src/frontend/src/router/index.ts](../../../src/frontend/src/router/index.ts)

**Routes Defined**:
| Path | Name | Component | Loading |
|------|------|-----------|---------|
| `/` | home | HomeView | Eager |
| `/about` | about | AboutView | Lazy (code-split) |

**Features**:
- HTML5 History mode
- Lazy loading for `/about` route
- Base URL from `import.meta.env.BASE_URL`

### View Components

#### 1. HomeView
**File**: [src/frontend/src/views/HomeView.vue](../../../src/frontend/src/views/HomeView.vue)
- **Purpose**: Main dashboard/landing page
- **Size**: ~200 lines
- **Sections**:
  - Hero section with statistics
  - "Live operations" panel
  - Weather forecast section
  - Temperature measurements section

**Child Components**:
- `<WeatherForecast />` - Displays 5-day forecast
- `<TemperatureManager />` - CRUD interface for temperatures

**Design Features**:
- Responsive grid layout
- CSS custom properties for theming
- Scoped styles

#### 2. AboutView
**File**: [src/frontend/src/views/AboutView.vue](../../../src/frontend/src/views/AboutView.vue)
- **Purpose**: About/info page
- Content not examined (likely minimal)

### Feature Components

#### 1. WeatherForecast Component
**File**: [src/frontend/src/components/WeatherForecast.vue](../../../src/frontend/src/components/WeatherForecast.vue)
- **Size**: ~160 lines
- **Purpose**: Display 5-day weather forecast

**Functionality**:
- Fetches data from `GET /weatherforecast` on mount
- Displays loading state
- Shows error messages
- Formats dates and temperatures
- Maps weather conditions to emojis

**State**:
```typescript
forecasts: WeatherForecast[]  // Forecast data
loading: boolean              // Loading indicator
error: string | null          // Error message
```

**Helper Functions**:
- `formatDate()` - Formats date strings
- `getWeatherEmoji()` - Maps summary to emoji

**UI Features**:
- Responsive grid (auto-fit, minmax)
- Hover effects on cards
- Celsius and Fahrenheit display

#### 2. TemperatureManager Component
**File**: [src/frontend/src/components/TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue)
- **Size**: ~250 lines
- **Purpose**: Full CRUD interface for temperature measurements

**Functionality**:
- List all measurements in a table
- Create new measurements (modal form)
- Edit existing measurements (modal form)
- Delete measurements (with confirmation)
- Fetches data on mount
- Auto-refreshes after mutations

**State**:
```typescript
measurements: TemperatureMeasurement[]  // All measurements
loading: boolean                        // Loading indicator
error: string | null                    // Error message
showForm: boolean                       // Modal visibility
editingId: string | null                // ID being edited
formData: CreateTemperatureMeasurement  // Form state
```

**Methods**:
- `loadMeasurements()` - Fetch all measurements
- `openCreateForm()` - Open create modal
- `openEditForm(measurement)` - Open edit modal with data
- `handleSubmit()` - Submit create or update
- `handleDelete(id)` - Delete measurement
- `resetForm()` - Clear form and close modal

**UI Features**:
- Modal overlay for create/edit
- Responsive table
- Gradient buttons
- Date/time formatting
- Celsius and Fahrenheit display

#### 3. Supporting Components

**HelloWorld.vue**
- Demo/example component
- Has unit test: [HelloWorld.spec.ts](../../../src/frontend/src/components/__tests__/HelloWorld.spec.ts)

**TheWelcome.vue**, **WelcomeItem.vue**
- Informational/welcome components
- Likely used in demo/about pages

### Service Layer

#### API Client
**File**: [src/frontend/src/services/api.ts](../../../src/frontend/src/services/api.ts)
- **Size**: ~80 lines
- **Purpose**: Centralized HTTP client for backend API

**Configuration**:
```typescript
const API_BASE = (import.meta.env.VITE_BACKEND_URL || '').replace(/\/$/, '')
```

**Functions**:

| Function | Method | Endpoint | Returns |
|----------|--------|----------|---------|
| getWeatherForecast() | GET | /weatherforecast | WeatherForecast[] |
| getTemperatures() | GET | /api/temperatures | TemperatureMeasurement[] |
| getTemperatureById(id) | GET | /api/temperatures/{id} | TemperatureMeasurement |
| createTemperature(data) | POST | /api/temperatures | TemperatureMeasurement |
| updateTemperature(id, data) | PUT | /api/temperatures/{id} | TemperatureMeasurement |
| deleteTemperature(id) | DELETE | /api/temperatures/{id} | void |

**Error Handling**:
- Checks `response.ok`
- Throws generic error messages
- No retry logic
- No request cancellation

### Type Definitions

**File**: [src/frontend/src/types/weather.ts](../../../src/frontend/src/types/weather.ts)

**Interfaces Defined**:
```typescript
WeatherForecast {
  date: string
  temperatureC: number
  temperatureF: number
  summary: string | null
}

TemperatureMeasurement {
  id: string
  location: string
  temperatureC: number
  temperatureF: number
  recordedAt: string
}

CreateTemperatureMeasurement {
  location: string
  temperatureC: number
  recordedAt?: string
}

UpdateTemperatureMeasurement {
  location?: string
  temperatureC?: number
  recordedAt?: string
}
```

### State Management

#### Pinia Store
**File**: [src/frontend/src/stores/counter.ts](../../../src/frontend/src/stores/counter.ts)
- Example counter store (demo only)
- **Not actively used** in the application
- Temperature state managed locally in components

## Backend Components

### Application Structure

All backend code is in a **single file**: [src/backend/Program.cs](../../../src/backend/Program.cs) (~325 lines)

#### 1. Configuration and Services (Lines 1-20)
```csharp
WebApplicationBuilder builder
├── AddOpenApi() - OpenAPI documentation
├── AddCors() - CORS policy (allow all)
├── AddAzureCosmosClient("cosmos-db") - Cosmos DB client
└── AddSingleton<TemperatureMeasurementStore>() - Data store
```

#### 2. Middleware Pipeline (Lines 22-29)
```csharp
WebApplication app
├── MapOpenApi() - OpenAPI endpoint (dev only)
├── UseCors() - Enable CORS
└── UseHttpsRedirection() - Redirect HTTP to HTTPS
```

#### 3. API Endpoints (Lines 31-95)

**Weather Endpoint (Lines 44-60)**:
```csharp
GET /weatherforecast
├── Generates random forecast for 5 days
├── Returns WeatherForecast[]
└── No external API call - randomly generated
```

**Temperature Endpoints (Lines 62-95)**:
```csharp
Group: /api/temperatures
├── GET / - GetAllAsync() → All measurements
├── GET /{id:guid} - GetByIdAsync(id) → Single measurement or 404
├── POST / - AddAsync(measurement) → Created measurement (201)
├── PUT /{id:guid} - UpdateAsync(measurement) → Updated or 404
└── DELETE /{id:guid} - DeleteAsync(id, location) → 204 or 404
```

#### 4. Data Models (Lines 98-145)

**WeatherForecast** (Record):
- Immutable data structure
- Computed property: `TemperatureF`

**TemperatureMeasurement** (Record):
- Domain model
- Computed property: `TemperatureF`

**TemperatureMeasurementDocument** (Class):
- Cosmos DB document format (lowercase properties)
- Maps to/from domain record
- `ToRecord()` - Convert to domain model
- `FromRecord()` - Convert from domain model

**DTOs** (Records):
- `CreateTemperatureMeasurement` - POST request
- `UpdateTemperatureMeasurement` - PUT request (all optional)

#### 5. Data Access Layer (Lines 146-238)

**TemperatureMeasurementStore** (Class):
- **Singleton** service
- Wraps Cosmos DB `Container` operations
- Database: `TemperatureDb`
- Container: `Temperatures`

**Methods**:
```csharp
GetAllAsync()
├── Query: "SELECT * FROM c ORDER BY c.recordedAt DESC"
├── Returns: IEnumerable<TemperatureMeasurement>
└── No pagination - returns all documents

GetByIdAsync(Guid id)
├── Query: "SELECT * FROM c WHERE c.id = @id"
├── ⚠️ Scans all partitions (no partition key)
└── Returns: First match or null

AddAsync(TemperatureMeasurement)
├── Generates GUID
├── CreateItemAsync(document, partitionKey)
├── Logs creation
└── Returns: Created measurement

UpdateAsync(TemperatureMeasurement)
├── ReplaceItemAsync(document, id, partitionKey)
├── Catches: CosmosException 404
├── Logs update
└── Returns: Updated measurement or null

DeleteAsync(Guid id, string location)
├── DeleteItemAsync(id, partitionKey)
├── Catches: CosmosException 404
├── Logs deletion
└── Returns: true or false
```

## Component Dependencies

### Frontend Dependency Graph
```
main.ts
└── App.vue
    └── router-view
        ├── HomeView.vue
        │   ├── WeatherForecast.vue
        │   │   └── api.ts → getWeatherForecast()
        │   └── TemperatureManager.vue
        │       └── api.ts → CRUD operations
        └── AboutView.vue

api.ts (all functions)
└── HTTP fetch to backend
```

### Backend Dependency Graph
```
Program.cs
├── WebApplicationBuilder
│   ├── OpenApi services
│   ├── CORS services
│   ├── Cosmos DB client (injected)
│   └── TemperatureMeasurementStore (singleton)
│
├── WebApplication
│   └── Middleware pipeline
│
├── API Endpoints
│   └── TemperatureMeasurementStore (injected)
│       └── CosmosClient
│           └── Azure Cosmos DB
│
└── Data Models (self-contained)
```

## Inter-Component Communication

### Frontend ↔ Backend
**Protocol**: HTTP/HTTPS with JSON payloads
**Authentication**: None
**Error Handling**: 
- Frontend: Basic error messages
- Backend: HTTP status codes (200, 201, 204, 404)

### Backend ↔ Database
**Protocol**: HTTPS (Cosmos DB REST API via SDK)
**Authentication**: Managed Identity (production) or connection string
**Connection**: 
```csharp
builder.AddAzureCosmosClient("cosmos-db");
// Looks for environment variable: ConnectionStrings__cosmos-db
```

## Component Coupling Analysis

### Tight Coupling
⚠️ **Frontend types match backend exactly** - Breaking changes propagate
⚠️ **No API versioning** - Backend changes break frontend
⚠️ **Hardcoded endpoints** in api.ts - Not configurable per environment

### Loose Coupling
✅ **Service layer abstraction** - Components don't call API directly
✅ **CORS allows any origin** - Frontend can be hosted anywhere
✅ **Container isolation** - Services deployed independently

## Component Reusability

### Frontend
✅ **High reusability**: 
- `WeatherForecast.vue` - Can be reused anywhere
- `TemperatureManager.vue` - Can be reused anywhere
- `api.ts` - Can be imported in any component

### Backend
❌ **Low reusability**:
- All code in single file
- No separate class libraries
- `TemperatureMeasurementStore` tightly coupled to Cosmos DB

## Missing Components

### Not Implemented
❌ **Authentication Service** - No login/logout
❌ **Authorization Logic** - No role checking
❌ **Caching Layer** - No Redis or in-memory cache
❌ **Validation Service** - Minimal validation
❌ **Logging Service** - Default ASP.NET logging only
❌ **Error Handling Middleware** - No global error handler
❌ **Health Check Endpoints** - No `/health` or `/ready`
❌ **API Gateway** - Direct frontend-to-backend
❌ **Message Queue** - No async processing
❌ **Background Services** - No scheduled tasks
