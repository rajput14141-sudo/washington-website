import { Link } from 'react-router-dom'

export default function Footer() {
  return (
    <footer id="contact" className="border-t border-teal-900 bg-teal-950 text-teal-100">
      <div className="page-shell grid gap-8 py-8 sm:grid-cols-3 lg:grid-cols-[1.5fr_1fr_1fr] lg:py-10">
        <div>
          <Link to="/" className="text-2xl font-black text-white">Mr.WashingTon</Link>
          <p className="mt-3 max-w-sm text-sm leading-6 text-slate-400">
            Reliable doorstep car washing and vehicle-care support across Greater Noida.
          </p>
          <p className="mt-4 text-sm"><a className="hover:text-white" href="tel:+919220475319">+91 92204 75319</a></p>
        </div>
        <div>
          <h3 className="text-sm font-extrabold uppercase text-white">Company</h3>
          <nav className="mt-3 grid gap-2 text-sm">
            <Link to="/" className="hover:text-white">Home</Link>
            <Link to="/services" className="hover:text-white">Services</Link>
            <a href="tel:+919220475319" className="hover:text-white">Contact Us</a>
          </nav>
        </div>
        <div>
          <h3 className="text-sm font-extrabold uppercase text-white">Policies</h3>
          <nav className="mt-3 grid gap-2 text-sm">
            <Link to="/policies/terms" className="hover:text-white">Terms &amp; Conditions</Link>
            <Link to="/policies/privacy" className="hover:text-white">Privacy Policy</Link>
            <Link to="/policies/refunds" className="hover:text-white">Cancellation &amp; Refunds</Link>
          </nav>
        </div>
      </div>
      <div className="border-t border-white/10 px-5 py-3 text-center text-xs text-slate-400">
        © {new Date().getFullYear()} Mr.WashingTon Car Wash. All rights reserved.
      </div>
    </footer>
  )
}