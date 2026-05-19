import type { User } from './user'

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
  /**
   * @deprecated Backend artık refresh token'ı HttpOnly cookie'ye yazıyor.
   * Bu alan eski clientlar için geriye dönük uyumluluk amacıyla bırakılmıştır.
   * Frontend'de KULLANILMAMALI ve persist EDİLMEMELİDİR.
   */
  refreshToken?: string
  expiresAt: string
  user: User
}

/**
 * @deprecated Refresh token artık body'de gönderilmiyor — HttpOnly cookie ile gidiyor.
 * Bu tip backend kontratının güncellenmesi için referans olarak duruyor.
 */
export interface RefreshTokenRequest {
  accessToken?: string
  refreshToken?: string
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

/**
 * JWT payload — sadece güvenli kullanım için tipi belirtildi.
 * Bu sadece UX amaçlı (token expired mı?) — güvenlik kararı için backend imzayı doğrulamalı.
 */
export interface JwtPayload {
  exp: number
  iat?: number
  sub?: string
  email?: string
  [key: string]: unknown
}
