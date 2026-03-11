# Tenant Bazlı Özelleştirme Rehberi

Subdomain (`acme.uygulama.com` → `acme`) veya URL param (`?tenant=acme`) ile tenant'a göre tema, layout ve login sayfası özelleştirmesi.

---

## Hızlı Başlangıç

### Yeni tenant ekleme (sadece data.json)

1. `public/data/sirket-x.json` oluştur
2. `data.json` yapısını kopyala
3. Renkler, logo, metinler, görselleri değiştir
4. Test: `?tenant=sirket-x` veya `sirket-x.uygulama.com`

Bu kadar. Login ve register sayfaları otomatik yeni veriyi kullanır.

---

## Tenant ID Kaynağı (Öncelik Sırası)

1. **URL parametresi**: `?tenant=acme` (test için)
2. **Subdomain**: `acme.uygulama.com` → `acme`
3. **User.tenantId**: Login sonrası (backend entegrasyonunda)
4. **localStorage / Cookie**: Kalıcılık (URL param ile gelen tenant otomatik kaydedilir)

---

## Dosya Yapısı

```
public/
  data.json              → Varsayılan (tenant yoksa)
  data/
    acme.json            → acme tenant
    sirket-x.json        → sirket-x tenant
```

---

## Özelleştirme Seviyeleri

### 1. Sadece data.json (çoğu tenant)

- Tema renkleri
- Logo, marka adı
- Login/register metinleri
- Arka plan görselleri
- Kart stilleri

**Yapılacak:** `public/data/tenant-id.json` oluştur.

---

### 2. Özel layout

Tenant'a özel sidebar/header tasarımı.

**Yapılacak:**

1. `layouts/tenant-sirket-x.vue` oluştur
2. `public/data/sirket-x.json` içinde:
   ```json
   "tenant": {
     "layout": "tenant-sirket-x"
   }
   ```

---

### 3. Özel login sayfası

Tamamen farklı login tasarımı (örn. split layout).

**Yapılacak:**

1. `components/Auth/LoginSirketX.vue` oluştur
2. `public/data/sirket-x.json` içinde:
   ```json
   "tenant": {
     "loginPage": "sirket-x"
   }
   ```
3. Component adı: `Login` + PascalCase (`sirket-x` → `LoginSirketX`)

**Referans:** `components/Auth/LoginAcme.vue`

---

## Geliştirme Ortamında Test

Localhost'ta subdomain yok. İki yöntem:

### URL parametresi

```
http://localhost:3000/?tenant=acme
```

### hosts dosyası

```
# Windows: C:\Windows\System32\drivers\etc\hosts
127.0.0.1 acme.localhost
```

Sonra: `http://acme.localhost:3000`

---

## Composables

### useTenant

```typescript
const { tenantId, resolveTenantId, getDataPath, setTenantId } = useTenant()
```

### useAppData

```typescript
const { loadAppData, getLoginConfig, getTheme } = useAppData()
```

Tenant'a göre otomatik doğru `data.json` yüklenir.

---

## Özet Tablo

| İhtiyaç              | Çözüm                                      |
|----------------------|--------------------------------------------|
| Renk, logo, metin    | `public/data/tenant-id.json`               |
| Özel layout          | `tenant.layout` + `layouts/tenant-*.vue`  |
| Özel login tasarımı  | `tenant.loginPage` + `Auth/Login*.vue`     |
