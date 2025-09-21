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
├── setup.html             # 🎨 Web tabanlı kurulum arayüzü
└── run-setup.ps1          # ⚙️ PowerShell kurulum scripti
```

## 🎯 Kullanım

### 1. Template'i Kullan
- GitHub'da **"Use this template"** butonuna tıkla
- Yeni repository adını belirle
- Repository'yi clone et

### 2. Web Arayüzü ile Kurulum
1. **setup.html** dosyasını tarayıcıda aç
2. Proje adını gir (örnek: `MyAwesomeProject`)
3. **"Projeyi Kur"** butonuna tıkla
4. Çıkan PowerShell komutunu kopyala
5. PowerShell'de komutu çalıştır

### 3. Manuel Kurulum (Alternatif)
```powershell
PowerShell -ExecutionPolicy Bypass -Command "& { $PROJECT_NAME='YourProjectName'; . .\run-setup.ps1 }"
```

## 🛠 Geliştirme

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

- **🎨 Web tabanlı kurulum arayüzü** - Kullanıcı dostu setup
- **🏗️ Clean Architecture** yapısı
- **🔐 JWT Authentication** hazır
- **🗄️ Entity Framework** entegrasyonu
- **⚡ Nuxt.js 3** modern frontend
- **🔄 Otomatik dosya/klasör değiştirme**
- **🧹 Otomatik temizlik** (setup dosyaları silinir)

## 📝 Kurulum Sonrası

- Tüm `{{PROJECT_NAME}}` placeholder'ları otomatik değişir
- Hem dosya isimleri hem içerikler güncellenir
- Setup dosyaları otomatik silinir
- Proje geliştirmeye hazır halde olur

## 🎭 Örnek Projeler

Bu template ile oluşturulabilecek projeler:
- **E-ticaret siteleri**
- **Blog platformları**
- **CRM sistemleri**
- **API servisleri**
- **Admin panelleri**

## 🤝 Katkıda Bulunma

Bu template'i geliştirmek için pull request gönderebilirsiniz!

## 📞 Destek

Sorun yaşıyorsanız issue açın, yardımcı olmaya çalışırız.

---

**🚀 Happy Coding! ✨**