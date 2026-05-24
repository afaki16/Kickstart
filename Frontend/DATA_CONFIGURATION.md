# Data.json Konfigürasyon Rehberi

`public/data.json` uygulamanın görsel kimliğini merkezi olarak yönetir.

## Yapı

```json
{
  "tenant": { "loginPage": "default" },
  "app": {
    "logo": { "src": "/images/logo.svg", "alt": "Kickstart Logo" },
    "brand": { "text": "Kickstart" }
  },
  "theme": {
    "colors": {
      "primary": "linear-gradient(180deg, #14532d 0%, #166534 50%, #064e3b 100%)",
      "accent": "#82B1FF",
      "success": "#4CAF50",
      "warning": "#FB8C00",
      "error": "#FF5252",
      "info": "#2196F3"
    }
  },
  "login":    { "backgroundImages": [...], "rotationInterval": 3000 },
  "register": {
    "backgroundImages": [...],
    "rotationInterval": 3000,
    "overlay": { "color": "rgba(0,0,0,0.4)" },
    "card":    { "background": "...", "backdropFilter": "...", "borderRadius": "...", "border": "...", "shadow": "..." }
  }
}
```

## Tema Renkleri (en önemli kısım)

`theme.colors.primary` tek bir gradient string'idir. Bu gradient'ten otomatik olarak şu CSS değişkenleri türetilir ve `:root`'a inject edilir:

| CSS Değişkeni | Kaynak |
|---|---|
| `--theme-primary` | Gradient'in 2. rengi (main) |
| `--theme-primary-light` | Gradient'in 3. rengi |
| `--theme-primary-dark` | Gradient'in 1. rengi |
| `--theme-primary-rgb` | Main rengin RGB karşılığı (rgba() için) |
| `--theme-primary-dark-rgb` | Dark rengin RGB karşılığı |
| `--theme-gradient` | `linear-gradient(135deg, dark, main)` |
| `--theme-gradient-hover` | `linear-gradient(135deg, dark, light)` |
| `--theme-gradient-sidebar` | Orijinal `primary` string'i |
| `--theme-gradient-text` | `linear-gradient(135deg, #1f2937, dark)` |
| `--theme-accent`, `--theme-success`, `--theme-warning`, `--theme-error`, `--theme-info` | Doğrudan hex |

### Kullanım örneği

```css
.my-button {
  background: var(--theme-gradient);
  border: 1px solid rgba(var(--theme-primary-rgb), 0.3);
}
```

## Temayı değiştirmek

`theme.colors.primary` gradient'ini değiştirmek yeterlidir. Örnekler:

```json
"primary": "linear-gradient(180deg, #14532d 0%, #166534 50%, #064e3b 100%)"  // yeşil
"primary": "linear-gradient(180deg, #4338ca 0%, #2563eb 50%, #3b82f6 100%)"  // mavi
"primary": "linear-gradient(180deg, #E55555 0%, #FF6B6B 50%, #FF8E8E 100%)"  // kırmızı
```

## Logo ve Marka

- `app.logo.src` — logo görseli (sidebar, login, splash ekranları).
- `app.logo.alt` — logo görselinin alt metni (erişilebilirlik). Yüklenemezse `"Logo"` fallback'i kullanılır.
- `app.brand.text` — sidebar marka adı. Yoksa `"Kickstart"` fallback'i kullanılır.

## Login/Register arka planları

`login.backgroundImages` ve `register.backgroundImages` array'ine URL ekle. `rotationInterval` (ms) kadar sürede bir görsel değişir.

## Metinler

Tüm UI metinleri `i18n` üzerinden yönetilir (`assets/i18n/` klasörü). `data.json` artık metin barındırmaz.

## Yüklenme

Uygulama açılırken `useAppData.loadAppData()` ile fetch edilir. SSR-safe (`useState` ile izole). İlk çağrıdan sonra cache'lenir; `forceReload` ile yenilenebilir.
