# Yeni CRUD Tablosu Ekleme Rehberi

Bu rehber, projeye yeni bir CRUD işlemli tablo eklemek için izlenmesi gereken adımları detaylı olarak açıklar.

## Proje Yapısı

Proje **Clean Architecture** ve **CQRS Pattern** (MediatR) kullanmaktadır. Katmanlar:

- **Domain**: Entities, Repository Interfaces
- **Application**: Commands, Queries, Handlers, DTOs, Mappings
- **Infrastructure**: Repositories, Entity Configurations, DbContext
- **API**: Controllers

---

## Adım Adım Ekleme Süreci

### Örnek: "Product" Tablosu Ekleyelim

---

## 1️⃣ DOMAIN KATMANI - Entity Oluşturma

**Dosya:** `MemberShip.Domain/Entities/Product.cs`

```csharp
using MemberShip.Domain.Common;
using System;

namespace MemberShip.Domain.Entities
{
    public class Product : BaseAuditableEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }

        public Product()
        {
            IsActive = true;
        }
    }
}
```

**Not:** 
- `BaseAuditableEntity` kullanırsanız: `Id`, `CreatedDate`, `UpdatedDate`, `IsDeleted`, `CreatedBy`, `UpdatedBy` otomatik gelir
- `BaseEntity` kullanırsanız: `CreatedBy` ve `UpdatedBy` olmaz

---

## 2️⃣ DOMAIN KATMANI - Repository Interface

**Dosya:** `MemberShip.Domain/Common/Interfaces/Repositories/IProductRepository.cs`

```csharp
using MemberShip.Domain.Entities;
using System.Threading.Tasks;

namespace MemberShip.Application.Interfaces
{
    public interface IProductRepository : IRepository<Product, int>
    {
        // Özel metodlar buraya eklenir (opsiyonel)
        Task<Product> GetByNameAsync(string name);
        Task<bool> NameExistsAsync(string name);
    }
}
```

---

## 3️⃣ INFRASTRUCTURE KATMANI - Entity Configuration

**Dosya:** `MemberShip.Infrastructure/Persistence/EntityConfigurations/ProductConfiguration.cs`

```csharp
using MemberShip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MemberShip.Infrastructure.Data.Configurations
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

            builder.HasIndex(x => x.Name)
                .IsUnique();

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

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
    }
}
```

**Önemli:** Bu dosya otomatik olarak `ApplicationDbContext.OnModelCreating` içinde yüklenir (Assembly reflection ile).

---

## 4️⃣ INFRASTRUCTURE KATMANI - Repository Implementasyonu

**Dosya:** `MemberShip.Infrastructure/Repositories/ProductRepository.cs`

```csharp
using MemberShip.Application.Interfaces;
using MemberShip.Domain.Entities;
using MemberShip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MemberShip.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBase<Product, int>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Product> GetByNameAsync(string name)
        {
            return await _context.Set<Product>()
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await _context.Set<Product>()
                .AnyAsync(p => p.Name == name);
        }
    }
}
```

---

## 5️⃣ INFRASTRUCTURE KATMANI - DbContext'e Ekleme

**Dosya:** `MemberShip.Infrastructure/Persistence/ApplicationDbContext.cs`

```csharp
// DbSet'leri ekleyin
public DbSet<Product> Products { get; set; }
```

---

## 6️⃣ INFRASTRUCTURE KATMANI - UnitOfWork'e Ekleme

**Dosya:** `MemberShip.Domain/Common/Interfaces/IUnitOfWork.cs`

```csharp
// Property ekleyin
IProductRepository Products { get; }
```

**Dosya:** `MemberShip.Infrastructure/Persistence/UnitOfWork.cs`

```csharp
// Lazy field ekleyin
private readonly Lazy<IProductRepository> _products;

// Constructor'da initialize edin
_products = new Lazy<IProductRepository>(() => _serviceProvider.GetRequiredService<IProductRepository>());

// Property ekleyin
public IProductRepository Products => _products.Value;
```

---

## 7️⃣ APPLICATION KATMANI - DTOs

### ProductDto.cs
**Dosya:** `MemberShip.Application/Features/Products/Dtos/ProductDto.cs`

```csharp
using System;

namespace MemberShip.Application.Features.Products.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
```

### ProductListDto.cs
**Dosya:** `MemberShip.Application/Features/Products/Dtos/ProductListDto.cs`

```csharp
namespace MemberShip.Application.Features.Products.Dtos
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
```

### CreateProductDto.cs
**Dosya:** `MemberShip.Application/Features/Products/Dtos/CreateProductDto.cs`

```csharp
namespace MemberShip.Application.Features.Products.Dtos
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
```

### UpdateProductDto.cs
**Dosya:** `MemberShip.Application/Features/Products/Dtos/UpdateProductDto.cs`

```csharp
namespace MemberShip.Application.Features.Products.Dtos
{
    public class UpdateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
```

---

## 8️⃣ APPLICATION KATMANI - Commands

### CreateProductCommand
**Dosya:** `MemberShip.Application/Features/Products/Commands/CreateProduct/CreateProductCommand.cs`

```csharp
using MemberShip.Application.Common.Results;
using MemberShip.Application.Features.Products.Dtos;
using MediatR;

namespace MemberShip.Application.Features.Products.Commands
{
    public class CreateProductCommand : IRequest<Result<ProductListDto>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
```

**Dosya:** `MemberShip.Application/Features/Products/Commands/CreateProduct/CreateProductCommandHandler.cs`

```csharp
using AutoMapper;
using MemberShip.Application.Features.Products.Commands;
using MemberShip.Application.Features.Products.Dtos;
using MemberShip.Application.Interfaces;
using MemberShip.Application.Common.Results;
using MemberShip.Domain.Entities;
using MemberShip.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MemberShip.Application.Features.Products.Handlers
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProductListDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // İsim kontrolü
            if (await _unitOfWork.Products.NameExistsAsync(request.Name))
            {
                return Result<ProductListDto>.Failure(Error.Failure(
                    ErrorCode.AlreadyExists,
                    "Product name already exists"));
            }

            // Entity oluştur
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                IsActive = request.IsActive
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var productDto = _mapper.Map<ProductListDto>(product);
            return Result<ProductListDto>.Success(productDto);
        }
    }
}
```

**Dosya:** `MemberShip.Application/Features/Products/Commands/CreateProduct/CreateProductCommandValidator.cs`

```csharp
using FluentValidation;
using MemberShip.Application.Features.Products.Commands;

namespace MemberShip.Application.Features.Products.Validators
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock must be greater than or equal to 0");
        }
    }
}
```

### UpdateProductCommand
**Dosya:** `MemberShip.Application/Features/Products/Commands/UpdateProduct/UpdateProductCommand.cs`

```csharp
using MemberShip.Application.Common.Results;
using MemberShip.Application.Features.Products.Dtos;
using MediatR;

namespace MemberShip.Application.Features.Products.Commands
{
    public class UpdateProductCommand : IRequest<Result<ProductListDto>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
```

**Dosya:** `MemberShip.Application/Features/Products/Commands/UpdateProduct/UpdateProductCommandHandler.cs`

```csharp
using AutoMapper;
using MemberShip.Application.Features.Products.Commands;
using MemberShip.Application.Features.Products.Dtos;
using MemberShip.Application.Interfaces;
using MemberShip.Application.Common.Results;
using MemberShip.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MemberShip.Application.Features.Products.Handlers
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProductListDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
            
            if (product == null)
            {
                return Result<ProductListDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "Product not found"));
            }

            // İsim kontrolü (eğer değiştiyse)
            if (product.Name != request.Name && await _unitOfWork.Products.NameExistsAsync(request.Name))
            {
                return Result<ProductListDto>.Failure(Error.Failure(
                    ErrorCode.AlreadyExists,
                    "Product name already exists"));
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.IsActive = request.IsActive;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            var productDto = _mapper.Map<ProductListDto>(product);
            return Result<ProductListDto>.Success(productDto);
        }
    }
}
```

**Dosya:** `MemberShip.Application/Features/Products/Commands/UpdateProduct/UpdateProductCommandValidator.cs`

```csharp
using FluentValidation;
using MemberShip.Application.Features.Products.Commands;

namespace MemberShip.Application.Features.Products.Validators
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Product ID is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock must be greater than or equal to 0");
        }
    }
}
```

### DeleteProductCommand
**Dosya:** `MemberShip.Application/Features/Products/Commands/DeleteProduct/DeleteProductCommand.cs`

```csharp
using MemberShip.Application.Common.Results;
using MediatR;

namespace MemberShip.Application.Features.Products.Commands
{
    public class DeleteProductCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }
}
```

**Dosya:** `MemberShip.Application/Features/Products/Commands/DeleteProduct/DeleteProductCommandHandler.cs`

```csharp
using MemberShip.Application.Features.Products.Commands;
using MemberShip.Application.Interfaces;
using MemberShip.Application.Common.Results;
using MemberShip.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MemberShip.Application.Features.Products.Handlers
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
            
            if (product == null)
            {
                return Result<bool>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "Product not found"));
            }

            // Soft delete
            _unitOfWork.Products.SoftDelete(product);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
```

---

## 9️⃣ APPLICATION KATMANI - Queries

### GetAllProductsQuery
**Dosya:** `MemberShip.Application/Features/Products/Queries/GetAllProducts/GetAllProductsQuery.cs`

```csharp
using MemberShip.Application.Common.Results;
using MemberShip.Application.Features.Products.Dtos;
using MediatR;
using System.Collections.Generic;

namespace MemberShip.Application.Features.Products.Queries
{
    public class GetAllProductsQuery : IRequest<Result<IEnumerable<ProductListDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
    }
}
```

**Dosya:** `MemberShip.Application/Features/Products/Queries/GetAllProducts/GetAllProductsQueryHandler.cs`

```csharp
using AutoMapper;
using MemberShip.Application.Features.Products.Dtos;
using MemberShip.Application.Features.Products.Queries;
using MemberShip.Application.Interfaces;
using MemberShip.Application.Common.Results;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MemberShip.Application.Features.Products.Handlers
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<IEnumerable<ProductListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProductListDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Products.GetQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            // Sayfalama
            var products = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var productDtos = _mapper.Map<IEnumerable<ProductListDto>>(products);
            return Result<IEnumerable<ProductListDto>>.Success(productDtos);
        }
    }
}
```

### GetProductByIdQuery
**Dosya:** `MemberShip.Application/Features/Products/Queries/GetProductById/GetProductByIdQuery.cs`

```csharp
using MemberShip.Application.Common.Results;
using MemberShip.Application.Features.Products.Dtos;
using MediatR;

namespace MemberShip.Application.Features.Products.Queries
{
    public class GetProductByIdQuery : IRequest<Result<ProductDto>>
    {
        public int Id { get; set; }
    }
}
```

**Dosya:** `MemberShip.Application/Features/Products/Queries/GetProductById/GetProductByIdQueryHandler.cs`

```csharp
using AutoMapper;
using MemberShip.Application.Features.Products.Dtos;
using MemberShip.Application.Features.Products.Queries;
using MemberShip.Application.Interfaces;
using MemberShip.Application.Common.Results;
using MemberShip.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MemberShip.Application.Features.Products.Handlers
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
            
            if (product == null)
            {
                return Result<ProductDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "Product not found"));
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return Result<ProductDto>.Success(productDto);
        }
    }
}
```

---

## 🔟 APPLICATION KATMANI - AutoMapper Mapping

**Dosya:** `MemberShip.Application/Features/Mappings/MappingProfile.cs`

```csharp
// MappingProfile constructor'ına ekleyin:

// Product mappings
CreateMap<Product, ProductDto>();
CreateMap<Product, ProductListDto>();

CreateMap<CreateProductCommand, Product>()
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

CreateMap<CreateProductDto, Product>();

CreateMap<UpdateProductCommand, Product>()
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

CreateMap<UpdateProductDto, Product>();
```

---

## 1️⃣1️⃣ API KATMANI - Controller

**Dosya:** `MemberShip.API/Controllers/ProductsController.cs`

```csharp
using MemberShip.API.Controllers;
using MemberShip.Application.Features.Products.Commands;
using MemberShip.Application.Features.Products.Dtos;
using MemberShip.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MemberShip.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
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
            return HandleResult(result);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "products.read")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        public async Task<IActionResult> GetProductById(int id)
        {
            var query = new GetProductByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "products.create")]
        [ProducesResponseType(typeof(ProductListDto), 201)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var command = new CreateProductCommand
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                IsActive = dto.IsActive
            };

            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetProductById), new { id = result.Value.Id }, 
                    new { success = true, data = result.Value });
            
            return HandleResult(result);
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "products.update")]
        [ProducesResponseType(typeof(ProductListDto), 200)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var command = new UpdateProductCommand
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                IsActive = dto.IsActive
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
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var command = new DeleteProductCommand { Id = id };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
```

---

## 1️⃣2️⃣ Migration Oluşturma

Terminal'de şu komutları çalıştırın:

```bash
# Migration oluştur
dotnet ef migrations add AddProductTable --project MemberShip.Infrastructure --startup-project MemberShip.API

# Database'e uygula
dotnet ef database update --project MemberShip.Infrastructure --startup-project MemberShip.API
```

---

## ✅ Özet Checklist

- [ ] Domain: Entity oluşturuldu
- [ ] Domain: Repository interface oluşturuldu
- [ ] Infrastructure: Entity configuration oluşturuldu
- [ ] Infrastructure: Repository implementasyonu yapıldı
- [ ] Infrastructure: DbContext'e DbSet eklendi
- [ ] Infrastructure: UnitOfWork'e repository eklendi
- [ ] Application: DTOs oluşturuldu (ProductDto, ProductListDto, CreateProductDto, UpdateProductDto)
- [ ] Application: Commands oluşturuldu (Create, Update, Delete)
- [ ] Application: Command Handlers yazıldı
- [ ] Application: Command Validators yazıldı
- [ ] Application: Queries oluşturuldu (GetAll, GetById)
- [ ] Application: Query Handlers yazıldı
- [ ] Application: AutoMapper mapping'leri eklendi
- [ ] API: Controller oluşturuldu ve endpoint'ler eklendi
- [ ] Migration oluşturuldu ve uygulandı

---

## 📝 Notlar

1. **Repository'ler otomatik kayıt edilir** - `DependencyInjection.cs` içindeki `AddRepositories` metodu sayesinde
2. **MediatR otomatik kayıt edilir** - `Application/DependencyInjection.cs` içinde
3. **AutoMapper otomatik kayıt edilir** - `Application/DependencyInjection.cs` içinde
4. **FluentValidation otomatik kayıt edilir** - Validator'lar otomatik bulunur
5. **Soft Delete** - `BaseEntity` içindeki `IsDeleted` property'si kullanılır
6. **Audit Fields** - `BaseAuditableEntity` kullanırsanız `CreatedBy` ve `UpdatedBy` otomatik takip edilir

---

## 🎯 Sonuç

Bu adımları takip ederek projeye yeni bir CRUD tablosu ekleyebilirsiniz. Her katmanın sorumluluğu net bir şekilde ayrılmıştır ve Clean Architecture prensipleri korunur.
