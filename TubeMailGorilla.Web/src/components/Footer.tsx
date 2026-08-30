import React from 'react';
import { ArrowUp, Heart, Youtube, Mail, Sparkles } from 'lucide-react';

interface FooterProps {
  onOpenTrial: () => void;
}

export const Footer: React.FC<FooterProps> = ({ onOpenTrial }) => {
  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <footer className="bg-[#050608] border-t border-slate-800/80 pt-16 pb-12 text-slate-400 text-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-10 mb-12">
          {/* Brand Col */}
          <div className="md:col-span-2 space-y-4">
            <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-[#ff003b] to-[#b30029] flex items-center justify-center text-xl shadow-[0_0_15px_rgba(255,0,59,0.4)] border border-[#ff4d73]/40">
                              <img src="/src/images/tubemailgorilla-icon.svg" alt="TubeMail Gorilla" className="w-full h-full object-contain filter drop-shadow-[0_0_8px_rgba(255,0,59,0.4)]" />
              </div>
              <span className="font-extrabold text-2xl text-white font-['Rajdhani',sans-serif] tracking-wider uppercase">
                TubeMail <span className="text-[#ff003b]">Gorilla</span>
              </span>
            </div>
            <p className="text-slate-400 text-sm max-w-md leading-relaxed">
              The client-finding machine for video editors. Discover YouTube creators, extract business contact info, and build a recurring client pipeline.
            </p>
            <p className="text-xs text-[#ff4d73] font-mono font-semibold">
              ⚡ LESS SEARCHING. MORE PROSPECTS. MORE EDITING RETAINERS.
            </p>
          </div>

          {/* Quick Links */}
          <div>
            <h4 className="font-bold text-white text-xs uppercase tracking-wider mb-4 font-mono text-[#ff4d73]">
              Navigation
            </h4>
                        <ul className="space-y-2.5 text-xs font-mono">
              <li>
                <a href="#the-problem" className="hover:text-[#ff003b] transition-colors">
                  Why Editors Struggle
                </a>
              </li>
              <li>
                <a href="#the-solution" className="hover:text-[#ff003b] transition-colors">
                  The Solution
                </a>
              </li>
              <li>
                <a href="#how-it-works" className="hover:text-[#ff003b] transition-colors">
                  How It Works
                </a>
              </li>
              <li>
                <a href="#calculator" className="hover:text-[#ff003b] transition-colors">
                  Earnings Calculator
                </a>
              </li>
              <li>
                <a href="#faq" className="hover:text-[#ff003b] transition-colors">
                  FAQ
                </a>
              </li>
            </ul>
          </div>

          {/* Action Col */}
          <div>
            <h4 className="font-bold text-white text-xs uppercase tracking-wider mb-4 font-mono text-[#ff4d73]">
              Get Started
            </h4>
            <p className="text-xs text-slate-400 mb-3 font-mono">
              Find YouTubers who need your editing skills today.
            </p>
            <button
              onClick={onOpenTrial}
              className="w-full py-2.5 px-4 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_15px_rgba(255,0,59,0.3)] transition-colors cursor-pointer border border-[#ff4d73]/40"
            >
              Start Free Trial →
            </button>
          </div>
        </div>

        {/* Bottom Bar */}
        <div className="pt-8 border-t border-slate-800/80 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs font-mono">
          <p className="text-slate-500">
            © {new Date().getFullYear()} TubeMail Gorilla. Built for high-performance video editors.
          </p>

          <button
            onClick={scrollToTop}
            className="flex items-center gap-1.5 text-slate-400 hover:text-[#ff003b] transition-colors cursor-pointer"
          >
            <span>BACK TO TOP</span>
            <ArrowUp className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </footer>
  );
};
