# Mr.WashingTon Car Wash

Doorstep car-wash booking platform built with ASP.NET Core 8, React, Vite, SQLite, and MySQL.

Copyright © 2026 Mr.WashingTon Car Wash. All rights reserved.

## Production-ready features

- Environment-driven frontend API URL, CORS, JWT, database, and admin settings
- Docker Compose deployment with Nginx, ASP.NET Core, MySQL, and persistent volumes
- Automatic Entity Framework migrations on API startup
- No default production administrator or committed production secret
- Global copyright footer on every frontend route
- SPA fallback and `/api` reverse proxy through Nginx

## Recommended deployment: Docker Compose

### Requirements

- Docker Engine 24+ with Docker Compose v2
- A server with ports 80 and 443 available
- A domain pointing to the server for public HTTPS deployment

### 1. Configure environment variables

Copy `.env.example` to `.env`, then replace every placeholder:

```powershell
Copy-Item .env.example .env
```

Required values:

| Variable | Purpose |
| --- | --- |
| `PUBLIC_ORIGIN` | Public frontend origin, such as `https://carwash.example.com` |
| `HTTP_PORT` | Host HTTP port, normally `80` |
| `JWT_KEY` | Unique random authentication key, at least 32 characters |
| `MYSQL_PASSWORD` | Password for the application MySQL user |
| `MYSQL_ROOT_PASSWORD` | Separate MySQL root password |
| `SEED_ADMIN_EMAIL` | Initial administrator email |
| `SEED_ADMIN_PASSWORD` | Strong initial administrator password |

Generate secure values with a password manager or cryptographic random generator. Never commit `.env`.

### 2. Build and start

```powershell
docker compose up -d --build
docker compose ps
```

Open `http://localhost` locally, or the value configured in `PUBLIC_ORIGIN` on a server.

### 3. Secure the first administrator

After the first successful startup and administrator login, set this in `.env`:

```dotenv
SEED_ADMIN_ENABLED=false
```

Apply the setting:

```powershell
docker compose up -d
```

The existing administrator remains in the database; disabling bootstrap prevents accidental recreation.

### 4. Enable HTTPS

Place the deployment behind a TLS reverse proxy such as Caddy, Traefik, Nginx Proxy Manager, or a cloud load balancer. Set:

```dotenv
PUBLIC_ORIGIN=https://carwash.example.com
```

Only the exact configured origin is accepted by CORS. Do not include a trailing slash.

## Deployment verification

```powershell
docker compose config
docker compose ps
curl.exe http://localhost/
curl.exe http://localhost/api/services
```

Expected results:

- Frontend returns HTTP `200`
- `/api/services` returns JSON
- Customer registration, login, booking, and admin login work
- Browser requests use `/api`, not `localhost:5001`
- Copyright appears in the footer on all routes

View container logs when diagnosing startup:

```powershell
docker compose logs --tail 200 api
docker compose logs --tail 200 mysql
docker compose logs --tail 200 web
```

## Backups and updates

The deployment stores SQLite and MySQL data in named Docker volumes. Back up both `api-data` and `mysql-data` before upgrades.

Update the application:

```powershell
docker compose down
docker compose up -d --build
```

Do not use `docker compose down -v` in production because `-v` deletes persistent data.

## Local development

### Backend

Requirements: .NET 8 SDK and a local MySQL 8 server.

```powershell
Set-Location backend/CarWash.Api
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet user-secrets set "SignupMySql:Password" "YOUR_LOCAL_MYSQL_PASSWORD"
dotnet user-secrets set "SeedAdmin:Enabled" "true"
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com"
dotnet user-secrets set "SeedAdmin:Password" "YOUR_STRONG_ADMIN_PASSWORD"
dotnet run
```

### Frontend

```powershell
Set-Location frontend
npm ci
npm run dev
```

Development defaults to `https://localhost:5001/api`. Override it with `VITE_API_BASE_URL` when necessary.

## Manual platform deployment

For separate hosting instead of Docker Compose:

- Build frontend with `VITE_API_BASE_URL=https://api.example.com/api npm run build`
- Publish backend with `dotnet publish -c Release`
- Configure ASP.NET Core environment variables using double underscores, such as `Jwt__Key`
- Configure every entry in `Cors__AllowedOrigins` to match the frontend origin exactly
- Provide persistent storage for the SQLite connection or migrate the main database to a managed provider
- Provide a reachable MySQL database for signup, login, service, and customer mirrors

See `.env.example`, `frontend/.env.example`, and `compose.yaml` for the complete configuration contract.
