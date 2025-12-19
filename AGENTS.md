# AGENTS.md
 
## Stack
- **Backend**: .NET 10, ASP.NET Core Minimal API, Cosmos DB SDK 3.46, Aspire 13.1 — `src/backend/Program.cs`
- **Frontend**: Vue 3.5 + TypeScript 5.7, Vite 7.2, Vue Router 4.5 — `src/frontend/`
- **Documentation**: MkDocs with Material theme — `specs/`
- **Infra**: Azure Container Apps, Cosmos DB (serverless), Bicep + AVM — `infra/`

## Commands
```bash
# Dev (recommended)
dotnet run --project apphost.cs    # Aspire: backend :5000, frontend :5173, docs :8100, dashboard :15888

# Build
cd src/backend && dotnet build
cd src/frontend && npm install && npm run build

# Documentation setup (first time)
cd specs && python -m venv .venv && source .venv/bin/activate && pip install -r requirements.txt

# Test
cd src/frontend && npm run lint && npm run test:unit && npm run test:e2e

# Deploy
azd up        # First time (provisions + deploys)
azd deploy    # Updates only
azd down --purge  # Destroy all
```

## Key Files
| Purpose | Location |
|---------|----------|
| Backend (all code) | `src/backend/Program.cs` |
| API client | `src/frontend/src/services/api.ts` |
| Types | `src/frontend/src/types/weather.ts` |
| Components | `src/frontend/src/components/` |
| Routes | `src/frontend/src/router/index.ts` |
| Infrastructure | `infra/resources.bicep` |
| Documentation config | `specs/mkdocs.yml` |
| Tech docs | `specs/docs/` |
| Feature specs (FRDs) | `specs/features/` |

## Code Patterns

### Backend Endpoint
```csharp
app.MapGet("/api/resource/{id}", async (string id, MyService svc) =>
    await svc.GetByIdAsync(id) is { } item ? Results.Ok(item) : Results.NotFound())
.WithName("GetResource").WithOpenApi();
```
- PascalCase types/methods, camelCase locals
- Records for DTOs, classes with `[JsonPropertyName]` for Cosmos docs
- Always `.WithName()` + `.WithOpenApi()`

### Frontend Component
```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
const data = ref([]), loading = ref(true), error = ref<string|null>(null)
onMounted(async () => {
  try { data.value = await api.getData() }
  catch (e: any) { error.value = e.message }
  finally { loading.value = false }
})
</script>
<template>
  <p v-if="loading">Loading...</p>
  <p v-if="error">{{ error }}</p>
  <div v-else><!-- content --></div>
</template>
<style scoped></style>
```
- TypeScript only, `interface` over `type`, `<style scoped>`

### Cosmos DB
- Container: `Temperatures`, Partition key: `/location`
- Point reads (ID + location) = 1 RU; avoid cross-partition queries
- Location change = delete + create (partition key)

## Adding Features

**New endpoint**: Add to `Program.cs` → add function to `api.ts` → add interface to `types/`

**New component**: Create in `components/` → add test in `components/__tests__/`

**New route**: Create view in `views/` → register in `router/index.ts` → add `<RouterLink>`

**New Azure resource**: Edit `infra/resources.bicep` → `azd provision`

## Gotchas
- Single-file backend (~325 lines) — split for larger projects
- No health endpoint — add `/health` for Container Apps
- Cross-partition `GetAllAsync()` — add pagination for scale
- No delete confirmation in UI
- Weather data is fake (randomly generated)
- Aspire requires .NET 10, frontend requires Node 22

## Environment Variables
- `AZURE_COSMOS_DB_ENDPOINT` — Cosmos endpoint (auto-set by Aspire)
- `VITE_API_BASE_URL` — Backend URL (empty = same origin)

## Docs
Detailed specs in `specs/docs/` (architecture, APIs, infrastructure)
