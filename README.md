# shell-dotnet

A starter shell application with a .NET backend and Vue.js frontend, configured for Azure Container Apps deployment.

## 🚀 Running the Solution

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)

### Run Locally with .NET Aspire

The solution uses .NET Aspire for local orchestration. Run the following command from the repository root:

```bash
dotnet run --project apphost.cs
```

This will:
- Start the **backend** API (ASP.NET Core) on port 5000
- Start the **frontend** (Vue.js) on a dynamically assigned port
- Open the Aspire Dashboard at `http://localhost:15888`

The frontend automatically connects to the backend via the configured proxy.

### Access the Application

- **Aspire Dashboard**: http://localhost:15888 (view logs, traces, and service status)
- **Frontend**: Check the Aspire Dashboard for the assigned port
- **Backend API**: http://localhost:5000

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/weatherforecast` | Get 5-day weather forecast |
| GET | `/api/temperatures` | List all temperature measurements |
| GET | `/api/temperatures/{id}` | Get a specific measurement |
| POST | `/api/temperatures` | Create a new measurement |
| PUT | `/api/temperatures/{id}` | Update a measurement |
| DELETE | `/api/temperatures/{id}` | Delete a measurement |

## ☁️ Deploy to Azure

### Using Azure Developer CLI (azd)

1. **Authenticate with Azure**:
   ```bash
   azd auth login
   ```

2. **Provision and deploy** (one command):
   ```bash
   azd up
   ```
   
   This will:
   - Create Azure Container Apps environment
   - Build and push Docker images
   - Deploy backend and frontend services
   - Configure networking and environment variables

3. **View deployed resources**:
   ```bash
   azd show
   ```

4. **Redeploy after changes**:
   ```bash
   azd deploy
   ```

5. **Tear down resources**:
   ```bash
   azd down
   ```

## 📁 Project Structure

```
src/
├── backend/            # ASP.NET Core API
└── frontend/           # Vue.js SPA

infra/
├── main.bicep          # Azure infrastructure
├── resources.bicep     # Resource definitions
└── scripts/            # Pre/post deployment hooks
```

## 🤝 Contributing

Contributions welcome!
