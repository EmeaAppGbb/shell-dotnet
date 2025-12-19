# Operational Procedures and Monitoring

## Monitoring and Observability

### Application Insights

**Enabled**: ✅ Yes (both backend and frontend)

**Connection String**: Injected via environment variable

**Auto-Instrumentation**:
- HTTP requests and responses
- Dependency calls (Cosmos DB)
- Exceptions and errors
- Performance metrics
- Custom logs via `ILogger`

**Access**:
1. Azure Portal → Application Insights resource
2. View: Live Metrics, Performance, Failures, Logs

**Query Language**: KQL (Kusto Query Language)

**File**: [infra/resources.bicep](../../../infra/resources.bicep#L26-L35)

---

### Log Analytics

**Purpose**: Centralized log storage

**Data Sources**:
- Container Apps system logs
- Application Insights telemetry
- Azure resource logs

**Retention**: 30 days (default)

**Access**:
```bash
az monitor log-analytics query \
  --workspace {workspaceId} \
  --analytics-query "ContainerAppSystemLogs_CL | limit 100"
```

---

### Container App Logs

**View Live Logs**:
```bash
# Backend logs
az containerapp logs show \
  --name backend \
  --resource-group {rg} \
  --follow

# Frontend logs
az containerapp logs show \
  --name frontend \
  --resource-group {rg} \
  --follow
```

**Log Streaming**: Available in Azure Portal (Container App → Log stream)

---

### Aspire Dashboard (Local Development)

**URL**: http://localhost:15888

**Features**:
- Service status and health
- Distributed tracing
- Logs from all services
- Environment variables
- Service metrics

**Access**: Automatically opens when running `dotnet run --project apphost.cs`

---

## Metrics and KPIs

### Current Metrics (Automatic)

**Application Insights**:
- Request rate (req/sec)
- Response time (avg, p95, p99)
- Failure rate (%)
- Dependency duration (Cosmos DB calls)
- Exception count

**Container Apps**:
- CPU usage (%)
- Memory usage (MB)
- Replica count
- Request count
- HTTP status codes

### Missing Metrics

❌ **No custom metrics** defined
❌ **No business metrics** (e.g., measurements created per hour)
❌ **No user metrics** (no authentication, so no user tracking)
❌ **No cost metrics** (RU consumption tracking)

---

## Alerting

### Current Alerts

❌ **No alerts configured**

### Recommended Alerts

**High Priority**:
1. **HTTP 5xx errors** > 5% of requests
2. **Average response time** > 1000ms
3. **Cosmos DB RU consumption** > 80% of capacity
4. **Container App replica count** = max (scaling limit reached)
5. **Exception rate** > threshold

**Medium Priority**:
6. **CPU usage** > 80%
7. **Memory usage** > 80%
8. **Disk space** > 80% (if applicable)
9. **Failed dependency calls** > threshold

**Configuration**:
```bash
# Create alert (example)
az monitor metrics alert create \
  --name high-error-rate \
  --resource-group {rg} \
  --scopes {resourceId} \
  --condition "avg Percentage HTTP Server Errors > 5"
```

---

## Health Checks

### Current State

❌ **No health check endpoints** implemented

**Default Container Apps Health Probes**:
- Liveness: TCP check on ingress port
- Readiness: TCP check on ingress port

### Recommended Implementation

**Backend** (`/health`):
```csharp
app.MapGet("/health", async (TemperatureMeasurementStore store) =>
{
    try
    {
        // Verify Cosmos DB connectivity
        await store.GetAllAsync();
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch
    {
        return Results.StatusCode(503); // Service Unavailable
    }
});
```

**Health Check Types**:
- **Liveness**: Is the application running?
- **Readiness**: Can the application accept traffic?
- **Startup**: Has the application finished starting?

---

## Backup and Recovery

### Automatic Backups

**Cosmos DB**:
- **Type**: Continuous backup (serverless mode default)
- **Retention**: 30 days
- **Granularity**: Point-in-time restore

**Container Images**:
- **Location**: Azure Container Registry
- **Retention**: Indefinite (unless manually deleted)
- **Tags**: `latest` only (no versioning)

### Recovery Procedures

**Cosmos DB Restore**:
```bash
az cosmosdb restore \
  --name {newAccountName} \
  --resource-group {rg} \
  --restore-timestamp "2025-12-19T10:00:00Z" \
  --location {location} \
  --source-account-name {originalAccountName}
```

**Application Restore**:
1. Identify working container image version
2. Update Container App with previous image
3. Restart Container App

### Missing Backup Strategy

❌ **No application data exports**
❌ **No configuration backups**
❌ **No disaster recovery plan**
❌ **No backup testing**

---

## Scaling Operations

### Auto-Scaling Configuration

**Current Settings** (both apps):
- **Min Replicas**: 1
- **Max Replicas**: 10
- **Trigger**: HTTP traffic, CPU, Memory

**Configuration**: [infra/resources.bicep](../../../infra/resources.bicep#L138-L139, L247-L248)

### Manual Scaling

**Scale Up**:
```bash
az containerapp update \
  --name backend \
  --resource-group {rg} \
  --min-replicas 2 \
  --max-replicas 20
```

**Scale Down**:
```bash
az containerapp update \
  --name backend \
  --resource-group {rg} \
  --min-replicas 1 \
  --max-replicas 5
```

### Scaling Limits

**Container Apps**:
- Max: 10 replicas (configured)
- Platform limit: 30 replicas per app

**Cosmos DB (Serverless)**:
- Max: 5,000 RU/s per partition
- Auto-scales based on demand

---

## Maintenance Procedures

### Planned Maintenance

**Container App Updates**:
1. Build new container image
2. Run `azd deploy`
3. Container Apps performs rolling update
4. Zero downtime (if healthy)

**Infrastructure Changes**:
1. Update Bicep templates
2. Run `azd provision`
3. Resources updated in place (where possible)
4. Some changes may require recreation

### Patching Strategy

❌ **No automated patching**

**Manual Patching**:
1. Update base images in Dockerfiles
2. Update dependencies (npm, NuGet)
3. Rebuild and redeploy

**Recommended**: Automated dependency updates (Dependabot)

---

## Incident Response

### Current State

❌ **No incident response plan**

### Recommended Procedures

**Severity 1 (Critical Outage)**:
1. Check Container App health (Azure Portal)
2. Check Application Insights for errors
3. Check Cosmos DB status
4. Restart Container Apps if needed
5. Rollback to previous version if needed

**Severity 2 (Degraded Performance)**:
1. Check metrics (CPU, memory, response time)
2. Scale up if needed
3. Check Cosmos DB RU consumption
4. Investigate slow queries

**Severity 3 (Minor Issue)**:
1. Create issue ticket
2. Investigate during business hours
3. Plan fix for next release

---

## Performance Tuning

### Current Performance

**Not Measured**: No baseline performance metrics

### Performance Optimization Checklist

**Backend**:
- [ ] Add caching (Redis)
- [ ] Optimize Cosmos DB queries (use partition keys)
- [ ] Enable response compression
- [ ] Connection pooling (automatic with SDK)
- [ ] Async/await properly used (✅ already done)

**Frontend**:
- [ ] Code splitting (✅ partially done with lazy routes)
- [ ] Image optimization
- [ ] Lazy loading
- [ ] Service worker for caching
- [ ] CDN for static assets

**Database**:
- [ ] Optimize partition key strategy
- [ ] Add composite indexes for common queries
- [ ] Enable integrated cache
- [ ] Review indexing policy

---

## Cost Management

### Current Costs

**Estimated Monthly** (low traffic):
- Cosmos DB: $5-10
- Container Apps: $10-20
- Container Registry: $5
- Application Insights: $0-5
- **Total**: ~$20-40/month

### Cost Optimization

**Immediate**:
1. Use `azd down` to delete dev environments when not in use
2. Set min replicas to 0 for dev (scale to zero)
3. Review and clean up old container images

**Long-term**:
1. Reserved capacity for Cosmos DB (if traffic predictable)
2. Shared resources across environments
3. Implement caching to reduce Cosmos DB calls
4. CDN to reduce Container Apps egress

---

## Troubleshooting Guide

### Application Not Starting

**Check**:
1. Container build logs in ACR
2. Container App deployment status
3. Environment variables configured
4. Managed identity assigned
5. Image pull successful

**Commands**:
```bash
# Check Container App status
az containerapp show --name backend --resource-group {rg}

# Check recent revisions
az containerapp revision list --name backend --resource-group {rg}

# View logs
az containerapp logs show --name backend --resource-group {rg} --tail 100
```

### Database Connection Errors

**Check**:
1. Cosmos DB account status
2. Firewall rules (should allow all for now)
3. Managed identity permissions
4. Connection string format

**Verify Access**:
```bash
# Check RBAC assignments
az cosmosdb sql role assignment list \
  --account-name {cosmosName} \
  --resource-group {rg}
```

### High Latency

**Check**:
1. Application Insights performance tab
2. Cosmos DB metrics (RU consumption)
3. Container App CPU/memory usage
4. Network latency

### 404 Errors (Frontend to Backend)

**Check**:
1. VITE_BACKEND_URL configured correctly
2. Backend URL in `.env.production`
3. CORS configuration
4. Backend endpoints responding

---

## Runbooks

### Deploy New Version

```bash
# 1. Verify current environment
azd env get-values

# 2. Build and deploy
azd deploy

# 3. Verify deployment
curl https://backend.{env}.azurecontainerapps.io/weatherforecast

# 4. Monitor logs
az containerapp logs show --name backend --resource-group {rg} --follow
```

### Rollback to Previous Version

```bash
# 1. List revisions
az containerapp revision list --name backend --resource-group {rg}

# 2. Activate previous revision
az containerapp revision activate \
  --name backend \
  --resource-group {rg} \
  --revision {previousRevisionName}

# 3. Deactivate current (optional)
az containerapp revision deactivate \
  --name backend \
  --resource-group {rg} \
  --revision {currentRevisionName}
```

### Scale for High Traffic

```bash
# Increase max replicas
az containerapp update \
  --name backend \
  --resource-group {rg} \
  --max-replicas 20

az containerapp update \
  --name frontend \
  --resource-group {rg} \
  --max-replicas 20
```

### Emergency Shutdown

```bash
# Stop accepting traffic
az containerapp ingress disable \
  --name backend \
  --resource-group {rg}

# Or delete entirely
azd down --force
```

---

## Operational Gaps

### Critical Gaps

❌ No health check endpoints
❌ No alerting configured
❌ No incident response plan
❌ No runbooks documented
❌ No SLA defined
❌ No on-call rotation

### High Priority

❌ No performance baselines
❌ No capacity planning
❌ No cost tracking
❌ No automated testing in production
❌ No canary deployments

### Medium Priority

❌ No operational dashboards
❌ No automated rollback
❌ No chaos engineering
❌ No load testing results
❌ No disaster recovery testing
