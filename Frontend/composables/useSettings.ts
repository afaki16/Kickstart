import { API_ENDPOINTS } from '~/utils/apiEndpoints'
import { useApi } from './useApi'

export const useSettings = () => {
  const api = useApi()

  /**
   * Get count of users with at least one active session.
   * @param tenantId - Optional. For SuperAdmin: null = all tenants. For Admin: ignored (uses own tenant).
   */
  const getActiveUserCount = async (tenantId?: number | null): Promise<number> => {
    try {
      const params = tenantId != null ? `?tenantId=${tenantId}` : ''
      const response = await api.get<number>(`${API_ENDPOINTS.ADMIN.ACTIVE_USERS_COUNT}${params}`)
      return response?.data ?? 0
    } catch (error) {
      console.error('Get active user count error:', error)
      throw error
    }
  }

  /**
   * Get users that can be revoked (active sessions only, SuperAdmin excluded).
   * For the remote logout dropdown.
   */
  const getRevokableUsers = async (tenantId?: number | null): Promise<Array<{ id: number; fullName: string; email: string }>> => {
    try {
      const params = tenantId != null ? `?tenantId=${tenantId}` : ''
      const response = await api.get<Array<{ id: number; fullName: string; email: string }>>(`${API_ENDPOINTS.ADMIN.REVOKABLE_USERS}${params}`)
      return response?.data ?? []
    } catch (error) {
      console.error('Get revokable users error:', error)
      throw error
    }
  }

  /**
   * Revoke all sessions for a user (admin logout from all devices).
   */
  const revokeUserSessions = async (userId: number, reason?: string): Promise<void> => {
    try {
      const params = reason ? `?reason=${encodeURIComponent(reason)}` : ''
      await api.post(`${API_ENDPOINTS.ADMIN.REVOKE_USER_SESSIONS(userId)}${params}`)
    } catch (error) {
      console.error('Revoke user sessions error:', error)
      throw error
    }
  }

  return {
    getActiveUserCount,
    getRevokableUsers,
    revokeUserSessions
  }
}
