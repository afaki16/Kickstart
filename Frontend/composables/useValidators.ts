export const useValidators = () => {
  const validationRules = {
    required: (value: any) => !!value || 'This field is required',
    
    email: (value: string) => {
      const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
      return pattern.test(value) || 'Please enter a valid email address'
    },
    
    password: (value: string) => {
      if (!value) return 'Password is required'
      if (value.length < 6) return 'Password must be at least 6 characters'
      if (!/(?=.*[a-z])/.test(value)) return 'Password must contain at least one lowercase letter'
      if (!/(?=.*[A-Z])/.test(value)) return 'Password must contain at least one uppercase letter'
      if (!/(?=.*\d)/.test(value)) return 'Password must contain at least one number'
      return true
    },
    
    confirmPassword: (password: string) => (value: string) => {
      return value === password || 'Passwords do not match'
    },
    
    minLength: (min: number) => (value: string) => {
      return value.length >= min || `Must be at least ${min} characters`
    },
    
    maxLength: (max: number) => (value: string) => {
      return value.length <= max || `Must be no more than ${max} characters`
    },
    
    phone: (value: string) => {
      if (!value) return true // Optional field
      const pattern = /^[\+]?[1-9][\d]{0,15}$/
      return pattern.test(value) || 'Please enter a valid phone number'
    }
  }
  
  return {
    validationRules
  }
} 