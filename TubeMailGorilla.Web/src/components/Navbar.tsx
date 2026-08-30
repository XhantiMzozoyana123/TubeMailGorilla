import React, { useState, useEffect } from 'react';
import { Menu, X, User } from 'lucide-react';
import { UserProfile } from '../types';

interface NavbarProps {
  onOpenTrial: () => void;
  savedCount: number;
  onOpenProspectDrawer: () => void;
  user: UserProfile | null;
  onOpenAuth: (mode: 'login' | 'register') => void;
  onOpenAccount: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({ user, onOpenAuth, onOpenAccount }) => {
  const [isScrolled, setIsScrolled] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    const onScroll = () => setIsScrolled(window.scrollY > 12);
    window.addEventListener('scroll', onScroll);
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  return (
    <nav
      className={`fixed top-0 left-0 right-0 z-50 border-b transition-colors ${isScrolled ? 'bg-[#08090c]/80 border-slate-800' : 'bg-transparent border-transparent'} pt-safe`}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex items-center justify-between h-14">
        {/* Brand */}
        <a href="#" className="flex items-center gap-3" id="nav-brand-logo">
                  <div className="w-9 h-9 rounded-lg flex items-center justify-center text-xl border border-[#ff3366]/40">
                        <img src="/src/images/tubemailgorilla-icon.svg" alt="TubeMail Gorilla" className="w-full h-full object-contain filter drop-shadow-[0_0_8px_rgba(255,0,59,0.4)]" />
          </div>
          <div className="flex flex-col">
            <div className="flex items-center gap-1 font-['Rajdhani',sans-serif] font-extrabold text-xl tracking-wide text-white uppercase">
              <span>TubeMail</span>
              <span className="text-[#ff003b]">Gorilla</span>
            </div>
            <span className="text-[10px] text-slate-500 font-mono tracking-wider uppercase">
              YouTube Client Outreach Engine
            </span>
          </div>
        </a>

        {/* Desktop links */}
        <div className="hidden md:flex items-center gap-6 text-sm font-medium text-slate-300 font-['Rajdhani',sans-serif] uppercase">
          <a
            href="#/subscription"
            className="hover:text-[#ff003b] transition-colors"
            id="nav-link-pricing"
          >
            Pricing
          </a>
          {user ? (
            <button
              onClick={onOpenAccount}
              className="px-3 py-1.5 rounded-lg border border-[#ff003b]/40 text-white text-xs font-mono font-bold uppercase tracking-wider hover:bg-[#141724] transition-colors"
              id="nav-account-btn"
            >
              <User className="w-3.5 h-3.5 text-[#ff003b]" />
            </button>
          ) : (
            <div className="flex items-center gap-2.5">
              <button
                onClick={() => onOpenAuth('login')}
                className="px-3 py-1.5 rounded-lg border border-slate-700 text-slate-200 text-xs font-mono font-bold uppercase tracking-wider hover:bg-[#141724] transition-colors"
                id="nav-signin-btn"
              >
                Sign In
              </button>
              <button
                onClick={() => onOpenAuth('register')}
                className="px-4 py-1.5 rounded-lg bg-[#ff003b] text-white text-xs font-extrabold font-['Rajdhani',sans-serif] uppercase tracking-wider hover:bg-[#ff1a4b] transition-colors"
                id="nav-cta-btn"
              >
                Register Free
              </button>
            </div>
          )}
        </div>

        {/* Mobile toggle */}
        <button
          onClick={() => setMobileOpen(!mobileOpen)}
          className="md:hidden p-2 rounded-lg text-slate-300 hover:text-white"
          aria-label="Toggle menu"
          id="nav-mobile-toggle"
        >
          {mobileOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
        </button>
      </div>

      {/* Mobile menu */}
      {mobileOpen && (
        <div className="md:hidden bg-[#08090c] border-t border-slate-800 py-3">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col gap-2 text-sm font-medium">
            <a
              href="#/subscription"
              onClick={() => setMobileOpen(false)}
              className="py-2 text-slate-300 hover:text-[#ff003b]"
            >
              Pricing
            </a>
            {user ? (
              <button
                onClick={() => {
                  setMobileOpen(false);
                  onOpenAccount();
                }}
                className="w-full py-2 text-left text-slate-300 hover:text-[#ff003b]"
              >
                <span className="uppercase">My Account</span>
              </button>
            ) : (
              <div className="flex gap-2">
                <button
                  onClick={() => {
                    setMobileOpen(false);
                    onOpenAuth('login');
                  }}
                  className="flex-1 py-2 rounded-lg border border-slate-700 text-white text-center text-xs font-mono font-bold uppercase"
                >
                  Sign In
                </button>
                <button
                  onClick={() => {
                    setMobileOpen(false);
                    onOpenAuth('register');
                  }}
                  className="flex-1 py-2 rounded-lg bg-[#ff003b] text-white text-center text-xs font-bold font-['Rajdhani',sans-serif] uppercase"
                >
                  Register
                </button>
              </div>
            )}
          </div>
        </div>
      )}
    </nav>
  );
};
