# Security Architecture and Patterns

## Security Overview

**Current Security Posture**: ⚠️ **Development/Demo Grade** - Not production-ready

The application implements **minimal security controls**, relying primarily on Azure platform security features. Critical security features like authentication, authorization, and input validation are not implemented.

## Authentication

### Current State: ❌ **NOT IMPLEMENTED**

**No authentication mechanism** is in place. All API endpoints are publicly accessible without any identity verification.

**Implications**:
- Anyone with the API URL can access all data
- No user identity tracking
- No audit trail of who performed actions
- Cannot restrict access to specific users or roles

**Recommended Solutions**:
1. **Azure AD B2C** - Consumer identity management
2. **Azure Entra ID** - Enterprise identity management  
3. **OAuth 2.0 / OpenID Connect** - Standard protocols
4. **API Keys** - Simple token-based authentication
5. **JWT Tokens** - Self-contained authentication

**Implementation Impact**: High - Requires significant code changes

---

## Authorization

### Current State: ❌ **NOT IMPLEMENTED**

**No authorization or access control** exists. All users (if authentication were implemented) would have the same permissions.

**Missing Capabilities**:
- No role-based access control (RBAC)
- No permission checks
- No resource-level access control
- No claims-based authorization

**Recommended Solutions**:
1. **Azure AD Roles** - Integrate with Azure AD groups/roles
2. **Policy-Based Authorization** - ASP.NET Core policies
3. **Attribute-Based Access Control** - Fine-grained permissions
4. **Resource-Based Authorization** - Check ownership

**Example Use Cases**:
- Admin role: Can delete any measurement
- User role: Can only delete their own measurements
- ReadOnly role: Can only view measurements

---

## Input Validation

### Current State: ⚠️ **MINIMAL**

**Validation Level**: JSON deserialization only

**What's Validated**:
✅ JSON syntax (invalid JSON returns 400)
✅ Required fields (missing fields return 400)
✅ Type checking (string vs number)

**What's NOT Validated**:
❌ String length limits (location could be 1MB string)
❌ Numeric ranges (temperature could be -1000000°C)
❌ Format validation (recordedAt must be ISO 8601 but not enforced)
❌ Business rules (e.g., location must be valid city)
❌ SQL/NoSQL injection (vulnerable to malicious strings)

**Vulnerable Code**:
```csharp
temperatureGroup.MapPost("/", async (CreateTemperatureMeasurement request, TemperatureMeasurementStore store) =>
{
    // No validation! Directly uses input
    var measurement = new TemperatureMeasurement(
        Guid.NewGuid(),
        request.Location,     // Could be malicious
        request.TemperatureC, // Could be extreme value
        request.RecordedAt ?? DateTime.UtcNow
    );
    var created = await store.AddAsync(measurement);
    return Results.Created($"/api/temperatures/{created.Id}", created);
});
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L72-L83)

**Recommended Solutions**:
1. **FluentValidation** - Comprehensive validation library
2. **Data Annotations** - Built-in ASP.NET Core validation
3. **Custom Validation Logic** - Business rule validation
4. **Input Sanitization** - Remove potentially harmful characters

**Example Validation** (not implemented):
```csharp
public class CreateTemperatureMeasurementValidator : AbstractValidator<CreateTemperatureMeasurement>
{
    public CreateTemperatureMeasurementValidator()
    {
        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-zA-Z0-9 ,-]+$"); // Alphanumeric only
        
        RuleFor(x => x.TemperatureC)
            .InclusiveBetween(-100, 100); // Realistic range
        
        RuleFor(x => x.RecordedAt)
            .LessThanOrEqualTo(DateTime.UtcNow); // No future dates
    }
}
```

---

## Data Protection

### Encryption at Rest

✅ **Cosmos DB**: Encrypted by default (Microsoft-managed keys)
✅ **Container Registry**: Encrypted by default
✅ **Log Analytics**: Encrypted by default

❌ **Customer-managed keys**: Not configured
❌ **Field-level encryption**: Not implemented (e.g., sensitive data in documents)

### Encryption in Transit

✅ **HTTPS**: Container Apps provide automatic HTTPS
✅ **TLS 1.2+**: Enforced by Azure services
✅ **Cosmos DB**: Always uses HTTPS

⚠️ **Internal traffic**: Container Apps internal communication (Container to Container) uses HTTP

### Secrets Management

⚠️ **Current Approach**: Environment variables

**Secrets Stored As Environment Variables**:
- `ConnectionStrings__cosmos-db` - Cosmos DB endpoint (not sensitive, endpoint URL only)
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - App Insights connection string

**Managed Identities** (No secrets):
✅ Backend uses managed identity for Cosmos DB
✅ Frontend uses managed identity (minimal usage)

❌ **No Azure Key Vault** integration
❌ **No secret rotation** strategy
❌ **No secret encryption** in configuration files

**Recommended Improvement**:
```csharp
// Use Key Vault references in Container Apps
env: [
  {
    name: 'ConnectionStrings__cosmos-db'
    secretRef: 'cosmos-connection-string'  // Reference to Key Vault
  }
]
```

### Data Privacy

❌ **No PII handling**: No special treatment for personal data
❌ **No data masking**: Logs may contain sensitive data
❌ **No GDPR compliance**: No right-to-erasure, data export
❌ **No data retention policy**: Data persists indefinitely

---

## Network Security

### Public Endpoints

**Current Configuration**: All services publicly accessible

| Service | Access | Security |
|---------|--------|----------|
| Frontend Container App | Public HTTPS | ✅ TLS |
| Backend Container App | Public HTTPS | ✅ TLS |
| Cosmos DB | Public HTTPS | ✅ TLS + Auth |
| Container Registry | Public (pull only) | ✅ TLS + Managed Identity |

### CORS Configuration

⚠️ **INSECURE**: Allows any origin

**Current Configuration**:
```csharp
options.AddDefaultPolicy(policy =>
{
    policy.AllowAnyOrigin()    // ❌ INSECURE
          .AllowAnyMethod()    // ❌ INSECURE
          .AllowAnyHeader();   // ❌ INSECURE
});
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L10-L17)

**Risk**: Cross-Site Request Forgery (CSRF) attacks

**Recommended Configuration**:
```csharp
policy.WithOrigins(
    "https://frontend.{env}.azurecontainerapps.io",
    "http://localhost:5173"  // Local dev only
)
.AllowCredentials()
.WithMethods("GET", "POST", "PUT", "DELETE")
.WithHeaders("Content-Type", "Authorization");
```

### Network Isolation

❌ **No Virtual Network**: All services use public endpoints
❌ **No Private Endpoints**: Cosmos DB accessible from internet
❌ **No Network Security Groups**: No traffic filtering
❌ **No Firewall Rules**: Cosmos DB allows all IPs

**Recommended Improvements**:
1. **Virtual Network Integration** - Container Apps in VNet
2. **Private Endpoints** - Cosmos DB, Container Registry
3. **Firewall Rules** - Restrict Cosmos DB access to Container Apps IPs
4. **Network Policies** - Control traffic between services

---

## Azure Platform Security

### Managed Identities

✅ **Implemented** - Both backend and frontend have managed identities

**Backend Identity**:
- **Purpose**: Cosmos DB access, Container Registry pull
- **Name**: `id-backend-{resourceToken}`
- **Permissions**:
  - Cosmos DB: `cosmosdb-data-plane-contributor` (custom role)
  - Container Registry: `AcrPull`
  - Resource Group: `Azure AI Developer` role
  - Resource Group: `Cognitive Services User` role

**Frontend Identity**:
- **Purpose**: Container Registry pull, Cosmos DB access (minimal)
- **Name**: `id-frontend-{resourceToken}`
- **Permissions**:
  - Cosmos DB: `cosmosdb-data-plane-contributor` (custom role)
  - Container Registry: `AcrPull`

**Files**: [infra/resources.bicep](../../../infra/resources.bicep#L104-L250)

**Benefits**:
- ✅ No passwords or API keys in code
- ✅ Automatic credential rotation by Azure
- ✅ Azure AD-based access control
- ✅ Audit logging via Azure AD

### Role-Based Access Control (RBAC)

✅ **Cosmos DB RBAC**: Custom data plane role

**Custom Role Definition**:
```bicep
sqlRoleDefinitions: [
  {
    name: 'cosmosdb-data-plane-contributor'
    dataAction: [
      'Microsoft.DocumentDB/databaseAccounts/*'
      'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
      'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
    ]
  }
]
```

**Assigned To**:
- Backend managed identity
- Frontend managed identity
- Deployment principal (user/service principal)

**File**: [infra/resources.bicep](../../../infra/resources.bicep#L88-L96)

### Resource Locking

❌ **Not Implemented**: No resource locks to prevent accidental deletion

**Recommendation**:
```bicep
resource lock 'Microsoft.Authorization/locks@2020-05-01' = {
  name: 'DoNotDelete'
  properties: {
    level: 'CanNotDelete'
    notes: 'Prevents accidental deletion of critical resources'
  }
}
```

---

## Application Security

### SQL/NoSQL Injection

⚠️ **Partially Protected**

**Cosmos DB SDK Protection**:
- ✅ Parameterized queries prevent SQL injection
- ✅ SDK escapes user input in queries

**Example Protected Code**:
```csharp
var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
    .WithParameter("@id", id.ToString());  // ✅ Parameterized
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L187-L188)

**However**:
- ❌ No validation of string content
- ❌ Malicious strings could be stored and returned to other users (XSS risk)

### Cross-Site Scripting (XSS)

⚠️ **Frontend Risk**

**Vue.js Default Protection**:
- ✅ Vue automatically escapes HTML in templates
- ✅ `{{ expression }}` is safe from XSS

**Example Safe Code**:
```vue
<template>
  <div>{{ measurement.location }}</div>  <!-- ✅ Escaped -->
</template>
```

**Risk Area**:
- ❌ If using `v-html` directive (not used currently)
- ❌ Malicious data from API could contain scripts

**Recommendation**: Content Security Policy (CSP) headers

### Cross-Site Request Forgery (CSRF)

❌ **Vulnerable**

**Current State**:
- No CSRF tokens
- CORS allows any origin
- No SameSite cookie configuration

**Risk**: Attacker site could make requests to API on behalf of user

**Recommendation**:
1. Restrict CORS to specific origins
2. Implement CSRF tokens (if using cookies)
3. Use SameSite cookie attribute

### Dependency Vulnerabilities

❌ **Not Scanned**

**No vulnerability scanning** for:
- NuGet packages (backend)
- npm packages (frontend)
- Container base images

**Recommendation**:
1. **Dependabot** - GitHub automated dependency updates
2. **Snyk** - Dependency vulnerability scanning
3. **Trivy** - Container image scanning
4. **npm audit** - Frontend dependency scanning
5. **dotnet list package --vulnerable** - Backend dependency scanning

---

## Logging and Monitoring Security

### Audit Logging

⚠️ **Minimal**

**What's Logged**:
- ✅ HTTP requests (via Application Insights)
- ✅ Cosmos DB operations (logged with `ILogger`)
- ✅ Container App system logs

**What's NOT Logged**:
- ❌ User actions (no authentication, no user context)
- ❌ Failed authentication attempts (no authentication)
- ❌ Authorization failures (no authorization)
- ❌ Data access patterns
- ❌ Configuration changes

**Example Logging**:
```csharp
_logger.LogInformation("Created temperature measurement {Id} in Cosmos DB", measurement.Id);
```

**File**: [src/backend/Program.cs](../../../src/backend/Program.cs#L210)

### Log Security

⚠️ **Risk**: Sensitive data in logs

**Potential Issues**:
- Logs may contain full request/response bodies
- Connection strings might be logged (via error messages)
- User input logged without sanitization

**Recommendation**:
1. **Log filtering** - Remove sensitive fields
2. **PII redaction** - Mask personal data
3. **Structured logging** - Use structured log format
4. **Log retention policy** - Limit retention period

### Security Monitoring

❌ **No Security Monitoring**

**Missing Capabilities**:
- No intrusion detection
- No anomaly detection
- No security alerts
- No threat intelligence

**Recommended Tools**:
- **Azure Sentinel** - SIEM and SOAR
- **Azure Security Center** - Security posture management
- **Azure Defender** - Threat protection

---

## Compliance and Regulations

### Current Compliance

❌ **No compliance certifications** or configurations

**Missing Compliance Features**:
- No GDPR compliance (data export, right to erasure)
- No HIPAA compliance (if healthcare data)
- No PCI DSS compliance (if payment data)
- No SOC 2 compliance (if SaaS product)

### Data Residency

⚠️ **Single Region**

**Current**: Data stored in deployment region only

**Considerations**:
- GDPR: EU data must stay in EU
- Some regulations require specific geographic storage

**Recommendation**: Multi-region deployment with data residency controls

---

## Security Hardening Checklist

### Critical (Must Fix Before Production)

- [ ] Implement authentication (Azure AD B2C or similar)
- [ ] Implement authorization and RBAC
- [ ] Add comprehensive input validation
- [ ] Restrict CORS to specific origins
- [ ] Add rate limiting and throttling
- [ ] Implement request/response validation
- [ ] Add security headers (CSP, HSTS, X-Frame-Options)
- [ ] Scan dependencies for vulnerabilities
- [ ] Add HTTPS redirect (already enabled)
- [ ] Remove public Cosmos DB access (use VNet)

### High Priority

- [ ] Implement Azure Key Vault for secrets
- [ ] Add logging of security events
- [ ] Implement API request signing
- [ ] Add audit trail for data changes
- [ ] Configure Private Endpoints
- [ ] Add Web Application Firewall (WAF)
- [ ] Implement field-level encryption for sensitive data
- [ ] Add security monitoring and alerts
- [ ] Create incident response plan
- [ ] Add health check endpoints

### Medium Priority

- [ ] Implement API versioning
- [ ] Add request correlation IDs
- [ ] Configure Azure Policy for compliance
- [ ] Add data retention policies
- [ ] Implement soft deletes
- [ ] Add GDPR compliance features
- [ ] Configure resource locks
- [ ] Add deployment approval gates
- [ ] Implement blue-green deployment
- [ ] Add security scanning in CI/CD

---

## Security Best Practices Not Followed

### Code Security

❌ **Secrets in code**: None found (good!)
❌ **Hardcoded URLs**: None found (good!)
⚠️ **No input sanitization**: User input used directly
⚠️ **No output encoding**: Relies on Vue defaults
❌ **No security headers**: CSP, X-Frame-Options not set
❌ **No rate limiting**: Unlimited requests allowed

### Infrastructure Security

❌ **Public endpoints**: All services publicly accessible
❌ **No WAF**: No Web Application Firewall
❌ **No DDoS protection**: No DDoS mitigation configured
❌ **No backup encryption**: Backups use default encryption
❌ **No MFA enforcement**: Not applicable (no auth)

---

## Security Testing

### Current Testing

❌ **No security testing** implemented

**Missing Tests**:
- No penetration testing
- No vulnerability scanning
- No security unit tests
- No fuzzing
- No SAST/DAST tools

**Recommended Tools**:
- **OWASP ZAP** - Web security scanner
- **SonarQube** - Code security analysis
- **GitHub Advanced Security** - CodeQL scanning
- **Burp Suite** - Penetration testing

---

## Security Incident Response

### Current State

❌ **No incident response plan**

**Missing**:
- No security incident procedures
- No contact list
- No escalation path
- No forensics capability
- No disaster recovery plan

**Recommendation**: Create security incident response plan

---

## Summary: Security Posture

### Current Strengths

✅ HTTPS everywhere
✅ Managed identities (no passwords)
✅ Azure platform security
✅ Encrypted data at rest
✅ Cosmos DB RBAC

### Critical Gaps

❌ No authentication
❌ No authorization
❌ No input validation
❌ CORS allows all origins
❌ No rate limiting
❌ No security monitoring
❌ Public network access

**Overall Assessment**: **Not production-ready**. Suitable for development/demo only.
