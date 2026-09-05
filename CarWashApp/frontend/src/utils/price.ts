export function formatPrice(price: string | number) {
  const value = String(price);
  const numericPrice = Number(value.replace(/,/g, ""));

  return Number.isFinite(numericPrice)
    ? `₹${numericPrice}`
    : value;
}