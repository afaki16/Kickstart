import type { ApiResponse } from '~/types'
import { useToastStore } from '~/stores/toast'

export const useApi = () => {
  const { $api } = useNuxtApp()
  const toastStore = useToastStore()

  const handleApiError = (error: any) => {
    const data = error?.response?.data ?? error?.data

    if (!data) {
      toastStore.add('error', error?.message || 'An unexpected error occurred')
      throw error
    }

    const collected: string[] = []

    if (data.error?.errors && typeof data.error.errors === 'object') {
      for (const field of Object.values(data.error.errors) as any[]) {
        if (Array.isArray(field)) {
          collected.push(...field)
        } else if (typeof field === 'string') {
          collected.push(field)
        }
      }
    }

    if (collected.length === 0 && Array.isArray(data.errors)) {
      collected.push(...data.errors)
    }

    if (collected.length === 0 && typeof data.error === 'string') {
      collected.push(data.error)
    }

    if (collected.length === 0 && data.message) {
      collected.push(data.message)
    }

    if (collected.length === 0) {
      collected.push('An unexpected error occurred')
    }

    toastStore.add('error', collected)
    throw error
  }

  const get = async <T>(url: string): Promise<ApiResponse<T>> => {
    try {
      const response = await $api.get(url)
      return response.data
    } catch (error) {
      handleApiError(error)
      throw error
    }
  }

  const post = async <T>(url: string, data?: any): Promise<ApiResponse<T>> => {
    try {
      const response = await $api.post(url, data)
      return response.data
    } catch (error) {
      handleApiError(error)
      throw error
    }
  }

  const put = async <T>(url: string, data?: any): Promise<ApiResponse<T>> => {
    try {
      const response = await $api.put(url, data)
      return response.data
    } catch (error) {
      handleApiError(error)
      throw error
    }
  }

  const del = async <T>(url: string): Promise<ApiResponse<T>> => {
    try {
      const response = await $api.delete(url)
      return response.data
    } catch (error) {
      handleApiError(error)
      throw error
    }
  }

  return {
    get,
    post,
    put,
    delete: del,
    handleApiError
  }
}
