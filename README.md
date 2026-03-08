# 🚀 Kickstart Template

.NET 9 Web API + Nuxt.js 3 fullstack proje template'i. İki farklı yöntemle yeni proje oluşturabilirsiniz.

## 📁 Proje Yapısı

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

## 🎯 Yöntem 1: GitHub "Use this template" (Kolay)

En hızlı yol — GitHub arayüzünden birkaç tıklamayla:

### Adımlar

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

## 🎯 Yöntem 2: dotnet new (Profesyonel)

.NET template engine kullanarak komut satırından tek komutla proje oluşturma:

### İlk Kurulum (Bir Kere)

```bash
dotnet new install https://github.com/afaki16/Kickstart
```

### Yeni Proje Oluşturma

```bash
dotnet new kickstart -n ECommerceApp
```

Bu komut otomatik olarak tüm dosya adlarını, namespace'leri, config dosyalarını günceller.

### Template Güncelleme

```bash
dotnet new uninstall https://github.com/afaki16/Kickstart
dotnet new install https://github.com/afaki16/Kickstart
```

---

## 🗄️ Veritabanı Kurulumu (PostgreSQL)

```bash
cd Backend
dotnet restore

# appsettings.json'da bağlantı bilgilerini düzenleyin
# Migration oluşturun
dotnet ef migrations add InitialCreate \
  --project <PROJE_ADI>.Infrastructure \
  --startup-project <PROJE_ADI>.API

dotnet ef database update \
  --project <PROJE_ADI>.Infrastructure \
  --startup-project <PROJE_ADI>.API
```

> `dotnet ef` yüklü değilse: `dotnet tool install --global dotnet-ef`

### Connection String

`Backend/<PROJE_ADI>.API/appsettings.json` dosyasını düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=<PROJE_ADI>;Username=postgres;Password=postgres;Trust Server Certificate=true"
  }
}
```

## 🛠 Geliştirme

```bash
# Backend
cd Backend
dotnet run --project <PROJE_ADI>.API

# Frontend (ayrı terminal)
cd Frontend
npm install
npm run dev
```

## 🗄️ Varsayılan Seed Data

| Veri | Detay |
|------|-------|
| Admin Kullanıcı | `admin@<proje-adi>.com` / `Admin123!` |
| Roller | Admin (tüm yetkiler) |
| Yetkiler | CRUD izinleri (Users, Roles, Tenants, Permissions) |
| Tenant'lar | Default Tenant + Demo Tenant |

## ✨ Neler Dahil?

### Backend (.NET 9)
- Clean Architecture (API → Application → Domain → Infrastructure)
- CQRS + MediatR pattern
- JWT Authentication + Refresh Token
- RBAC + Permission sistemi
- Multi-tenancy desteği
- FluentValidation + AutoMapper
- Entity Framework Core + PostgreSQL
- Serilog structured logging

### Frontend (Nuxt.js 3)
- Vuetify 3 UI framework
- Pinia state management
- Composable-based CRUD architecture
- VeeValidate + Yup form validation
- JWT token management + auto refresh
- Permission-based route guards
- Responsive admin panel layout

## 🚨 Sistem Gereksinimleri

- .NET 9.0 SDK
- Node.js 18+
- PostgreSQL 14+
- dotnet-ef tool

## 📞 Destek

Sorun yaşıyorsanız issue açın!
