import type {
  LoginRequest,
  RegisterRequest,
  LoginResponse,
  User,
  Session,
} from "~/types";
import { API_ENDPOINTS } from "~/utils/apiEndpoints";
import { useRouter } from "nuxt/app";
import { useAuthStore } from "~/stores/auth";
import { useApi } from "./useApi";
import { useToast } from "./useToast";

export const useAuth = () => {
  const api = useApi();
  const authStore = useAuthStore();
  const router = useRouter();
  const toast = useToast();

  // Device ID oluştur
  const generateDeviceId = (): string => {
    // Client-side kontrolü
    if (process.client) {
      // Local storage'dan mevcut device ID'yi al veya yeni oluştur
      let deviceId = localStorage.getItem("deviceId");
      if (!deviceId) {
        deviceId =
          "device_" +
          Date.now() +
          "_" +
          Math.random().toString(36).substr(2, 9);
        localStorage.setItem("deviceId", deviceId);
      }
      return deviceId;
    }
    // Server-side için fallback
    return (
      "device_" + Date.now() + "_" + Math.random().toString(36).substr(2, 9)
    );
  };

  // Device name al
  const getDeviceName = (): string => {
    // Client-side kontrolü
    if (process.client) {
      const userAgent = navigator.userAgent;
      const platform = navigator.platform;

      if (userAgent.includes("Windows")) {
        return "Windows Device";
      } else if (userAgent.includes("Mac")) {
        return "Mac Device";
      } else if (userAgent.includes("Linux")) {
        return "Linux Device";
      } else if (userAgent.includes("Android")) {
        return "Android Device";
      } else if (userAgent.includes("iOS")) {
        return "iOS Device";
      } else {
        return "Unknown Device";
      }
    }
    // Server-side için fallback
    return "Unknown Device";
  };

  const login = async (credentials: LoginRequest) => {
    try {
      authStore.setLoading(true);

      // Device bilgilerini ekle
      const requestData = {
        ...credentials,
        deviceId: credentials.deviceId || generateDeviceId(),
        deviceName: credentials.deviceName || getDeviceName(),
      };

      const response = await api.post<LoginResponse>(
        API_ENDPOINTS.AUTH.LOGIN,
        requestData,
        { silent: true },
      );

      // Farklı response formatlarını destekle
      let loginData = null;

      if (response.isSuccess && response.value) {
        loginData = response.value;
      } else if (response.success && response.data) {
        loginData = response.data;
      } else if (response.accessToken) {
        loginData = response;
      } else if (response.value && response.value.accessToken) {
        loginData = response.value;
      } else if (response.data && response.data.accessToken) {
        loginData = response.data;
      }

      if (loginData && loginData.accessToken) {
        const deviceId = credentials.deviceId || generateDeviceId();
        await authStore.setAuth(loginData, deviceId);
        // Middleware will call fetchUserFromApi via initializeAuth on next route,
        // so we don't need to do it here.
        await router.push("/dashboard");
        return loginData;
      } else {
        throw new Error("Invalid login response format");
      }
    } catch (error) {
      console.error("Login error:", error);
      throw error;
    } finally {
      authStore.setLoading(false);
    }
  };

  const register = async (userData: RegisterRequest) => {
    try {
      authStore.setLoading(true);
      const response = await api.post<User>(
        API_ENDPOINTS.AUTH.REGISTER,
        userData,
      );

      if (response.success) {
        await router.push(
          `/auth/check-email?email=${encodeURIComponent(userData.email)}`,
        );
        return response.data;
      }
    } catch (error) {
      console.error("Registration error:", error);
      throw error;
    } finally {
      authStore.setLoading(false);
    }
  };

  const verifyEmail = async (email: string, token: string) => {
    return await api.post(
      API_ENDPOINTS.AUTH.VERIFY_EMAIL,
      { email, token },
      { silent: true },
    );
  };

  const resendVerification = async (email: string, tenantId: number = 0) => {
    return await api.post(
      API_ENDPOINTS.AUTH.RESEND_VERIFICATION,
      { email, tenantId },
      { silent: true },
    );
  };

  const logout = async () => {
    try {
      // Refresh token comes from HttpOnly cookie, no need to send it explicitly.
      // Backend reads the cookie, revokes the token, and clears the cookie.
      await api.post(API_ENDPOINTS.AUTH.LOGOUT, {});
    } catch (error) {
      console.error("Logout error:", error);
    } finally {
      authStore.clearAuth();
      await router.push("/");
    }
  };

  const refreshToken = async () => {
    try {
      const accessToken = authStore.accessToken;

      if (!accessToken) {
        throw new Error("No access token available");
      }

      const response = await api.post<LoginResponse>(
        API_ENDPOINTS.AUTH.REFRESH_TOKEN,
        {
          accessToken,
        },
      );

      // Farklı response formatlarını destekle (login ile aynı)
      const loginData =
        response.success && response.data
          ? response.data
          : response.value
            ? response.value
            : response.data?.accessToken
              ? response.data
              : null;

      if (loginData?.accessToken) {
        await authStore.setAuth(loginData);
        return loginData;
      }

      throw new Error("Invalid refresh response");
    } catch (error) {
      console.error("Token refresh error:", error);
      authStore.clearAuth();
      throw error;
    }
  };

  const getCurrentUser = async () => {
    try {
      const response = await api.get<User>(API_ENDPOINTS.AUTH.ME);
      if (response.success && response.data) {
        authStore.setUser(response.data);
        return response.data;
      }
    } catch (error) {
      console.error("Get current user error:", error);
      throw error;
    }
  };

  const hasPermission = (permission: string): boolean => {
    return authStore.permissions.includes(permission);
  };

  const hasRole = (role: string): boolean => {
    return authStore.roles.includes(role);
  };

  const hasAnyPermission = (permissions: string[]): boolean => {
    return permissions.some((permission) => hasPermission(permission));
  };

  const hasAllPermissions = (permissions: string[]): boolean => {
    return permissions.every((permission) => hasPermission(permission));
  };

  const hasSystemRole = (): boolean => {
    return authStore.user?.roles?.some((r) => r.isSystemRole) ?? false;
  };

  const getUserSessions = async (): Promise<Session[]> => {
    try {
      const response = await api.get<Session[]>(API_ENDPOINTS.AUTH.SESSIONS);
      // useApi.get returns response.data → ApiResponse shape: { success, data: [...] }
      const list =
        (response as any)?.data ?? (response as any)?.value ?? response ?? [];
      return Array.isArray(list) ? list : [];
    } catch (error) {
      console.error("Get sessions error:", error);
      return [];
    }
  };

  const logoutDevice = async (deviceId: string): Promise<void> => {
    try {
      await api.post(API_ENDPOINTS.AUTH.LOGOUT_DEVICE(deviceId), {});
    } catch (error) {
      console.error("Logout device error:", error);
      throw error;
    }
  };

  const revokeSessionById = async (sessionId: number): Promise<void> => {
    await api.post(API_ENDPOINTS.AUTH.REVOKE_SESSION_BY_ID(sessionId), {});
  };

  const changePassword = async (
    currentPassword: string,
    newPassword: string,
  ): Promise<void> => {
    await api.post(API_ENDPOINTS.AUTH.CHANGE_PASSWORD, {
      currentPassword,
      newPassword,
    });
    toast.success("Şifreniz başarıyla güncellendi");
  };

  const logoutAllDevices = async (): Promise<void> => {
    try {
      await api.post(API_ENDPOINTS.AUTH.LOGOUT_ALL, {});
      authStore.clearAuth();
      await router.push("/");
    } catch (error) {
      console.error("Logout all devices error:", error);
      throw error;
    }
  };

  return {
    login,
    register,
    logout,
    refreshToken,
    getCurrentUser,
    verifyEmail,
    resendVerification,
    hasPermission,
    hasRole,
    hasAnyPermission,
    hasAllPermissions,
    hasSystemRole,
    getUserSessions,
    logoutDevice,
    logoutAllDevices,
    revokeSessionById,
    changePassword,
  };
};
