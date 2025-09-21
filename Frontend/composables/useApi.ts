import type { ApiResponse } from '~/types'

export const useApi = () => {
  const { $api } = useNuxtApp()


  const handleApiError = (error: any) => {
    console.error('API Error:', error)
    console.error('Response Status:', error.response?.status)
    console.error('Response Data:', error.response?.data)
    console.error('Response Headers:', error.response?.headers)
    
    if (error.response?.data?.errors) {
      // Multiple validation errors
      error.response.data.errors.forEach((err: string) => {
        
      })
    } else if (error.response?.data?.error) {
      // Single error message
    } else if (error.response?.data?.message) {
      // API error message
    } else if (error.response?.data) {
      // Any other error data
    } else if (error.message) {
      // Network or other errors
    } else {
    }
    
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