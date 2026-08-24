/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        teal: {
          50: '#F2F5FF',
          100: '#E3E9FF',
          200: '#C7D3FF',
          300: '#A3B5FA',
          400: '#7892F0',
          500: '#5B78E8',
          600: '#4A6FE5',
          700: '#4169E1',
          800: '#3455B8',
          900: '#29438F',
          950: '#172554',
        },
      },
    },
  },
  plugins: [],
}
