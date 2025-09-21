# Data.json Konfigürasyon Rehberi

Bu dokümantasyon, `public/data.json` dosyasının nasıl kullanılacağını ve yapılandırılacağını açıklar.

## Genel Bakış

`data.json` dosyası, uygulamanın tüm görsel ve metin öğelerini merkezi bir yerden yönetmenizi sağlar. Bu dosyayı değiştirerek:

- Login sayfasındaki arka plan fotoğrafları
- Uygulama logosu ve marka adı
- Tema renkleri ve gradyanlar
- Login sayfası metinleri
- Navigasyon stilleri

gibi öğeleri kolayca özelleştirebilirsiniz.

## Dosya Yapısı

### 1. App Bilgileri

```json
{
  "app": {
    "name": "{{PROJECT_NAME}}",
    "version": "1.0.0",
    "description": "Modern Authentication System",
    "logo": {
      "src": "/images/logo.svg",
      "alt": "{{PROJECT_NAME}} Logo",
      "width": "32",
      "height": "32"
    },
    "brand": {
      "text": "{{PROJECT_NAME}}",
      "tagline": "Secure Authentication System"
    }
  }
}
```

**Açıklama:**
- `name`: Uygulama adı
- `version`: Uygulama versiyonu
- `description`: Uygulama açıklaması
- `logo`: Logo bilgileri (src, alt, width, height)
- `brand`: Marka bilgileri (text, tagline)

### 2. Tema Renkleri

```json
{
  "theme": {
    "colors": {
      "primary": {
        "main": "#667eea",
        "light": "#8b9df0",
        "dark": "#4c63d2",
        "gradient": "linear-gradient(135deg, #667eea 0%, #764ba2 100%)"
      },
      "secondary": {
        "main": "#764ba2",
        "light": "#9b6bc4",
        "dark": "#5a3b7a"
      },
      "accent": {
        "main": "#82B1FF",
        "light": "#a8c8ff",
        "dark": "#5c8aff"
      },
      "success": "#4CAF50",
      "warning": "#FB8C00",
      "error": "#FF5252",
      "info": "#2196F3",
      "background": {
        "light": "#FAFAFA",
        "dark": "#121212"
      },
      "surface": {
        "light": "#FFFFFF",
        "dark": "#1E1E1E"
      },
      "text": {
        "primary": "#333333",
        "secondary": "#666666",
        "disabled": "#999999"
      }
    },
    "gradients": {
      "navbar": "linear-gradient(135deg, #ffffff 0%, #2563eb 100%)",
      "login": "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
      "button": "linear-gradient(135deg, #667eea 0%, #764ba2 100%)"
    }
  }
}
```

**Açıklama:**
- `colors`: Ana renk paleti (primary, secondary, accent, vb.)
- `gradients`: Gradyan tanımları (navbar, login, button)

### 3. Login Sayfası Konfigürasyonu

```json
{
  "login": {
    "backgroundImages": [
      "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?ixlib=rb-4.0.3&auto=format&fit=crop&w=2070&q=80",
      "https://images.unsplash.com/photo-1557804506-669a67965ba0?ixlib=rb-4.0.3&auto=format&fit=crop&w=2074&q=80"
    ],
    "rotationInterval": 3000,
    "overlay": {
      "opacity": 0.4,
      "color": "rgba(0, 0, 0, 0.4)"
    },
    "card": {
      "background": "rgba(255, 255, 255, 0.1)",
      "backdropFilter": "blur(20px)",
      "borderRadius": "20px",
      "border": "1px solid rgba(255, 255, 255, 0.2)",
      "shadow": "0 25px 45px rgba(0, 0, 0, 0.2)"
    },
    "texts": {
      "welcome": "Welcome Back",
      "subtitle": "Sign in to your account",
      "emailLabel": "Email Address",
      "passwordLabel": "Password",
      "rememberMe": "Remember me",
      "forgotPassword": "Forgot password?",
      "signIn": "Sign In",
      "divider": "or",
      "noAccount": "Don't have an account?",
      "createAccount": "Create Account"
    }
  }
}
```

**Açıklama:**
- `backgroundImages`: Arka plan fotoğrafları dizisi
- `rotationInterval`: Fotoğraf değişim süresi (milisaniye)
- `overlay`: Arka plan overlay ayarları
- `card`: Login kartı stilleri
- `texts`: Login sayfası metinleri

### 4. Navigasyon Konfigürasyonu

```json
{
  "navigation": {
    "sidebar": {
      "width": "256px",
      "background": "#ffffff",
      "shadow": "0 4px 6px -1px rgba(0, 0, 0, 0.1)",
      "borderColor": "#e5e7eb"
    },
    "navbar": {
      "height": "64px",
      "background": "linear-gradient(135deg, #ffffff 0%, #2563eb 100%)",
      "shadow": "0 1px 3px rgba(0, 0, 0, 0.1)"
    }
  }
}
```

### 5. UI Konfigürasyonu

```json
{
  "ui": {
    "borderRadius": {
      "small": "8px",
      "medium": "12px",
      "large": "20px"
    },
    "shadows": {
      "small": "0 2px 4px rgba(0, 0, 0, 0.1)",
      "medium": "0 4px 8px rgba(0, 0, 0, 0.1)",
      "large": "0 8px 16px rgba(0, 0, 0, 0.1)"
    },
    "transitions": {
      "fast": "0.2s ease",
      "medium": "0.3s ease",
      "slow": "0.5s ease"
    }
  }
}
```

## Kullanım Örnekleri

### 1. Logo Değiştirme

```json
{
  "app": {
    "logo": {
      "src": "/images/new-logo.svg",
      "alt": "Yeni Logo",
      "width": "40",
      "height": "40"
    }
  }
}
```

### 2. Tema Rengi Değiştirme

```json
{
  "theme": {
    "colors": {
      "primary": {
        "main": "#FF6B6B",
        "light": "#FF8E8E",
        "dark": "#E55555"
      }
    }
  }
}
```

### 3. Login Arka Plan Fotoğrafları Ekleme

```json
{
  "login": {
    "backgroundImages": [
      "https://your-domain.com/image1.jpg",
      "https://your-domain.com/image2.jpg",
      "https://your-domain.com/image3.jpg"
    ]
  }
}
```

### 4. Metinleri Türkçe Yapma

```json
{
  "login": {
    "texts": {
      "welcome": "Hoş Geldiniz",
      "subtitle": "Hesabınıza giriş yapın",
      "emailLabel": "E-posta Adresi",
      "passwordLabel": "Şifre",
      "rememberMe": "Beni hatırla",
      "forgotPassword": "Şifremi unuttum?",
      "signIn": "Giriş Yap",
      "divider": "veya",
      "noAccount": "Hesabınız yok mu?",
      "createAccount": "Hesap Oluştur"
    }
  }
}
```

## Teknik Detaylar

### Composables Kullanımı

```typescript
// Herhangi bir component'te
const { loadAppData, getAppInfo, getTheme, getLoginConfig } = useAppData()

// Verileri yükle
await loadAppData()

// Verilere erişim
const appName = getAppInfo.value?.name
const primaryColor = getTheme.value?.colors.primary.main
const backgroundImages = getLoginConfig.value?.backgroundImages
```

### Otomatik Yükleme

Uygulama başlatıldığında `data.json` dosyası otomatik olarak yüklenir ve:

1. Layout'taki logo ve marka adı güncellenir
2. Vuetify tema renkleri ayarlanır
3. Login sayfası konfigürasyonu uygulanır

### Fallback Değerler

Eğer `data.json` dosyası yüklenemezse veya belirli alanlar eksikse, uygulama varsayılan değerleri kullanır.

## Öneriler

1. **Logo Dosyaları**: Logo dosyalarını `public/images/` klasörüne yerleştirin
2. **Resim Optimizasyonu**: Arka plan fotoğrafları için optimize edilmiş resimler kullanın
3. **Renk Uyumu**: Tema renklerini seçerken uyumlu renk paletleri kullanın
4. **Yedekleme**: `data.json` dosyasının yedeğini alın
5. **Test**: Değişiklikleri yapmadan önce test ortamında deneyin

## Sorun Giderme

### Dosya Yüklenmiyor
- `data.json` dosyasının `public/` klasöründe olduğundan emin olun
- JSON formatının geçerli olduğunu kontrol edin
- Tarayıcı konsolunda hata mesajlarını kontrol edin

### Değişiklikler Görünmüyor
- Tarayıcı önbelleğini temizleyin
- Uygulamayı yeniden başlatın
- `data.json` dosyasının doğru konumda olduğunu kontrol edin

### Performans Sorunları
- Arka plan fotoğraflarını optimize edin
- Çok fazla fotoğraf kullanmaktan kaçının
- CDN kullanmayı düşünün
