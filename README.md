# 🚀 Kickstart Template

Bu template ile hızlıca .NET Web API + Nuxt.js projesi oluşturabilirsiniz.

## 📁 Proje Yapısı

```
Kickstart/
├── backend/                 # .NET Web API
│   ├── {{PROJECT_NAME}}.API/
│   ├── {{PROJECT_NAME}}.Application/
│   ├── {{PROJECT_NAME}}.Domain/
│   ├── {{PROJECT_NAME}}.Infrastructure/
│   └── {{PROJECT_NAME}}.sln
├── frontend/               # Nuxt.js Frontend
│   ├── package.json
│   ├── nuxt.config.ts
│   └── ...
└── auto-setup.bat         # 🎯 Tek tıkla otomatik kurulum
```

## 🎯 Kullanım (Süper Kolay!)

### 1. Template'i Kullan
- GitHub'da **"Use this template"** butonuna tıkla
- Yeni repository adını belirle
- Repository'yi clone et

### 2. Tek Tıkla Kurulum ⚡
1. **auto-setup.bat** dosyasına **çift tıkla**
2. Proje adını gir (örnek: `MyAwesomeProject`)
3. Enter'a bas
4. **Bitir! 🎉**

Bu kadar basit! Hiç komut yazmanıza gerek yok.

## 🗄️ Veritabanı Kurulumu (PostgreSQL)

Proje **PostgreSQL** veritabanı kullanmaktadır. Aşağıdaki adımları takip edin:

### 1. PostgreSQL Kurulumu

PostgreSQL'i henüz kurmadıysanız [postgresql.org](https://www.postgresql.org/download/) adresinden indirip kurun.

> Kurulum sırasında belirlediğiniz **kullanıcı adı** ve **şifreyi** not edin. Varsayılan kullanıcı adı `postgres`'tir.

### 2. Connection String Ayarı

`backend/<PROJE_ADI>.API/appsettings.json` dosyasındaki bağlantı bilgilerini kendi ortamınıza göre düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=<PROJE_ADI>;Username=postgres;Password=postgres;Trust Server Certificate=true"
  }
}
```

| Parametre  | Açıklama                          | Varsayılan  |
|------------|-----------------------------------|-------------|
| `Host`     | PostgreSQL sunucu adresi          | `localhost` |
| `Port`     | PostgreSQL portu                  | `5432`      |
| `Database` | Veritabanı adı (otomatik oluşur) | Proje adı   |
| `Username` | PostgreSQL kullanıcı adı         | `postgres`  |
| `Password` | PostgreSQL şifresi               | `postgres`  |

### 3. Migration Oluşturma ve Veritabanını Güncelleme

İlk migration'ı oluşturup veritabanını yaratmak için backend klasöründe aşağıdaki komutları çalıştırın:

```bash
cd backend

# İlk migration'ı oluştur
dotnet ef migrations add InitialCreate --project <PROJE_ADI>.Infrastructure --startup-project <PROJE_ADI>.API

# Veritabanını oluştur ve migration'ı uygula
dotnet ef database update --project <PROJE_ADI>.Infrastructure --startup-project <PROJE_ADI>.API
```

> `dotnet ef` aracı yüklü değilse önce şu komutu çalıştırın:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

### 4. Seed Data (Otomatik)

Uygulama ilk çalıştığında veritabanı boşsa aşağıdaki veriler otomatik olarak oluşturulur:

| Veri           | Detay                                    |
|----------------|------------------------------------------|
| **Admin Kullanıcı** | `admin@<PROJE_ADI>.com` / `Admin123!` |
| **Roller**     | Admin rolü (tüm yetkilerle)              |
| **Yetkiler**   | Sistemdeki tüm izinler                   |
| **Tenant'lar** | Default Tenant ve Demo Tenant            |

> Seed data sadece veritabanı boşken çalışır, mevcut verileri etkilemez.

## 🛠 Geliştirme

Kurulum bittikten sonra:

### Backend (.NET Web API)
```bash
cd backend
dotnet restore
dotnet run
```

### Frontend (Nuxt.js)
```bash
cd frontend
npm install
npm run dev
```

## ✨ Özellikler

- **⚡ Tek tıkla kurulum** - auto-setup.bat ile 3 saniyede hazır
- **🏗️ Clean Architecture** yapısı
- **🔐 JWT Authentication** hazır
- **🗄️ Entity Framework** entegrasyonu
- **⚡ Nuxt.js 3** modern frontend
- **🔄 Otomatik dosya/klasör değiştirme**
- **🧹 Otomatik temizlik** (setup dosyası kendini siler)
- **❌ Komut satırı gerektirmez**

## 📝 Kurulum Sonrası Ne Olur?

- ✅ Tüm `{{PROJECT_NAME}}` placeholder'ları değişir
- ✅ Dosya ve klasör isimleri güncellenir
- ✅ Namespace'ler otomatik düzenlenir
- ✅ Package.json güncellenir
- ✅ Setup dosyası kendini siler
- ✅ Proje geliştirmeye hazır!

## 🎭 Örnek Projeler

Bu template ile oluşturulabilecek projeler:
- **E-ticaret siteleri**
- **Blog platformları**
- **CRM sistemleri**
- **API servisleri**
- **Admin panelleri**
- **SaaS uygulamaları**

## 🚨 Sistem Gereksinimleri

- Windows (auto-setup.bat için)
- .NET 8.0+
- Node.js 18+
- PostgreSQL 14+
- PowerShell (Windows'ta varsayılan)

## ❓ Sorun Giderme

**Setup çalışmıyor mu?**
- Klasör iznini kontrol edin
- Antivürüs programını geçici kapatın
- PowerShell ExecutionPolicy sorun çıkarabilir (normalde otomatik çözülür)

**Veritabanı bağlantı hatası mı alıyorsunuz?**
- PostgreSQL servisinin çalıştığından emin olun (`services.msc` veya `pg_isready` komutu ile kontrol edin)
- `appsettings.json` içindeki kullanıcı adı ve şifrenin doğru olduğunu kontrol edin
- PostgreSQL'in varsayılan port olan `5432`'de çalıştığını doğrulayın
- Firewall ayarlarının bağlantıyı engellemediğinden emin olun

**Migration hatası mı alıyorsunuz?**
- `dotnet ef` aracının yüklü olduğundan emin olun: `dotnet tool install --global dotnet-ef`
- Komutları `backend` klasöründen çalıştırdığınızdan emin olun
- `--project` ve `--startup-project` parametrelerinin doğru olduğunu kontrol edin

## 🤝 Katkıda Bulunma

Bu template'i geliştirmek için pull request gönderebilirsiniz!

## 📞 Destek

Sorun yaşıyorsanız issue açın, yardımcı olmaya çalışırız.

---

**🚀 Happy Coding! Artık proje kurmak çok kolay! ✨**