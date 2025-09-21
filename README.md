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
- PowerShell (Windows'ta varsayılan)

## ❓ Sorun Giderme

**Setup çalışmıyor mu?**
- Klasör iznini kontrol edin
- Antivürüs programını geçici kapatın
- PowerShell ExecutionPolicy sorun çıkarabilir (normalde otomatik çözülür)

## 🤝 Katkıda Bulunma

Bu template'i geliştirmek için pull request gönderebilirsiniz!

## 📞 Destek

Sorun yaşıyorsanız issue açın, yardımcı olmaya çalışırız.

---

**🚀 Happy Coding! Artık proje kurmak çok kolay! ✨**