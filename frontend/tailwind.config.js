/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        '7ouma': {
          50:  '#eff6ff',
          100: '#dbeafe',
          500: '#3b82f6',
          600: '#1A56DB',
          700: '#1e40af',
          800: '#1e3a8a',
          900: '#1e3a5f',
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
      },
      borderRadius: {
        '2xl': '1rem',
        '3xl': '1.5rem',
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms'),
  ]
}
