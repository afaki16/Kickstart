# Admin Kullanıcısı

Bu proje için otomatik olarak oluşturulan admin kullanıcısı bilgileri aşağıdadır:

## Admin Kullanıcı Bilgileri

- **Email:** admin@kickstart.com
- **Şifre:** Admin123!
- **Ad:** System
- **Soyad:** Administrator
- **Telefon:** +905551234567
- **Durum:** Aktif
- **Email Onaylı:** Evet
- **Telefon Onaylı:** Evet

## Diğer seed kullanıcıları (ilk kurulum)

Veritabanı boşken `SeedData` ayrıca şunları oluşturur:

| Rol | Email | Şifre |
|-----|-------|-------|
| SuperAdmin | superadmin@kickstart.com | SuperAdmin123! |
| Admin | admin@kickstart.com | Admin123! |
| (standart) | user@kickstart.com | User123! |

## Admin Rolü

Admin kullanıcısı "Admin" rolüne sahiptir ve aşağıdaki tüm izinlere sahiptir:

### Kullanıcı İzinleri
- Kullanıcı oluşturma
- Kullanıcı görüntüleme
- Kullanıcı güncelleme
- Kullanıcı silme
- Kullanıcı yönetimi

### Rol İzinleri
- Rol oluşturma
- Rol görüntüleme
- Rol güncelleme
- Rol silme
- Rol yönetimi

### İzin Yönetimi
- İzin oluşturma
- İzin görüntüleme
- İzin güncelleme
- İzin silme
- İzin yönetimi

### Sistem İzinleri
- Sistem yönetimi
- Log görüntüleme
- Ayarları yönetme

## Güvenlik Notları

1. **İlk Giriş:** Veritabanı ilk kez doldurulduğunda seed kullanıcıları oluşturulur (`SeedAsyncIfEmpty`).
2. **Şifre Değiştirme:** Güvenlik için admin kullanıcısının şifresini ilk girişten sonra değiştirmeniz önerilir.
3. **Sistem Rolü:** Admin rolü sistem rolü olarak işaretlenmiştir ve silinemez.

## API Kullanımı

Admin kullanıcısı ile giriş yapmak için:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@kickstart.com",
  "password": "Admin123!"
}
```

## Seed Data

Kullanıcılar ve ilgili veriler `Kickstart.Infrastructure/Persistence/SeedData.cs` içinde tanımlıdır. Boş veritabanında uygulama başlarken seed çalışır; ayrıca eksik permission’lar için `SeedPermissionsAsync` her başlangıçta kontrol edilir.

## Değişiklikler

**GUID'den INT'e Geçiş:**
- Tüm entity'lerde ID alanları GUID'den INT'e çevrildi
- Foreign key'ler INT olarak güncellendi
- Repository'ler ve service'ler INT ID'leri kullanacak şekilde güncellendi
- DTO'lar ve Command/Query'ler INT ID'leri kullanacak şekilde güncellendi
- JWT token'larda user ID'ler INT olarak saklanıyor
