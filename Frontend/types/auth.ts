export interface LoginRequest {
  email: string
  password: string
  rememberMe?: boolean
  deviceId?: string
  deviceName?: string
}

export interface RegisterRequest {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
  phoneNumber?: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: any
}

export interface RefreshTokenRequest {
  accessToken: string
  refreshToken: string
}
