// API Response Types
export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message?: string
  error?: string
  errors?: string[]
}

// Auth Types
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
  user: User
}

export interface RefreshTokenRequest {
  accessToken: string
  refreshToken: string
}

// User Types
export interface User {
  id: string
  firstName: string
  lastName: string
  fullName: string
  email: string
  phoneNumber?: string
  status: UserStatus
  lastLoginDate?: string
  emailConfirmed: boolean
  phoneConfirmed: boolean
  profileImageUrl?: string
  createdDate: string
  roles: Role[]
  permissions: Permission[]
}

export interface CreateUserRequest {
  firstName: string
  lastName: string
  email: string
  password: string
  phoneNumber?: string
  status: UserStatus
  roleIds: string[]
}

export interface UpdateUserRequest {
  id: string
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  status: UserStatus
  profileImageUrl?: string
  roleIds: string[]
}

export enum UserStatus {
  Active = 1,
  Inactive = 2,
  Banned = 3,
  PendingVerification = 4
}

// Role Types
export interface Role {
  id: string
  name: string
  description?: string
  isSystemRole: boolean
  createdDate: string
  permissions: Permission[]
}

export interface CreateRoleRequest {
  name: string
  description?: string
  permissionIds: string[]
}

export interface UpdateRoleRequest {
  id: string
  name: string
  description?: string
  permissionIds: string[]
}

// Permission Types
export interface Permission {
  id: string
  name: string
  displayName?: string
  description?: string
  resource: string
  action?: string
  type?: PermissionType
  fullPermission?: string
  isActive?: boolean
  assignedCount?: number
  createdDate?: string
}

export enum PermissionType {
  Create = 1,
  Read = 2,
  Update = 3,
  Delete = 4,
  Manage = 5
}

// UI Types
export interface NavigationItem {
  title: string
  icon: string
  to?: string
  children?: NavigationItem[]
  permission?: string
  roles?: string[]
}

export interface BreadcrumbItem {
  title: string
  to?: string
  disabled?: boolean
}

export interface TableHeader {
  title: string
  key: string
  sortable?: boolean
  align?: 'start' | 'center' | 'end'
  width?: string | number
}

export interface DataTableOptions {
  page: number
  itemsPerPage: number
  sortBy: string[]
  search?: string
}

// Form Types
export interface FormField {
  name: string
  label: string
  type: 'text' | 'email' | 'password' | 'select' | 'textarea' | 'checkbox' | 'switch'
  placeholder?: string
  required?: boolean
  rules?: string[]
  options?: { title: string; value: any }[]
  multiple?: boolean
  disabled?: boolean
  hint?: string
}

// Error Types
export interface ValidationError {
  field: string
  message: string
}

export interface ApiError {
  message: string
  errors?: ValidationError[]
  statusCode?: number
}

// Store Types
export interface AuthState {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
  permissions: string[]
  roles: string[]
}

export interface UserState {
  users: User[]
  currentUser: User | null
  totalUsers: number
  isLoading: boolean
  searchTerm: string
  currentPage: number
  itemsPerPage: number
}

export interface RoleState {
  roles: Role[]
  currentRole: Role | null
  isLoading: boolean
}

export interface PermissionState {
  permissions: Permission[]
  isLoading: boolean
}

// App Configuration
export interface AppConfig {
  apiBase: string
  appName: string
  appVersion: string
}

// Component Props
export interface BaseDialogProps {
  modelValue: boolean
  title?: string
  maxWidth?: string | number
  persistent?: boolean
}

export interface ConfirmDialogProps extends BaseDialogProps {
  message: string
  confirmText?: string
  cancelText?: string
  color?: string
}

export interface UserDialogProps extends BaseDialogProps {
  user?: User | null
  mode: 'create' | 'edit' | 'view'
}

export interface RoleDialogProps extends BaseDialogProps {
  role?: Role | null
  mode: 'create' | 'edit' | 'view'
}

// Utility Types
export type Nullable<T> = T | null
export type Optional<T> = T | undefined
export type DeepPartial<T> = {
  [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P]
}

// Menu & Dashboard Types
export interface DashboardStats {
  totalUsers: number
  activeUsers: number
  totalRoles: number
  totalPermissions: number
  recentLogins: number
}

export interface MenuItem {
  id: string
  title: string
  icon: string
  path?: string
  children?: MenuItem[]
  permission?: string
  badge?: string | number
  color?: string
}

export interface UserPreferences {
  theme: 'light' | 'dark' | 'auto'
  language: string
  notifications: boolean
  compactMode: boolean
  sidebarCollapsed: boolean
} 