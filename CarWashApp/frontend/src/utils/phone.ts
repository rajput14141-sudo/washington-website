export const INDIAN_MOBILE_PATTERN = '[789][0-9]{9}'

export function sanitizeIndianMobile(value: string) {
  const digits = value.replace(/\D/g, '').slice(0, 10)
  return digits === '' || /^[789]/.test(digits) ? digits : ''
}
