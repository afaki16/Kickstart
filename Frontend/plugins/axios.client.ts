import axios, { type InternalAxiosRequestConfig } from "axios";
import { API_ENDPOINTS } from "~/utils/apiEndpoints";

/**
 * Axios plugin — güvenlik notları:
 *
 * 1) Access token COOKIE'DEN değil, Pinia STORE'DAN okunur (memory).
 *    Cookie'de access token tutmuyoruz — XSS koruması.
 *
 * 2) withCredentials=true TÜM isteklerde:
 *    - Refresh-token endpoint için HttpOnly refresh_token cookie'sini taşır
 *    - Diğer endpoint'ler bu cookie'yi sadece /api/auth path'inde alır
 *      (Path scoping backend tarafında ayarlandı)
 *
 * 3) 401 olduğunda otomatik silent refresh denenir. Başarısızsa
 *    kullanıcı login sayfasına yönlendirilir.
 *
 * 4) _skipAuthRefresh flag'i: silentRefresh çağrısı kendisi refresh endpoint'ini
 *    çağırıyor, bu istek 401 dönerse tekrar refresh atmaya çalışma (sonsuz döngü önleme).
 */

// Tip extension — interceptor için custom flag'lar
declare module "axios" {
  export interface InternalAxiosRequestConfig {
    _retry?: boolean;
    _skipAuthRefresh?: boolean;
  }
}

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig();
  const router = useRouter();
  const authStore = useAuthStore();

  const api = axios.create({
    baseURL: config.public.apiBase,
    timeout: 30000,
    withCredentials: true,
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
    },
  });

  // Refresh işlemi sırasında bekleyen istekler için kuyruk
  let isRefreshing = false;
  let failedQueue: Array<{
    resolve: (value?: unknown) => void;
    reject: (reason?: unknown) => void;
    config: InternalAxiosRequestConfig;
  }> = [];

  const processQueue = (err: Error | null, token: string | null = null) => {
    failedQueue.forEach(({ resolve, reject, config }) => {
      if (err) {
        reject(err);
      } else if (config && token) {
        config.headers.Authorization = `Bearer ${token}`;
        config._retry = true;
        resolve(api(config));
      }
    });
    failedQueue = [];
  };

  // Request interceptor — token'ı STORE'DAN al
  api.interceptors.request.use(
    (reqConfig) => {
      // Access token sadece memory'de; cookie'den okumayız
      const token = authStore.accessToken;
      if (token) {
        reqConfig.headers.Authorization = `Bearer ${token}`;
      }
      return reqConfig;
    },
    (error) => Promise.reject(error),
  );

  // Response interceptor — 401'de silent refresh
  api.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config as
        | InternalAxiosRequestConfig
        | undefined;

      if (error.response?.status !== 401 || !originalRequest) {
        return Promise.reject(error);
      }

      // silentRefresh kendisi refresh çağırıyor; sonsuz döngüyü önle
      if (originalRequest._skipAuthRefresh) {
        return Promise.reject(error);
      }

      // Refresh endpoint'i 401 dönerse — refresh token geçersiz, oturum bitti
      if (originalRequest.url?.includes("/auth/refresh-token")) {
        authStore.clearAuth();
        await router.push("/?expired=true");
        return Promise.reject(error);
      }

      // Zaten retry edilmiş bir istek tekrar 401 dönerse vazgeç
      if (originalRequest._retry) {
        authStore.clearAuth();
        await router.push("/?expired=true");
        return Promise.reject(error);
      }

      // Başka istek zaten refresh yapıyorsa kuyruğa al
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject, config: originalRequest });
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Refresh token HttpOnly cookie'de — body boş gidiyor
        // Doğrudan axios kullanıyoruz (kendi instance'ımız değil) çünkü
        // bu çağrının kendi response interceptor'una takılmasını istemiyoruz.
        const response = await axios.post(
          `${config.public.apiBase}${API_ENDPOINTS.AUTH.REFRESH_TOKEN}`,
          {},
          {
            headers: { "Content-Type": "application/json" },
            withCredentials: true,
          },
        );

        const data =
          response.data?.data ?? response.data?.value ?? response.data;

        if (!data?.accessToken) {
          throw new Error("Invalid refresh response");
        }

        // Yeni token'ı store'a yaz
        await authStore.setAuth(data);

        // Kuyruğu işle
        processQueue(null, data.accessToken);

        // Orijinal isteği yeni token ile tekrar dene
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError as Error, null);
        authStore.clearAuth();
        await router.push("/?expired=true");
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    },
  );

  return {
    provide: {
      api,
    },
  };
});
