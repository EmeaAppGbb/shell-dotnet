# Database Schemas and Data Models

## Database Platform

**Azure Cosmos DB**
- **API**: NoSQL (SQL API)
- **Capacity Mode**: Serverless
- **Region**: Single region (location parameter-based)
- **Public Network Access**: Enabled
- **File**: [infra/resources.bicep](../../../infra/resources.bicep#L66-L100)

## Database Structure

### Database: TemperatureDb

**Created By**: Bicep infrastructure deployment

**Configuration**:
```bicep
sqlDatabases: [
  {
    name: 'TemperatureDb'
    containers: [
      {
        name: 'Temperatures'
        paths: [ '/location' ]
      }
    ]
  }
]
```

### Container: Temperatures

**Purpose**: Stores temperature measurement records

**Partition Key**: `/location`
- **Type**: String
- **Purpose**: Geographic location of measurement
- **Benefits**: Enables efficient queries by location
- **Limitations**: Uneven distribution if locations are skewed

**Indexing**: Default Cosmos DB indexing (all properties indexed)

**Throughput**: Serverless (auto-scaling based on demand)

**File**: [infra/resources.bicep](../../../infra/resources.bicep#L74-L79)

## Document Schema

### Temperature Measurement Document

**Cosmos DB Document Format** (lowercase properties for compatibility):

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "location": "New York",
  "temperatureC": 22.5,
  "recordedAt": "2025-12-19T10:30:00Z"
}
```

**Schema**:
| Field | Type | Required | Description | Constraints |
|-------|------|----------|-------------|-------------|
| id | string | Yes | Unique identifier (GUID) | Must be unique |
| location | string | Yes | Measurement location | Partition key |
| temperatureC | number | Yes | Temperature in Celsius | No constraints |
| recordedAt | string | Yes | ISO 8601 timestamp | DateTime serialized |

**Type Definition** (C#):
```csharp
class TemperatureMeasurementDocument
{
    public string id { get; set; }          // GUID as string
    public string location { get; set; }    // Partition key
    public double temperatureC { get; set; }
    public DateTime recordedAt { get; set; }
}
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L106-L132)

### Why Lowercase Properties?

Cosmos DB documents use lowercase by default in many SDKs. The code explicitly uses lowercase properties to ensure compatibility:

```csharp
public string id { get; set; }      // lowercase 'id'
public string location { get; set; } // lowercase 'location'
```

This matches Cosmos DB naming conventions and avoids case sensitivity issues.

## Domain Model

The application uses a separate domain model that gets converted to/from the document format.

**Domain Model** (C# Record):
```csharp
record TemperatureMeasurement(
    Guid Id,
    string Location,
    double TemperatureC,
    DateTime RecordedAt
)
{
    public double TemperatureF => 32 + (TemperatureC * 9 / 5);
}
```

**Mapping Functions**:
```csharp
// Document → Domain
public TemperatureMeasurement ToRecord() => 
    new(Guid.Parse(id), location, temperatureC, recordedAt);

// Domain → Document
public static TemperatureMeasurementDocument FromRecord(TemperatureMeasurement record) => new()
{
    id = record.Id.ToString(),
    location = record.Location,
    temperatureC = record.TemperatureC,
    recordedAt = record.RecordedAt
};
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L124-L132)

## Data Access Patterns

### Create (Insert)
```csharp
await _container.CreateItemAsync(document, new PartitionKey(document.location));
```
- **Partition Key**: Specified explicitly
- **ID Generation**: GUID generated in application code
- **Conflict Resolution**: Default (fails on duplicate ID)

### Read All (Query)
```csharp
var query = new QueryDefinition("SELECT * FROM c ORDER BY c.recordedAt DESC");
using var iterator = _container.GetItemQueryIterator<TemperatureMeasurementDocument>(query);
```
- **Cross-Partition Query**: Yes (scans all partitions)
- **Ordering**: By `recordedAt` descending (newest first)
- **Pagination**: Not implemented (reads all pages)
- **RU Cost**: High for large datasets

### Read By ID
```csharp
var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
    .WithParameter("@id", id.ToString());
```
- ⚠️ **Cross-Partition Query**: Scans all partitions (inefficient)
- **RU Cost**: Higher than point read
- **Alternative**: Use `ReadItemAsync` if partition key known

### Update (Replace)
```csharp
await _container.ReplaceItemAsync(
    document,
    document.id,
    new PartitionKey(document.location)
);
```
- **Requires**: Both ID and partition key
- **Optimistic Concurrency**: Not implemented (no ETag checking)
- **Fetch-then-Update**: Current implementation fetches first to get partition key

### Delete
```csharp
await _container.DeleteItemAsync<TemperatureMeasurementDocument>(
    id.ToString(),
    new PartitionKey(location)
);
```
- **Requires**: Both ID and partition key
- **Fetch-then-Delete**: Current implementation fetches first to get partition key

## Queries Used

### 1. Get All Measurements
```sql
SELECT * FROM c ORDER BY c.recordedAt DESC
```
- **Type**: Cross-partition query
- **Index Used**: Default index on `recordedAt`
- **Cost**: O(n) where n = total documents
- **File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L170)

### 2. Get Measurement By ID
```sql
SELECT * FROM c WHERE c.id = @id
```
- **Type**: Cross-partition query
- **Index Used**: Default index on `id`
- **Cost**: O(n) where n = total documents
- **Optimization Opportunity**: Should use `ReadItemAsync` with partition key
- **File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L187)

## Indexing Strategy

**Current Strategy**: Default Cosmos DB indexing

**Default Index Policy**:
```json
{
  "indexingMode": "consistent",
  "automatic": true,
  "includedPaths": [
    {
      "path": "/*"
    }
  ],
  "excludedPaths": [
    {
      "path": "/\"_etag\"/?"
    }
  ]
}
```

**Implications**:
- ✅ All properties are indexed automatically
- ✅ Queries on any property are efficient
- ⚠️ Higher RU cost for writes (due to indexing overhead)
- ⚠️ Storage cost for indexes

**Optimization Opportunities**:
- Exclude unused properties from indexing
- Create composite index for common query patterns
- Add range index for numeric queries

## Data Model Validation

### Backend Validation
❌ **No validation** beyond JSON deserialization

**What's Missing**:
- No required field validation (handled by deserialization)
- No format validation (e.g., location length)
- No range validation (e.g., temperature limits)
- No business rule validation

### Database Constraints
❌ **No constraints** in Cosmos DB

**What's Missing**:
- No unique constraints (beyond ID)
- No foreign key constraints
- No check constraints
- No default values (handled in application)

## Data Relationships

### Current Relationships
**None** - The application uses a single, flat document structure with no relationships.

### Potential Relationships (Not Implemented)
- **User → Measurements** - Associate measurements with users
- **Location → Measurements** - Denormalize location details
- **Device → Measurements** - Track measurement source devices

## Sample Data

**Example Document**:
```json
{
  "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "location": "New York",
  "temperatureC": 22.5,
  "recordedAt": "2025-12-19T10:30:00.000Z"
}
```

**Example Query Result**:
```json
[
  {
    "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
    "location": "New York",
    "temperatureC": 22.5,
    "recordedAt": "2025-12-19T10:30:00.000Z"
  },
  {
    "id": "b2c3d4e5-6789-01bc-def1-234567890abc",
    "location": "London",
    "temperatureC": 15.0,
    "recordedAt": "2025-12-19T09:15:00.000Z"
  },
  {
    "id": "c3d4e5f6-7890-12cd-ef12-34567890abcd",
    "location": "Tokyo",
    "temperatureC": 28.5,
    "recordedAt": "2025-12-19T08:00:00.000Z"
  }
]
```

## Connection Configuration

### Connection String
**Environment Variable**: `ConnectionStrings__cosmos-db`

**Format**: 
```
https://{cosmosAccountName}.documents.azure.com:443/
```

**Authentication**:
- **Local**: Connection string with master key (development)
- **Production**: Managed Identity (passwordless)

**Configuration** (Backend):
```csharp
builder.AddAzureCosmosClient("cosmos-db");
```

**Environment Variable Injection** (Container App):
```bicep
env: [
  {
    name: 'ConnectionStrings__cosmos-db'
    value: cosmos.outputs.endpoint
  },
  {
    name: 'AZURE_CLIENT_ID'
    value: backendIdentity.outputs.clientId
  }
]
```

**File**: [infra/resources.bicep](../../../infra/resources.bicep#L171-L175)

## Database Security

### Authentication
- **Production**: Managed Identity with RBAC
- **Development**: Connection string with master key

### RBAC Roles
**Custom Role**: `cosmosdb-data-plane-contributor`

**Permissions**:
```bicep
dataAction: [
  'Microsoft.DocumentDB/databaseAccounts/*'
  'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
  'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
]
```

**Assigned To**:
- Backend managed identity
- Frontend managed identity
- Principal (deployment user)

**File**: [infra/resources.bicep](../../../infra/resources.bicep#L88-L96)

### Network Security
- **Public Network Access**: Enabled
- **Firewall Rules**: None configured
- **Virtual Network**: Not configured
- **Private Endpoint**: Not configured

⚠️ **Security Gap**: Database is publicly accessible (relies on authentication only)

## Performance Characteristics

### Read Performance
- **Point Read** (with partition key): ~10ms, 1 RU
- **Query by ID** (cross-partition): Variable, high RU cost
- **Get All**: O(n), 100+ RUs for large datasets

### Write Performance
- **Insert**: ~10ms, 5-10 RUs (depends on document size and indexing)
- **Update**: ~10ms, 5-10 RUs
- **Delete**: ~10ms, 5 RUs

### Scalability
- **Serverless**: Auto-scales based on demand
- **Max Throughput**: 5,000 RU/s per partition
- **Max Storage**: 50 GB per partition (logical partition limit)

**Partition Key Considerations**:
- Current partitioning by `/location` may create hot partitions
- If measurements are heavily skewed to certain locations, performance will degrade
- **Recommendation**: Consider hierarchical partition keys or date-based partitioning

## Data Migration

### Initial Setup
**Created By**: Bicep deployment automatically creates database and container

**Setup Process**:
1. Bicep creates Cosmos DB account
2. Bicep creates `TemperatureDb` database
3. Bicep creates `Temperatures` container with partition key
4. RBAC roles assigned to managed identities

### Schema Evolution
❌ **No migration strategy** implemented

**Current Approach**: Code handles schema directly (no migrations)

**Limitations**:
- Schema changes require code updates
- No version tracking
- No rollback capability

## Backup and Recovery

### Backup
**Type**: Automatic Cosmos DB backups
- **Frequency**: Continuous (serverless mode)
- **Retention**: 30 days (default)
- **Configuration**: Default (not explicitly configured)

### Recovery
**Method**: Azure Portal restore or Azure CLI

**No application-level backup** implemented

## Monitoring

### Metrics Available
- Request Units (RUs) consumed
- Request rate
- Storage usage
- Throttling events
- Latency

### Current Monitoring
✅ **Application Insights** - Cosmos DB operations logged automatically via SDK

❌ **No custom metrics** - No specific Cosmos DB monitoring configured

## Data Model Limitations

### Current Limitations

1. **No Soft Deletes** - Deleted records are permanently removed
   - **Risk**: Accidental deletions cannot be recovered
   - **Recommendation**: Add `isDeleted` field

2. **No Audit Trail** - No tracking of who changed what and when
   - **Risk**: Cannot audit data changes
   - **Recommendation**: Add `createdBy`, `modifiedBy`, `modifiedAt` fields

3. **No Versioning** - No history of changes
   - **Risk**: Cannot track changes over time
   - **Recommendation**: Implement event sourcing or change feed

4. **Flat Structure** - No nested documents or relationships
   - **Limitation**: Cannot model complex relationships
   - **Recommendation**: Consider denormalization for related data

5. **No Computed Fields in Database** - `temperatureF` computed in application
   - **Limitation**: Cannot query by Fahrenheit temperature
   - **Recommendation**: Store both values if querying needed

6. **Cross-Partition Queries** - Inefficient lookups by ID
   - **Risk**: High RU costs at scale
   - **Recommendation**: Include partition key in queries or create secondary index

## Future Data Model Enhancements

### Recommended Additions

1. **Audit Fields**:
   ```csharp
   string CreatedBy
   DateTime CreatedAt
   string ModifiedBy
   DateTime ModifiedAt
   ```

2. **Soft Delete**:
   ```csharp
   bool IsDeleted
   DateTime? DeletedAt
   string DeletedBy
   ```

3. **Denormalized Location**:
   ```csharp
   LocationDetails {
     string Name
     double Latitude
     double Longitude
     string Country
     string TimeZone
   }
   ```

4. **Device Tracking**:
   ```csharp
   string DeviceId
   string DeviceType
   ```

5. **Measurement Metadata**:
   ```csharp
   MeasurementMetadata {
     string Source  // e.g., "manual", "sensor", "api"
     double Humidity
     double Pressure
     string Notes
   }
   ```
