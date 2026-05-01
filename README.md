# Kickstart - Clean Architecture .NET 9 + Nuxt 3 Template

.NET 9 Web API + Nuxt.js 3 fullstack proje template'i. İki farklı yöntemle yeni proje oluşturabilirsiniz.

## Proje Yapısı

```
YourProject/
├── Backend/
│   ├── YourProject.API/            # .NET 9 Web API
│   ├── YourProject.Application/    # CQRS + MediatR + FluentValidation
│   ├── YourProject.Domain/         # Entity & Interface'ler
│   ├── YourProject.Infrastructure/ # EF Core, JWT, Repository
│   └── YourProject.sln
├── Frontend/                       # Nuxt.js 3 + Vuetify + Pinia
│   ├── components/
│   ├── composables/
│   ├── pages/
│   ├── stores/
│   └── package.json
└── .template.config/              # dotnet new template config
```

---

## Quick Start (New Project)

GitHub "Use this template" → Clone → Run setup.sh (Linux/Mac) or setup.ps1 (Windows) → setup creates your project name automatically.

```bash
git clone https://github.com/YOUR_USER/YOUR_PROJECT.git
cd YOUR_PROJECT
./setup.sh    # or setup.ps1 on Windows
```

### Yöntem 1: GitHub "Use this template" (Kolay)

1. Bu sayfanın üstündeki yeşil **"Use this template"** → **"Create a new repository"** butonuna tıklayın
2. Repository adını girin (örnek: `ECommerceApp`)
3. Oluşan repoyu klonlayın:
   ```bash
   git clone https://github.com/KULLANICI/ECommerceApp.git
   cd ECommerceApp
   ```
4. Setup script'ini çalıştırın:

   **Windows (PowerShell):**
   ```powershell
   .\setup.ps1
   # veya parametre ile:
   .\setup.ps1 -ProjectName ECommerceApp
   ```

   **macOS / Linux:**
   ```bash
   chmod +x setup.sh
   ./setup.sh
   # veya parametre ile:
   ./setup.sh ECommerceApp
   ```

5. Script tüm namespace'leri, dosya adlarını, config'leri otomatik değiştirir ve kendini siler.

---

## Local Development Setup

1. Backend dependencies:
   ```bash
   cd Backend
   dotnet restore
   ```

2. `Backend/YOUR_PROJECT.API/appsettings.Development.json` dosyasını kontrol edin. Setup sonrası aşağıdaki değerler otomatik doldurulur:
   - `ConnectionStrings:DefaultConnection` — yerel PostgreSQL bağlantısı
   - `JwtSettings:SecretKey` — geliştirme için yerel secret

3. Create initial migration (setup.sh mevcut migration'ları siler):
   ```bash
   dotnet ef migrations add InitialCreate \
     --project YOUR_PROJECT.Infrastructure \
     --startup-project YOUR_PROJECT.API
   ```

4. Run the application — migrations will be applied automatically on startup:
   ```bash
   dotnet run --project YOUR_PROJECT.API
   ```

   > **Note:** Auto migration runs on startup. The first run after `InitialCreate` will create all tables automatically. No need to run `dotnet ef database update` manually.

5. Frontend:
   ```bash
   cd Frontend
   npm install
   npm run dev
   ```

---

## Varsayılan Seed Data

| Veri | Detay |
|------|-------|
| Admin Kullanıcı | `admin@<proje-adi>.com` / `Admin123!` |
| Roller | Admin (tüm yetkiler) |
| Yetkiler | CRUD izinleri (Users, Roles, Tenants, Permissions) |
| Tenant'lar | Default Tenant + Demo Tenant |

---

## Configuration

| Setting | Development | Staging | Production |
|---------|------------|---------|------------|
| Source | appsettings.Development.json | env vars + appsettings.Staging.json | env vars + appsettings.Production.json |
| JWT Secret | hardcoded (local only) | env var: `JWT_SECRET_*_TEST` | env var: `JWT_SECRET_*_PROD` |
| DB Connection | hardcoded localhost | env var | env var |
| CORS Origins | localhost:3000 | empty (set in Staging.json) | empty (set in Production.json) |

> **Önemli:** `appsettings.Development.json` `.gitignore`'a eklidir — git'e gitmez. Production/Staging'de tüm secret'lar environment variable olarak inject edilir.

---

## Production Deployment

See `yeni_proje_deploy_rehberi.docx` for full VPS setup.

### Required Environment Variables (in `/opt/services/.env` on VPS)

```bash
# JWT secrets (generate with: openssl rand -base64 48)
JWT_SECRET_YOURPROJECT_PROD=<min 32 chars>
JWT_SECRET_YOURPROJECT_TEST=<min 32 chars>

# Shared PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=<strong password>

# SMTP
SMTP_USER_YOURPROJECT=user@gmail.com
SMTP_PASS_YOURPROJECT=<app password>
SMTP_FROM_YOURPROJECT=noreply@yourdomain.com
```

### Required GitHub Secrets (Repo → Settings → Secrets → Actions)

- `VPS_HOST`, `VPS_USER`, `VPS_SSH_KEY` — SSH deploy step için (derived project'te eklenmeli)

### docker-compose.example.yml

`docker-compose.example.yml` dosyası, `/opt/services/docker-compose.yml`'e eklenecek servis bloklarını içerir.

---

## Migrations

The project uses EF Core with **auto-migration on startup**. When you push a new migration to the repository, the production container will apply it automatically on next deploy.

```
Startup sequence:
1. MigrateAsync()         → pending migration'ları uygula (tablolar)
2. SeedAsyncIfEmpty()     → initial admin user, roles, permissions
3. SeedPermissionsAsync() → yeni permission'ları ekle
```

### New Migration

```bash
dotnet ef migrations add MigrationName \
  --project YOUR_PROJECT.Infrastructure \
  --startup-project YOUR_PROJECT.API
```

Migration dosyalarını git'e commit edin. Container bir sonraki başlatmada otomatik uygular.

> **Not:** Migrations klasörü boşsa `MigrateAsync()` crash etmez, sessizce geçer. Bu, setup sonrası ilk run için beklenen davranıştır.

---

## Neler Dahil?

### Backend (.NET 9)
- Clean Architecture (API → Application → Domain → Infrastructure)
- CQRS + MediatR pattern
- JWT Authentication + Refresh Token
- RBAC + Permission sistemi
- Multi-tenancy desteği
- FluentValidation + AutoMapper
- Entity Framework Core + PostgreSQL
- Serilog structured logging
- Auto migration on startup
- Configuration validation (eksik secret'larda açık hata)

### Frontend (Nuxt.js 3)
- Vuetify 3 UI framework
- Pinia state management
- Composable-based CRUD architecture
- VeeValidate + Yup form validation
- JWT token management + auto refresh
- Permission-based route guards
- Responsive admin panel layout

## Sistem Gereksinimleri

- .NET 9.0 SDK
- Node.js 18+
- PostgreSQL 14+
- dotnet-ef tool (`dotnet tool install --global dotnet-ef`)
- Docker (production deploy için)
