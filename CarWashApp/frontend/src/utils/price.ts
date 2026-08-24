export function formatPrice(price: string) {
  const numericPrice = Number(price.replace(/,/g, ''))
  return price.trim() !== '' && Number.isFinite(numericPrice) ? `₹${price}` : price
}