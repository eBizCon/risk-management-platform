# Azure Infrastructure Deployment

Kosteneffiziente Azure-Deployment-Konfiguration für das Risk Management Platform.

## Deployed Services

| Service | Type | SKU / Config | Cost Optimizations |
|---------|------|--------------|-------------------|
| **PostgreSQL** | Flexible Server | B1ms Burstable, 32GB | No HA, geo-redundancy disabled |
| **Container Apps** | Managed Environment | Consumption plan | Scale-to-zero for APIs |
| **RiskManagement.Api** | Container App | 0.5 CPU, 1Gi RAM | minReplicas: 0 (cold start) |
| **CustomerManagement.Api** | Container App | 0.5 CPU, 1Gi RAM | minReplicas: 0 (cold start) |
| **SvelteKit Frontend** | Container App | 0.5 CPU, 1Gi RAM | minReplicas: 0 |
| **Keycloak** | Container App | 0.5 CPU, 1Gi RAM | minReplicas: 1 (always-on), auto realm import |
| **RabbitMQ** | Container App | 0.5 CPU, 1Gi RAM | minReplicas: 1 (always-on) |
| **Database Seeder** | Container App Job | 0.5 CPU, 1Gi RAM | Manual trigger, runs once |
| **Application Insights** | Web | Pay-as-you-go | 5GB free tier |
| **Log Analytics** | Workspace | PerGB2018 | 30-day retention |
| **Container Registry** | Basic SKU | | Sufficient for dev/test |

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Container Apps Env                  │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐  │
│  │ Risk API     │  │ Customer API │  │ SvelteKit Frontend │  │
│  │ (scale-to-0) │  │ (scale-to-0) │  │ (scale-to-0)       │  │
│  └──────────────┘  └──────────────┘  └────────────────────┘  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐ │
│  │ Keycloak     │  │ RabbitMQ     │  │ Database Seeder      │ │
│  │ (always-on)  │  │ (always-on)  │  │ (manual job)         │ │
│  └──────────────┘  └──────────────┘  └──────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────┴─────────────────────────────┐
│  PostgreSQL Flexible Server (B1ms)                        │
│  - risk-management DB                                     │
│  - customer-management DB                                 │
│  - keycloak DB                                            │
└───────────────────────────────────────────────────────────┘
```

## Deployment

### Prerequisites

- Azure CLI (`az`)
- Bicep CLI (comes with Azure CLI)

### Deploy

```bash
# Login to Azure
az login

# Set subscription (if needed)
az account set --subscription "Your Subscription"

# Deploy
./deploy.sh <resource-group> <postgres-password> <keycloak-password> <service-api-key> <rabbitmq-password>
```

Example:
```bash
./deploy.sh riskmgmt-dev $(openssl rand -base64 24) $(openssl rand -base64 24) $(openssl rand -base64 32) $(openssl rand -base64 24)
```

### Outputs

After deployment, the following URLs are available:

- `acrLoginServer` - Container Registry endpoint
- `keycloakFqdn` - Keycloak authentication endpoint
- `appFqdn` - SvelteKit frontend endpoint
- `postgresFqdn` - PostgreSQL server endpoint
- `rabbitMqFqdn` - RabbitMQ management UI
- `customerApiFqdn` - Customer Management API
- `riskApiFqdn` - Risk Management API
- `appInsightsConnectionString` - Application Insights connection string
- `databaseSeederJobName` - Database Seeder job name (trigger manually after deployment)

## CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/build-and-push.yml`) builds and pushes container images on push to main:

| Image | Path |
|-------|------|
| `riskmanagement-api` | `src/backend/RiskManagement.Api/Dockerfile` |
| `customermanagement-api` | `src/backend/CustomerManagement.Api/Dockerfile` |
| `databaseseeder` | `src/backend/DatabaseSeeder/Dockerfile` |
| `risk-management-app` | `src/frontend/Dockerfile` |

Images are pushed to GitHub Container Registry (`ghcr.io`).

## Database Seeding

After deployment, run the database seeder job:

```bash
# Start the job execution
az containerapp job start \
  --name <database-seeder-job-name> \
  --resource-group <resource-group>
```

The job:
1. Runs database migrations for both contexts
2. Seeds test data (customers, applications, credit checks)
3. Exits on completion

## Keycloak Realm Import

Keycloak automatically imports the `risk-management` realm on first startup:
- Realm: `risk-management`
- Clients: `risk-management-platform`
- Users: `applicant`, `processor`, `riskmanager`
- Roles: `applicant`, `processor`, `risk_manager`

Default user credentials match usernames (e.g., applicant/applicant).

## Estimated Monthly Costs (Dev)

| Service | Est. Monthly Cost |
|---------|-------------------|
| PostgreSQL B1ms | ~€10 |
| Container Apps (idle) | ~€0 |
| Container Apps (1 replica always-on) | ~€15 each |
| Application Insights | ~€0 (within free tier) |
| Log Analytics | ~€0 (minimal logs) |
| Container Registry | ~€5 |
| **Total** | **~€50-60** |

*Note: Costs vary based on actual usage and scale.*

## Security Considerations

- All passwords are passed as secure parameters
- Internal service communication uses API key authentication
- PostgreSQL enforces SSL
- Keycloak uses PostgreSQL for persistence
- Application Insights connection string is injected into all services

## Next Steps / TODO

- [x] Set up CI/CD pipeline for container image builds
- [x] Configure database seeder as init container or job
- [x] Add Keycloak realm import automation
- [ ] Configure custom domain and SSL certificates
