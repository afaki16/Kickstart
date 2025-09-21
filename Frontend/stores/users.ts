import { defineStore } from 'pinia'
import type { User, UserState } from '~/types'

export const useUsersStore = defineStore('users', {
  state: (): UserState => ({
    users: [],
    currentUser: null,
    totalUsers: 0,
    isLoading: false,
    searchTerm: '',
    currentPage: 1,
    itemsPerPage: 10
  }),

  getters: {
    filteredUsers: (state) => {
      if (!state.searchTerm) return state.users
      
      const search = state.searchTerm.toLowerCase()
      return state.users.filter(user => 
        user.firstName.toLowerCase().includes(search) ||
        user.lastName.toLowerCase().includes(search) ||
        user.email.toLowerCase().includes(search)
      )
    },
    
    paginatedUsers: (state) => {
      const start = (state.currentPage - 1) * state.itemsPerPage
      const end = start + state.itemsPerPage
      return state.users.slice(start, end)
    },
    
    totalPages: (state) => {
      return Math.ceil(state.totalUsers / state.itemsPerPage)
    },
    
    getUserById: (state) => (id: string) => {
      return state.users.find(user => user.id === id)
    }
  },

  actions: {
    setUsers(users: User[]) {
      this.users = users
      this.totalUsers = users.length
    },

    addUser(user: User) {
      this.users.unshift(user)
      this.totalUsers++
    },

    updateUser(updatedUser: User) {
      const index = this.users.findIndex(user => user.id === updatedUser.id)
      if (index !== -1) {
        this.users[index] = updatedUser
      }
    },

    removeUser(userId: string) {
      const index = this.users.findIndex(user => user.id === userId)
      if (index !== -1) {
        this.users.splice(index, 1)
        this.totalUsers--
      }
    },

    setCurrentUser(user: User | null) {
      this.currentUser = user
    },

    setLoading(loading: boolean) {
      this.isLoading = loading
    },

    setSearchTerm(term: string) {
      this.searchTerm = term
      this.currentPage = 1 // Reset to first page when searching
    },

    setCurrentPage(page: number) {
      this.currentPage = page
    },

    setItemsPerPage(items: number) {
      this.itemsPerPage = items
      this.currentPage = 1 // Reset to first page
    },

    clearUsers() {
      this.users = []
      this.currentUser = null
      this.totalUsers = 0
    }
  }
}) 