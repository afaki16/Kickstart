import { defineRule, configure } from 'vee-validate'
import { required, email, min, confirmed } from '@vee-validate/rules'

export default defineNuxtPlugin(() => {
  // Define rules
  defineRule('required', required)
  defineRule('email', email)
  defineRule('min', min)
  defineRule('confirmed', confirmed)

  // Custom rules
  defineRule('password', (value: string) => {
    if (!value) return 'Password is required'
    if (value.length < 6) return 'Password must be at least 6 characters'
    if (!/(?=.*[a-z])/.test(value)) return 'Password must contain at least one lowercase letter'
    if (!/(?=.*[A-Z])/.test(value)) return 'Password must contain at least one uppercase letter'
    if (!/(?=.*\d)/.test(value)) return 'Password must contain at least one number'
    return true
  })

  // Configure
  configure({
    generateMessage: (ctx) => {
      const messages: Record<string, string> = {
        required: `${ctx.field} is required`,
        email: `${ctx.field} must be a valid email`,
        min: `${ctx.field} must be at least ${ctx.rule?.params?.[0]} characters`,
        confirmed: `${ctx.field} confirmation does not match`
      }
      return messages[ctx.rule?.name as string] || `${ctx.field} is invalid`
    }
  })
}) 