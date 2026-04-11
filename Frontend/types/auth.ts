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

export interface Session {
  id: number
  token: string
  createdDate: string
  expiryDate: string
  isActive: boolean
  ipAddress: string
  userAgent: string
  deviceId: string
  deviceName: string
  deviceType: string
  location: string
  remainingTime: string
  isCurrentSession: boolean
}
