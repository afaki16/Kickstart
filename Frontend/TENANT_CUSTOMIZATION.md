# Tenant Bazlı Özelleştirme Rehberi

Subdomain tabanlı tenant sistemi: `acme.uygulama.com` → tema, layout, login sayfası otomatik.

---

## Öncelik Sırası (Tenant Çözümleme)

1. **Subdomain**: `acme.uygulama.com` → `acme`
2. **User (JWT/API)**: Login sonrası `user.tenantDomain` veya `user.tenantId`
3. **Cookie / localStorage**: Kalıcılık (login'de otomatik kaydedilir)
4. **URL param**: Sadece localhost geliştirme için `?tenant=acme`

---

## Dosya Yapısı

```
public/
  data.json              → Varsayılan (tenant yoksa veya domain="default")
  data/
    acme.json            → acme.uygulama.com
    demo.json            → demo.uygulama.com
```

---

## Backend (Tenant.Domain)

| Tablo | Alan | Açıklama |
|-------|------|----------|
| Tenant | Domain | Subdomain eşlemesi için (acme, demo, default) |
| User | TenantId | Kullanıcının tenant'ı |
| JWT | tenant_id claim | Her istekte tenant bilgisi |

**Register**: Frontend subdomain'den `tenantDomain` alır, backend'e gönderir. Backend `Tenant.Domain` ile eşleştirip `User.TenantId` atar.

---

## Yeni Tenant Ekleme

### 1. Backend (DB)
- Tenant tablosuna kayıt: `Domain = "sirket-x"`
- Seed veya admin panelinden eklenebilir

### 2. Frontend (data.json)
- `public/data/sirket-x.json` oluştur
- `data.json` yapısını kopyala, renkleri/metinleri değiştir

### 3. DNS
- `sirket-x.uygulama.com` → uygulama sunucusuna yönlendir

---

## Özelleştirme Seviyeleri

### Sadece data.json (çoğu tenant)
Tema, logo, metinler, görseller. `public/data/{domain}.json` yeterli.

### Özel layout
`data.json` → `"tenant": { "layout": "tenant-sirket-x" }`  
`layouts/tenant-sirket-x.vue` oluştur.

### Özel login sayfası
`data.json` → `"tenant": { "loginPage": "sirket-x" }`  
`components/Auth/LoginSirketX.vue` oluştur.

---

## Geliştirme (localhost)

Subdomain yok. İki yöntem:

**1. hosts dosyası**
```
127.0.0.1 acme.localhost
```
→ `http://acme.localhost:3000`

**2. URL param** (sadece localhost)
```
http://localhost:3000/?tenant=acme
```

---

## Composables

```typescript
const { resolveTenantId, getDataPath, setTenantId } = useTenant()
const { loadAppData } = useAppData()
```
