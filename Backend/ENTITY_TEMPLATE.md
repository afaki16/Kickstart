# Kickstart Backend — Yeni Entity Oluşturma Rehberi

Bu doküman, **User** entity'si baz alınarak hazırlanmıştır.  
Yeni bir entity oluşturulurken bu dosyadaki adımlar sırayla uygulanmalıdır.

---

## Mimari Genel Bakış

```
Kickstart.Domain          → Entity, Interface, Enum, Constant (dış bağımlılık yok)
Kickstart.Application     → CQRS Handler, DTO, Validator, Mapping (Domain'e bağımlı)
Kickstart.Infrastructure  → Repository, EF Config, Migration (Application + Domain'e bağımlı)
Kickstart.API             → Controller, Endpoint (Application'a bağımlı)
```

**Kurallar:**
- Her entity için tam CRUD: Create / Update / Delete / GetAll (sayfalı) / GetById
- Tüm handler'lar `IUnitOfWork` üzerinden repository'e erişir — doğrudan repository inject edilmez
- Soft delete kullanılır (`IsDeleted` flag), fiziksel silme yapılmaz
- Tüm response'lar `Result<T>` veya `PagedResult<T>` ile sarılır
- Validation `FluentValidation` ile Command validator'da yapılır
- AutoMapper `MappingProfile.cs`'e eklenir
- Permissions `Domain/Constants/Permissions.cs`'e, `IUnitOfWork`'e ve `UnitOfWork.cs`'e eklenir

---

## Adım 1 — Domain Layer

### 1.1 Entity Sınıfı

**Dosya:** `Kickstart.Domain/Entities/{Entity}.cs`

```csharp
using Kickstart.Domain.Entities;

namespace Kickstart.Domain.Entities
{
    public class Product : BaseAuditableEntity   // Her zaman BaseAuditableEntity kullan
    {
        // Zorunlu alanlar
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }

        // Tenant desteği varsa (opsiyonel)
        public int? TenantId { get; set; }

        // ---- İlişki Örnekleri ----

        // Many-to-One (FK kendi tarafında)
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // One-to-Many (FK karşı tarafta)
        public ICollection<ProductImage> Images { get; set; }

        // Many-to-Many (ara tablo ile — User→Role gibi)
        public ICollection<ProductTag> ProductTags { get; set; }

        public Product()
        {
            Images = new HashSet<ProductImage>();
            ProductTags = new HashSet<ProductTag>();
            IsActive = true;
        }

        // Computed property (DB'ye yazılmaz)
        public string DisplayName => $"{Name} - {Price:C}";
    }
}
```

**Base sınıf referansları:**

| Sınıf | Alanlar |
|-------|---------|
| `BaseEntity` | `Id (int)`, `CreatedDate`, `UpdatedDate`, `IsDeleted` |
| `BaseAuditableEntity : BaseEntity` | + `CreatedBy (int?)`, `UpdatedBy (int?)` |

> Hem `CreatedDate`/`UpdatedDate` hem de `CreatedBy`/`UpdatedBy` `DbContext.SaveChangesAsync()` override'ında **otomatik** set edilir. Manuel set etme.

---

### 1.2 Repository Interface

**Dosya:** `Kickstart.Domain/Common/Interfaces/Repositories/I{Entity}Repository.cs`

```csharp
using Kickstart.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product, int>
    {
        // IRepository<T,TKey> base metotları zaten sağlar:
        //   GetByIdAsync, AddAsync, Update, Delete, SoftDelete,
        //   GetAllAsync, FindAsync, FindFirstAsync, ExistsAsync,
        //   GetQueryable, UpdateRange

        // Entity'ye özel sorgu metotları buraya eklenir:
        Task<bool> NameExistsAsync(string name, int? tenantId, int? excludeId = null);
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPagedAsync(
            int page, int pageSize, string searchTerm = null, int? tenantId = null);

        // İlişkili veri yükleyen metotlar
        Task<Product> GetProductWithImagesAsync(int productId);
        Task<Product> GetProductWithAllRelationsAsync(int productId);
    }
}
```

---

### 1.3 IUnitOfWork Güncelleme

**Dosya:** `Kickstart.Domain/Common/Interfaces/IUnitOfWork.cs`

```csharp
// Mevcut repository property'lerin yanına ekle:
IProductRepository Products { get; }
```

---

### 1.4 Permissions Sabitleri

**Dosya:** `Kickstart.Domain/Constants/Permissions.cs`

```csharp
// Mevcut blokların yanına ekle:
public static class Products
{
    public const string Read   = "products.read";
    public const string Create = "products.create";
    public const string Update = "products.update";
    public const string Delete = "products.delete";
}
```

> **Önemli:** Permission string'leri **küçük harf + nokta** formatında yazılır: `"products.read"`.  
> Controller'daki `[Authorize(Policy = "products.read")]` bu string'le eşleşmelidir.

---

### 1.5 Enum (gerekiyorsa)

**Dosya:** `Kickstart.Domain/Common/Enums/ProductStatus.cs`

```csharp
namespace Kickstart.Domain.Common.Enums
{
    public enum ProductStatus
    {
        Active   = 1,
        Inactive = 2,
        Archived = 3
    }
}
```

---

## Permission Sistemi — Detaylı Açıklama

### Permission Nasıl Çalışır?

```
Permissions.cs (sabit string)
    ↓  reflection ile okunur
SeedData.SeedPermissionsAsync()
    ↓  DB'ye Permission kaydı oluşturur
Rol oluşturulurken RolePermission olarak atanır
    ↓
JWT token'da claim olarak taşınır
    ↓
[Authorize(Policy = "products.read")] → PermissionPolicyProvider → PermissionAuthorizationHandler
```

**Önemli:** `SeedPermissionsAsync()` **reflection** kullanır — `Permissions.cs`'e yeni bir nested class eklemek yeterlidir, `SeedData.cs`'e dokunmana gerek yoktur. Yeni permission'lar uygulama başlangıcında otomatik seed edilir.

---

### Permission Sabiti Format Kuralı

```csharp
// Kickstart.Domain/Constants/Permissions.cs

public static class Products          // ← Resource adı (PascalCase, çoğul)
{
    public const string Read   = "products.read";    // ← küçük harf + nokta
    public const string Create = "products.create";  // ← küçük harf + nokta
    public const string Update = "products.update";
    public const string Delete = "products.delete";
}
```

`SeedData` bu string'i `.` üzerinden split ederek `Resource = "products"`, `Action = "read"` çıkarır ve şu `Permission` entity'sini oluşturur:

```csharp
new Permission
{
    Name        = "products.read",         // Permissions.cs'teki sabit
    Description = "Can read products",     // Otomatik üretilir
    Resource    = "products",              // Split[0]
    Type        = PermissionType.Read      // MapActionToPermissionType("read")
}
```

---

### PermissionType Enum (`[Flags]`)

```csharp
// Kickstart.Domain/Common/Enums/PermissionType.cs

[Flags]
public enum PermissionType
{
    None      = 0,
    Create    = 1,   // 1 << 0
    Read      = 2,   // 1 << 1
    Update    = 4,   // 1 << 2
    Delete    = 8,   // 1 << 3

    ReadWrite  = Read | Create | Update,   // = 7
    FullAccess = Create | Read | Update | Delete  // = 15
}
```

`[Flags]` sayesinde tek bir `PermissionType` alanında birden fazla izin saklanabilir.  
`GetIndividualPermissions()` extension metodu composite değerleri (`ReadWrite`, `FullAccess`) açar, tek tek flag'leri döndürür.  
Yeni action türü eklersen (örn. `Export = 1 << 4`) hem enum'a hem `SeedData.MapActionToPermissionType()`'a hem de Permissions.cs sabitine eklemelisin.

---

### ICurrentUserService — Kullanılabilir Property'ler

Handler'larda `_currentUserService` üzerinden erişilebilir:

| Property / Metot | Tip | Açıklama |
|---|---|---|
| `UserId` | `int?` | Oturum açmış kullanıcının ID'si |
| `TenantId` | `int?` | Kullanıcının tenant'ı |
| `Email` | `string` | Kullanıcı e-postası |
| `FullName` | `string` | Ad soyad |
| `IsAuthenticated` | `bool` | Oturum açık mı? |
| `CanAccessAllTenants` | `bool` | `true` ise SuperAdmin — tüm tenant'lara erişir |
| `IsInRole(roleName)` | `bool` | Belirli bir rolde mi? |
| `HasPermission(perm)` | `bool` | Belirli bir permission var mı? |

**Standart Tenant Kontrolü (her handler'da tekrar edilir):**

```csharp
// Yazma işlemleri (Create) — hangi tenant'a kayıt açılıyor?
int? tenantId;
if (request.TenantId.HasValue)
{
    if (!_currentUserService.CanAccessAllTenants)
        return Result<ProductListDto>.Failure(Error.Failure(ErrorCode.Forbidden,
            "Only SuperAdmin can create records in a specific tenant"));
    tenantId = request.TenantId;
}
else
{
    tenantId = _currentUserService.TenantId;
}

// Okuma / Güncelleme / Silme — mevcut kayda erişim var mı?
if (!_currentUserService.CanAccessAllTenants && entity.TenantId != _currentUserService.TenantId)
    return Result<...>.Failure(Error.Failure(ErrorCode.Forbidden, "Access denied"));
```

---

### TenantAdminVisibility — Sadece User-Tipi Entitylerde

`TenantAdminVisibility`, bir tenant Admin'inin kendi tenant'ındaki SuperAdmin kullanıcılarını **görmesini / yönetmesini** engeller. Bu kontrol **yalnızca `User` entity'si** veya kullanıcıyla doğrudan ilişkili entityler için gereklidir.

```csharp
// UpdateUserCommandHandler ve DeleteUserCommandHandler'da var:
if (TenantAdminVisibility.IsHiddenFromTenantAdmin(_currentUserService, user))
    return Result<...>.Failure(Error.Failure(ErrorCode.Forbidden, "Access denied"));
```

**Ürün, kategori, sipariş gibi domain entity'lerde bu kontrole gerek yoktur.**  
Sadece bir entity "kim yönetebilir" mantığı User rolleriyle ilgiliyse ekle.

---

### IPermissionService — Permission Cache

Kullanıcının permission'ları Redis/Memory cache'de tutulur.  
Cache'i ne zaman temizlemelisin?

| Durum | Temizleme |
|-------|-----------|
| Kullanıcının rolleri değiştirildi | `_permissionService.ClearUserPermissionCache(userId)` |
| Rol silindiği için etkilenen tüm kullanıcılar | `_permissionService.ClearRolePermissionCache()` |
| Permission değişmedi | Temizleme gerekmez |

```csharp
// UpdateUserCommandHandler'da (kullanıcı rollerini değiştiriyorsa):
_permissionService.ClearUserPermissionCache(request.Id);

// DeleteRoleCommandHandler'da (rolü silince tüm cache geçersiz):
_permissionService.ClearRolePermissionCache();
```

**Ürün / Kategori gibi entity handler'larında** permission cache temizlemeye **gerek yoktur** — bu sadece User/Role/Permission değişikliklerini ilgilendirir.

---

### ServiceCollectionExtensions — Authorization Policy Kayıtları

**Dosya:** `Kickstart.API/Extensions/ServiceCollectionExtensions.cs`

Bu dosyada üç tür policy tanımlanır:

#### 1. Tekil Permission Policy'leri (Otomatik)

`AddResourcePermissionPolicies` → `Permissions.Helper.GetAllPermissions()` reflection ile tüm string sabitlerini okur ve her biri için otomatik policy oluşturur:

```
"Products.Read"   → policy adı: "products.read"
"Products.Create" → policy adı: "products.create"
... vb.
```

**`Permissions.cs`'e yeni class eklemek yeterlidir — bu kısım için başka bir şey yapılmaz.**

---

#### 2. Kombine Permission Policy'leri (MANUEL GÜNCELLEME GEREKİR)

`AddCombinedPermissionPolicies` içindeki diziler **hardcoded**'dır:

```csharp
// ServiceCollectionExtensions.cs — AddCombinedPermissionPolicies metodu

var readWriteResources = new[] { "Users", "Roles", "Permissions" };   // ← buraya ekle
var fullAccessResources = new[] { "Users", "Roles", "Permissions" };  // ← buraya ekle
```

Eğer controller'ında `"products.readwrite"` veya `"products.fullaccess"` policy'si kullanmak istiyorsan bu dizilere `"Products"` eklemelisin:

```csharp
var readWriteResources = new[] { "Users", "Roles", "Permissions", "Products" };
var fullAccessResources = new[] { "Users", "Roles", "Permissions", "Products" };
```

Bu policy'ler şu anlama gelir:

| Policy | Gerekli Permission'lar |
|--------|----------------------|
| `products.readwrite` | `Products.Read` + `Products.Create` + `Products.Update` |
| `products.fullaccess` | `Products.Read` + `Products.Create` + `Products.Update` + `Products.Delete` |

**Tekil CRUD endpoint'leri için bu dizilere eklemeye gerek yoktur.** Sadece tek bir endpoint'te birden fazla izni aynı anda zorunlu kılmak istersen ekle.

---

#### 3. Rol Tabanlı Policy'ler (Mevcut, Değişmez)

Aşağıdaki policy'ler hazır tanımlıdır, controller'larda doğrudan kullanılabilir:

| Policy Adı | Anlamı |
|-----------|--------|
| `RequireAdminRole` | Admin veya SuperAdmin rolü gerektirir |
| `RequireSuperAdminRole` | Sadece SuperAdmin rolü |
| `RequireManagerRole` | Manager, Admin veya SuperAdmin |
| `RequireUserRole` | Herhangi bir tanımlı rol |
| `RequireActiveUser` | `status` claim'i `Active` olan kullanıcı |
| `RequireEmailVerified` | E-postası onaylanmış kullanıcı |

```csharp
// Kullanım örneği (Controller'da):
[Authorize(Policy = "RequireAdminRole")]
[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteProduct(int id) { ... }
```

---

### Otomatik Kayıt Olan Her Şey (Özet)

| Bileşen | Nasıl Kaydediliyor? |
|---------|-------------------|
| Repository | `DependencyInjection.AddRepositories()` — reflection, sınıf adı `Repository` ile bitiyor olmalı |
| MediatR Handler | `services.AddMediatR(assembly)` — otomatik |
| FluentValidation Validator | `services.AddValidatorsFromAssembly(assembly)` — otomatik |
| AutoMapper Profile | `services.AddAutoMapper(assembly)` — otomatik |
| Tekil Permission Policy | `AddResourcePermissionPolicies()` — `Permissions.cs` reflection ile okunur |
| Soft Delete Filter | `DbContext.OnModelCreating()` — `BaseEntity` türevlerine otomatik uygulanır |
| Audit Fields | `DbContext.SaveChangesAsync()` override — `BaseAuditableEntity` türevlerine otomatik |
| Permission Seed | `SeedPermissionsAsync()` — `Permissions.cs` reflection ile okunur |

**Kombine policy dizileri (`readWriteResources`, `fullAccessResources`) elle güncellenmesi gereken tek şeydir.**

---

### Validation Pipeline — Otomatik Çalışır

`ValidationBehavior<TRequest,TResponse>` MediatR pipeline'ına kayıtlıdır. Her Command için:

1. `AbstractValidator<TCommand>` sınıfı oluşturulur
2. DI container bu validator'ı otomatik bulur
3. `_mediator.Send(command)` çağrısında, handler çalışmadan önce validator tetiklenir
4. Hata varsa handler'a hiç girmez, `Result.Failure(ValidationFailed, ...)` döner

**Manuel validator tetiklemene gerek yoktur.** Sadece şu kuralı uygula:
- Her `CreateXCommand` için → `CreateXCommandValidator`
- Her `UpdateXCommand` için → `UpdateXCommandValidator`

```csharp
// Validator örüntüsü (her zaman aynı):
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        // Opsiyonel alan — sadece dolu ise kontrol et:
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Enum validasyonu:
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");
    }
}
```

---

## Adım 2 — Infrastructure Layer

### 2.1 Entity Configuration (EF Core Fluent API)

**Dosya:** `Kickstart.Infrastructure/Persistence/EntityConfigurations/{Entity}Configuration.cs`

```csharp
using Kickstart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickstart.Infrastructure.Persistence.EntityConfigurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Stock)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Soft delete (BaseEntity'den geliyor)
            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Tenant desteği varsa
            builder.Property(x => x.TenantId)
                .IsRequired(false);

            // Unique index örneği (soft delete hariç)
            builder.HasIndex(x => new { x.TenantId, x.Name })
                .IsUnique()
                .HasFilter("\"TenantId\" IS NOT NULL AND NOT \"IsDeleted\"");

            // ---- İlişki Konfigürasyonları ----

            // Many-to-One
            builder.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);  // Cascade vs Restrict senaryoya göre seç

            // One-to-Many
            builder.HasMany(x => x.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many (ara tablo)
            // Ara tablonun kendi Configuration'ı ayrı dosyada yapılır
        }
    }
}
```

**Cascade silme politikası:**

| Senaryo | `OnDelete` |
|---------|-----------|
| Child parent olmadan anlamsız | `Cascade` |
| Child başka parent'a atanabilir | `Restrict` veya `SetNull` |
| Soft delete kullanılıyorsa genelde | `Cascade` (DB soft-delete ile işaretlenir) |

---

### 2.2 Repository Implementasyonu

**Dosya:** `Kickstart.Infrastructure/Repositories/{Entity}Repository.cs`

```csharp
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Entities;
using Kickstart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBase<Product, int>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> NameExistsAsync(string name, int? tenantId, int? excludeId = null)
        {
            var query = _context.Set<Product>()
                .Where(p => p.Name == name && p.TenantId == tenantId);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPagedAsync(
            int page, int pageSize, string searchTerm = null, int? tenantId = null)
        {
            var query = BuildProductsQuery(searchTerm, tenantId);
            var totalCount = await query.CountAsync();

            var products = await query
                .Include(p => p.Category)      // İlişkileri burada include et
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<Product> GetProductWithImagesAsync(int productId)
        {
            return await _context.Set<Product>()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<Product> GetProductWithAllRelationsAsync(int productId)
        {
            return await _context.Set<Product>()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        private IQueryable<Product> BuildProductsQuery(string searchTerm, int? tenantId)
        {
            var query = _context.Set<Product>().AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(p => p.TenantId == tenantId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    p.Description.ToLower().Contains(searchTerm.ToLower()));

            return query;
        }
    }
}
```

> **Not:** `RepositoryBase<T,TKey>` şu metotları sağlar: `GetByIdAsync`, `AddAsync`, `Update`, `Delete`, `SoftDelete`, `GetAllAsync`, `GetQueryable`, `FindAsync`, `FindFirstAsync`, `ExistsAsync`, `UpdateRange`. Bunları tekrar implement etme.

---

### 2.3 DbContext'e DbSet Ekleme

**Dosya:** `Kickstart.Infrastructure/Persistence/ApplicationDbContext.cs`

```csharp
// Mevcut DbSet'lerin yanına ekle:
public DbSet<Product> Products { get; set; }
```

---

### 2.4 UnitOfWork Güncelleme

**Dosya:** `Kickstart.Infrastructure/Persistence/UnitOfWork.cs`

```csharp
// 1. Lazy field ekle (diğer field'ların yanına):
private readonly Lazy<IProductRepository> _products;

// 2. Constructor'da initialize et:
_products = new Lazy<IProductRepository>(() => _serviceProvider.GetRequiredService<IProductRepository>());

// 3. Property ekle:
public IProductRepository Products => _products.Value;
```

---

### 2.5 Migration Oluşturma

```bash
# Infrastructure projesinden çalıştır:
dotnet ef migrations add Add{Entity}Table --project Kickstart.Infrastructure --startup-project Kickstart.API

# Uygula:
dotnet ef database update --project Kickstart.Infrastructure --startup-project Kickstart.API
```

> Repository'ler `DependencyInjection.cs`'deki `AddRepositories()` reflection metodu sayesinde **otomatik** kaydedilir. Manuel kayıt gerekmez; sadece sınıf adının `Repository` ile bitmesi yeterlidir.

---

## Adım 3 — Application Layer

### 3.1 DTO'lar

**Dosya:** `Kickstart.Application/Features/{Entity}s/Dtos/`

#### Create DTO
```csharp
// CreateProductDto.cs
namespace Kickstart.Application.Features.Products.Dtos
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public int? TenantId { get; set; }  // Sadece SuperAdmin kullanır
    }
}
```

#### Update DTO
```csharp
// UpdateProductDto.cs
namespace Kickstart.Application.Features.Products.Dtos
{
    public class UpdateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
    }
}
```

#### Response DTO (liste için)
```csharp
// ProductListDto.cs
namespace Kickstart.Application.Features.Products.Dtos
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }   // Navigation'dan flatten
        public DateTime CreatedDate { get; set; }
        public int? TenantId { get; set; }
    }
}
```

#### Response DTO (detay için — ilişkili verilerle)
```csharp
// ProductDto.cs
namespace Kickstart.Application.Features.Products.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public CategoryDto Category { get; set; }         // İlişkili nesne
        public List<ProductImageDto> Images { get; set; } // İlişkili koleksiyon
        public DateTime CreatedDate { get; set; }
        public int? TenantId { get; set; }
    }
}
```

---

### 3.2 Commands

#### Create Command

**Dosya:** `Kickstart.Application/Features/{Entity}s/Commands/Create{Entity}/Create{Entity}Command.cs`

```csharp
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using MediatR;

namespace Kickstart.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<Result<ProductListDto>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public int? TenantId { get; set; }
    }
}
```

**Handler:** `Create{Entity}CommandHandler.cs`

```csharp
using AutoMapper;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Products.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Entities;
using Kickstart.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ProductListDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Tenant belirleme (User handler'ıyla aynı pattern)
            int? tenantId;
            if (request.TenantId.HasValue)
            {
                if (!_currentUserService.CanAccessAllTenants)
                    return Result<ProductListDto>.Failure(Error.Failure(ErrorCode.Forbidden,
                        "Only SuperAdmin can create products in a specific tenant"));
                tenantId = request.TenantId;
            }
            else
            {
                tenantId = _currentUserService.TenantId;
            }

            // Unique kontrol
            if (await _unitOfWork.Products.NameExistsAsync(request.Name, tenantId))
                return Result<ProductListDto>.Failure(Error.Failure(ErrorCode.AlreadyExists,
                    "A product with this name already exists"));

            // İlişkili entity varlık kontrolü
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
            if (category == null)
                return Result<ProductListDto>.Failure(Error.Failure(ErrorCode.NotFound,
                    "Category not found"));

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId,
                TenantId = tenantId,
                IsActive = true
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Many-to-Many ilişki örneği (ProductTag gibi):
            // if (request.TagIds?.Any() == true)
            // {
            //     foreach (var tagId in request.TagIds)
            //         await _unitOfWork.Products.AddProductTagAsync(new ProductTag { ProductId = product.Id, TagId = tagId });
            //     await _unitOfWork.SaveChangesAsync(cancellationToken);
            // }

            // İlişkili verilerle yeniden yükle
            var productWithRelations = await _unitOfWork.Products.GetProductWithAllRelationsAsync(product.Id);
            return Result<ProductListDto>.Success(_mapper.Map<ProductListDto>(productWithRelations));
        }
    }
}
```

**Validator:** `Create{Entity}CommandValidator.cs`

```csharp
using FluentValidation;

namespace Kickstart.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required.");
        }
    }
}
```

---

#### Update Command

**Handler mantığı (User'dan devralınan pattern):**

```csharp
public async Task<Result<ProductListDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
{
    var product = await _unitOfWork.Products.GetProductWithAllRelationsAsync(request.Id);

    if (product == null)
        return Result<ProductListDto>.Failure(Error.Failure(ErrorCode.NotFound, "Product not found"));

    // Tenant izin kontrolü
    if (!_currentUserService.CanAccessAllTenants && product.TenantId != _currentUserService.TenantId)
        return Result<ProductListDto>.Failure(Error.Failure(ErrorCode.Forbidden,
            "You do not have access to update this product"));

    // Unique kontrol (güncellenen kayıt hariç)
    if (await _unitOfWork.Products.NameExistsAsync(request.Name, product.TenantId, request.Id))
        return Result<ProductListDto>.Failure(Error.Failure(ErrorCode.AlreadyExists,
            "A product with this name already exists"));

    // Alanları güncelle
    product.Name = request.Name;
    product.Description = request.Description;
    product.Price = request.Price;
    product.Stock = request.Stock;
    product.IsActive = request.IsActive;
    product.CategoryId = request.CategoryId;

    // Many-to-Many güncelleme örneği (User→Role pattern'ı gibi):
    // foreach (var tag in product.ProductTags.ToList())
    //     _unitOfWork.Products.RemoveProductTag(tag);
    // foreach (var tagId in request.TagIds)
    //     await _unitOfWork.Products.AddProductTagAsync(new ProductTag { ProductId = product.Id, TagId = tagId });

    _unitOfWork.Products.Update(product);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var updated = await _unitOfWork.Products.GetProductWithAllRelationsAsync(product.Id);
    return Result<ProductListDto>.Success(_mapper.Map<ProductListDto>(updated));
}
```

---

#### Delete Command

```csharp
public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
{
    var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

    if (product == null)
        return Result.Failure(Error.Failure(ErrorCode.NotFound, "Product not found"));

    if (!_currentUserService.CanAccessAllTenants && product.TenantId != _currentUserService.TenantId)
        return Result.Failure(Error.Failure(ErrorCode.Forbidden,
            "You do not have access to delete this product"));

    _unitOfWork.Products.SoftDelete(product);  // IsDeleted = true, fiziksel silme değil
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
}
```

---

### 3.3 Queries

#### GetAll (Sayfalı)

**Query:**
```csharp
public class GetAllProductsQuery : IRequest<PagedResult<ProductListDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; }
}
```

**Handler:**
```csharp
public async Task<PagedResult<ProductListDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
{
    int? tenantId = _currentUserService.CanAccessAllTenants ? null : _currentUserService.TenantId;

    var (products, totalCount) = await _unitOfWork.Products.GetProductsPagedAsync(
        request.Page, request.PageSize, request.SearchTerm, tenantId);

    var dtos = _mapper.Map<IEnumerable<ProductListDto>>(products);
    var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

    return PagedResult<ProductListDto>.Success(dtos, request.Page, totalPages, totalCount);
}
```

---

#### GetById

**Handler:**
```csharp
public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
{
    var product = await _unitOfWork.Products.GetProductWithAllRelationsAsync(request.Id);

    if (product == null)
        return Result<ProductDto>.Failure(Error.Failure(ErrorCode.NotFound, "Product not found"));

    if (!_currentUserService.CanAccessAllTenants && product.TenantId != _currentUserService.TenantId)
        return Result<ProductDto>.Failure(Error.Failure(ErrorCode.Forbidden,
            "You do not have access to this product"));

    return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
}
```

---

### 3.4 AutoMapper — MappingProfile

**Dosya:** `Kickstart.Application/Features/Mappings/MappingProfile.cs`

```csharp
// MappingProfile constructor içine ekle:

// Product → Response DTO
CreateMap<Product, ProductListDto>()
    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

// Command → Entity (handler'da manuel atama tercih edilir, ancak mapper da kullanılabilir)
CreateMap<CreateProductCommand, Product>()
    .ForMember(dest => dest.Images, opt => opt.Ignore())
    .ForMember(dest => dest.ProductTags, opt => opt.Ignore());

CreateMap<UpdateProductCommand, Product>()
    .ForMember(dest => dest.Images, opt => opt.Ignore())
    .ForMember(dest => dest.ProductTags, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

// DTO → Command (Controller'da kullanım için)
CreateMap<CreateProductDto, CreateProductCommand>();
CreateMap<UpdateProductDto, UpdateProductCommand>();
```

> **Kural:** Navigation property koleksiyonları (`UserRoles`, `Images`, vb.) mapping'de `opt.Ignore()` ile pas geçilir. Handler'da elle yönetilir.

---

## Adım 4 — API Layer

### 4.1 Controller

**Dosya:** `Kickstart.API/Controllers/{Entity}sController.cs`

```csharp
using Kickstart.Application.Features.Products.Commands.CreateProduct;
using Kickstart.Application.Features.Products.Commands.UpdateProduct;
using Kickstart.Application.Features.Products.Commands.DeleteProduct;
using Kickstart.Application.Features.Products.Dtos;
using Kickstart.Application.Features.Products.Queries.GetAllProducts;
using Kickstart.Application.Features.Products.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickstart.API.Controllers
{
    [Authorize]
    public class ProductsController : BaseController
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "products.read")]
        [ProducesResponseType(typeof(IEnumerable<ProductListDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            var query = new GetAllProductsQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var result = await _mediator.Send(query);
            return HandlePagedResult(result);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "products.read")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductById(int id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
            return HandleResult(result);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "products.create")]
        [ProducesResponseType(typeof(ProductListDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var command = new CreateProductCommand
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock,
                CategoryId  = dto.CategoryId,
                TenantId    = dto.TenantId
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetProductById),
                    new { id = result.Value.Id },
                    new { success = true, data = result.Value });

            return HandleResult(result);
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "products.update")]
        [ProducesResponseType(typeof(ProductListDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var command = new UpdateProductCommand
            {
                Id          = id,
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock,
                IsActive    = dto.IsActive,
                CategoryId  = dto.CategoryId
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Delete a product (soft delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "products.delete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _mediator.Send(new DeleteProductCommand { Id = id });
            return HandleResult(result);
        }
    }
}
```

**Response formatları:**

| Durum | Format |
|-------|--------|
| Başarılı (200) | `{ success: true, data: {...} }` |
| Oluşturuldu (201) | `{ success: true, data: {...} }` + `Location` header |
| Hata | `{ success: false, error: { code, message, status } }` |
| Sayfalı (200) | `{ success: true, data: { items, totalCount, totalPages, pageNumber } }` |

---

## İlişki Senaryoları

### Many-to-One (Product → Category)

```
Entity:    public int CategoryId { get; set; }
           public Category Category { get; set; }

Config:    builder.HasOne(x => x.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

Handler:   var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
           if (category == null) → NotFound hatası döndür
           product.CategoryId = request.CategoryId;
```

### One-to-Many (Product → ProductImage)

```
Entity:    public ICollection<ProductImage> Images { get; set; }

Config:    builder.HasMany(x => x.Images)
               .WithOne(i => i.Product)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

Create:    // ProductImage ayrı command ile eklenir, ya da Create içinde:
           foreach (var imageUrl in request.ImageUrls)
               await _context.ProductImages.AddAsync(new ProductImage { ProductId = product.Id, Url = imageUrl });

Delete:    // Cascade ile otomatik silinir
```

### Many-to-Many (Product ↔ Tag) — User→Role Gibi

```
Ara Tablo: public class ProductTag : BaseEntity
           {
               public int ProductId { get; set; }
               public Product Product { get; set; }
               public int TagId { get; set; }
               public Tag Tag { get; set; }
           }

Repository Metotları:
    Task AddProductTagAsync(ProductTag productTag);
    void RemoveProductTag(ProductTag productTag);

Create Handler:
    foreach (var tagId in request.TagIds)
        await _unitOfWork.Products.AddProductTagAsync(new ProductTag { ProductId = product.Id, TagId = tagId });
    await _unitOfWork.SaveChangesAsync();

Update Handler (tam yenileme — User→Role pattern):
    foreach (var tag in product.ProductTags.ToList())
        _unitOfWork.Products.RemoveProductTag(tag);
    foreach (var tagId in request.TagIds)
        await _unitOfWork.Products.AddProductTagAsync(new ProductTag { ProductId = product.Id, TagId = tagId });
    await _unitOfWork.SaveChangesAsync();
```

---

## Kontrol Listesi

Yeni bir entity oluştururken bu listedeki her maddeyi işaretle:

### Domain
- [ ] `Entities/{Entity}.cs` — `BaseAuditableEntity`'den türetildi
- [ ] `Common/Interfaces/Repositories/I{Entity}Repository.cs` — özel metotlar tanımlandı
- [ ] `IUnitOfWork.cs` — yeni repository property eklendi
- [ ] `Constants/Permissions.cs` — `Read/Create/Update/Delete` string sabitleri eklendi (`"entity.read"` formatında)
- [ ] Permission string formatı doğrulandı: tamamı **küçük harf**, resource ve action arasında **nokta**
- [ ] `readwrite`/`fullaccess` policy gerekiyorsa `ServiceCollectionExtensions.cs`'deki dizilere resource adı eklendi
- [ ] Enum gerekiyorsa `Common/Enums/` altına eklendi
- [ ] Yeni `PermissionType` action eklendiyse `SeedData.MapActionToPermissionType()` güncellendi

### Infrastructure
- [ ] `Persistence/EntityConfigurations/{Entity}Configuration.cs` — Fluent API yapılandırıldı
- [ ] `Repositories/{Entity}Repository.cs` — interface implement edildi
- [ ] `Persistence/ApplicationDbContext.cs` — `DbSet<{Entity}>` eklendi
- [ ] `Persistence/UnitOfWork.cs` — lazy field + property + constructor init eklendi
- [ ] Migration oluşturuldu ve uygulandı

### Application
- [ ] `Features/{Entity}s/Dtos/Create{Entity}Dto.cs`
- [ ] `Features/{Entity}s/Dtos/Update{Entity}Dto.cs`
- [ ] `Features/{Entity}s/Dtos/{Entity}ListDto.cs`
- [ ] `Features/{Entity}s/Dtos/{Entity}Dto.cs` (detay için — gerekirse)
- [ ] `Commands/Create{Entity}/Create{Entity}Command.cs`
- [ ] `Commands/Create{Entity}/Create{Entity}CommandHandler.cs`
- [ ] `Commands/Create{Entity}/Create{Entity}CommandValidator.cs`
- [ ] `Commands/Update{Entity}/Update{Entity}Command.cs`
- [ ] `Commands/Update{Entity}/Update{Entity}CommandHandler.cs`
- [ ] `Commands/Update{Entity}/Update{Entity}CommandValidator.cs`
- [ ] `Commands/Delete{Entity}/Delete{Entity}Command.cs`
- [ ] `Commands/Delete{Entity}/Delete{Entity}CommandHandler.cs`
- [ ] `Queries/GetAll{Entity}s/GetAll{Entity}sQuery.cs`
- [ ] `Queries/GetAll{Entity}s/GetAll{Entity}sQueryHandler.cs`
- [ ] `Queries/Get{Entity}ById/Get{Entity}ByIdQuery.cs`
- [ ] `Queries/Get{Entity}ById/Get{Entity}ByIdQueryHandler.cs`
- [ ] `Features/Mappings/MappingProfile.cs` — entity mapping'leri eklendi

### API
- [ ] `Controllers/{Entity}sController.cs` — 5 endpoint (GetAll, GetById, Create, Update, Delete)
- [ ] Tüm endpoint'lerde `[Authorize(Policy = "...")]` tanımlandı

---

## Klasör Yapısı (Referans)

```
Kickstart.Application/
└── Features/
    └── Products/
        ├── Commands/
        │   ├── CreateProduct/
        │   │   ├── CreateProductCommand.cs
        │   │   ├── CreateProductCommandHandler.cs
        │   │   └── CreateProductCommandValidator.cs
        │   ├── UpdateProduct/
        │   │   ├── UpdateProductCommand.cs
        │   │   ├── UpdateProductCommandHandler.cs
        │   │   └── UpdateProductCommandValidator.cs
        │   └── DeleteProduct/
        │       ├── DeleteProductCommand.cs
        │       └── DeleteProductCommandHandler.cs
        ├── Queries/
        │   ├── GetAllProducts/
        │   │   ├── GetAllProductsQuery.cs
        │   │   └── GetAllProductsQueryHandler.cs
        │   └── GetProductById/
        │       ├── GetProductByIdQuery.cs
        │       └── GetProductByIdQueryHandler.cs
        └── Dtos/
            ├── CreateProductDto.cs
            ├── UpdateProductDto.cs
            ├── ProductListDto.cs
            └── ProductDto.cs

Kickstart.Domain/
└── Entities/
    └── Product.cs
└── Common/Interfaces/Repositories/
    └── IProductRepository.cs
└── Constants/
    └── Permissions.cs  (güncellendi)

Kickstart.Infrastructure/
└── Persistence/
│   ├── EntityConfigurations/
│   │   └── ProductConfiguration.cs
│   ├── ApplicationDbContext.cs  (güncellendi)
│   └── UnitOfWork.cs           (güncellendi)
└── Repositories/
    └── ProductRepository.cs

Kickstart.API/
└── Controllers/
    └── ProductsController.cs
```

---

## Sık Yapılan Hatalar

| Hata | Doğrusu |
|------|---------|
| Repository'yi doğrudan handler'a inject etmek | `IUnitOfWork` inject et, `_unitOfWork.Products` kullan |
| `SaveChangesAsync()` çağrısını unutmak | `AddAsync` / `Update` / `SoftDelete` sonrası mutlaka çağır |
| Navigation property'yi Mapper'da ignore etmemek | Koleksiyonlar için `opt.Ignore()` + handler'da elle yönet |
| Fiziksel silme yapmak | `Delete()` değil `SoftDelete()` kullan |
| Permission string'ini büyük harfle yazmak | `"products.read"` (tamamı küçük harf + nokta) |
| Many-to-many güncellemede var olanı temizlememek | Önce `Remove`, sonra `Add` — User→Role pattern'ını taklit et |
| `CreatedDate`/`CreatedBy`'ı manuel set etmek | `DbContext.SaveChangesAsync()` override'ı otomatik halleder |
| Permissions.cs'e ekleyince SeedData'ya da dokunmak | `SeedPermissionsAsync` reflection kullanır, otomatik seed edilir |
| Permission string'ini DB'ye manuel eklemek | Uygulama başlangıcında `SeedAsyncIfEmpty` otomatik ekler |
| User dışı entity'de `TenantAdminVisibility` kontrolü yapmak | Sadece `User` entity yönetiminde gereklidir |
| Ürün/Kategori handler'ında `ClearUserPermissionCache` çağırmak | Sadece User/Role değişikliklerinde gereklidir |
| Validator'ı handler içinde elle çağırmak | `ValidationBehavior` pipeline'ı otomatik tetikler |
| `PermissionType` enum'una yeni değer ekleyince `MapActionToPermissionType` güncellememek | `SeedData.cs`'deki switch'e yeni case eklenmelidir |
| Delete handler'da `Result<bool>` dönüş tipi kullanmak | Gerçek pattern `Result` (generic değil) — `DeleteUserCommandHandler` referans al |
| Controller'a `[ApiController]` attribute eklemek | `BaseController`'da zaten var, tekrar ekleme |
| Sayfalı liste için iki ayrı DB sorgusu (Count + Data) | Tek sorguda tuple döndür: `GetProductsPagedAsync` → `(IEnumerable<Product>, int TotalCount)` |
| Yeni entity için `readwrite`/`fullaccess` policy istiyorken `ServiceCollectionExtensions.cs` güncellememek | `AddCombinedPermissionPolicies` içindeki dizilere resource adı eklenmeli |
