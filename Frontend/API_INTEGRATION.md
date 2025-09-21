# API Integration with Swagger API

Bu proje artık `https://localhost:44346/swagger/index.html` adresindeki Swagger API ile entegre edilmiştir.

## Yapılan Değişiklikler

### 1. API Base URL Güncellendi
- **Önceki**: `https://localhost:7001/api/v1`
- **Yeni**: `https://localhost:44346`

### 2. API Endpoints Yapılandırması
Yeni `utils/apiEndpoints.ts` dosyası oluşturuldu ve tüm API endpoint'leri merkezi olarak yönetiliyor:

```typescript
export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
    LOGOUT: '/api/auth/logout',
    REFRESH_TOKEN: '/api/auth/refresh-token',
    ME: '/api/auth/me',
    // ... diğer auth endpoint'leri
  },
  USERS: {
    LIST: '/api/users',
    CREATE: '/api/users',
    GET_BY_ID: (id: string) => `/api/users/${id}`,
    // ... diğer user endpoint'leri
  },
  // ... diğer endpoint grupları
}
```

### 3. Güncellenen Composable'lar
Aşağıdaki composable'lar yeni API endpoint'lerini kullanacak şekilde güncellendi:

- `useAuth.ts` - Kimlik doğrulama işlemleri
- `useUsers.ts` - Kullanıcı yönetimi
- `useRoles.ts` - Rol yönetimi
- `usePermissions.ts` - İzin yönetimi

### 4. Axios Yapılandırması
- Timeout süresi 30 saniyeye çıkarıldı
- Accept header'ı eklendi
- Swagger API ile uyumlu hale getirildi

## Kullanım

### 1. API Test Sayfası
API entegrasyonunu test etmek için `/api-test` sayfasını kullanabilirsiniz:

```bash
npm run dev
# Tarayıcıda http://localhost:3000/api-test adresine gidin
```

### 2. Environment Variables
Eğer farklı bir API URL kullanmak istiyorsanız, `.env` dosyası oluşturun:

```env
API_BASE_URL=https://your-api-url.com
```

### 3. API Endpoint Kullanımı
Composable'larda API endpoint'lerini kullanırken:

```typescript
import { API_ENDPOINTS } from '~/utils/apiEndpoints'

// Kullanıcıları listele
const users = await api.get(API_ENDPOINTS.USERS.LIST)

// Belirli bir kullanıcıyı getir
const user = await api.get(API_ENDPOINTS.USERS.GET_BY_ID('user-id'))

// Yeni kullanıcı oluştur
const newUser = await api.post(API_ENDPOINTS.USERS.CREATE, userData)
```

### 4. Pagination Desteği
Sayfalama için yardımcı fonksiyon:

```typescript
import { getPaginatedEndpoint } from '~/utils/apiEndpoints'

const endpoint = getPaginatedEndpoint(
  API_ENDPOINTS.USERS.LIST, 
  page, 
  pageSize, 
  searchTerm
)
const users = await api.get(endpoint)
```

## Swagger API Uyumluluğu

### Beklenen Response Format
API'den beklenen response formatı:

```typescript
interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  error?: string
  errors?: string[]
}
```

### Authentication
- Bearer token authentication desteklenir
- Token'lar otomatik olarak request header'larına eklenir
- 401 hatalarında otomatik logout işlemi yapılır

### Error Handling
- API hataları console'a loglanır
- Validation hataları kullanıcı dostu mesajlar olarak gösterilir
- Network hataları uygun şekilde handle edilir

## Geliştirme Notları

### 1. Yeni Endpoint Ekleme
Yeni bir endpoint eklemek için:

1. `utils/apiEndpoints.ts` dosyasına endpoint'i ekleyin
2. İlgili composable'ı güncelleyin
3. Gerekirse type tanımlarını güncelleyin

### 2. API Response Mapping
Eğer Swagger API'nizin response formatı farklıysa, `composables/useApi.ts` dosyasındaki response interceptor'ını güncelleyin.

### 3. CORS Ayarları
Swagger API'nizde CORS ayarlarının doğru yapılandırıldığından emin olun:

```csharp
// Swagger API tarafında
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
```

## Sorun Giderme

### 1. CORS Hatası
- Swagger API'nizin CORS ayarlarını kontrol edin
- API'nin çalışır durumda olduğundan emin olun

### 2. 401 Unauthorized
- Token'ların doğru şekilde gönderildiğini kontrol edin
- Token'ların geçerli olduğundan emin olun

### 3. Timeout Hatası
- API'nin yavaş yanıt verdiği durumlarda timeout süresini artırabilirsiniz
- `plugins/axios.client.ts` dosyasında timeout değerini güncelleyin

### 4. Endpoint Bulunamadı
- Swagger UI'da endpoint'in mevcut olduğunu kontrol edin
- `utils/apiEndpoints.ts` dosyasında endpoint path'ini doğrulayın

## Test

API entegrasyonunu test etmek için:

1. Swagger API'nizin çalıştığından emin olun
2. Uygulamayı başlatın: `npm run dev`
3. `/api-test` sayfasına gidin
4. "Test Connection" butonuna tıklayın
5. Test sonuçlarını kontrol edin

Başarılı bir test sonucunda tüm endpoint'lerin erişilebilir olduğunu göreceksiniz. 