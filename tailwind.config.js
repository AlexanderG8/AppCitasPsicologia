/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml",
  ],
  theme: {
    extend: {
      colors: {
        sidebar: {
          DEFAULT: '#1e293b',
          hover:   '#334155',
          active:  '#334155',
          border:  'rgba(255,255,255,0.08)',
        },
      },
      fontFamily: {
        sans: ['-apple-system', 'BlinkMacSystemFont', '"Segoe UI"', 'Roboto', '"Helvetica Neue"', 'Arial', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
