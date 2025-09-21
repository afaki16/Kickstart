import { defineStore } from 'pinia'
import type { Permission, PermissionState } from '~/types'

export const usePermissionsStore = defineStore('permissions', {
  state: (): PermissionState => ({
    permissions: [],
    isLoading: false
  }),

  getters: {
    groupedPermissions: (state) => {
      return state.permissions.reduce((groups, permission) => {
        const resource = permission.resource
        if (!groups[resource]) {
          groups[resource] = []
        }
        groups[resource].push(permission)
        return groups
      }, {} as Record<string, Permission[]>)
    },
    
    getPermissionById: (state) => (id: string) => {
      return state.permissions.find(permission => permission.id === id)
    },
    
    getPermissionsByResource: (state) => (resource: string) => {
      return state.permissions.filter(permission => permission.resource === resource)
    },

    activePermissions: (state) => {
      return state.permissions.filter(permission => permission.isActive !== false)
    },

    uniqueResources: (state) => {
      return [...new Set(state.permissions.map(permission => permission.resource))]
    }
  },

  actions: {
    setPermissions(permissions: Permission[]) {
      this.permissions = permissions
    },

    setLoading(loading: boolean) {
      this.isLoading = loading
    },

    clearPermissions() {
      this.permissions = []
    },

    // API Methods
    async fetchPermissions() {
      try {
        this.setLoading(true)
        const { $api } = useNuxtApp()
        
        const response = await $api.get('/Permissions?version=1.0')
        
        if (response.data && Array.isArray(response.data)) {
          this.setPermissions(response.data)
        } else if (response.data && response.data.data && Array.isArray(response.data.data)) {
          // API response wrapper format
          this.setPermissions(response.data.data)
        } else {
          throw new Error('Invalid response format from API')
        }
      } catch (error) {
        console.error('Error fetching permissions:', error)
        throw error
      } finally {
        this.setLoading(false)
      }
    },

    async createPermission(permissionData: Partial<Permission>) {
      try {
        this.setLoading(true)
        const { $api } = useNuxtApp()
        
        const response = await $api.post('/Permissions?version=1.0', permissionData)
        
        if (response.data) {
          const newPermission = response.data.data || response.data
          this.permissions.push(newPermission)
          return newPermission
        } else {
          throw new Error('Failed to create permission')
        }
      } catch (error) {
        console.error('Error creating permission:', error)
        throw error
      } finally {
        this.setLoading(false)
      }
    },

    async updatePermission(id: string, permissionData: Partial<Permission>) {
      try {
        this.setLoading(true)
        const { $api } = useNuxtApp()
        
        const response = await $api.put(`/Permissions/${id}?version=1.0`, permissionData)
        
        if (response.data) {
          const updatedPermission = response.data.data || response.data
          const index = this.permissions.findIndex(p => p.id === id)
          if (index !== -1) {
            this.permissions[index] = updatedPermission
          }
          return updatedPermission
        } else {
          throw new Error('Failed to update permission')
        }
      } catch (error) {
        console.error('Error updating permission:', error)
        throw error
      } finally {
        this.setLoading(false)
      }
    },

    async deletePermission(id: string) {
      try {
        this.setLoading(true)
        const { $api } = useNuxtApp()
        
        const response = await $api.delete(`/Permissions/${id}?version=1.0`)
        
        if (response.status === 200 || response.status === 204) {
          this.permissions = this.permissions.filter(p => p.id !== id)
          return true
        } else {
          throw new Error('Failed to delete permission')
        }
      } catch (error) {
        console.error('Error deleting permission:', error)
        throw error
      } finally {
        this.setLoading(false)
      }
    },

    async fetchPermissionById(id: string) {
      try {
        this.setLoading(true)
        const { $api } = useNuxtApp()
        
        const response = await $api.get(`/Permissions/${id}?version=1.0`)
        
        if (response.data) {
          return response.data.data || response.data
        } else {
          throw new Error('Permission not found')
        }
      } catch (error) {
        console.error('Error fetching permission:', error)
        throw error
      } finally {
        this.setLoading(false)
      }
    }
  }
}) 