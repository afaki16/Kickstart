import type { ToastType } from '~/stores/toast'

interface ToastOptions {
  title?: string
  duration?: number
}

export const useToast = () => {
  const store = useToastStore()

  const success = (messages: string | string[], options?: ToastOptions) =>
    store.add('success', messages, options?.title, options?.duration)

  const error = (messages: string | string[], options?: ToastOptions) =>
    store.add('error', messages, options?.title, options?.duration)

  const warning = (messages: string | string[], options?: ToastOptions) =>
    store.add('warning', messages, options?.title, options?.duration)

  const info = (messages: string | string[], options?: ToastOptions) =>
    store.add('info', messages, options?.title, options?.duration)

  /**
   * Parses various API error response formats and shows an error toast.
   *
   * Supported shapes:
   *  - { error: { errors: { fieldName: [...msgs] } } }
   *  - { errors: [...msgs] }
   *  - { error: "msg" }
   *  - { message: "msg" }
   */
  const showApiError = (error: any, fallbackMessage = 'An unexpected error occurred') => {
    const data = error?.response?.data ?? error?.data

    if (!data) {
      store.add('error', error?.message || fallbackMessage)
      return
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
      collected.push(fallbackMessage)
    }

    store.add('error', collected)
  }

  return {
    toasts: computed(() => store.items),
    success,
    error,
    warning,
    info,
    remove: store.remove,
    clearAll: store.clear,
    showApiError,
  }
}
