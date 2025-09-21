// API Endpoints configuration for Swagger API
export const API_ENDPOINTS = {
  // Authentication endpoints
  AUTH: {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
    LOGOUT: '/api/auth/logout',
    REFRESH_TOKEN: '/api/auth/refresh-token',
    ME: '/api/auth/me',
    FORGOT_PASSWORD: '/api/auth/forgot-password',
    RESET_PASSWORD: '/api/auth/reset-password',
    CONFIRM_EMAIL: '/api/auth/confirm-email',
    RESEND_CONFIRMATION: '/api/auth/resend-confirmation'
  },

  // User management endpoints
  USERS: {
    LIST: '/api/users',
    CREATE: '/api/users',
    GET_BY_ID: (id: string) => `/api/users/${id}`,
    UPDATE: (id: string) => `/api/users/${id}`,
    DELETE: (id: string) => `/api/users/${id}`,
    CHANGE_PASSWORD: (id: string) => `/api/users/${id}/change-password`,
    UPDATE_PROFILE: '/api/users/profile',
    UPLOAD_AVATAR: '/api/users/avatar'
  },

  // Role management endpoints
  ROLES: {
    LIST: '/api/roles',
    CREATE: '/api/roles',
    GET_BY_ID: (id: string) => `/api/roles/${id}`,
    UPDATE: (id: string) => `/api/roles/${id}`,
    DELETE: (id: string) => `/api/roles/${id}`,
    ASSIGN_PERMISSIONS: (id: string) => `/api/roles/${id}/permissions`
  },

  // Permission management endpoints
  PERMISSIONS: {
    LIST: '/api/permissions',
    GET_BY_ID: (id: string) => `/api/permissions/${id}`,
    CREATE: '/api/permissions',
    UPDATE: (id: string) => `/api/permissions/${id}`,
    DELETE: (id: string) => `/api/permissions/${id}`
  },

  // Dashboard endpoints
  DASHBOARD: {
    STATS: '/api/dashboard/stats',
    RECENT_ACTIVITIES: '/api/dashboard/recent-activities',
    USER_GROWTH: '/api/dashboard/user-growth'
  },

  // File upload endpoints
  FILES: {
    UPLOAD: '/api/files/upload',
    DELETE: (id: string) => `/api/files/${id}`
  }
}

// Helper function to build full API URLs
export const buildApiUrl = (endpoint: string): string => {
  const config = useRuntimeConfig()
  return `${config.public.apiBase}${endpoint}`
}

// Helper function to get paginated endpoint
export const getPaginatedEndpoint = (baseEndpoint: string, page: number = 1, pageSize: number = 10, search?: string): string => {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString()
  })
  
  if (search) {
    params.append('search', search)
  }
  
  return `${baseEndpoint}?${params.toString()}`
} 