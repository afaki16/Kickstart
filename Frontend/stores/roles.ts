import { defineStore } from 'pinia'
import type { Role, RoleState } from '~/types'

export const useRolesStore = defineStore('roles', {
  state: (): RoleState => ({
    roles: [],
    currentRole: null,
    isLoading: false
  }),

  getters: {
    systemRoles: (state) => state.roles.filter(role => role.isSystemRole),
    customRoles: (state) => state.roles.filter(role => !role.isSystemRole),
    getRoleById: (state) => (id: string) => {
      return state.roles.find(role => role.id === id)
    },
    getRoleByName: (state) => (name: string) => {
      return state.roles.find(role => role.name === name)
    }
  },

  actions: {
    setRoles(roles: Role[]) {
      this.roles = roles
    },

    addRole(role: Role) {
      this.roles.push(role)
    },

    updateRole(updatedRole: Role) {
      const index = this.roles.findIndex(role => role.id === updatedRole.id)
      if (index !== -1) {
        this.roles[index] = updatedRole
      }
    },

    removeRole(roleId: string) {
      const index = this.roles.findIndex(role => role.id === roleId)
      if (index !== -1) {
        this.roles.splice(index, 1)
      }
    },

    setCurrentRole(role: Role | null) {
      this.currentRole = role
    },

    setLoading(loading: boolean) {
      this.isLoading = loading
    },

    clearRoles() {
      this.roles = []
      this.currentRole = null
    }
  }
}) 