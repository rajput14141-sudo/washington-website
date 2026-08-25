export function formatPrice(price: string | number) {
  const value = String(price)
  const numericPrice = Number(value.replace(/,/g, ''))

  return value.trim() !== '' && Number.isFinite(numericPrice)
    ? `₹${value}`
    : value
}