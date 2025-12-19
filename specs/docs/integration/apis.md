# API Documentation

## API Overview

The backend exposes a RESTful API with two main endpoint groups:
1. **Weather Forecast** - Read-only endpoint providing randomly generated weather data
2. **Temperature Measurements** - Full CRUD API for temperature measurements stored in Cosmos DB

**Base URL**:
- Local: `http://localhost:5000` (when run via Aspire) or `http://localhost:8080` (direct)
- Production: `https://backend.{containerAppsEnvironment}.azurecontainerapps.io`

**Content Type**: `application/json`
**Authentication**: ❌ None implemented

## Endpoints

### Weather Forecast

#### Get Weather Forecast
```
GET /weatherforecast
```

**Description**: Returns a randomly generated 5-day weather forecast.

**Query Parameters**: None

**Request Headers**: None required

**Response**: `200 OK`
```json
[
  {
    "date": "2025-12-20",
    "temperatureC": 15,
    "temperatureF": 59,
    "summary": "Mild"
  },
  {
    "date": "2025-12-21",
    "temperatureC": -5,
    "temperatureF": 23,
    "summary": "Freezing"
  }
]
```

**Response Schema**:
```typescript
WeatherForecast {
  date: string         // ISO 8601 date (DateOnly serialized)
  temperatureC: number // Temperature in Celsius
  temperatureF: number // Temperature in Fahrenheit (computed)
  summary: string | null // Weather description
}
```

**Possible Summaries**:
- "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"

**Implementation Details**:
- **Not a real weather API** - Data is randomly generated
- Returns exactly 5 forecast items
- Each item is for the next consecutive day
- Temperature range: -20°C to 55°C
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L44-L60)

**Errors**: None - Always returns 200 OK

---

### Temperature Measurements

Base path: `/api/temperatures`

#### List All Temperature Measurements
```
GET /api/temperatures
```

**Description**: Retrieves all temperature measurements from the database, ordered by most recent first.

**Query Parameters**: None

**Pagination**: ❌ Not implemented - Returns all documents

**Request Headers**: None required

**Response**: `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "location": "New York",
    "temperatureC": 22.5,
    "temperatureF": 72.5,
    "recordedAt": "2025-12-19T10:30:00Z"
  },
  {
    "id": "7fa85f64-5717-4562-b3fc-2c963f66afa7",
    "location": "London",
    "temperatureC": 15.0,
    "temperatureF": 59.0,
    "recordedAt": "2025-12-19T09:15:00Z"
  }
]
```

**Response Schema**:
```typescript
TemperatureMeasurement[] {
  id: string           // GUID
  location: string     // Measurement location (partition key)
  temperatureC: number // Temperature in Celsius
  temperatureF: number // Temperature in Fahrenheit (computed)
  recordedAt: string   // ISO 8601 timestamp
}
```

**Query Details**:
- Cosmos DB Query: `SELECT * FROM c ORDER BY c.recordedAt DESC`
- Scans all documents (no filtering)
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L168-L182)

**Errors**:
- `500 Internal Server Error` - Cosmos DB connection issues

---

#### Get Temperature Measurement by ID
```
GET /api/temperatures/{id}
```

**Description**: Retrieves a single temperature measurement by its ID.

**Path Parameters**:
- `id` (GUID, required) - The measurement ID

**Request Headers**: None required

**Response**: `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "location": "New York",
  "temperatureC": 22.5,
  "temperatureF": 72.5,
  "recordedAt": "2025-12-19T10:30:00Z"
}
```

**Response Schema**: Same as `TemperatureMeasurement` above

**Query Details**:
- Cosmos DB Query: `SELECT * FROM c WHERE c.id = @id`
- ⚠️ **Performance Issue**: Scans all partitions (no partition key provided)
- Returns first match or null
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L184-L197)

**Errors**:
- `404 Not Found` - Measurement with given ID does not exist
- `500 Internal Server Error` - Cosmos DB connection issues

**Example Request**:
```bash
curl http://localhost:5000/api/temperatures/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

#### Create Temperature Measurement
```
POST /api/temperatures
```

**Description**: Creates a new temperature measurement.

**Request Headers**:
- `Content-Type: application/json` (required)

**Request Body**:
```json
{
  "location": "New York",
  "temperatureC": 22.5,
  "recordedAt": "2025-12-19T10:30:00Z"
}
```

**Request Schema**:
```typescript
CreateTemperatureMeasurement {
  location: string       // Required - Measurement location
  temperatureC: number   // Required - Temperature in Celsius
  recordedAt?: string    // Optional - ISO 8601 timestamp (defaults to DateTime.UtcNow)
}
```

**Validation**:
- ❌ **No validation** - ASP.NET Core deserializes directly
- Missing required fields result in `400 Bad Request` (default behavior)

**Response**: `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "location": "New York",
  "temperatureC": 22.5,
  "temperatureF": 72.5,
  "recordedAt": "2025-12-19T10:30:00Z"
}
```

**Response Headers**:
- `Location: /api/temperatures/3fa85f64-5717-4562-b3fc-2c963f66afa6`

**Implementation Details**:
- Generates new GUID for `id`
- Uses `location` as partition key
- Computes `temperatureF` automatically
- Defaults `recordedAt` to `DateTime.UtcNow` if not provided
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L72-L83)

**Errors**:
- `400 Bad Request` - Invalid JSON or missing required fields
- `500 Internal Server Error` - Cosmos DB connection issues

**Example Request**:
```bash
curl -X POST http://localhost:5000/api/temperatures \
  -H "Content-Type: application/json" \
  -d '{"location":"New York","temperatureC":22.5}'
```

---

#### Update Temperature Measurement
```
PUT /api/temperatures/{id}
```

**Description**: Updates an existing temperature measurement. All fields are optional.

**Path Parameters**:
- `id` (GUID, required) - The measurement ID to update

**Request Headers**:
- `Content-Type: application/json` (required)

**Request Body**:
```json
{
  "location": "New York City",
  "temperatureC": 23.0,
  "recordedAt": "2025-12-19T11:00:00Z"
}
```

**Request Schema**:
```typescript
UpdateTemperatureMeasurement {
  location?: string      // Optional - New location
  temperatureC?: number  // Optional - New temperature
  recordedAt?: string    // Optional - New timestamp
}
```

**Behavior**:
- Fields not provided retain existing values
- At least one field should be provided (not enforced)

**Response**: `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "location": "New York City",
  "temperatureC": 23.0,
  "temperatureF": 73.4,
  "recordedAt": "2025-12-19T11:00:00Z"
}
```

**Implementation Details**:
- Fetches existing measurement first (cross-partition query)
- Merges update with existing values
- Uses existing `location` as partition key for update
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L85-L95)

**Errors**:
- `404 Not Found` - Measurement with given ID does not exist
- `400 Bad Request` - Invalid JSON
- `500 Internal Server Error` - Cosmos DB connection issues

**Example Request**:
```bash
curl -X PUT http://localhost:5000/api/temperatures/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Content-Type: application/json" \
  -d '{"temperatureC":23.0}'
```

---

#### Delete Temperature Measurement
```
DELETE /api/temperatures/{id}
```

**Description**: Deletes a temperature measurement.

**Path Parameters**:
- `id` (GUID, required) - The measurement ID to delete

**Request Headers**: None required

**Request Body**: None

**Response**: `204 No Content` (successful deletion, no body)

**Implementation Details**:
- Fetches existing measurement first to get partition key
- Requires partition key (`location`) for efficient deletion
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L97-L104)

**Errors**:
- `404 Not Found` - Measurement with given ID does not exist
- `500 Internal Server Error` - Cosmos DB connection issues

**Example Request**:
```bash
curl -X DELETE http://localhost:5000/api/temperatures/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

## CORS Configuration

**Policy**: Allow any origin, method, and header

**Configuration**:
```csharp
options.AddDefaultPolicy(policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L10-L17)

⚠️ **Security Risk**: This configuration is **insecure for production**. Should be restricted to specific origins.

## OpenAPI Documentation

**Endpoint**: `/openapi/v1.json` (Development only)

**Configuration**: Automatically generated by `Microsoft.AspNetCore.OpenApi` package

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L7)

**Access**:
```bash
curl http://localhost:5000/openapi/v1.json
```

**Swagger UI**: ❌ Not configured (only OpenAPI spec generation)

## Rate Limiting

❌ **Not implemented** - No rate limiting or throttling

## Authentication & Authorization

❌ **Not implemented** - All endpoints are public and unauthenticated

**Security Gap**: Any client can:
- Read all temperature measurements
- Create unlimited measurements
- Update any measurement
- Delete any measurement

## Error Responses

### Standard HTTP Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET or PUT |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Invalid JSON or missing required fields |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Unhandled exceptions or database errors |

### Error Response Format

**No standardized error format** - Default ASP.NET Core error responses:

**400 Bad Request**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "location": ["The location field is required."]
  }
}
```

**404 Not Found**:
```
(Empty body)
```

**500 Internal Server Error**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

## API Client (Frontend)

The frontend uses a centralized API client: [src/frontend/src/services/api.ts](../../../src/frontend/src/services/api.ts)

**Configuration**:
```typescript
const API_BASE = (import.meta.env.VITE_BACKEND_URL || '').replace(/\/$/, '')
```

**Error Handling**:
- Throws generic error messages
- Does not parse error response bodies
- No retry logic

**Example Usage**:
```typescript
import { getTemperatures, createTemperature } from '@/services/api'

// Get all temperatures
const measurements = await getTemperatures()

// Create new measurement
const newMeasurement = await createTemperature({
  location: 'Paris',
  temperatureC: 18.5
})
```

## HTTP Request Examples

### Manual Testing File
**File**: [src/backend/backend.http](../../../src/backend/backend.http)

Contains example HTTP requests for manual API testing (VS Code REST Client format).

### cURL Examples

**Get all measurements**:
```bash
curl http://localhost:5000/api/temperatures
```

**Get by ID**:
```bash
curl http://localhost:5000/api/temperatures/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Create measurement**:
```bash
curl -X POST http://localhost:5000/api/temperatures \
  -H "Content-Type: application/json" \
  -d '{
    "location": "Tokyo",
    "temperatureC": 28.5,
    "recordedAt": "2025-12-19T12:00:00Z"
  }'
```

**Update measurement**:
```bash
curl -X PUT http://localhost:5000/api/temperatures/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Content-Type: application/json" \
  -d '{
    "temperatureC": 29.0
  }'
```

**Delete measurement**:
```bash
curl -X DELETE http://localhost:5000/api/temperatures/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Get weather forecast**:
```bash
curl http://localhost:5000/weatherforecast
```

## API Limitations

### Current Limitations

1. **No Pagination** - `GET /api/temperatures` returns all documents
   - **Risk**: Performance degrades with large datasets
   - **Recommendation**: Implement pagination with continuation tokens

2. **Cross-Partition Queries** - `GetByIdAsync` scans all partitions
   - **Risk**: High RU consumption
   - **Recommendation**: Require location in query or index by id

3. **No Validation** - No input validation beyond JSON deserialization
   - **Risk**: Invalid data can be persisted
   - **Recommendation**: Add FluentValidation or Data Annotations

4. **No Authentication** - All endpoints are public
   - **Risk**: Anyone can read/modify/delete data
   - **Recommendation**: Implement Azure AD B2C or API keys

5. **No Rate Limiting** - Unlimited requests allowed
   - **Risk**: DoS attacks, cost explosion
   - **Recommendation**: Add rate limiting middleware

6. **No API Versioning** - Breaking changes will affect all clients
   - **Risk**: Cannot evolve API without breaking clients
   - **Recommendation**: Implement versioning (URL or header-based)

7. **Generic Error Messages** - Frontend loses error context
   - **Risk**: Poor user experience, hard to debug
   - **Recommendation**: Standardize error response format

8. **No Request Logging** - Difficult to diagnose issues
   - **Risk**: Cannot audit or debug production issues
   - **Recommendation**: Add request/response logging middleware

9. **CORS Allows All** - Insecure configuration
   - **Risk**: CSRF attacks
   - **Recommendation**: Restrict to specific frontend origins

10. **No Health Checks** - Cannot monitor service health
    - **Risk**: Cannot detect partial outages
    - **Recommendation**: Add `/health` endpoint

## API Evolution Recommendations

### Short-term
1. Add input validation with Data Annotations
2. Implement proper error response format
3. Add health check endpoint
4. Restrict CORS to specific origins
5. Add request/response logging

### Medium-term
1. Implement authentication (Azure AD B2C)
2. Add authorization policies
3. Implement pagination
4. Add rate limiting
5. Create API documentation (Swagger UI)

### Long-term
1. Implement API versioning
2. Add API Gateway (Azure API Management)
3. Implement caching layer
4. Add distributed tracing
5. Create API client SDK
