# Feature: Temperature Measurement Management

## Feature Overview

**Feature Name**: Temperature Measurement CRUD Operations

**Purpose**: Allow users to create, read, update, and delete temperature measurements stored in Azure Cosmos DB

**Business Value**: Demonstrates full CRUD operations with NoSQL database persistence, form validation, and RESTful API patterns

**Status**: ✅ Fully Implemented

**Priority**: Core Feature

---

## User Stories

### US-1: View Temperature Measurements
**As a** operator  
**I want to** view all recorded temperature measurements  
**So that** I can monitor temperature data across locations

**Acceptance Criteria**:
- ✅ Display all measurements in a table
- ✅ Show location, temperature (C), timestamp, and description
- ✅ Display loading state during fetch
- ✅ Show error message if fetch fails
- ✅ Empty state message when no measurements exist

---

### US-2: Create Temperature Measurement
**As a** operator  
**I want to** record a new temperature measurement  
**So that** I can track temperature data over time

**Acceptance Criteria**:
- ✅ Modal form for data entry
- ✅ Required fields: location, temperature in Celsius
- ✅ Optional field: description
- ✅ Timestamp auto-generated
- ✅ Validation for required fields
- ✅ Success/error feedback
- ✅ Table refreshes after creation

---

### US-3: Edit Temperature Measurement
**As a** operator  
**I want to** update an existing temperature measurement  
**So that** I can correct errors or add missing information

**Acceptance Criteria**:
- ✅ Edit button for each measurement
- ✅ Modal form pre-filled with current values
- ✅ Can modify location, temperature, and description
- ✅ Cannot modify timestamp or ID
- ✅ Validation for required fields
- ✅ Success/error feedback
- ✅ Table refreshes after update

---

### US-4: Delete Temperature Measurement
**As a** operator  
**I want to** delete a temperature measurement  
**So that** I can remove incorrect or unnecessary data

**Acceptance Criteria**:
- ✅ Delete button for each measurement
- ✅ Immediate deletion without confirmation (note: UX gap)
- ✅ Success/error feedback
- ✅ Table refreshes after deletion
- ❌ No undo functionality (limitation)

---

## Functional Requirements

### FR-1: List All Temperature Measurements
**Requirement**: System shall display all temperature measurements sorted by timestamp

**Implementation**:
- Backend queries Cosmos DB container
- **Cross-partition query** (queries all locations)
- Sorted by `timestamp` descending (newest first)
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L187-L206)

**Status**: ✅ Implemented

**Performance Note**: ⚠️ Cross-partition query - may be slow with large datasets

---

### FR-2: View Single Measurement
**Requirement**: System shall retrieve a single measurement by ID

**Implementation**:
- Backend uses point read with ID and partition key (location)
- Most efficient Cosmos DB operation (1 RU)
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L208-L217)

**Status**: ✅ Implemented

---

### FR-3: Create New Measurement
**Requirement**: System shall create a new temperature measurement with location, temperature, and optional description

**Implementation**:
- Backend generates GUID for ID
- Timestamp set to `DateTime.UtcNow`
- Saves to Cosmos DB
- Location used as partition key
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L219-L230)

**Status**: ✅ Implemented

**Validation**:
- Location: Required, non-empty string
- Temperature: Required, numeric value
- Description: Optional string

---

### FR-4: Update Existing Measurement
**Requirement**: System shall update an existing measurement's location, temperature, and description

**Implementation**:
- Backend retrieves existing document
- Updates modifiable fields
- Replaces document in Cosmos DB
- Preserves original ID and timestamp
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L232-L251)

**Status**: ✅ Implemented

**Note**: ⚠️ If location (partition key) changes, this is actually a delete + create operation

---

### FR-5: Delete Measurement
**Requirement**: System shall delete a measurement by ID

**Implementation**:
- Backend deletes using ID and partition key
- Returns `204 No Content` on success
- Returns `404 Not Found` if doesn't exist
- File: [src/backend/Program.cs](../../../src/backend/Program.cs#L253-L262)

**Status**: ✅ Implemented

---

## Non-Functional Requirements

### NFR-1: Data Persistence
**Requirement**: Temperature measurements shall be persisted in Azure Cosmos DB

**Implementation**:
- Cosmos DB container: `Temperatures`
- Partition key: `/location`
- Serverless capacity mode
- Schema-less NoSQL storage

**Status**: ✅ Implemented

---

### NFR-2: Performance
**Requirement**: CRUD operations shall complete within 2 seconds

**Implementation**:
- Point reads: < 10ms (1 RU)
- Writes: < 50ms (5-10 RUs)
- List all: Variable (depends on data volume)

**Status**: ✅ Implemented (single operations)

**Gap**: ⚠️ List all uses cross-partition query, may degrade with scale

---

### NFR-3: Data Validation
**Requirement**: System shall validate all input data

**Implementation**:
- Frontend: Required field validation
- Backend: Minimal validation (existence checks)

**Status**: ⚠️ Partially Implemented

**Gaps**:
- No temperature range validation
- No location format validation
- No description length limits
- No sanitization of inputs

---

### NFR-4: Usability
**Requirement**: CRUD interface shall be intuitive and responsive

**Implementation**:
- Modal forms for create/edit
- Inline action buttons in table
- Loading states
- Error messages
- Responsive table layout

**Status**: ✅ Implemented

---

## User Workflows

### Workflow 1: View All Measurements

```
1. User navigates to "About" page (or URL with TemperatureManager)
   ↓
2. Component mounts
   ↓
3. Auto-fetch all measurements (onMounted)
   ↓
4. Loading state displays
   ↓
5. Backend queries Cosmos DB
   ↓
6. Measurements render in table
```

**Code**:
```typescript
onMounted(async () => {
  await loadMeasurements()
})

const loadMeasurements = async () => {
  try {
    loading.value = true
    measurements.value = await getTemperatures()  // API call
  } catch (e: any) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
```

**File**: [src/frontend/src/components/TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue#L21-L30)

---

### Workflow 2: Create New Measurement

```
1. User clicks "Add Temperature" button
   ↓
2. Modal opens with empty form
   ↓
3. User enters location, temperature, description
   ↓
4. User clicks "Save"
   ↓
5. Form validation runs
   ↓
6. If valid:
   a. API POST request to /api/temperatures
   b. Backend generates ID and timestamp
   c. Document saved to Cosmos DB
   d. Success response returned
   e. Modal closes
   f. Table refreshes
   ↓
7. If invalid:
   a. Error message displays
   b. Form remains open
```

**Code**:
```typescript
const handleSubmit = async () => {
  if (!formData.location || formData.temperatureC === null) {
    error.value = 'Location and Temperature are required'
    return
  }

  try {
    if (editingId.value) {
      await updateTemperature(editingId.value, formData)
    } else {
      await createTemperature(formData)
    }
    closeModal()
    await loadMeasurements()  // Refresh list
  } catch (e: any) {
    error.value = e.message
  }
}
```

**File**: [src/frontend/src/components/TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue#L55-L71)

---

### Workflow 3: Edit Measurement

```
1. User clicks "Edit" button on a measurement row
   ↓
2. Modal opens with form pre-filled
   ↓
3. formData populated from measurement
   ↓
4. editingId set to measurement ID
   ↓
5. User modifies fields
   ↓
6. User clicks "Save"
   ↓
7. API PUT request to /api/temperatures/{id}
   ↓
8. Backend updates document in Cosmos DB
   ↓
9. Modal closes, table refreshes
```

**Code**:
```typescript
const openEditModal = (measurement: TemperatureMeasurement) => {
  editingId.value = measurement.id
  formData.location = measurement.location
  formData.temperatureC = measurement.temperatureC
  formData.description = measurement.description || ''
  showModal.value = true
}
```

**File**: [src/frontend/src/components/TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue#L42-L48)

---

### Workflow 4: Delete Measurement

```
1. User clicks "Delete" button
   ↓
2. API DELETE request immediately sent (no confirmation!)
   ↓
3. Backend deletes document from Cosmos DB
   ↓
4. Success: Table refreshes
   ↓
5. Error: Error message displays
```

**Code**:
```typescript
const handleDelete = async (id: string) => {
  try {
    await deleteTemperature(id)
    await loadMeasurements()  // Refresh list
  } catch (e: any) {
    error.value = e.message
  }
}
```

**File**: [src/frontend/src/components/TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue#L73-L80)

**UX Gap**: ❌ No confirmation dialog before deletion

---

## API Integration

### 1. List All Measurements

**Endpoint**: `GET /api/temperatures`

**Request**:
```http
GET /api/temperatures HTTP/1.1
Host: localhost:5000
```

**Response**:
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "location": "Office",
    "temperatureC": 22.5,
    "timestamp": "2025-12-19T10:30:00Z",
    "description": "Morning reading"
  }
]
```

**Backend Implementation**: [Program.cs](../../../src/backend/Program.cs#L62-L67)

---

### 2. Get Single Measurement

**Endpoint**: `GET /api/temperatures/{id}`

**Request**:
```http
GET /api/temperatures/550e8400-e29b-41d4-a716-446655440000 HTTP/1.1
Host: localhost:5000
```

**Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "location": "Office",
  "temperatureC": 22.5,
  "timestamp": "2025-12-19T10:30:00Z",
  "description": "Morning reading"
}
```

**Backend Implementation**: [Program.cs](../../../src/backend/Program.cs#L69-L76)

---

### 3. Create Measurement

**Endpoint**: `POST /api/temperatures`

**Request**:
```http
POST /api/temperatures HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "location": "Warehouse",
  "temperatureC": 18.0,
  "description": "Cold storage area"
}
```

**Response**:
```json
{
  "id": "660f9500-f39c-52e5-b827-557766551111",
  "location": "Warehouse",
  "temperatureC": 18.0,
  "timestamp": "2025-12-19T11:00:00Z",
  "description": "Cold storage area"
}
```

**Backend Implementation**: [Program.cs](../../../src/backend/Program.cs#L78-L86)

---

### 4. Update Measurement

**Endpoint**: `PUT /api/temperatures/{id}`

**Request**:
```http
PUT /api/temperatures/550e8400-e29b-41d4-a716-446655440000 HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "location": "Office",
  "temperatureC": 23.5,
  "description": "Afternoon reading - warmer"
}
```

**Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "location": "Office",
  "temperatureC": 23.5,
  "timestamp": "2025-12-19T10:30:00Z",
  "description": "Afternoon reading - warmer"
}
```

**Backend Implementation**: [Program.cs](../../../src/backend/Program.cs#L88-L93)

---

### 5. Delete Measurement

**Endpoint**: `DELETE /api/temperatures/{id}`

**Request**:
```http
DELETE /api/temperatures/550e8400-e29b-41d4-a716-446655440000 HTTP/1.1
Host: localhost:5000
```

**Response**:
```http
HTTP/1.1 204 No Content
```

**Backend Implementation**: [Program.cs](../../../src/backend/Program.cs#L95-L96)

---

**Full API Documentation**: [specs/docs/integration/apis.md](../docs/integration/apis.md#temperature-management)

---

## UI Components

### TemperatureManager Component

**File**: [src/frontend/src/components/TemperatureManager.vue](../../../src/frontend/src/components/TemperatureManager.vue)

**Size**: ~250 lines

**Structure**:
```
<script setup>
  - State management (measurements, loading, error, form data)
  - CRUD functions
  - Modal management
</script>

<template>
  - Header with "Add Temperature" button
  - Error/loading states
  - Measurements table
  - Modal form (create/edit)
</template>

<style scoped>
  - Responsive table styling
  - Modal overlay and form styles
  - Button styles
</style>
```

**State**:
```typescript
const measurements = ref<TemperatureMeasurement[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const showModal = ref(false)
const editingId = ref<string | null>(null)
const formData = reactive({
  location: '',
  temperatureC: null as number | null,
  description: ''
})
```

**Methods**:
- `loadMeasurements()` - Fetch all measurements
- `openAddModal()` - Open modal for creating
- `openEditModal(measurement)` - Open modal for editing
- `closeModal()` - Close modal and reset form
- `handleSubmit()` - Create or update measurement
- `handleDelete(id)` - Delete measurement

---

## Data Models

### Frontend TypeScript Interface

```typescript
export interface TemperatureMeasurement {
  id: string
  location: string
  temperatureC: number
  timestamp: string       // ISO 8601 format
  description?: string
}

export interface TemperatureInput {
  location: string
  temperatureC: number
  description?: string
}
```

**File**: [src/frontend/src/types/weather.ts](../../../src/frontend/src/types/weather.ts#L8-L21)

---

### Backend C# Records

```csharp
public record TemperatureMeasurementInput(
    string Location,
    double TemperatureC,
    string? Description
);

public record TemperatureMeasurement(
    string Id,
    string Location,
    double TemperatureC,
    DateTime Timestamp,
    string? Description
);
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L104-L121)

---

### Cosmos DB Document

```csharp
public class TemperatureMeasurementDocument
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("location")]
    public required string Location { get; set; }

    [JsonPropertyName("temperatureC")]
    public double TemperatureC { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L123-L144)

**Notes**:
- `id` is the unique identifier (GUID)
- `location` is the partition key
- All fields stored in lowercase JSON property names

---

## Database Schema

### Container Configuration

**Container Name**: `Temperatures`

**Partition Key**: `/location`

**Indexing Policy**: Default (all properties indexed)

**Capacity Mode**: Serverless (pay per operation)

**Full Documentation**: [specs/docs/integration/databases.md](../docs/integration/databases.md#temperatures-container)

---

### Data Distribution

**Partition Key Strategy**: Location-based partitioning

**Implications**:
- Each unique location is a separate logical partition
- All measurements for a location stored together
- Efficient for location-specific queries
- Cross-partition query needed for "get all"

**Scalability Considerations**:
- ⚠️ If one location has > 20 GB data, partitioning fails
- ⚠️ Hot partitions possible if one location is heavily used
- ✅ Good distribution if measurements spread across locations

---

## Testing

### Current Tests

❌ **No tests** for this feature

**Missing Test Coverage**:
- Backend: No unit tests for CRUD operations
- Frontend: No component tests for TemperatureManager.vue
- Integration: No API integration tests
- E2E: No end-to-end CRUD workflows

### Recommended Tests

#### Backend Unit Tests (xUnit)

```csharp
public class TemperatureMeasurementStoreTests
{
    [Fact]
    public async Task CreateAsync_Should_GenerateId_And_SetTimestamp()
    {
        // Arrange
        var store = new TemperatureMeasurementStore(mockContainer);
        var input = new TemperatureMeasurementInput("Office", 22.5, "Test");

        // Act
        var result = await store.CreateAsync(input);

        // Assert
        Assert.NotNull(result.Id);
        Assert.True(result.Timestamp <= DateTime.UtcNow);
        Assert.Equal("Office", result.Location);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
    {
        // Arrange & Act
        var result = await store.GetByIdAsync("nonexistent", "Office");

        // Assert
        Assert.Null(result);
    }
}
```

#### Frontend Component Tests (Vitest)

```typescript
describe('TemperatureManager', () => {
  it('loads measurements on mount', async () => {
    const mockMeasurements = [
      { id: '1', location: 'Office', temperatureC: 22, timestamp: '2025-12-19T10:00:00Z' }
    ]
    
    vi.mocked(getTemperatures).mockResolvedValue(mockMeasurements)
    
    const wrapper = mount(TemperatureManager)
    await flushPromises()
    
    expect(wrapper.text()).toContain('Office')
    expect(wrapper.text()).toContain('22')
  })

  it('opens modal when Add Temperature is clicked', async () => {
    const wrapper = mount(TemperatureManager)
    
    await wrapper.find('.add-button').trigger('click')
    
    expect(wrapper.find('.modal').isVisible()).toBe(true)
    expect(wrapper.find('h2').text()).toBe('Add Temperature')
  })

  it('deletes measurement when delete button clicked', async () => {
    // Setup and test delete workflow
  })
})
```

#### E2E Tests (Playwright)

```typescript
test('complete CRUD workflow', async ({ page }) => {
  await page.goto('/about')
  
  // Create
  await page.click('text=Add Temperature')
  await page.fill('input[name="location"]', 'Test Location')
  await page.fill('input[name="temperatureC"]', '25')
  await page.click('text=Save')
  
  // Verify created
  await expect(page.locator('text=Test Location')).toBeVisible()
  
  // Edit
  await page.click('button:has-text("Edit")')
  await page.fill('input[name="temperatureC"]', '26')
  await page.click('text=Save')
  
  // Verify edited
  await expect(page.locator('text=26')).toBeVisible()
  
  // Delete
  await page.click('button:has-text("Delete")')
  await expect(page.locator('text=Test Location')).not.toBeVisible()
})
```

---

## Known Limitations

### Limitation 1: No Delete Confirmation
**Description**: Delete button immediately deletes without asking for confirmation

**Impact**: Accidental deletions are permanent

**Workaround**: None - user must be careful

**Priority**: High

**Recommendation**: Add confirmation dialog

---

### Limitation 2: Cross-Partition Query Performance
**Description**: List all uses cross-partition query

**Impact**: Slow performance with large datasets, high RU consumption

**Workaround**: Filter by location in query

**Priority**: Medium

**Recommendation**: Implement pagination and filtering

---

### Limitation 3: Minimal Input Validation
**Description**: No validation for temperature ranges, location format, description length

**Impact**: Potentially invalid data stored

**Workaround**: None

**Priority**: Medium

**Recommendation**: Add comprehensive validation

---

### Limitation 4: No Pagination
**Description**: All measurements fetched at once

**Impact**: Performance degradation with many records

**Workaround**: None

**Priority**: Medium

**Recommendation**: Implement pagination (continuation tokens)

---

### Limitation 5: No Sorting or Filtering
**Description**: User cannot sort by column or filter results

**Impact**: Hard to find specific measurements

**Workaround**: Use browser search (Ctrl+F)

**Priority**: Low

**Recommendation**: Add table sorting and search

---

### Limitation 6: No Bulk Operations
**Description**: Cannot delete or edit multiple measurements at once

**Impact**: Tedious for managing many records

**Workaround**: Delete one by one

**Priority**: Low

**Recommendation**: Add multi-select and bulk actions

---

### Limitation 7: No Data Export
**Description**: Cannot export measurements to CSV/Excel

**Impact**: Cannot analyze data in external tools

**Workaround**: Copy from table manually

**Priority**: Low

**Recommendation**: Add export functionality

---

## Dependencies

### Internal Dependencies
- **API Client**: `src/frontend/src/services/api.ts`
- **Type Definitions**: `src/frontend/src/types/weather.ts`
- **Database Store**: Backend `TemperatureMeasurementStore` class

### External Dependencies
- **Azure Cosmos DB**: NoSQL database for persistence
- **Cosmos DB SDK**: `Microsoft.Azure.Cosmos` (backend)

---

## Configuration

### Backend Configuration

**appsettings.json**:
```json
{
  "AZURE_COSMOS_DB_ENDPOINT": "https://[account].documents.azure.com:443/",
  "DatabaseName": "TemperatureDB",
  "ContainerName": "Temperatures"
}
```

**Environment Variables** (production):
```bash
AZURE_COSMOS_DB_ENDPOINT=https://[account].documents.azure.com:443/
```

**File**: [src/backend/appsettings.json](../../../src/backend/appsettings.json)

---

### Frontend Configuration

**No configuration required** - API base URL from `VITE_API_BASE_URL` or defaults to `/`

---

## Security Considerations

### Current Security Posture

❌ **No Authentication** - Anyone can create/edit/delete measurements

❌ **No Authorization** - No access control or permissions

❌ **No Input Sanitization** - Description field vulnerable to XSS if rendered unsafely

❌ **No Rate Limiting** - API can be abused

✅ **Managed Identity** for Cosmos DB access (no connection strings in code)

### Recommendations

1. **Add Authentication**: Require login before CRUD operations
2. **Add Authorization**: Role-based access (e.g., admin can delete, user can only read)
3. **Input Validation**: Sanitize all inputs, validate ranges
4. **Rate Limiting**: Prevent abuse of API endpoints
5. **Audit Logging**: Track who created/modified/deleted measurements

**Full Security Documentation**: [specs/docs/architecture/security.md](../docs/architecture/security.md)

---

## Performance Metrics

### Expected Performance

| Operation | Expected Time | RU Cost |
|-----------|---------------|---------|
| List all (10 items) | < 200ms | ~5 RUs |
| Get by ID | < 50ms | 1 RU |
| Create | < 100ms | ~5 RUs |
| Update | < 100ms | ~5 RUs |
| Delete | < 50ms | 1 RU |

### Performance Bottlenecks

1. **Cross-Partition Query** (list all) - scales poorly
2. **No Caching** - every request hits database
3. **No Pagination** - fetches all records

---

## Acceptance Criteria

| Criteria | Status | Notes |
|----------|--------|-------|
| Create temperature measurement | ✅ | Via modal form |
| Read all measurements | ✅ | Displayed in table |
| Read single measurement | ✅ | Via API (not directly in UI) |
| Update measurement | ✅ | Via edit modal |
| Delete measurement | ✅ | Via delete button |
| Data persisted in Cosmos DB | ✅ | Verified |
| Form validation | ⚠️ | Basic only |
| Error handling | ✅ | Messages displayed |
| Loading states | ✅ | Shown during operations |
| Responsive UI | ✅ | Works on mobile |
| No delete confirmation | ❌ | UX gap |
| No pagination | ❌ | Scalability gap |

**Overall Status**: ✅ **Feature Complete** (with documented limitations)

---

## Future Enhancements

1. **Delete Confirmation Dialog**
2. **Pagination & Infinite Scroll**
3. **Sorting & Filtering**
4. **Search Functionality**
5. **Data Export (CSV/Excel)**
6. **Bulk Operations**
7. **Temperature Unit Toggle (C/F)**
8. **Charts & Visualizations**
9. **Historical Trends**
10. **Location-based Filtering**
11. **Authentication & Authorization**
12. **Audit Logging**
13. **Real-time Updates** (SignalR/WebSockets)
