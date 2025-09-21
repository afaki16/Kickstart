import type { Role, CreateRoleRequest, UpdateRoleRequest } from '~/types'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'
import { useApi } from './useApi'

export const useRoles = () => {
  const api = useApi()

  const getRoles = async () => {
    try {
      const response = await api.get<Role[]>(API_ENDPOINTS.ROLES.LIST)
      return response.data || []
    } catch (error) {
      console.error('Get roles error:', error)
      throw error
    }
  }

  const getRoleById = async (id: string) => {
    try {
      const response = await api.get<Role>(API_ENDPOINTS.ROLES.GET_BY_ID(id))
      return response.data
    } catch (error) {
      console.error('Get role error:', error)
      throw error
    }
  }

  const createRole = async (roleData: CreateRoleRequest) => {
    try {
      const response = await api.post<Role>(API_ENDPOINTS.ROLES.CREATE, roleData)
      if (response.success) {
        return response.data
      }
    } catch (error) {
      console.error('Create role error:', error)
      throw error
    }
  }

  const updateRole = async (id: string, roleData: UpdateRoleRequest) => {
    try {
      const response = await api.put<Role>(API_ENDPOINTS.ROLES.UPDATE(id), roleData)
      if (response.success) {
        return response.data
      }
    } catch (error) {
      console.error('Update role error:', error)
      throw error
    }
  }

  const deleteRole = async (id: string) => {
    try {
      await api.delete(API_ENDPOINTS.ROLES.DELETE(id))
      return true
    } catch (error) {
      console.error('Delete role error:', error)
      throw error
    }
  }

  return {
    getRoles,
    getRoleById,
    createRole,
    updateRole,
    deleteRole
  }
} 