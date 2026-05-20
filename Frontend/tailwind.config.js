/**
 * Tailwind config.
 *
 * @nuxtjs/tailwindcss bu dosyayı kendi default content/preset ayarlarıyla
 * birleştirir; burada yalnızca tema uzantılarını tanımlıyoruz.
 *
 * RENKLER: Whoooper premium dark tema paleti (Material 3 token isimleriyle).
 * SPACING / TYPOGRAPHY: layouts/default.vue'nin kullandığı özel tokenlar
 *   (py-base, px-md, px-margin-desktop, text-headline-md, font-body-md, ...).
 *   Bu tokenlar başka yerde tanımlı olmadığından layout'un doğru render
 *   olması için burada tanımlanır.
 */
module.exports = {
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // CSS variables RGB channel formatında ("R G B" — değer theme.css'te).
        // rgb(var(--color-x) / <alpha-value>) wrapper'ı Tailwind opacity modifier'larını destekler:
        //   bg-primary    → rgb(0 108 73 / 1)
        //   bg-primary/10 → rgb(0 108 73 / 0.1)
        //   bg-primary/50 → rgb(0 108 73 / 0.5)

        // Surface palette
        'surface-bright': 'rgb(var(--color-surface-bright) / <alpha-value>)',
        'surface': 'rgb(var(--color-surface) / <alpha-value>)',
        'surface-dim': 'rgb(var(--color-surface-dim) / <alpha-value>)',
        'surface-container-lowest': 'rgb(var(--color-surface-container-lowest) / <alpha-value>)',
        'surface-container-low': 'rgb(var(--color-surface-container-low) / <alpha-value>)',
        'surface-container': 'rgb(var(--color-surface-container) / <alpha-value>)',
        'surface-container-high': 'rgb(var(--color-surface-container-high) / <alpha-value>)',
        'surface-container-highest': 'rgb(var(--color-surface-container-highest) / <alpha-value>)',
        'surface-variant': 'rgb(var(--color-surface-variant) / <alpha-value>)',
        'surface-tint': 'rgb(var(--color-surface-tint) / <alpha-value>)',

        // On-surface
        'on-surface': 'rgb(var(--color-on-surface) / <alpha-value>)',
        'on-surface-variant': 'rgb(var(--color-on-surface-variant) / <alpha-value>)',

        // Background
        'background': 'rgb(var(--color-background) / <alpha-value>)',
        'on-background': 'rgb(var(--color-on-background) / <alpha-value>)',

        // Primary
        'primary': 'rgb(var(--color-primary) / <alpha-value>)',
        'on-primary': 'rgb(var(--color-on-primary) / <alpha-value>)',
        'primary-container': 'rgb(var(--color-primary-container) / <alpha-value>)',
        'on-primary-container': 'rgb(var(--color-on-primary-container) / <alpha-value>)',
        'primary-fixed': 'rgb(var(--color-primary-fixed) / <alpha-value>)',
        'primary-fixed-dim': 'rgb(var(--color-primary-fixed-dim) / <alpha-value>)',
        'on-primary-fixed': 'rgb(var(--color-on-primary-fixed) / <alpha-value>)',
        'on-primary-fixed-variant': 'rgb(var(--color-on-primary-fixed-variant) / <alpha-value>)',
        'inverse-primary': 'rgb(var(--color-inverse-primary) / <alpha-value>)',

        // Secondary
        'secondary': 'rgb(var(--color-secondary) / <alpha-value>)',
        'on-secondary': 'rgb(var(--color-on-secondary) / <alpha-value>)',
        'secondary-container': 'rgb(var(--color-secondary-container) / <alpha-value>)',
        'on-secondary-container': 'rgb(var(--color-on-secondary-container) / <alpha-value>)',
        'secondary-fixed': 'rgb(var(--color-secondary-fixed) / <alpha-value>)',
        'secondary-fixed-dim': 'rgb(var(--color-secondary-fixed-dim) / <alpha-value>)',
        'on-secondary-fixed': 'rgb(var(--color-on-secondary-fixed) / <alpha-value>)',
        'on-secondary-fixed-variant': 'rgb(var(--color-on-secondary-fixed-variant) / <alpha-value>)',

        // Tertiary
        'tertiary': 'rgb(var(--color-tertiary) / <alpha-value>)',
        'on-tertiary': 'rgb(var(--color-on-tertiary) / <alpha-value>)',
        'tertiary-container': 'rgb(var(--color-tertiary-container) / <alpha-value>)',
        'on-tertiary-container': 'rgb(var(--color-on-tertiary-container) / <alpha-value>)',
        'tertiary-fixed': 'rgb(var(--color-tertiary-fixed) / <alpha-value>)',
        'tertiary-fixed-dim': 'rgb(var(--color-tertiary-fixed-dim) / <alpha-value>)',
        'on-tertiary-fixed': 'rgb(var(--color-on-tertiary-fixed) / <alpha-value>)',
        'on-tertiary-fixed-variant': 'rgb(var(--color-on-tertiary-fixed-variant) / <alpha-value>)',

        // Error
        'error': 'rgb(var(--color-error) / <alpha-value>)',
        'on-error': 'rgb(var(--color-on-error) / <alpha-value>)',
        'error-container': 'rgb(var(--color-error-container) / <alpha-value>)',
        'on-error-container': 'rgb(var(--color-on-error-container) / <alpha-value>)',

        // Outlines
        'outline': 'rgb(var(--color-outline) / <alpha-value>)',
        'outline-variant': 'rgb(var(--color-outline-variant) / <alpha-value>)',

        // Inverse
        'inverse-surface': 'rgb(var(--color-inverse-surface) / <alpha-value>)',
        'inverse-on-surface': 'rgb(var(--color-inverse-on-surface) / <alpha-value>)'
      },
      // Layout'un kullandığı özel spacing tokenları
      spacing: {
        'xs': '0.5rem',
        'md': '1rem',
        'base': '1rem',
        'margin-desktop': '2rem'
      },
      // Material 3 tipografi ölçeği (text-* ile kullanılır)
      fontSize: {
        'headline-md': ['1.75rem', { lineHeight: '2.25rem' }],
        'body-md': ['0.875rem', { lineHeight: '1.25rem' }],
        'body-sm': ['0.8125rem', { lineHeight: '1.125rem' }],
        'label-sm': ['0.6875rem', { lineHeight: '1rem' }]
      },
      // font-* ile kullanılan font aileleri
      fontFamily: {
        'headline-md': ['Plus Jakarta Sans', 'Inter', 'sans-serif'],
        'body-md': ['Inter', 'sans-serif']
      }
    }
  }
}
